using MineStatLib;
using MCServerManager.Data;
using MCServerManager.Models;

namespace MCServerManager.Services;

public struct ServerStats
{
    public int CurrentPlayers;
    public int MaxPlayers;
    public Player[] Players;
}

public delegate void ServerStatsUpdateEventHandler(object sender, ServerStats? serverStats);

public class ServerStatsService
{
    public ServerStats? CurrentStats { get; private set; } = null;

    public event ServerStatsUpdateEventHandler? OnServerStatsUpdate;

    public ServerStatsService()
    {
        _ = PollServerStatsAsync();
    }

    private async Task PollServerStatsAsync()
    {
        for (;;)
        {
            CurrentStats = GetServerStats();
            
            OnServerStatsUpdate?.Invoke(this, CurrentStats);

            await Task.Delay(1000);
        }
    }

    private ServerStats? GetServerStats()
    {
        ServerConfigModel serverConfig = ConfigManager.ServerConfig;

        if (string.IsNullOrWhiteSpace(serverConfig.ServerDirectory))
        {
            return null;
        }

        string propertiesPath = Path.Combine(serverConfig.ServerDirectory, "server.properties");

        if (!File.Exists(propertiesPath))
        {
            return null;
        }

        try
        {
            ServerProperties properties = ServerProperties.FromFile(propertiesPath);
            return TryGetServerStats(properties);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private ServerStats? TryGetServerStats(ServerProperties properties)
    {
        ushort port = ushort.Parse(properties["server-port"]);
        MineStat ms = new MineStat("127.0.0.1", port, protocol: SlpProtocol.Json);
        
        if (ms.ServerUp)
        {
            return new ServerStats
            {
                CurrentPlayers = ms.CurrentPlayersInt,
                MaxPlayers = ms.MaximumPlayersInt,
                Players = ms.Players
            };
        }

        return null;
    }
}