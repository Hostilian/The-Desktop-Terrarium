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
        MessageBox.Show($"Unhandled Exception: {e.Exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
