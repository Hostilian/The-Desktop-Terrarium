using System;
using System.Diagnostics;
using System.IO;

namespace Terrarium.Desktop;

public partial class MainWindow
{
    /// <summary>
    /// Launches the external Live Sandbox (Powder Toy) particle physics simulation.
    /// </summary>
    private void LaunchLiveSandbox()
    {
        LaunchPythonSimulator("powder_toy_demo.py", "Live Sandbox");
    }
    
    /// <summary>
    /// Launches the Civilization Builder 4X strategy game.
    /// </summary>
    private void LaunchCivilizationBuilder()
    {
        LaunchPythonSimulator("civilization_builder.py", "Civilization Builder");
    }

    /// <summary>
    /// Launches the 2048 widget game.
    /// </summary>
    private void Launch2048()
    {
        LaunchPythonSimulator("game_2048.py", "2048 Widget");
    }

    /// <summary>
    /// Launches the Tetris widget game.
    /// </summary>
    private void LaunchTetris()
    {
        LaunchPythonSimulator("game_tetris.py", "Tetris Widget");
    }

    /// <summary>
    /// Launches the Dino Runner widget game.
    /// </summary>
    private void LaunchDino()
    {
        LaunchPythonSimulator("game_dino.py", "Dino Widget");
    }

    /// <summary>
    /// Launches the Arcade widget (Snake).
    /// </summary>
    private void LaunchArcade()
    {
        LaunchPythonSimulator("game_arcade.py", "Arcade Widget");
    }

    /// <summary>
    /// Launches the Webview widget.
    /// </summary>
    private void LaunchWeb()
    {
        LaunchPythonSimulator("widget_web.py", "Web Widget");
    }

    /// <summary>
    /// Launches the Desktop Pet widget.
    /// </summary>
    private void LaunchPet()
    {
        LaunchPythonSimulator("widget_pet.py", "Desktop Pet");
    }
    
    /// <summary>
    /// Launches the Pacman widget.
    /// </summary>
    private void LaunchPacman()
    {
        LaunchPythonSimulator("game_pacman.py", "Pacman Widget");
    }
    
    private void LaunchPythonSimulator(string scriptName, string displayName)
    {
        try
        {
            // 1. Resolve Script Path
            // Search order:
            // a. Current Directory (Release/Deployed)
            // b. ../../../ (Development)
            
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = Path.Combine(baseDir, scriptName);
            
            if (!File.Exists(scriptPath))
            {
                // Check in 'widgets' subdirectory (Deployment Structure)
                string widgetsPath = Path.Combine(baseDir, "widgets", scriptName);
                if (File.Exists(widgetsPath))
                {
                    scriptPath = widgetsPath;
                }
                else
                {
                    // Try development path (New Structure: root/widgets/)
                    // From bin/Debug/net8.0-windows, we go up 3 levels to root, then into widgets
                    string devPath = Path.Combine(baseDir, "..", "..", "..", "widgets", scriptName);
                    if (File.Exists(devPath))
                    {
                        scriptPath = Path.GetFullPath(devPath);
                    }
                    else
                    {
                         // Fallback check in root (if user moves them back)
                         string oldDevPath = Path.Combine(baseDir, "..", "..", "..", scriptName);
                         if (File.Exists(oldDevPath))
                         {
                             scriptPath = Path.GetFullPath(oldDevPath);
                         }
                         else 
                         {
                            throw new FileNotFoundException($"Could not find script '{scriptName}'. Checked:\n1. {scriptPath}\n2. {widgetsPath}\n3. {devPath}\n4. {oldDevPath}");
                         }
                    }
                }
            }

            // 2. Resolve Python Path
            // Simple check: try running 'python --version'
            // In a real app, might want to allow configuring python path in settings
            var pythonPath = "python";
            
            Console.WriteLine($"[MainWindow] Launching {displayName}: {scriptPath}");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) // Important: Run from script dir so relative imports work
            };
            
            Process.Start(startInfo);
            Console.WriteLine($"[MainWindow] {displayName} launched successfully");
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2) // File not found (Python likely missing)
        {
             System.Windows.MessageBox.Show(
                $"Could not find 'python' executable.\n\nPlease ensure Python 3.8+ is installed and added to your system PATH.\n\nError: {ex.Message}",
                "Python Missing",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainWindow] Failed to launch {displayName}: {ex.Message}");
            System.Windows.MessageBox.Show(
                $"Failed to launch {displayName} simulation.\n\nError: {ex.Message}",
                "Launch Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
        }
    }
}
