namespace Terrarium.Desktop;

using System;
using System.Windows;
using Terrarium.Desktop.Rendering;
using Terrarium.Logic.Simulation;

public partial class StatsWindow : Window
{
    private readonly SimulationEngine simulationEngine;
    private readonly SystemMonitor systemMonitor;

    public StatsWindow(SimulationEngine simulationEngine, SystemMonitor systemMonitor)
    {
        InitializeComponent();
        this.simulationEngine = simulationEngine;
        this.systemMonitor = systemMonitor;

        // Update stats initially
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (simulationEngine == null)
        {
            return;
        }

        var stats = simulationEngine.Statistics;

        PlantsCountText.Text = simulationEngine.World.Plants.Count.ToString();
        PlantsPeakText.Text = $"(Peak: {stats.PeakPlants})";

        HerbivoresCountText.Text = simulationEngine.World.Herbivores.Count.ToString();
        HerbivoresPeakText.Text = $"(Peak: {stats.PeakHerbivores})";

        CarnivoresCountText.Text = simulationEngine.World.Carnivores.Count.ToString();
        CarnivoresPeakText.Text = $"(Peak: {stats.PeakCarnivores})";

        int total = simulationEngine.World.Plants.Count + simulationEngine.World.Herbivores.Count + simulationEngine.World.Carnivores.Count;
        TotalPopulationText.Text = total.ToString();
        TotalPeakText.Text = $"(Peak: {stats.PeakPopulation})";

        PlantsGrownText.Text = stats.TotalPlantsGrown.ToString();
        PlantsEatenText.Text = stats.TotalPlantsEaten.ToString();
        BirthsText.Text = stats.TotalBirths.ToString();
        DeathsText.Text = stats.TotalDeaths.ToString();

        TimeSpan simTime = TimeSpan.FromSeconds(stats.SessionTime);
        SimulationTimeText.Text = $"{simTime.Hours:D2}:{simTime.Minutes:D2}:{simTime.Seconds:D2}";

        double health = EcosystemHealthScorer.CalculateHealthPercent(
            simulationEngine.World.Plants.Count,
            simulationEngine.World.Herbivores.Count,
            simulationEngine.World.Carnivores.Count);
        EcosystemHealthText.Text = $"{health:F1}%";

        // Note: CurrentSpeedText would need to be passed from MainWindow
        CurrentSpeedText.Text = "1.0x"; // Placeholder
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
