using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SmakolikBot.Models;
using MongoDatabaseSettings = SmakolikBot.Models.MongoDatabaseSettings;

namespace SmakolikBot.Services;

public class MongoService
{
    private readonly IMongoCollection<ChatMessagesUpdateSettings> _chatMessagesUpdateCollection;

    public MongoService(IOptions<MongoDatabaseSettings> mongoDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            mongoDatabaseSettings.Value.ConnectionString);
        
        var mongoDatabase = mongoClient.GetDatabase(
            mongoDatabaseSettings.Value.DatabaseName);
        
        _chatMessagesUpdateCollection =
            mongoDatabase.GetCollection<ChatMessagesUpdateSettings>(
                mongoDatabaseSettings.Value.CollectionName);
    }

    public async Task<ChatMessagesUpdateSettings?> GetAsync(long chatId) =>
        await _chatMessagesUpdateCollection.Find(x => 
            x.ChatId == chatId).FirstOrDefaultAsync();

    public async Task CreateAsync(ChatMessagesUpdateSettings chatMessagesUpdateSettings) =>
        await _chatMessagesUpdateCollection.InsertOneAsync(chatMessagesUpdateSettings);

    public async Task UpdateAsync(string id, ChatMessagesUpdateSettings chatMessagesUpdateSettings) =>
        await _chatMessagesUpdateCollection.ReplaceOneAsync(x => 
            x.Id == id, chatMessagesUpdateSettings);

    public async Task RemoveAsync(long chatId) =>
        await _chatMessagesUpdateCollection.DeleteOneAsync(x =>
            x.ChatId == chatId);
}