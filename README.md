# Labs.Discord.Bot

Discord bot built with .NET 10 and [NetCord](https://github.com/NetCordDev/NetCord) that forwards user messages to an n8n webhook.

## Features

- Runs on multiple Discord servers simultaneously
- Forwards all user messages (non-bot) to a configurable n8n webhook
- Optional guild allowlist to restrict which servers are monitored
- Includes message metadata: author, channel, guild, timestamp, attachments

## Configuration

| Environment Variable | Required | Description |
|---|---|---|
| `Discord__Token` | Yes | Discord bot token |
| `N8N_WEBHOOK_URL` | Yes | n8n webhook URL to receive messages |
| `ALLOWED_GUILD_IDS` | No | Comma-separated list of guild IDs to monitor (empty = all guilds) |

## Discord Bot Setup

1. Go to the [Discord Developer Portal](https://discord.com/developers/applications)
2. Create a new application and add a bot
3. Enable **Message Content Intent** under Bot > Privileged Gateway Intents
4. Invite the bot to your server(s) with the `bot` scope and `Read Messages` permission

## Run with Docker Compose

```bash
# Create .env file
cat > .env <<EOF
DISCORD_TOKEN=your-bot-token
N8N_WEBHOOK_URL=https://your-n8n-instance.com/webhook/your-webhook-id
ALLOWED_GUILD_IDS=
EOF

docker compose up -d
```

## Run Locally

```bash
export Discord__Token="your-bot-token"
export N8N_WEBHOOK_URL="https://your-n8n-instance.com/webhook/your-webhook-id"

cd src/Labs.Discord.Bot
dotnet run
```

## n8n Webhook Payload

The bot sends the following JSON payload to the webhook:

```json
{
  "messageId": "123456789",
  "content": "Hello world!",
  "username": "user123",
  "userId": "987654321",
  "channelId": "111222333",
  "channelName": "111222333",
  "guildId": "444555666",
  "guildName": "444555666",
  "timestamp": "2026-04-13T12:00:00+00:00",
  "attachments": [
    {
      "filename": "image.png",
      "url": "https://cdn.discordapp.com/...",
      "contentType": "image/png",
      "size": 12345
    }
  ]
}
```
