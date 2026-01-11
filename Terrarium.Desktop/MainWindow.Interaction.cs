using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Terrarium.Desktop.Rendering;

namespace Terrarium.Desktop
{
    public partial class MainWindow
    {
        // Input tuning constants
        private const double SpeedStep = 0.25;
        private const int WaterPlantsMaxPerKeypress = 10;
        private const double WaterAmountPerPlant = 20;
        private bool _uiVisible = true;

        /// <summary>
        /// Direct left-click handler for reliable entity interaction.
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_simulationEngine == null)
                return;

            var position = e.GetPosition(RenderCanvas);

            // First try to interact with clickable entities (feed creatures)
            var clickable = _simulationEngine.FindClickableAt(position.X, position.Y);
            if (clickable != null)
            {
                clickable.OnClick();
                _particleSystem?.SpawnFeedEffect(position.X, position.Y);
                _notificationManager?.Notify("🍖 Fed!", NotificationType.Success);
                e.Handled = true;
                return;
            }

            // Then try entity selection
            if (_entitySelector != null)
            {
                bool selected = _entitySelector.TrySelect(
                    position.X, position.Y,
                    _simulationEngine.World.Plants,
                    _simulationEngine.World.Herbivores,
                    _simulationEngine.World.Carnivores);

                if (selected)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Handles mouse click events (backup handler).
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_simulationEngine == null)
                return;

            // Only handle non-left clicks here (left clicks go to Window_MouseLeftButtonDown)
            if (e.ChangedButton == MouseButton.Left)
                return;

            var position = e.GetPosition(RenderCanvas);

