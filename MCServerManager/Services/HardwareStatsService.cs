using System.Runtime.InteropServices;
using MCServerManager.Utils;

namespace MCServerManager.Services;

public struct HardwareStats
{
    public float CpuUsage;
    public float StorageUsage;
    public float MaxStorage;
    public float MemoryUsage;
    public float MaxMemory;
}

public class HardwareUpdateEventArgs : EventArgs
{
    public HardwareStats Stats { get; }

    public HardwareUpdateEventArgs(HardwareStats stats)
    {
        Stats = stats;
    }
}

public delegate void HardwareUpdateEventHandler(object sender, HardwareUpdateEventArgs eventArgs);

public class HardwareStatsService
{
    public HardwareStats Stats { get; private set; }

    public event HardwareUpdateEventHandler? OnHardwareUpdate;
    
    private readonly DriveInfo driveInfo;

    public HardwareStatsService()
    {
        driveInfo = DriveInfo.GetDrives().First(x => x.IsReady);

        _ = UpdatePeriodicallyAsync();
    }

    private async Task UpdatePeriodicallyAsync()
    {
        for (;;)
        {
            Stats = new HardwareStats
            {
                CpuUsage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 
                    (float)CpuMemoryMetrics4LinuxUtils.GetOverallCpuUsagePercentage() :
                    float.NaN,
                StorageUsage = (driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / 1024.0f / 1024.0f / 1024.0f,
                MaxStorage = driveInfo.TotalSize / 1024.0f / 1024.0f / 1024.0f,
                MemoryUsage = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 
                    CpuMemoryMetrics4LinuxUtils.GetUsedMemoryForAllProcesses() / 1024.0f / 1024.0f / 1024.0f : 
                    float.NaN,
                MaxMemory = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 
                    CpuMemoryMetrics4LinuxUtils.GetTotalMemory() / 1024.0f / 1024.0f / 1024.0f : 
                    float.NaN,
            };

            OnHardwareUpdate?.Invoke(this, new HardwareUpdateEventArgs(Stats));

            await Task.Delay(500);
        }
    }
}