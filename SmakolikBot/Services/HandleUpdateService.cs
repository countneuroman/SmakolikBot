using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SmakolikBot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly GetMessageService _smakolikMessages;
    private readonly GetChatSettingsService _chatSettings;


    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger,
        GetMessageService smakolikMessages, GetChatSettingsService chatSettings)
    {
        _botClient = botClient;
        _logger = logger;
        _smakolikMessages = smakolikMessages;
        _chatSettings = chatSettings;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!),
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
        var check = await _chatSettings.CounterMessages(message);

        if (message.ReplyToMessage?.From?.Id == _botClient.BotId)
        { 
            await SendReplyMessage(_botClient, message);
        }

        if (check)
        {
            await SendMessage(_botClient, message);
        }
        
        switch (message.Type)
        {
            case MessageType.ChatMembersAdded:
                await SendWelcomeMessage(_botClient, message);
                break;
            case MessageType.ChatMemberLeft:
                await SendLeftMessage(_botClient, message);
                break;
            case MessageType.Text:
            {
                var action = message.Text switch
                {
                    var messageText when new Regex(@"(\B\/help\b$)|(\B\/help@smakolik_bot\b)")
                        .IsMatch(messageText!) => SendHelp(_botClient, message),
                    var messageText when new Regex(@"\B\@smakolik_bot\b")
                        .IsMatch(messageText!) => SendReplyMessage(_botClient, message),
                    _ => UnknownMessageHandlerAsync()
                };
                await action;
                break;
            }
            default:
                return;
        }

        static async Task SendWelcomeMessage(ITelegramBotClient bot, Message message)
        {
            var newChatMembers = message.NewChatMembers;
            foreach (var user in newChatMembers!)
            {
                var username = user.Username;
                var text = $"Милости прошу к нашему шалашу @{username}!";
                await bot.SendTextMessageAsync(message.Chat.Id, text);
            }
        }

        static async Task SendLeftMessage(ITelegramBotClient bot, Message message)
        {
            //TODO: Add counter who days user was group.
            var leftChatMember = message.LeftChatMember!.Username;
            var text = $"Ну и пошел ты, @{leftChatMember}!";
            await bot.SendTextMessageAsync(message.Chat.Id, text);
        }   

        static async Task<Message> SendHelp(ITelegramBotClient bot, Message message)
        {
            const string usage = "Данный бот просто любит общаться! \n";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove());
        }
    }
    
    private async Task<Message> SendMessage(ITelegramBotClient bot, Message message)
    {
        var messageText = _smakolikMessages.GetRandomMessage();
        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            text: messageText);
    }

    
    private async Task<Message> SendReplyMessage(ITelegramBotClient bot, Message message)
    {
        var messageText = _smakolikMessages.GetRandomMessage();
        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            text: messageText, replyToMessageId: message.MessageId);
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