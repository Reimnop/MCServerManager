using System.Diagnostics;

namespace MCServerManager.Utils;

public static class CpuMemoryMetrics4LinuxUtils
{
    private static long totalMemoryInKb;

    /// <summary>
    /// Get the system overall CPU usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30.1234 %,
    /// then it will return 30.12.</returns>
    public static double GetOverallCpuUsagePercentage()
    {
        // refer to https://stackoverflow.com/questions/59465212/net-core-cpu-usage-for-machine
        Stopwatch sw = new Stopwatch();
        
        sw.Start();
        var startCpuUsage = Process.GetProcesses().Sum(a => a.TotalProcessorTime.TotalMilliseconds);

        Thread.Sleep(500);

        sw.Stop();
        var endCpuUsage = Process.GetProcesses().Sum(a => a.TotalProcessorTime.TotalMilliseconds);

        var cpuUsedMs = endCpuUsage - startCpuUsage;
        var totalMsPassed = sw.ElapsedMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

        return Math.Clamp(cpuUsageTotal * 100, 0.0, 100.0);
    }

    /// <summary>
    /// Get the system overall memory usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30.1234 %,
    /// then it will return 30.12.</returns>
    public static double GetOccupiedMemoryPercentage()
    {
        var totalMemory = GetTotalMemory();
        var usedMemory = GetUsedMemoryForAllProcesses();

        var percentage = (usedMemory * 100) / totalMemory;
        return percentage;
    }

    public static long GetUsedMemoryForAllProcesses()
    {
        return Process.GetProcesses().Sum(a => a.PrivateMemorySize64);
    }

    public static long GetTotalMemory()
    {
        // only parse the file once
        if (totalMemoryInKb > 0)
        {
            return totalMemoryInKb * 1024;
        }

        string path = "/proc/meminfo";
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        using (var reader = new StreamReader(path))
        {
            string line = string.Empty;
            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                if (line.Contains("MemTotal", StringComparison.OrdinalIgnoreCase))
                {
                    // e.g. MemTotal:       16370152 kB
                    var parts = line.Split(':');
                    var valuePart = parts[1].Trim();
                    parts = valuePart.Split(' ');
                    var numberString = parts[0].Trim();

                    var result = long.TryParse(numberString, out totalMemoryInKb);
                    return result
                        ? totalMemoryInKb * 1024
                        : throw new Exception($"Cannot parse 'MemTotal' value from the file {path}.");
                }
            }

            throw new Exception($"Cannot find the 'MemTotal' property from the file {path}.");
        }
    }
}