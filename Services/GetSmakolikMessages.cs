using System.Text.Json;
using SmakolikBot.Models;

namespace SmakolikBot.Services;

//TODO: Add caching.
public  class GetSmakolikMessages
{
    private const string SmakolikMessagesFilePath = "Files/smakolik.json";
    private SmakolikDto? SmakolikMessages { get; }

    public GetSmakolikMessages()
    {
        var smakolikMessagesString = File.ReadAllText(SmakolikMessagesFilePath);
        SmakolikMessages = JsonSerializer.Deserialize<SmakolikDto>(smakolikMessagesString);
    }

    public List<SmakolikMessagesDto>? GetMessages()
    {
        return SmakolikMessages?.Data.ToList();
    }
}