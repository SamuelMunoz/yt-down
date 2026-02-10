using System.Windows;
using System.Windows.Threading;

namespace YtDownloader;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Error no manejado: {args.Exception.Message}\n\n{args.Exception.StackTrace}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
