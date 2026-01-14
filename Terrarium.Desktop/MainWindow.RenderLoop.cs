using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop;

public partial class MainWindow
{
    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        if (_simulationEngine == null || _renderer == null)
        {
            return;
        }

        try
        {
            double deltaTime = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();

            _simulationEngine.Update(deltaTime);

            _renderer.Clear();
            _renderer.RenderWorld(_simulationEngine.World, _simulationEngine.WeatherIntensity, _mousePosition, _mouseInCanvas);

            // Check for sound events
            CheckForSoundEvents();

            UpdateFpsCounter(deltaTime);
            UpdateStatusDisplay();
            UpdateFactionDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error in RenderTimer_Tick: {ex.Message}", "Error");
            _renderTimer?.Stop();
            _systemMonitorTimer?.Stop();
        }
    }

    private void SystemMonitorTimer_Tick(object? sender, EventArgs e)
    {
        // Simplified: no weather effects
    }

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

    private void UpdateStatusDisplay()
    {
        if (_simulationEngine == null)
        {
            return;
        }

        int plantCount = _simulationEngine.World.Plants.Count;
        int herbivoreCount = _simulationEngine.World.Herbivores.Count;
        int carnivoreCount = _simulationEngine.World.Carnivores.Count;

        // UI updates removed for god simulator - population tracking now handled by faction system
        /*
        if (plantCount != _lastPlantCountDisplayed)
        {
            PlantCountText.Text = plantCount.ToString();
            DetailedPlantCount.Text = plantCount.ToString();
            _lastPlantCountDisplayed = plantCount;
        }

        if (herbivoreCount != _lastHerbivoreCountDisplayed)
        {
            HerbivoreCountText.Text = herbivoreCount.ToString();
            DetailedHerbivoreCount.Text = herbivoreCount.ToString();
            _lastHerbivoreCountDisplayed = herbivoreCount;
        }

        if (carnivoreCount != _lastCarnivoreCountDisplayed)
        {
            CarnivoreCountText.Text = carnivoreCount.ToString();
            DetailedCarnivoreCount.Text = carnivoreCount.ToString();
            _lastCarnivoreCountDisplayed = carnivoreCount;
        }

        int totalPopulation = plantCount + herbivoreCount + carnivoreCount;
        TotalPopulationText.Text = totalPopulation.ToString();

        // Simple health calculation based on population balance
        int healthPercent = totalPopulation > 0 ? Math.Min(100, (plantCount * 40 + herbivoreCount * 30 + carnivoreCount * 30) / totalPopulation) : 0;

        if (healthPercent != _lastHealthPercentDisplayed)
        {
            EcosystemHealthText.Text = $"{healthPercent}%";
            DetailedHealthText.Text = $"{healthPercent}%";
            _lastHealthPercentDisplayed = healthPercent;
        }

        // Update simulation time
        TimeSpan simulationTime = _frameStopwatch.Elapsed;
        SimulationTimeText.Text = $"{simulationTime.Minutes:D2}:{simulationTime.Seconds:D2}";
        */
    }

    private void CheckForSoundEvents()
    {
        if (_simulationEngine == null || _soundManager == null)
        {
            return;
        }

        var stats = _simulationEngine.Statistics;

        // Check for births
        if (stats.TotalBirths > _lastTotalBirths)
        {
            _soundManager.PlayEffect("birth");
            _lastTotalBirths = stats.TotalBirths;
        }

        // Check for deaths
        if (stats.TotalDeaths > _lastTotalDeaths)
        {
            _soundManager.PlayEffect("death");
            _lastTotalDeaths = stats.TotalDeaths;
        }

        // Check for eating
        if (stats.TotalPlantsEaten > _lastTotalPlantsEaten)
        {
            _soundManager.PlayEffect("eat");
            _lastTotalPlantsEaten = stats.TotalPlantsEaten;
        }
    }


}
