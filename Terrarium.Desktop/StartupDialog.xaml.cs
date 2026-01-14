using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Terrarium.Desktop;

/// <summary>
/// Interaction logic for StartupDialog.xaml
/// </summary>
public partial class StartupDialog : Window
{
    public string SelectedTerrariumType { get; private set; } = "LiveSandbox"; // Default to a widget

    public ObservableCollection<TerrariumTypeOption> Options { get; set; } = new ObservableCollection<TerrariumTypeOption>();

    public StartupDialog()
    {
        InitializeComponent();
        this.Loaded += (s, e) => 
        {
            if (TerrariumComboBox != null) 
                TerrariumComboBox.IsDropDownOpen = true;
        };
        
        // --- Populate the Data ---
        // 1. Live Sandbox (Physics)
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "Live Sandbox", 
            Subtitle = "Particle Physics Simulation",
            TitleFont = new FontFamily("Segoe UI"),
            ThemeColor = Brushes.Blue,
            IconGeometry = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6M12,8A4,4 0 0,0 8,12A4,4 0 0,0 12,16A4,4 0 0,0 16,12A4,4 0 0,0 12,8Z", 
            Id = "LiveSandbox"
        });

        // 2. Civilization Builder (4X Strategy)
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "Civilization Builder", 
            Subtitle = "4X Strategy Game",
            TitleFont = new FontFamily("Cambria"),
            ThemeColor = Brushes.DarkRed,
            IconGeometry = "M2,22L12,2L22,22H17L12,12L7,22H2M12,6L14.5,11H9.5L12,6Z", 
            Id = "CivilizationBuilder"
        });

        // 3. 2048 (Puzzle)
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "2048", 
            Subtitle = "Classic Puzzle Game",
            TitleFont = new FontFamily("Consolas"),
            ThemeColor = Brushes.Orange,
            IconGeometry = "M4,4H20V20H4V4M6,6V18H18V6H6M8,8H16V16H8V8Z", 
            Id = "Game2048"
        });

        // 4. Tetris
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "Tetris", 
            Subtitle = "Arcade Stack",
            TitleFont = new FontFamily("Impact"),
            ThemeColor = Brushes.Purple,
            IconGeometry = "M2,10H6V14H2V10M6,10H10V14H6V10M10,10H14V14H10V10M10,6H14V10H10V6Z", 
            Id = "GameTetris"
        });

        // 5. T-Rex Runner
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "T-Rex Runner", 
            Subtitle = "Dinosaur Run",
            TitleFont = new FontFamily("Courier New"),
            ThemeColor = Brushes.Gray,
            IconGeometry = "M4,20H20V18H18V16H16V14H18V10H16V8H14V6H10V8H8V14H6V16H4V20Z", 
            Id = "GameDino"
        });

        // 6. Arcade - Snake
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "Arcade - Snake", 
            Subtitle = "Classic Snake",
            TitleFont = new FontFamily("Comic Sans MS"), 
            ThemeColor = Brushes.Green,
            IconGeometry = "M2,12A2,2 0 0,1 4,10H16A2,2 0 0,1 18,12A2,2 0 0,1 16,14H8V16H6V14H4A2,2 0 0,1 2,12Z", 
            Id = "GameArcade"
        });

        // 7. Pacman
        Options.Add(new TerrariumTypeOption 
        { 
            Title = "Pacman", 
            Subtitle = "Arcade Classic",
            TitleFont = new FontFamily("Comic Sans MS"),
            ThemeColor = Brushes.Gold, 
            IconGeometry = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M16,12L21,9.5L16,12L21,14.5L16,12Z", 
            Id = "GamePacman"
        });

        // 8. Other Widgets
         Options.Add(new TerrariumTypeOption 
        { 
            Title = "Web Widget", 
            Subtitle = "Browser Portal",
            TitleFont = new FontFamily("Verdana"),
            ThemeColor = Brushes.DodgerBlue,
            IconGeometry = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12C4,13.85 4.63,15.55 5.68,16.91L18.29,4.32C16.53,4.11 14.68,4 12,4M12,20A8,8 0 0,0 20,12C20,10.15 19.37,8.45 18.32,7.09L5.71,19.68C7.47,19.89 9.32,20 12,20Z", 
            Id = "WidgetWeb"
        });
        
         Options.Add(new TerrariumTypeOption 
        { 
            Title = "Desktop Pet", 
            Subtitle = "Virtual Companion",
            TitleFont = new FontFamily("Segoe Print"),
            ThemeColor = Brushes.HotPink,
            IconGeometry = "M12,2A10,10 0 0,0 2,12C2,17.5 6.5,22 12,22C17.5,22 22,17.5 22,12A10,10 0 0,0 12,2M7,9.5C7,8.7 7.7,8 8.5,8C9.3,8 10,8.7 10,9.5C10,10.3 9.3,11 8.5,11C7.7,11 7,10.3 7,9.5M17,9.5C17,8.7 17.7,8 18.5,8C19.3,8 20,8.7 20,9.5C20,10.3 19.3,11 18.5,11C17.7,11 17,10.3 17,9.5M12,17.3C9.67,17.3 7.69,15.9 6.5,13.8H17.5C16.31,15.9 14.33,17.3 12,17.3Z", 
            Id = "WidgetPet"
        });

        // Legacy / Demos (Bottom)
        AddLegacyOption("Forest", "in development", Brushes.DarkGreen, "Forest");
        AddLegacyOption("Desert", "in development", Brushes.SaddleBrown, "Desert");
        AddLegacyOption("Aquatic", "in development", Brushes.Navy, "Aquatic");
        AddLegacyOption("God Simulator", "in development", Brushes.Indigo, "GodSimulator");

        // Bind DataContext
        DataContext = this;
    }

    private void AddLegacyOption(string title, string subtitle, Brush color, string id)
    {
        Options.Add(new TerrariumTypeOption
        {
            Title = title,
            Subtitle = subtitle,
            TitleFont = SystemFonts.MessageFontFamily, // Standard font
            TitleFontSize = 12, // Small
            SubtitleFontSize = 10, // Very small
            ThemeColor = color,
            IconGeometry = "M12,2L2,22H22L12,2Z", // Simple triangle/mountain
            Id = id
        });
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        // Try getting selection from ComboBox named 'TerrariumComboBox'
        object selectedItem = null;
        
        // Find ComboBox in visual tree if needed, or binding
        // Assuming we bind SelectedItem in XAML to a property, or verify accessing it directly.
        // For WPF, named elements in XAML are accessible in code behind.
        
        if (this.FindName("TerrariumComboBox") is ComboBox combo)
        {
            selectedItem = combo.SelectedItem;
        }

        if (selectedItem is TerrariumTypeOption selected)
        {
            SelectedTerrariumType = selected.Id;

            // Check Python only for Widgets/Games that need it
            // Based on ID naming convention from MainWindow
            bool requiresPython = !IsLegacy(selected.Id);

            if (requiresPython && !IsPythonInstalled())
            {
                var result = MessageBox.Show(
                    "This mode requires Python 3.8+ to be installed and available in your system PATH.\n\nWe couldn't detect 'python'. Do you want to try launching anyway?",
                    "Python Requirement",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Please select a game type first.", "Selection Required");
        }
    }
    
    private bool IsLegacy(string id)
    {
        return id == "Forest" || id == "Desert" || id == "Aquatic" || id == "GodSimulator";
    }

    private bool IsPythonInstalled()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "--version",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var process = Process.Start(startInfo);
            if (process == null) return false;
            
            process.WaitForExit(1000); // Wait max 1 second
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
