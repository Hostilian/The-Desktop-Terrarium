namespace Terrarium.Desktop.Rendering;

using System;
using System.Diagnostics;

/// <summary>
/// Monitors system resources (CPU usage) to affect simulation weather.
/// </summary>
public class SystemMonitor : IDisposable
{
    private readonly PerformanceCounter? cpuCounter;
    private bool disposed;

    public SystemMonitor()
    {
        try
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            // Initialize with first read
            cpuCounter.NextValue();
        }
        catch (Exception)
        {
            // Performance counters may not be available on all systems (e.g., restricted environments,
            // missing performance counter categories, or insufficient permissions).
            // Graceful degradation: continue without CPU monitoring - weather will remain calm.
            cpuCounter = null;
        }
    }

    /// <summary>
    /// Gets current CPU usage as a value between 0.0 and 1.0.
    /// </summary>
    /// <returns></returns>
    public double GetCpuUsage()
    {
        if (cpuCounter == null)
        {
            return 0.0;
        }

        try
        {
            float cpuPercent = cpuCounter.NextValue();
            return Math.Clamp(cpuPercent / 100.0, 0.0, 1.0);
        }
        catch (Exception)
        {
            // Performance counter read can fail intermittently due to system state changes.
            // Return 0.0 as safe default - simulation continues with calm weather.
            return 0.0;
        }
    }

    /// <summary>
    /// Gets current memory usage in MB.
    /// </summary>
    /// <returns></returns>
    public double GetMemoryUsageMB()
    {
        return GC.GetTotalMemory(false) / (1024.0 * 1024.0);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        cpuCounter?.Dispose();
        disposed = true;
    }
}
