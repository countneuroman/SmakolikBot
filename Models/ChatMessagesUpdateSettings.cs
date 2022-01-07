﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmakolikBot.Models;

public class ChatMessagesUpdateSettings
{
    public ChatMessagesUpdateSettings(long chatId, int counterValue = 0, int counterValueToNextMessage = 10)
    {
        ChatId = chatId;
        CounterValue = counterValue;
        CounterValueToNextMessage = counterValueToNextMessage;
    }
    public ChatMessagesUpdateSettings() { }

    [BsonId]
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Id { get; set; }

    public long ChatId { get; set; }
    public int CounterValue { get; set; }
    public int CounterValueToNextMessage { get; set; }
}