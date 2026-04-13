using Labs.Discord.Bot.Services;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Labs.Discord.Bot.Handlers;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public sealed class MessageHandler : IGatewayEventHandler<Message>
{
    private readonly WebhookForwarder _forwarder;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(
        WebhookForwarder forwarder,
        ILogger<MessageHandler> logger)
    {
        _forwarder = forwarder;
        _logger = logger;
    }

    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        if (!message.GuildId.HasValue)
            return;

        var guildId = message.GuildId.Value.ToString();

        if (!_forwarder.HasWebhook(guildId))
            return;

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
            Username: message.Author.Username,
            UserId: message.Author.Id.ToString(),
            ChannelId: message.ChannelId.ToString(),
            ChannelName: message.ChannelId.ToString(),
            GuildId: guildId,
            GuildName: guildId,
            Timestamp: message.CreatedAt,
            Attachments: attachments);

        _logger.LogDebug("Processing message {MessageId} from {Username} in guild {GuildId}",
            message.Id, message.Author.Username, guildId);

        await _forwarder.ForwardMessageAsync(guildId, payload);
    }
}
