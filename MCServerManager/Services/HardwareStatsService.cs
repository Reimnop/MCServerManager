using LibreHardwareMonitor.Hardware;

namespace MCServerManager.Services;

public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

public struct HardwareStats
{
    public float CpuLoad;
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

    public event HardwareUpdateEventHandler OnHardwareUpdate;

    private Computer computer;
    private DriveInfo driveInfo;

    public HardwareStatsService()
    {
        computer = new Computer
        {
            IsCpuEnabled = true,
            IsMemoryEnabled = true
        };
        computer.Open();

        driveInfo = DriveInfo.GetDrives().First(x => x.IsReady);
        
        _ = UpdatePeriodicallyAsync();
    }

    private async Task UpdatePeriodicallyAsync()
    {
        for (;;)
        {
            computer.Accept(new UpdateVisitor());
        
            IHardware cpu = computer.Hardware.First(x => x.HardwareType == HardwareType.Cpu);
            ISensor cpuLoadSensor = cpu.Sensors.First(x => x.SensorType == SensorType.Load);
            float cpuLoad = (float)cpuLoadSensor.Value;

            IHardware memory = computer.Hardware.First(x => x.HardwareType == HardwareType.Memory);
            ISensor memoryUsedSensor = memory.Sensors.First(x => x.Name == "Memory Used");
            ISensor memoryAvailSensor = memory.Sensors.First(x => x.Name == "Memory Available");
            
            float memoryUsage = (float)memoryUsedSensor.Value;
            float maxMemory = (float)memoryAvailSensor.Value + memoryUsage;
            
            Stats = new HardwareStats
            {
                CpuLoad = cpuLoad,
                StorageUsage = (driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / 1024.0f / 1024.0f / 1024.0f,
                MaxStorage = driveInfo.TotalSize / 1024.0f / 1024.0f / 1024.0f,
                MemoryUsage = memoryUsage,
                MaxMemory = maxMemory
            };
            
            OnHardwareUpdate?.Invoke(this, new HardwareUpdateEventArgs(Stats));
            
            await Task.Delay(1000);
        }
    }
}