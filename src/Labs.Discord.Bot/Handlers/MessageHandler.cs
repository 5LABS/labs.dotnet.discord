using Labs.Discord.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Labs.Discord.Bot.Handlers;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public sealed class MessageHandler : IGatewayEventHandler<Message>
{
    private readonly WebhookForwarder _forwarder;
    private readonly ILogger<MessageHandler> _logger;
    private readonly HashSet<string> _allowedGuildIds;

    public MessageHandler(
        WebhookForwarder forwarder,
        IConfiguration configuration,
        ILogger<MessageHandler> logger)
    {
        _forwarder = forwarder;
        _logger = logger;

        var allowedGuilds = configuration["ALLOWED_GUILD_IDS"];
        _allowedGuildIds = string.IsNullOrWhiteSpace(allowedGuilds)
            ? []
            : allowedGuilds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
    }

    public async ValueTask HandleAsync(Message message)
    {
        // Ignore bot messages
        if (message.Author.IsBot)
            return;

        // If allowed guild IDs are configured, only forward messages from those guilds
        if (_allowedGuildIds.Count > 0 && message.GuildId.HasValue)
        {
            if (!_allowedGuildIds.Contains(message.GuildId.Value.ToString()))
                return;
        }

        var attachments = message.Attachments
            .Select(a => new WebhookAttachment(
                a.FileName,
                a.Url,
                a.ContentType ?? "application/octet-stream",
                a.Size))
            .ToList();

        var payload = new WebhookPayload(
            MessageId: message.Id.ToString(),
            Content: message.Content,
            Username: $"{message.Author.Username}",
            UserId: message.Author.Id.ToString(),
            ChannelId: message.ChannelId.ToString(),
            ChannelName: message.ChannelId.ToString(),
            GuildId: message.GuildId?.ToString(),
            GuildName: message.GuildId?.ToString(),
            Timestamp: message.CreatedAt,
            Attachments: attachments);

        _logger.LogDebug("Processing message {MessageId} from {Username}", message.Id, message.Author.Username);

        await _forwarder.ForwardMessageAsync(payload);
    }
}
