using Microsoft.AspNetCore.Mvc;
using SmakolikBot.Services;
using Telegram.Bot.Types;

namespace SmakolikBot.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
        [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}