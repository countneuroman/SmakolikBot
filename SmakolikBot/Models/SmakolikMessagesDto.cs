using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmakolikBot.Models;

public class MessagesDto
{
    [BsonId]
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Id { get; set; }
    public string Message { get; set; }
}