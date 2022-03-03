using System.Diagnostics;
using MCServerManager.Data;

namespace MCServerManager.Services;

public delegate void ServerStatusChangedEventHandler(object sender, bool serverStatus);
public delegate void ServerConsoleReceivedEventHandler(object sender);

public class MinecraftServerService
{
    public bool IsServerOnline => !process?.HasExited ?? false;

    public event ServerStatusChangedEventHandler? OnServerStatusChanged;
    public event ServerConsoleReceivedEventHandler? OnServerConsoleReceived;

    public string ConsoleOutput { get; private set; } = string.Empty;

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
        
        ConsoleOutput = string.Empty;
        OnServerConsoleReceived?.Invoke(this);
        
        process.OutputDataReceived += OnOutputDataReceived;
        process.Exited += OnExited;
        process.Start();
        process.BeginOutputReadLine();
        
        OnServerStatusChanged?.Invoke(this, true);
    }
    
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        ConsoleOutput += args.Data + "\n";
        OnServerConsoleReceived?.Invoke(this);
    }

    private void OnExited(object? sender, EventArgs args)
    {
        OnServerStatusChanged?.Invoke(this, false);
        
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