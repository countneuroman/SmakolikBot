﻿using Microsoft.AspNetCore.HttpOverrides;
using SmakolikBot.Models;
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

        services.Configure<MongoDatabaseSettings>(Configuration.GetSection("MongoDatabase"));

        services.AddSingleton<MongoService>();
        services.AddScoped<ChatSettingsService>();
        services.AddScoped<MessagesService>();
        
        services.AddHostedService<ConfigureWebhook>();

        services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient 
                => new TelegramBotClient(BotConfig.BotToken, httpClient));
        
        services.AddScoped<HandleUpdateService>();

        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddMemoryCache();
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
            endpoints.MapControllerRoute(name: "tgwebhook",
                pattern: $"bot",
                new {controller = "Webhook", action = "Post"});
            endpoints.MapControllers();
        });
    }
}