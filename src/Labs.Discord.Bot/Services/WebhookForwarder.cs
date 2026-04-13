using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Labs.Discord.Bot.Services;

public sealed class WebhookForwarder
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookForwarder> _logger;
    private readonly string _webhookUrl;

    public WebhookForwarder(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebhookForwarder> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = configuration["N8N_WEBHOOK_URL"]
            ?? throw new InvalidOperationException(
                "N8N_WEBHOOK_URL is not configured. Set it via environment variable or appsettings.json.");
    }

    public async Task ForwardMessageAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation(
                "Forwarded message from {Username} in guild {GuildId}/channel {ChannelId} to n8n webhook",
                payload.Username, payload.GuildId, payload.ChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to forward message from {Username} to n8n webhook",
                payload.Username);
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
