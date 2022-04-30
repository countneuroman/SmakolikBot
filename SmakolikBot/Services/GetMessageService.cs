using SmakolikBot.Models;

namespace SmakolikBot.Services;

//TODO: Add caching.
public  class GetMessageService
{
    private List<MessagesDto> Messages { get; }

    public GetMessageService(MongoService mongoService)
    {
        Messages = mongoService.GetMessagesAsync().Result;
    }
    
    public string GetRandomMessage()
    {
        var r = new Random();
        var count = r.Next(0, Messages.Count - 1);
        return Messages[count].Message;
    }
}