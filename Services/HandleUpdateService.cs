using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using SmakolikBot.Models;

namespace SmakolikBot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly GetSmakolikMessages _smakolikMessages;
    private readonly MongoService _mongoService;


    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger,
        GetSmakolikMessages smakolikMessages, MongoService mongoService)
    {
        _botClient = botClient;
        _logger = logger;
        _smakolikMessages = smakolikMessages;
        _mongoService = mongoService;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            _ => UnkownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        var check = await CounterMessages(message);

        if (message.ReplyToMessage?.From?.Id == _botClient.BotId || check)
        { 
            await SendSmakolMessage(_botClient, message);
        }
        
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Trim().Split(' ')[0] switch
        {
            "/help@smakolik_bot" or "/help" => SendHelp(_botClient, message),
            _ => UnknownMessageHandlerAsync()
        };
        
        await action;

        static async Task<Message> SendHelp(ITelegramBotClient bot, Message message)
        {
            const string usage = "Данный бот просто любит общаться! \n";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private async Task<bool> CounterMessages(Message message)
    {
        var chatId = message.Chat.Id;
        var chatObj = await _mongoService.GetAsync(message.Chat.Id);
        if (chatObj is null)
        {
            var chat = new ChatMessagesUpdateSettings(chatId);
            await _mongoService.CreateAsync(chat);
        }
        else
        {
            if(chatObj.CounterValue >= 10)
            {
                chatObj.CounterValue = 0;
                await _mongoService.UpdateAsync(chatObj.Id!, chatObj);
                return true;
            }

            chatObj.CounterValue += 1;
            await _mongoService.UpdateAsync(chatObj.Id!, chatObj);
        }

        return false;
    }
    
    private async Task<Message> SendSmakolMessage(ITelegramBotClient bot, Message message)
    {
        var smakolikMessages = _smakolikMessages.GetMessages();
        var r = new Random();
        var count = r.Next(0, smakolikMessages!.Count - 1);
        var reply = smakolikMessages[count].Message;
        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            text: reply);
    }
    
    private Task UnkownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unkown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
    
    private Task UnknownMessageHandlerAsync()
    {
        return Task.CompletedTask;
    }
    
    public Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegrap API Error: \n[{apiRequestException.ErrorCode}]" +
                                                       $"\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        
        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}