using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using SmakolikBot.Models;
using SmakolikBot.Services;

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
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery => BotCallbackQueryReceived(update.CallbackQuery!),
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
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        var check = await CounterMessages(message);
        if (check)
        {
            var act =SendSmakolMessage(_botClient, message);
            await act;
        }
        if (message.Type != MessageType.Text)
            return;
        
        var action = message.Text!.Trim().Split(' ')[0] switch
        {
            "/help@smakolik_bot" or "/help" => SendHelp(_botClient, message),
            "/спиздани" => SendSmakolMessage(_botClient, message)
        };
        
        await action;

        static async Task<Message> SendHelp(ITelegramBotClient bot, Message message)
        {
            const string usage = "/рова - переводим роваязык (в разработке) \n" +
                                 "/спиздани - спиздануть что нибудь \n" +
                                 "/смаколик - посмотрите на Смаколика!";
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

    private async Task BotCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}");

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: $"Received {callbackQuery.Data}");
    }

    private Task UnkownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unkown update type: {UpdateType}", update.Type);
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