using SmakolikBot.Models;

namespace SmakolikBot.Services;

//TODO: Add caching.
public  class GetSmakolikMessages
{
    private List<SmakolikMessagesDto> SmakolikMessages { get; }

    public GetSmakolikMessages(MongoService mongoService)
    {
        SmakolikMessages = mongoService.GetMessagesAsync().Result;
    }
    
    public string GetRandomMessage()
    {
        var r = new Random();
        var count = r.Next(0, SmakolikMessages.Count - 1);
        return SmakolikMessages[count].Message;
    }
}