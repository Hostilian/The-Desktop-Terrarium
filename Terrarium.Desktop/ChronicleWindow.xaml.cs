namespace Terrarium.Desktop;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Terrarium.Logic.Simulation;

/// <summary>
/// Window for displaying the world chronicle and notable characters.
/// Inspired by Caves of Qud's lore system.
/// </summary>
public partial class ChronicleWindow : Window
{
    private readonly SimulationEngine simulationEngine;
    private readonly ObservableCollection<LoreEvent> chronicleEvents = new();
    private readonly ObservableCollection<NamedCharacter> namedCharacters = new();

    public ChronicleWindow(SimulationEngine simulationEngine)
    {
        InitializeComponent();
        this.simulationEngine = simulationEngine ?? throw new ArgumentNullException(nameof(simulationEngine));

        ChronicleList.ItemsSource = chronicleEvents;
        NamedCharactersList.ItemsSource = namedCharacters;

        UpdateChronicle();
    }

    /// <summary>
    /// Updates the chronicle display with current events and characters.
    /// </summary>
    public void UpdateChronicle()
    {
        // Update chronicle events
        chronicleEvents.Clear();
        foreach (var loreEvent in simulationEngine.LoreManager.Chronicle.ToList().AsEnumerable().Reverse()) // Most recent first
        {
            chronicleEvents.Add(loreEvent);
        }

        // Update named characters
        namedCharacters.Clear();
        foreach (var character in simulationEngine.LoreManager.NamedCharacters.Values.Where(c => c.IsAlive))
        {
            namedCharacters.Add(character);
        }

        // Update UI elements
        EventCountText.Text = $"({chronicleEvents.Count} events)";
        NoEventsText.Visibility = chronicleEvents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        NoCharactersText.Visibility = namedCharacters.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Hide instead of close to allow reopening
        e.Cancel = true;
        Hide();
    }
}
