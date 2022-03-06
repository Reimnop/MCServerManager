using System.Diagnostics;
using MCServerManager.Data;

namespace MCServerManager.Services;

public class ServerStatusChangedEventArgs : EventArgs 
{
    public bool ServerStatus;
    public DateTime Time;
}

public delegate void ServerStatusChangedEventHandler(object sender, ServerStatusChangedEventArgs args);
public delegate void ServerConsoleReceivedEventHandler(object sender);

public class MinecraftServerService
{
    public bool IsServerOnline => !process?.HasExited ?? false;

    public event ServerStatusChangedEventHandler? OnServerStatusChanged;
    public event ServerConsoleReceivedEventHandler? OnServerConsoleReceived;

    public DateTime? ServerStartedTime;

    public string ConsoleOutput => string.Join("\n", consoleLines);

    private readonly List<string> consoleLines = new();

    private static Process? process;

    public void StartServer()
    {
        process = new Process();
        process.StartInfo.FileName = ConfigManager.JvmConfig.JavaPath;
        process.StartInfo.WorkingDirectory = ConfigManager.ServerConfig.ServerDirectory;
        process.StartInfo.Arguments = 
            $"-Xms{ConfigManager.JvmConfig.MinRamMb}M " +
            $"-Xmx{ConfigManager.JvmConfig.MaxRamMb}M " +
            (!string.IsNullOrEmpty(ConfigManager.JvmConfig.ExtraArgs) ? ConfigManager.JvmConfig.ExtraArgs : "") + " " +
            $"-jar \"{Path.Combine(ConfigManager.ServerConfig.ServerDirectory, ConfigManager.ServerConfig.JarFile)}\" " +
            (ConfigManager.ServerConfig.NoGui ? "nogui " : " ") +
            (!string.IsNullOrEmpty(ConfigManager.ServerConfig.ExtraArgs) ? ConfigManager.ServerConfig.ExtraArgs : "");
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.EnableRaisingEvents = true;
        
        consoleLines.Clear();
        OnServerConsoleReceived?.Invoke(this);
        
        process.OutputDataReceived += OnOutputDataReceived;
        process.Exited += OnExited;
        process.Start();
        process.BeginOutputReadLine();
        
        ServerStartedTime = DateTime.UtcNow;
        OnServerStatusChanged?.Invoke(this, new ServerStatusChangedEventArgs 
        {
            ServerStatus = true,
            Time = ServerStartedTime.Value
        });
    }
    
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        if (args.Data == null) 
        {
            return;
        }

        consoleLines.Add(args.Data);
        if (consoleLines.Count > 80)
        {
            consoleLines.RemoveAt(0);
        }
        OnServerConsoleReceived?.Invoke(this);
    }

    private void OnExited(object? sender, EventArgs args)
    {
        ServerStartedTime = null;
        OnServerStatusChanged?.Invoke(this, new ServerStatusChangedEventArgs 
        {
            ServerStatus = false,
            Time = DateTime.UtcNow
        });
        
        process.OutputDataReceived -= OnOutputDataReceived;
        process.Exited -= OnExited;
        
        process.Dispose();
        process = null;
    }

    public void SendCommand(string command)
    {
        process.StandardInput.Write($"{command}\n");
    }

    public void StopServer()
    {
        process.StandardInput.Write("stop\n");
    }

    public void ForceStopServer()
    {
        process.Kill();
    }
}