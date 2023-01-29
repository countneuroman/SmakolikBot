using Microsoft.Extensions.Caching.Memory;
using SmakolikBot.Models;
using Telegram.Bot.Types;

namespace SmakolikBot.Services;

//TODO: After crashing/restart bot in database not save messages counter in chat groups. Not critical.
public class ChatSettingsService
{
    private readonly MongoService _mongoService;
    private readonly IMemoryCache _memoryCache;

    public ChatSettingsService(MongoService mongoService, IMemoryCache memoryCache)
    {
        _mongoService = mongoService;
        _memoryCache = memoryCache;
    }

    private async Task<ChatMessagesSettings?> GetChatSettings(long chatId)
    {
        if (!_memoryCache.TryGetValue<ChatMessagesSettings>(chatId, out var result))
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(30));
            
            ChatMessagesSettings? chatSettings = await _mongoService.GetAsync(chatId);
            _memoryCache.Set(chatId, chatSettings, cacheEntryOptions);
            result = chatSettings;
        }

        return result;
    }
    
    public async Task<bool> CounterMessages(Message message)
    {
        ChatMessagesSettings? chatSettings = await GetChatSettings(message.Chat.Id);
        
        if (chatSettings is null)
        {
            await _mongoService.CreateAsync(new ChatMessagesSettings(message.Chat.Id));
        }
        else
        {
            if(chatSettings.CounterValue >= chatSettings.CounterValueToNextMessage)
            {
                chatSettings.CounterValue = 0;
                return true;
            }

            chatSettings.CounterValue += 1;
        }

        return false;
    }
}