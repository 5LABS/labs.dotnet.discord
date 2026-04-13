# Labs.Discord.Bot

Discord bot built with .NET 10 and [NetCord](https://github.com/NetCordDev/NetCord) that forwards user messages to n8n webhooks. Each Discord server (guild) gets its own webhook URL, enabling separate n8n workflows per server.

## Features

- Runs on multiple Discord servers simultaneously
- Each guild has its own dedicated n8n webhook URL
- Only guilds with a configured webhook are monitored
- Forwards all user messages (non-bot) including metadata: author, channel, guild, timestamp, attachments

## Configuration

| Environment Variable | Required | Description |
|---|---|---|
| `Discord__Token` | Yes | Discord bot token |
| `Webhooks__<GuildId>` | Yes | n8n webhook URL for a specific guild. Add one per server. |

### Example

```bash
# Guild 111222333 gets its own n8n workflow
export Webhooks__111222333="https://n8n.example.com/webhook/workflow-server-a"

# Guild 444555666 gets a different n8n workflow
export Webhooks__444555666="https://n8n.example.com/webhook/workflow-server-b"
```

Messages from guilds without a configured webhook are silently ignored.

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
GUILD_ID_1=111222333
WEBHOOK_URL_1=https://n8n.example.com/webhook/workflow-server-a
GUILD_ID_2=444555666
WEBHOOK_URL_2=https://n8n.example.com/webhook/workflow-server-b
EOF

docker compose up -d
```

## Run Locally

```bash
export Discord__Token="your-bot-token"
export Webhooks__111222333="https://n8n.example.com/webhook/workflow-server-a"
export Webhooks__444555666="https://n8n.example.com/webhook/workflow-server-b"

cd src/Labs.Discord.Bot
dotnet run
```

## appsettings.json Alternative

```json
{
  "Discord": {
    "Token": "your-bot-token"
  },
  "Webhooks": {
    "111222333": "https://n8n.example.com/webhook/workflow-server-a",
    "444555666": "https://n8n.example.com/webhook/workflow-server-b"
  }
}
```

## n8n Webhook Payload

The bot sends the following JSON payload to each guild's webhook:

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
