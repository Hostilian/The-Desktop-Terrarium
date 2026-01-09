using System;
using System.Linq;
using System.Windows.Media;

namespace Terrarium.Desktop
{
    public partial class MainWindow
    {
        /// <summary>
        /// Main render loop (called 60 times per second).
        /// </summary>
        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (_simulationEngine == null || _renderer == null)
                return;

            // Calculate delta time
            double deltaTime = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();

            // Update simulation
            _simulationEngine.Update(deltaTime);

            // Update weather effects
            _weatherEffects?.Update(deltaTime, _simulationEngine.WeatherIntensity);

            // Update particle system
            _particleSystem?.Update(deltaTime);

            // Update notifications
            _notificationManager?.Update(deltaTime);

            // Update tooltips (live data refresh)
            _tooltipManager?.Update();

            // Update sound manager
            _soundManager?.Update(deltaTime, _simulationEngine.DayNightCycle.IsDay, _simulationEngine.WeatherIntensity);

            // Update mini-map
            _miniMap?.Update(_simulationEngine.World, RenderCanvas.ActualWidth, RenderCanvas.ActualHeight);

            // Update new visual systems
            var herbivores = _simulationEngine.World.Herbivores;
            var carnivores = _simulationEngine.World.Carnivores;
            var plants = _simulationEngine.World.Plants;

            _moodIndicator?.Update(deltaTime, herbivores, carnivores);
            _populationGraph?.Update(deltaTime, plants.Count, herbivores.Count, carnivores.Count);
            _predatorWarning?.Update(deltaTime, herbivores, carnivores);
            _entitySelector?.Update(deltaTime);
            _speedIndicator?.SetSpeed(_simulationEngine.SimulationSpeed);
            _speedIndicator?.Update(deltaTime);
            _breedingIndicator?.Update(deltaTime, herbivores, carnivores);
            _ecosystemHealthBar?.Update(deltaTime, plants.Count, herbivores.Count, carnivores.Count);
            _sessionTimer?.Update(deltaTime);

            // Check achievements
            var stats = _simulationEngine.Statistics;
            _achievementSystem?.CheckAchievements(
                stats.TotalBirths,
                stats.TotalDeaths,
                stats.PeakPopulation,
                stats.CurrentPlants,
                stats.CurrentHerbivores,
                stats.CurrentCarnivores,
                stats.SessionTime);

            // Render
            _renderer.Clear();
            _renderer.RenderWorld(_simulationEngine.World, _simulationEngine.WeatherIntensity);

            // Update FPS counter
            UpdateFpsCounter(deltaTime);
            UpdateStatusDisplay();
        }

        /// <summary>
        /// System monitoring timer (updates weather based on CPU usage).
        /// </summary>
        private void SystemMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (_simulationEngine == null || _systemMonitor == null)
                return;

            double cpuUsage = _systemMonitor.GetCpuUsage();

            // High CPU usage causes stormy weather
            if (cpuUsage <= CpuStormStartThreshold)
            {
                _simulationEngine.WeatherIntensity = 0.0;
                return;
            }

