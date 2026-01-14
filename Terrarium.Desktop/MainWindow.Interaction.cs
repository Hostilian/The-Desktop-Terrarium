using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Terrarium.Desktop.Rendering;

namespace Terrarium.Desktop;

public partial class MainWindow
{
    // Input tuning constants
    private const double SpeedStep = 0.25;
    private const int WaterPlantsMaxPerKeypress = 10;
    private const double WaterAmountPerPlant = 20;

    /// <summary>
    /// Direct left-click handler for reliable entity interaction.
    /// </summary>
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        var position = e.GetPosition(RenderCanvas);

        // Try to interact with clickable entities (feed creatures)
        var clickable = _simulationEngine.FindClickableAt(position.X, position.Y);
        if (clickable != null)
        {
            clickable.OnClick();
            e.Handled = true;
            return;
        }
    }

    /// <summary>
    /// Handles mouse click events (backup handler).
    /// </summary>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Simplified: no special handling
    }

    /// <summary>
    /// Handles mouse move events.
    /// </summary>
    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(RenderCanvas);
        _mousePosition = position;
        _mouseInCanvas = position.X >= 0 && position.X <= RenderCanvas.ActualWidth &&
                       position.Y >= 0 && position.Y <= RenderCanvas.ActualHeight;
    }

    /// <summary>
    /// Handles keyboard input for basic controls.
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (_simulationEngine == null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Space:
                _simulationEngine.TogglePause();
                e.Handled = true;
                break;

            case Key.OemPlus:
            case Key.Add:
                _simulationEngine.SetSimulationSpeed(_simulationEngine.SimulationSpeed + SpeedStep);
                e.Handled = true;
                break;

            case Key.OemMinus:
            case Key.Subtract:
                _simulationEngine.SetSimulationSpeed(_simulationEngine.SimulationSpeed - SpeedStep);
                e.Handled = true;
                break;

            case Key.P:
                // P: Spawn plant
                _simulationEngine.World.SpawnRandomPlant();
                e.Handled = true;
                break;

            case Key.H:
                // H: Spawn herbivore
                _simulationEngine.World.SpawnRandomHerbivore();
                e.Handled = true;
                break;

            case Key.C:
                // C: Spawn carnivore
                _simulationEngine.World.SpawnRandomCarnivore();
                e.Handled = true;
                break;

            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// Updates the visibility of god powers based on the current terrarium mode.
    /// Only God Simulator mode should show destructive powers.
    /// </summary>
    private void UpdateGodPowersVisibility()
    {
        bool isGodMode = _terrariumType == Terrarium.Logic.Simulation.TerrariumType.GodSimulator;

        // Find the god powers toolbar sections
        var godPowersBorder = FindName("GodPowersBorder") as Border;
        if (godPowersBorder != null)
        {
            // In non-God modes, hide the entire god powers toolbar
            godPowersBorder.Visibility = isGodMode ? Visibility.Visible : Visibility.Collapsed;
        }

        // Alternative: if we want to show creation powers in all modes but hide destructive ones
        // This would require finding individual StackPanels and setting their visibility
        // For now, we'll hide the entire toolbar in non-God modes for a clean, peaceful interface
    }
}
