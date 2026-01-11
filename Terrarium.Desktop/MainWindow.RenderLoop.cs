using System;
using System.Linq;
using System.Windows.Media;
using Terrarium.Logic.Simulation;

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

            double deltaTime = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();

            _simulationEngine.Update(deltaTime);

            _weatherEffects?.Update(deltaTime, _simulationEngine.WeatherIntensity);

            _particleSystem?.Update(deltaTime);

            _notificationManager?.Update(deltaTime);

            _tooltipManager?.Update();

            _soundManager?.Update(deltaTime, _simulationEngine.DayNightCycle.IsDay, _simulationEngine.WeatherIntensity);

            _miniMap?.Update(_simulationEngine.World, RenderCanvas.ActualWidth, RenderCanvas.ActualHeight);

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

            var stats = _simulationEngine.Statistics;
            _achievementSystem?.CheckAchievements(
                stats.TotalBirths,
                stats.TotalDeaths,
                stats.PeakPopulation,
                stats.CurrentPlants,
                stats.CurrentHerbivores,
                stats.CurrentCarnivores,
                stats.SessionTime);

            _renderer.Clear();
            _renderer.RenderWorld(_simulationEngine.World, _simulationEngine.WeatherIntensity);

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
            int fpsDisplayed = (int)Math.Round(_currentFps);
            if (fpsDisplayed != _lastFpsDisplayed)
            {
                FpsText.Text = fpsDisplayed.ToString();
                _lastFpsDisplayed = fpsDisplayed;
            }

            // Entity counts with new separate fields
            int plantCount = _simulationEngine.World.Plants.Count;
            int herbivoreCount = _simulationEngine.World.Herbivores.Count;
            int carnivoreCount = _simulationEngine.World.Carnivores.Count;

            if (plantCount != _lastPlantCountDisplayed)
            {
                PlantCountText.Text = plantCount.ToString();
                _lastPlantCountDisplayed = plantCount;
            }

            if (herbivoreCount != _lastHerbivoreCountDisplayed)
            {
                HerbivoreCountText.Text = herbivoreCount.ToString();
                _lastHerbivoreCountDisplayed = herbivoreCount;
            }

            if (carnivoreCount != _lastCarnivoreCountDisplayed)
            {
                CarnivoreCountText.Text = carnivoreCount.ToString();
                _lastCarnivoreCountDisplayed = carnivoreCount;
            }

            double healthPercent = _simulationEngine.GetEcosystemHealth();
            int healthPercentDisplayed = (int)Math.Round(healthPercent * 100);
            if (healthPercentDisplayed != _lastHealthPercentDisplayed)
            {
                EcosystemHealthText.Text = $"{healthPercentDisplayed}%";
                _lastHealthPercentDisplayed = healthPercentDisplayed;
            }
            // New compact health bar is 60px wide
            HealthBar.Width = Math.Max(0, Math.Min(60, healthPercent * 60));

            int healthBand = healthPercent >= 0.7 ? 2 : (healthPercent >= 0.4 ? 1 : 0);
            if (healthBand != _lastHealthBandDisplayed)
            {
                if (healthBand == 2)
                    HealthBar.Background = _healthGoodBrush ??= CreateFrozenBrush(Color.FromRgb(46, 204, 113));
                else if (healthBand == 1)
                    HealthBar.Background = _healthWarnBrush ??= CreateFrozenBrush(Color.FromRgb(241, 196, 15));
                else
                    HealthBar.Background = _healthBadBrush ??= CreateFrozenBrush(Color.FromRgb(231, 76, 60));

                _lastHealthBandDisplayed = healthBand;
            }

            bool isStormy = _simulationEngine.WeatherIntensity > StormyWeatherThreshold;
            if (_lastIsStormyDisplayed != isStormy)
            {
                WeatherIcon.Text = isStormy ? "⛈️" : "☀️";
                WeatherText.Text = isStormy ? "Stormy" : "Calm";
                _lastIsStormyDisplayed = isStormy;
            }

            DayPhase currentPhase = _simulationEngine.DayNightCycle.CurrentPhase;
            if (_lastDayPhaseDisplayed != currentPhase)
            {
                string timeIcon = GetTimeIcon(currentPhase);
                TimeOfDayText.Text = $"{timeIcon} {currentPhase}";
                UpdateDayNightOrb(currentPhase);
                _lastDayPhaseDisplayed = currentPhase;
            }

            var stats = _simulationEngine.Statistics;
            if (stats.TotalBirths != _lastBirthsDisplayed ||
                stats.TotalDeaths != _lastDeathsDisplayed ||
                stats.PeakPopulation != _lastPeakPopulationDisplayed)
            {
                StatsText.Text = $"Births: {stats.TotalBirths} | Deaths: {stats.TotalDeaths} | Peak: {stats.PeakPopulation}";
                _lastBirthsDisplayed = stats.TotalBirths;
                _lastDeathsDisplayed = stats.TotalDeaths;
                _lastPeakPopulationDisplayed = stats.PeakPopulation;
            }

            if (FindName("SpeedText") is System.Windows.Controls.TextBlock speedText)
            {
                speedText.Text = $"{_simulationEngine.SimulationSpeed:0.0}x";
            }

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
        private static string GetTimeIcon(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Dawn => "🌅",
                DayPhase.Day => "☀️",
                DayPhase.Dusk => "🌇",
                DayPhase.Night => "🌙",
                _ => "🌍"
            };
        }

        /// <summary>
        /// Updates the day/night indicator orb (simplified version).
        /// </summary>
        private void UpdateDayNightOrb(DayPhase phase)
        {
            switch (phase)
            {
                case DayPhase.Dawn:
                    OrbCenterColor.Color = _orbDawnCenter;
                    OrbEdgeColor.Color = _orbDawnEdge;
                    DayNightIcon.Text = "🌅";
                    break;
                case DayPhase.Day:
                    OrbCenterColor.Color = _orbDayCenter;
                    OrbEdgeColor.Color = _orbDayEdge;
                    DayNightIcon.Text = "☀️";
                    break;
                case DayPhase.Dusk:
                    OrbCenterColor.Color = _orbDuskCenter;
                    OrbEdgeColor.Color = _orbDuskEdge;
                    DayNightIcon.Text = "🌇";
                    break;
                case DayPhase.Night:
                    OrbCenterColor.Color = _orbNightCenter;
                    OrbEdgeColor.Color = _orbNightEdge;
                    DayNightIcon.Text = "🌙";
                    break;
            }
        }

        private static Brush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
