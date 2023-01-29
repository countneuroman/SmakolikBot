using Microsoft.Extensions.Caching.Memory;
using SmakolikBot.Models;

namespace SmakolikBot.Services;

public  class MessagesService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MongoService _mongoService;
    private readonly ILogger<MessagesService> _logger;

    private List<MessagesDto>? Messages { get; set; }

    public MessagesService(MongoService mongoService, IMemoryCache memoryCache, ILogger<MessagesService> logger)
    {
        _mongoService = mongoService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public string? GetRandomMessage()
    {
        GetAllMesages();
        
        if (Messages == null)
        {
            _logger.LogError("Messages object is null.");
            return "Упс, я паходу сломался..";
        }
        
        var r = new Random();
        var count = r.Next(0, Messages.Count - 1);
        return Messages[count].Message;

    }
    
    private void GetAllMesages()
    {
        const string key = "MessagesKey";
        
        if (!_memoryCache.TryGetValue<List<MessagesDto>>(key, out var result))
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(30));
            
            Messages = _mongoService.GetMessagesAsync().Result;
            _memoryCache.Set(key, Messages, cacheEntryOptions);
        }

        Messages = result;
    }
}