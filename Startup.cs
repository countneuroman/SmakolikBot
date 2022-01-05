using Microsoft.AspNetCore.HttpOverrides;
using SmakolikBot.Services;
using Telegram.Bot;

namespace SmakolikBot;

public class Startup
{
    public IConfiguration Configuration { get;}
    private BotConfiguration BotConfig { get; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        BotConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<ConfigureWebhook>();

        services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient 
                => new TelegramBotClient(BotConfig.BotToken, httpClient));
        
        services.AddScoped<HandleUpdateService>();

        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddSingleton<GetSmakolikMessages>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            var token = BotConfig.BotToken;
            endpoints.MapControllerRoute(name: "tgwebhook",
                pattern: $"bot/{token}",
                new {controller = "Webhook", action = "Post"});
            endpoints.MapControllers();
        });
    }
}