using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Labs.Discord.Bot.Services;

public sealed class WebhookForwarder
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookForwarder> _logger;
    private readonly Dictionary<string, string> _guildWebhooks;

    public WebhookForwarder(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebhookForwarder> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _guildWebhooks = configuration.GetSection("Webhooks")
            .GetChildren()
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .ToDictionary(c => c.Key, c => c.Value!);

        if (_guildWebhooks.Count == 0)
            logger.LogWarning("No guild webhooks configured. Add Webhooks:<GuildId> entries to configuration.");
        else
            logger.LogInformation("Loaded webhooks for {Count} guild(s): {GuildIds}",
                _guildWebhooks.Count, string.Join(", ", _guildWebhooks.Keys));
    }

    public bool HasWebhook(string guildId) => _guildWebhooks.ContainsKey(guildId);

    public async Task ForwardMessageAsync(string guildId, WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        if (!_guildWebhooks.TryGetValue(guildId, out var webhookUrl))
        {
            _logger.LogDebug("No webhook configured for guild {GuildId}, skipping", guildId);
            return;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation(
                "Forwarded message from {Username} in guild {GuildId}/channel {ChannelId} to n8n webhook",
                payload.Username, payload.GuildId, payload.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to forward message from {Username} in guild {GuildId} to n8n webhook",
                payload.Username, guildId);
        }
    }
}

public sealed record WebhookPayload(
    string MessageId,
    string Content,
    string Username,
    string UserId,
    string ChannelId,
    string ChannelName,
    string? GuildId,
    string? GuildName,
    DateTimeOffset Timestamp,
    List<WebhookAttachment> Attachments);

public sealed record WebhookAttachment(
    string Filename,
    string Url,
    string ContentType,
    long Size);
