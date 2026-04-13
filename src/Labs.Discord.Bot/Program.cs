using Labs.Discord.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<WebhookForwarder>();

builder.Services.AddSingleton<WebhookForwarder>();

builder.Services.AddDiscordGateway(options =>
{
    options.Intents = GatewayIntents.GuildMessages
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.MessageContent;
});

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

host.UseGatewayEventHandlers();

await host.RunAsync();
