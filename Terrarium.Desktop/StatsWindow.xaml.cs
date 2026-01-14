using System;
using System.Windows;
using Terrarium.Logic.Simulation;
using Terrarium.Desktop.Rendering;

namespace Terrarium.Desktop;

public partial class StatsWindow : Window
{
    private readonly SimulationEngine _simulationEngine;
    private readonly SystemMonitor _systemMonitor;

    public StatsWindow(SimulationEngine simulationEngine, SystemMonitor systemMonitor)
    {
        InitializeComponent();
        _simulationEngine = simulationEngine;
        _systemMonitor = systemMonitor;

        // Update stats initially
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var stats = _simulationEngine.Statistics;

        PlantsCountText.Text = _simulationEngine.World.Plants.Count.ToString();
        PlantsPeakText.Text = $"(Peak: {stats.PeakPlants})";

        HerbivoresCountText.Text = _simulationEngine.World.Herbivores.Count.ToString();
        HerbivoresPeakText.Text = $"(Peak: {stats.PeakHerbivores})";

        CarnivoresCountText.Text = _simulationEngine.World.Carnivores.Count.ToString();
        CarnivoresPeakText.Text = $"(Peak: {stats.PeakCarnivores})";

        int total = _simulationEngine.World.Plants.Count + _simulationEngine.World.Herbivores.Count + _simulationEngine.World.Carnivores.Count;
        TotalPopulationText.Text = total.ToString();
        TotalPeakText.Text = $"(Peak: {stats.PeakPopulation})";

        PlantsGrownText.Text = stats.TotalPlantsGrown.ToString();
        PlantsEatenText.Text = stats.TotalPlantsEaten.ToString();
        BirthsText.Text = stats.TotalBirths.ToString();
        DeathsText.Text = stats.TotalDeaths.ToString();

        TimeSpan simTime = TimeSpan.FromSeconds(stats.SessionTime);
        SimulationTimeText.Text = $"{simTime.Hours:D2}:{simTime.Minutes:D2}:{simTime.Seconds:D2}";

        double health = EcosystemHealthScorer.CalculateHealthPercent(
            _simulationEngine.World.Plants.Count,
            _simulationEngine.World.Herbivores.Count,
            _simulationEngine.World.Carnivores.Count);
        EcosystemHealthText.Text = $"{health:F1}%";

        // Note: CurrentSpeedText would need to be passed from MainWindow
        CurrentSpeedText.Text = "1.0x"; // Placeholder
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
