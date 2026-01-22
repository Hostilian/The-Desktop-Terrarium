using System;
using System.Windows;

namespace Terrarium.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public string? SelectedTerrariumType { get; set; }

    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create a dummy window as MainWindow to keep app alive
        var dummyWindow = new Window { Visibility = Visibility.Hidden };
        MainWindow = dummyWindow;
        dummyWindow.Show();

        var dialog = new StartupDialog();
        if (dialog.ShowDialog() == true)
        {
            SelectedTerrariumType = dialog.SelectedTerrariumType;

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Width = 800;
            mainWindow.Height = 600;
            mainWindow.Show();
            dummyWindow.Close(); // Close the dummy
        }
        else
        {
            Shutdown();
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            string appDataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DesktopTerrarium");
            
            System.IO.Directory.CreateDirectory(appDataPath);
            string logPath = System.IO.Path.Combine(appDataPath, "crash.log");
            
            string logContent = $"[{DateTime.Now}] CRASH REPORT\n" +
                                $"Exception: {e.Exception.Message}\n" +
                                $"Stack Trace:\n{e.Exception.StackTrace}\n" +
                                $"Source: {e.Exception.Source}\n" +
                                "--------------------------------------------------\n\n";

            System.IO.File.AppendAllText(logPath, logContent);
            
            MessageBox.Show($"An unexpected error occurred. Logs saved to:\n{logPath}\n\nError: {e.Exception.Message}", 
                          "Desktop Terrarium Crash", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
        }
        catch
        {
            // Fallback if logging fails
            MessageBox.Show($"Fatal Error: {e.Exception.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        e.Handled = true;
        Shutdown();
    }
}