            double denom = CpuStormMaxThreshold - CpuStormStartThreshold;
            _simulationEngine.WeatherIntensity = denom <= 0
                ? 0.0
                : Math.Clamp((cpuUsage - CpuStormStartThreshold) / denom, 0.0, 1.0);
        }

        /// <summary>
        /// Updates FPS counter.
        /// </summary>
        private void UpdateFpsCounter(double deltaTime)
        {
            _frameCount++;
            _fpsAccumulator += deltaTime;

            if (_fpsAccumulator >= 1.0)
            {
                _currentFps = _frameCount / _fpsAccumulator;
                _frameCount = 0;
                _fpsAccumulator = 0;
            }
        }

        /// <summary>
        /// Updates status display.
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (_simulationEngine == null)
                return;

            // FPS Display
            FpsText.Text = $"{_currentFps:F0}";

            // Entity counts with new separate fields
            PlantCountText.Text = _simulationEngine.World.Plants.Count.ToString();
            HerbivoreCountText.Text = _simulationEngine.World.Herbivores.Count.ToString();
            CarnivoreCountText.Text = _simulationEngine.World.Carnivores.Count.ToString();

            // Health bar with visual
            double healthPercent = _simulationEngine.GetEcosystemHealth();
            EcosystemHealthText.Text = $"{healthPercent:P0}";
            HealthBar.Width = Math.Max(0, Math.Min(100, healthPercent * 100));

            // Color health bar based on value
            if (healthPercent >= 0.7)
                HealthBar.Background = _healthGoodBrush ?? new SolidColorBrush(Color.FromRgb(46, 204, 113));
            else if (healthPercent >= 0.4)
                HealthBar.Background = _healthWarnBrush ?? new SolidColorBrush(Color.FromRgb(241, 196, 15));
            else
                HealthBar.Background = _healthBadBrush ?? new SolidColorBrush(Color.FromRgb(231, 76, 60));

            // Weather display with icons
            bool isStormy = _simulationEngine.WeatherIntensity > StormyWeatherThreshold;
            WeatherIcon.Text = isStormy ? "⛈️" : "☀️";
            WeatherText.Text = isStormy ? "Stormy" : "Calm";

            // Time of day display
            string timeOfDay = _simulationEngine.GetTimeOfDayString();
            string timeIcon = GetTimeIcon(timeOfDay);
            TimeOfDayText.Text = $"{timeIcon} {timeOfDay}";

            // Update day/night orb
            UpdateDayNightOrb(timeOfDay);

            // Statistics panel
            var stats = _simulationEngine.Statistics;
            StatsText.Text = $"Births: {stats.TotalBirths} | Deaths: {stats.TotalDeaths} | Peak Pop: {stats.PeakPopulation}";

            // Background opacity based on time and weather
            double weatherOpacity = _simulationEngine.WeatherIntensity * (StormMaxBackgroundOpacity - CalmBackgroundOpacity);
            double nightOpacity = (1.0 - _simulationEngine.GetLightLevel()) * NightBackgroundOpacity;
            double totalOpacity = Math.Min(weatherOpacity + nightOpacity, StormMaxBackgroundOpacity);

            BackgroundRect.Opacity = Math.Clamp(
                CalmBackgroundOpacity + totalOpacity,
                CalmBackgroundOpacity,
                StormMaxBackgroundOpacity);
        }

        /// <summary>
        /// Gets the appropriate icon for the time of day.
        /// </summary>
        private static string GetTimeIcon(string timeOfDay)
        {
            return timeOfDay.ToLower() switch
            {
                "dawn" => "🌅",
                "day" => "☀️",
                "dusk" => "🌇",
                "night" => "🌙",
                _ => "🌍"
            };
        }

        /// <summary>
        /// Updates the day/night indicator orb.
        /// </summary>
        private void UpdateDayNightOrb(string timeOfDay)
        {
            switch (timeOfDay.ToLower())
            {
                case "dawn":
                    OrbCenterColor.Color = _orbDawnCenter;
                    OrbEdgeColor.Color = _orbDawnEdge;
                    OrbGlow.Color = _orbDawnEdge;
                    DayNightIcon.Text = "🌅";
                    break;
                case "day":
                    OrbCenterColor.Color = _orbDayCenter;
                    OrbEdgeColor.Color = _orbDayEdge;
                    OrbGlow.Color = _orbDayCenter;
                    DayNightIcon.Text = "☀️";
                    break;
                case "dusk":
                    OrbCenterColor.Color = _orbDuskCenter;
                    OrbEdgeColor.Color = _orbDuskEdge;
                    OrbGlow.Color = _orbDuskCenter;
                    DayNightIcon.Text = "🌇";
                    break;
                case "night":
                    OrbCenterColor.Color = _orbNightCenter;
                    OrbEdgeColor.Color = _orbNightEdge;
                    OrbGlow.Color = _orbNightGlow;
                    DayNightIcon.Text = "🌙";
                    break;
            }
        }
    }
}
