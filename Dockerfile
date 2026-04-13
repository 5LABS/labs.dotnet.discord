FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Labs.Discord.Bot.sln ./
COPY src/Labs.Discord.Bot/Labs.Discord.Bot.csproj src/Labs.Discord.Bot/
RUN dotnet restore

COPY . .
WORKDIR /src/src/Labs.Discord.Bot
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0-preview AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "Labs.Discord.Bot.dll"]