            // Right-click: deselect
            if (e.ChangedButton == MouseButton.Right && _entitySelector != null)
            {
                _entitySelector.Deselect();
                _isFollowingEntity = false;
            }
        }

        /// <summary>
        /// Handles mouse move events (for hover effects and tooltips).
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_simulationEngine == null)
                return;

            var position = e.GetPosition(RenderCanvas);

            // Check for entity under cursor for tooltip
            Terrarium.Logic.Entities.WorldEntity? hoveredEntity = null;

            // Check plants first
            hoveredEntity = _simulationEngine.World.Plants
                .FirstOrDefault(p => p.IsAlive && p.ContainsPoint(position.X, position.Y));

            // Check herbivores
            if (hoveredEntity == null)
            {
                hoveredEntity = _simulationEngine.World.Herbivores
                    .FirstOrDefault(h => h.IsAlive && h.ContainsPoint(position.X, position.Y));
            }

            // Check carnivores
            if (hoveredEntity == null)
            {
                hoveredEntity = _simulationEngine.World.Carnivores
                    .FirstOrDefault(c => c.IsAlive && c.ContainsPoint(position.X, position.Y));
            }

            if (hoveredEntity != null)
            {
                _tooltipManager?.ShowTooltip(hoveredEntity, position.X, position.Y);

                // Plant shake effect
                if (hoveredEntity is Terrarium.Logic.Entities.Plant plant)
                {
                    plant.Shake();
                    _renderer?.TriggerPlantShake(plant);
                }
            }
            else
            {
                _tooltipManager?.HideTooltip();
            }
        }

        /// <summary>
        /// Handles keyboard input for save/load and controls.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_simulationEngine == null)
                return;

            switch (e.Key)
            {
                case Key.Space:
                    _simulationEngine.TogglePause();
                    _notificationManager?.Notify(
                        _simulationEngine.IsPaused ? "⏸️ Paused" : "▶️ Resumed",
                        NotificationType.Info);
                    e.Handled = true;
                    break;

                case Key.OemPlus:
                case Key.Add:
                    _simulationEngine.SetSimulationSpeed(_simulationEngine.SimulationSpeed + SpeedStep);
                    _notificationManager?.Notify($"⏱️ Speed: {_simulationEngine.SimulationSpeed:0.##}x", NotificationType.Info);
                    e.Handled = true;
                    break;

                case Key.OemMinus:
                case Key.Subtract:
                    _simulationEngine.SetSimulationSpeed(_simulationEngine.SimulationSpeed - SpeedStep);
                    _notificationManager?.Notify($"⏱️ Speed: {_simulationEngine.SimulationSpeed:0.##}x", NotificationType.Info);
                    e.Handled = true;
                    break;

                case Key.S when Keyboard.Modifiers == ModifierKeys.Control:
                    // Ctrl+S: Save game
                    SaveGame();
                    e.Handled = true;
                    break;

                case Key.L when Keyboard.Modifiers == ModifierKeys.Control:
                    // Ctrl+L: Load game
                    LoadGame();
                    e.Handled = true;
                    break;

                case Key.P:
                    // P: Spawn plant
                    _simulationEngine.World.SpawnRandomPlant();
                    _particleSystem?.SpawnFeedEffect(
                        _simulationEngine.World.Plants.LastOrDefault()?.X ?? 400,
                        _simulationEngine.World.Plants.LastOrDefault()?.Y ?? 100);
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

                case Key.F1:
                    // F1: Toggle status panel
                    ToggleUIVisibility();
                    e.Handled = true;
                    break;

                case Key.F2:
                    // F2: Toggle settings panel
                    _settingsPanel?.Toggle();
                    e.Handled = true;
                    break;

                case Key.M:
                    // M: Toggle mini-map
                    _miniMap?.Toggle();
                    e.Handled = true;
                    break;

                case Key.G:
                    // G: Toggle population graph
                    if (_populationGraph != null)
                    {
                        _populationGraph.IsVisible = !_populationGraph.IsVisible;
                        _notificationManager?.Notify(
                            _populationGraph.IsVisible ? "📊 Population Graph ON" : "📊 Population Graph OFF",
                            NotificationType.Info);
                    }
                    e.Handled = true;
                    break;

                case Key.F:
                    // F: Toggle follow mode for selected entity
                    if (_entitySelector?.SelectedEntity != null)
                    {
                        _isFollowingEntity = !_isFollowingEntity;
                        _notificationManager?.Notify(
                            _isFollowingEntity ? "👁️ Following entity" : "👁️ Stopped following",
                            NotificationType.Info);
                    }
                    e.Handled = true;
                    break;

                case Key.W:
                    // W: Water plants (spawn rain particles)
                    WaterAllPlants();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    // ESC: Close settings, deselect entity, or close application
                    if (_settingsPanel?.IsVisible == true)
                    {
                        _settingsPanel.Hide();
                    }
                    else if (_entitySelector?.SelectedEntity != null)
                    {
                        _entitySelector.Deselect();
                        _isFollowingEntity = false;
                    }
                    else
                    {
                        Close();
                    }
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Waters all plants and shows particle effects.
        /// </summary>
        private void WaterAllPlants()
        {
            if (_simulationEngine == null)
                return;

            foreach (var plant in _simulationEngine.World.Plants.Where(p => p.IsAlive).Take(WaterPlantsMaxPerKeypress))
            {
                plant.Water(WaterAmountPerPlant);
                _particleSystem?.SpawnWaterEffect(plant.X, plant.Y);
            }

            _notificationManager?.Notify("💧 Watered plants!", NotificationType.Info);
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        private void SaveGame()
        {
            if (_saveManager == null || _simulationEngine == null)
                return;

            try
            {
                _saveManager.SaveWorld(_simulationEngine.World);
                // Could show a save confirmation visual here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads a saved game state.
        /// </summary>
        private void LoadGame()
        {
            if (_saveManager == null)
                return;

            try
            {
                var loadedWorld = _saveManager.LoadWorld();
                _simulationEngine = new Terrarium.Logic.Simulation.SimulationEngine(loadedWorld);

                // Could show a load confirmation visual here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Toggles the visibility of all UI panels for a clean minimalist view.
        /// </summary>
        private void ToggleUIVisibility()
        {
            _uiVisible = !_uiVisible;
            var newVisibility = _uiVisible ? Visibility.Visible : Visibility.Collapsed;

            // Toggle all UI panels
            if (FindName("TopBar") is UIElement topBar)
                topBar.Visibility = newVisibility;
            if (FindName("BottomStats") is UIElement bottomStats)
                bottomStats.Visibility = newVisibility;
            if (FindName("SpeedPanel") is UIElement speedPanel)
                speedPanel.Visibility = newVisibility;
            if (FindName("FpsPanel") is UIElement fpsPanel)
                fpsPanel.Visibility = newVisibility;
            if (FindName("WeatherPanel") is UIElement weatherPanel)
                weatherPanel.Visibility = newVisibility;

            // Legacy panel names (fallback)
            if (FindName("StatusPanel") is UIElement statusPanel)
                statusPanel.Visibility = newVisibility;
            if (FindName("StatsPanel") is UIElement statsPanel)
                statsPanel.Visibility = newVisibility;
            if (FindName("HotkeyPanel") is UIElement hotkeyPanel)
                hotkeyPanel.Visibility = newVisibility;
            if (FindName("DayNightOrb") is UIElement dayNightOrb)
                dayNightOrb.Visibility = newVisibility;

            // Toggle minimap visibility
            if (_miniMap != null)
            {
                _miniMap.IsVisible = _uiVisible;
            }

            _notificationManager?.Notify(
                _uiVisible ? "👁️ UI Visible" : "👁️ UI Hidden (F1 to show)",
                NotificationType.Info);
        }

        /// <summary>
        /// Cleanup when window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Auto-save on exit
            SaveGame();

            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            {
                UnregisterGlobalHotkeys(hwndSource.Handle);
            }

            _renderTimer?.Stop();
            _systemMonitorTimer?.Stop();
            _systemMonitor?.Dispose();
            _soundManager?.Dispose();
            _weatherEffects?.Clear();
            _particleSystem?.Clear();
            _notificationManager?.ClearAll();
            _moodIndicator?.Clear();
            _predatorWarning?.Clear();
            _breedingIndicator?.Clear();
            _entitySelector?.Clear();
            base.OnClosed(e);
        }

        /// <summary>
        /// Gets a display name for an entity.
        /// </summary>
        private static string GetEntityName(Terrarium.Logic.Entities.LivingEntity entity)
        {
            return entity switch
            {
                Terrarium.Logic.Entities.Carnivore c => $"🔴 {c.Type}",
                Terrarium.Logic.Entities.Herbivore h => $"🟢 {h.Type}",
                Terrarium.Logic.Entities.Plant _ => "🌿 Plant",
                _ => "Entity"
            };
        }
    }
}
