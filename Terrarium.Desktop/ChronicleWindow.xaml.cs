using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Terrarium.Logic.Simulation;

namespace Terrarium.Desktop;

/// <summary>
/// Window for displaying the world chronicle and notable characters.
/// Inspired by Caves of Qud's lore system.
/// </summary>
public partial class ChronicleWindow : Window
{
    private readonly SimulationEngine _simulationEngine;
    private readonly ObservableCollection<LoreEvent> _chronicleEvents = new();
    private readonly ObservableCollection<NamedCharacter> _namedCharacters = new();

    public ChronicleWindow(SimulationEngine simulationEngine)
    {
        InitializeComponent();
        _simulationEngine = simulationEngine ?? throw new ArgumentNullException(nameof(simulationEngine));

        ChronicleList.ItemsSource = _chronicleEvents;
        NamedCharactersList.ItemsSource = _namedCharacters;

        UpdateChronicle();
    }

    /// <summary>
    /// Updates the chronicle display with current events and characters.
    /// </summary>
    public void UpdateChronicle()
    {
        // Update chronicle events
        _chronicleEvents.Clear();
        foreach (var loreEvent in _simulationEngine.LoreManager.Chronicle.ToList().AsEnumerable().Reverse()) // Most recent first
        {
            _chronicleEvents.Add(loreEvent);
        }

        // Update named characters
        _namedCharacters.Clear();
        foreach (var character in _simulationEngine.LoreManager.NamedCharacters.Values.Where(c => c.IsAlive))
        {
            _namedCharacters.Add(character);
        }

        // Update UI elements
        EventCountText.Text = $"({_chronicleEvents.Count} events)";
        NoEventsText.Visibility = _chronicleEvents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        NoCharactersText.Visibility = _namedCharacters.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Hide instead of close to allow reopening
        e.Cancel = true;
        Hide();
    }
}
