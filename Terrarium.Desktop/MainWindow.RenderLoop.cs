namespace Terrarium.Desktop;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Terrarium.Logic.Simulation;

public partial class MainWindow
{
    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        if (simulationEngine == null || renderer == null)
        {
            return;
        }

        try
        {
            double deltaTime = frameStopwatch.Elapsed.TotalSeconds;
            frameStopwatch.Restart();

            simulationEngine.Update(deltaTime);

            renderer.Clear();
            renderer.RenderWorld(simulationEngine.World, simulationEngine.WeatherIntensity, mousePosition, mouseInCanvas);

            // Check for sound events
            CheckForSoundEvents();

            UpdateFpsCounter(deltaTime);
            UpdateStatusDisplay();
            UpdateFactionDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error in RenderTimer_Tick: {ex.Message}", "Error");
            renderTimer?.Stop();
            systemMonitorTimer?.Stop();
        }
    }

    private void SystemMonitorTimer_Tick(object? sender, EventArgs e)
    {
        // Simplified: no weather effects
    }

    private void UpdateFpsCounter(double deltaTime)
    {
        frameCount++;
        fpsAccumulator += deltaTime;
        if (fpsAccumulator >= 1.0)
        {
            currentFps = frameCount / fpsAccumulator;
            frameCount = 0;
            fpsAccumulator = 0;

            // Update UI
            Dispatcher.Invoke(() =>
            {
                FpsTextBlock.Text = $"FPS: {currentFps:F1}";
                if (systemMonitor != null)
                {
                    MemoryTextBlock.Text = $"MEM: {systemMonitor.GetMemoryUsageMB():F1} MB";
                }
            });
        }
    }

    private void UpdateStatusDisplay()
    {
        if (simulationEngine == null)
        {
            return;
        }

        int plantCount = simulationEngine.World.Plants.Count;
        int herbivoreCount = simulationEngine.World.Herbivores.Count;
        int carnivoreCount = simulationEngine.World.Carnivores.Count;

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
        if (simulationEngine == null || soundManager == null)
        {
            return;
        }

        var stats = simulationEngine.Statistics;

        // Check for births
        if (stats.TotalBirths > lastTotalBirths)
        {
            soundManager.PlayEffect("birth");
            lastTotalBirths = stats.TotalBirths;
        }

        // Check for deaths
        if (stats.TotalDeaths > lastTotalDeaths)
        {
            soundManager.PlayEffect("death");
            lastTotalDeaths = stats.TotalDeaths;
        }

        // Check for eating
        if (stats.TotalPlantsEaten > lastTotalPlantsEaten)
        {
            soundManager.PlayEffect("eat");
            lastTotalPlantsEaten = stats.TotalPlantsEaten;
        }
    }
}
