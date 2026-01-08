using System.Diagnostics;

namespace Terrarium.Desktop.Rendering
{
    /// <summary>
    /// Monitors system resources (CPU usage) to affect simulation weather.
    /// </summary>
    public class SystemMonitor : IDisposable
    {
        private PerformanceCounter? _cpuCounter;
        private bool _disposed;

        public SystemMonitor()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                // Initialize with first read
                _cpuCounter.NextValue();
            }
            catch
            {
                // Performance counter may not be available on all systems
                _cpuCounter = null;
            }
        }

        /// <summary>
        /// Gets current CPU usage as a value between 0.0 and 1.0.
        /// </summary>
        public double GetCpuUsage()
        {
            if (_cpuCounter == null) return 0.0;

            try
            {
                float cpuPercent = _cpuCounter.NextValue();
                return Math.Clamp(cpuPercent / 100.0, 0.0, 1.0);
            }
            catch
            {
                return 0.0;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _cpuCounter?.Dispose();
            _disposed = true;
        }
    }
}
