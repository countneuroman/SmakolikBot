using SmakolikBot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;

namespace SmakolikBot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly SmakolikDto? _smakolikMessages;


    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
        _smakolikMessages = GetSmakolikMessages.GetMesssages();
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
        if (message.Type != MessageType.Text)
            return;
        
        var action = message.Text!.Split(' ')[0] switch
        {
            "/рова" => SendRowaReply(_botClient, message),
            "/смаколик" => SendSmakolPhoto(_botClient, message),
            "/спиздани" => SendSmakolMessage(_botClient, message)
            //_ => Usage(_botClient, message)
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> SendRowaReply(ITelegramBotClient bot, Message message)
        {
            const string rowaReply = "ДРУГ.";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                text: rowaReply);
        }

        static async Task<Message> SendSmakolPhoto(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            var filesPath = new string[] {@"Files/SmakolEpic.jpg", @"Files/SmakolNaruto.jpg"};
            var r = new Random();
            var count = r.Next(0,filesPath.Length);
            using FileStream fileStream = new(filesPath[count], FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filesPath[count].Split(Path.DirectorySeparatorChar).Last();

            return await bot.SendPhotoAsync(chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "СМАААААКОООЛИК!");
        }
        
        static async Task<Message> Help(ITelegramBotClient bot, Message message)
        {
            const string usage = "Help: \n" +
                                 "/рова - переводим роваязык (в разработке)";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private async Task<Message> SendSmakolMessage(ITelegramBotClient bot, Message message)
    {
        var smakolikMessages = _smakolikMessages.Data.ToList();
        var r = new Random();
        var count = r.Next(0,smakolikMessages.Count - 1);
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