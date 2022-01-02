using System.Text.Json;
using SmakolikBot.Models;

namespace SmakolikBot.Services;

public static class GetSmakolikMessages
{
    private const string SmakolikMessagesFilePath = "Files/smakolik.json";
    
    public static SmakolikDto? GetMesssages()
    {
        var smakolikMessagesString = File.ReadAllText(SmakolikMessagesFilePath);
        var smakolikMessages = JsonSerializer.Deserialize<SmakolikDto>(smakolikMessagesString);
        return smakolikMessages;
    }
}