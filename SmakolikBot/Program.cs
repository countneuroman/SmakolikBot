using MongoDB.Driver;
using SmakolikBot;
using SmakolikBot.Controllers;
using SmakolikBot.Extensions;
using SmakolikBot.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var botConfigurationSection = builder.Configuration.GetSection("BotConfiguration");
builder.Services.Configure<BotConfiguration>(botConfigurationSection);
var botConfiguration = botConfigurationSection.Get<BotConfiguration>();

var mongoConfigurationSection = builder.Configuration.GetSection("MongoDatabase");
builder.Services.Configure<MongoDatabaseSettings>(mongoConfigurationSection);

builder.Services.AddSingleton<MongoService>();
builder.Services.AddScoped<ChatSettingsService>();
builder.Services.AddScoped<MessagesService>();

builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddScoped<HandleUpdateService>();

builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        // var botConfig = sp.GetConfiguration<BotConfiguration>();
        TelegramBotClientOptions options = new(botConfiguration.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddMemoryCache();

var app = builder.Build();

app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);
app.MapControllers();
app.Run();
