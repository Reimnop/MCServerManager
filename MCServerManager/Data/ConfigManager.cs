using System.Text.Json;
using MCServerManager.Models;

namespace MCServerManager.Data;

public static class ConfigManager
{
    private const string JvmConfigFile = "jvm_config.json";
    private const string ServerConfigFile = "server_config.json";

    public static JvmConfigModel JvmConfig { get; set; } = new();
    public static ServerConfigModel ServerConfig { get; set; } = new();

    static ConfigManager()
    {
        if (File.Exists(JvmConfigFile))
        {
            JvmConfig = JsonSerializer.Deserialize<JvmConfigModel>(File.ReadAllText(JvmConfigFile));
        }
        
        if (File.Exists(ServerConfigFile))
        {
            ServerConfig = JsonSerializer.Deserialize<ServerConfigModel>(File.ReadAllText(ServerConfigFile));
        }
    }
    
    public static void SaveJvmConfig()
    {
        File.WriteAllText(JvmConfigFile, JsonSerializer.Serialize(JvmConfig));
    }

    public static void SaveServerConfig()
    {
        File.WriteAllText(ServerConfigFile, JsonSerializer.Serialize(ServerConfig));
    }
}