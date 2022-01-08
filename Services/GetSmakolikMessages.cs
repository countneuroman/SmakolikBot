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

    public List<SmakolikMessagesDto> GetMessages()
    {
        return SmakolikMessages;
    }
}