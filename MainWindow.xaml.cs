using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace YtDownloader;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cancellationTokenSource;
    private AppSettings _settings;

    public MainWindow()
    {
        try
        {
            Console.WriteLine("Iniciando MainWindow...");
            InitializeComponent();
            Console.WriteLine("InitializeComponent completado");
            
            // Cargar configuración guardada
            _settings = AppSettings.Load();
            Console.WriteLine("Configuración cargada");
            
            LoadFormats();
            Console.WriteLine("LoadFormats completado");
            LoadLastFolder();
            Console.WriteLine("LoadLastFolder completado");
            CheckYtDlpInstallation();
            Console.WriteLine("CheckYtDlpInstallation completado");
            
            // Forzar que la ventana sea visible y activa
            this.Visibility = Visibility.Visible;
            this.Activate();
            this.Focus();
            Console.WriteLine("Ventana activada y enfocada");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al iniciar la ventana: {ex.Message}\n\n{ex.StackTrace}", 
                "Error de Inicialización", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void LoadLastFolder()
    {
        if (!string.IsNullOrEmpty(_settings.LastDownloadFolder) && Directory.Exists(_settings.LastDownloadFolder))
        {
            OutputFolderTextBox.Text = _settings.LastDownloadFolder;
        }
        else
        {
            OutputFolderTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        if (_settings.LastFormatIndex >= 0 && _settings.LastFormatIndex < FormatComboBox.Items.Count)
        {
            FormatComboBox.SelectedIndex = _settings.LastFormatIndex;
        }
    }

    private void SaveCurrentSettings()
    {
        _settings.LastDownloadFolder = OutputFolderTextBox.Text;
        _settings.LastFormatIndex = FormatComboBox.SelectedIndex;
        _settings.Save();
    }

    private void LoadFormats()
    {
        FormatComboBox.ItemsSource = new[]
        {
            "Mejor calidad (video + audio)",
            "Solo audio (MP3)",
            "Solo audio (M4A)",
            "Video 1080p",
            "Video 720p",
            "Video 480p",
            "Video 360p"
        };
        FormatComboBox.SelectedIndex = 0;
    }

    private async void CheckYtDlpInstallation()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string version = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                StatusTextBlock.Text = $"yt-dlp instalado: {version.Trim()}";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                StatusTextBlock.Text = "yt-dlp no encontrado. Por favor instálalo primero.";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch
        {
            StatusTextBlock.Text = "yt-dlp no encontrado. Por favor instálalo primero.";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlTextBox.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("Por favor ingresa una URL válida", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string outputFolder = OutputFolderTextBox.Text.Trim();
        if (string.IsNullOrEmpty(outputFolder))
        {
            outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            OutputFolderTextBox.Text = outputFolder;
        }

        if (!Directory.Exists(outputFolder))
        {
            MessageBox.Show("La carpeta de salida no existe", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        DownloadButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        ProgressBar.Value = 0;
        ProgressTextBlock.Text = "Iniciando descarga...";

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await DownloadVideoAsync(url, outputFolder, FormatComboBox.SelectedIndex, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            ProgressTextBlock.Text = "Descarga cancelada";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ProgressTextBlock.Text = "Error en la descarga";
        }
        finally
        {
            DownloadButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }
    }

    private async Task DownloadVideoAsync(string url, string outputFolder, int formatIndex, CancellationToken cancellationToken)
    {
        string formatArgument = formatIndex switch
        {
            0 => "-f b",
            1 => "-x --audio-format mp3 --audio-quality 0",
            2 => "-x --audio-format m4a --audio-quality 0",
            3 => "-f 'bestvideo[height<=1080]+bestaudio/best[height<=1080]'",
            4 => "-f 'bestvideo[height<=720]+bestaudio/best[height<=720]'",
            5 => "-f 'bestvideo[height<=480]+bestaudio/best[height<=480]'",
            6 => "-f 'bestvideo[height<=360]+bestaudio/best[height<=360]'",
            _ => "-f b"
        };

        // Usar -P para la carpeta de salida y template relativo para evitar el warning
        string arguments = $"{formatArgument} -P \"{outputFolder}\" -o \"%(title)s.%(ext)s\" --newline --progress {url}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            },
            EnableRaisingEvents = true
        };

        var progressRegex = new System.Text.RegularExpressions.Regex(@"\[download\]\s+(\d+\.?\d*)%");

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText(e.Data + Environment.NewLine);
                    LogTextBox.ScrollToEnd();

                    var match = progressRegex.Match(e.Data);
                    if (match.Success && double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double progress))
                    {
                        ProgressBar.Value = progress;
                        ProgressTextBlock.Text = $"Progreso: {progress:F1}%";
                    }
                });
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText("[ERROR] " + e.Data + Environment.NewLine);
                    LogTextBox.ScrollToEnd();
                });
            }
        };

        using (cancellationToken.Register(() => process.Kill()))
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
        }

        if (process.ExitCode == 0)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 100;
                ProgressTextBlock.Text = "¡Descarga completada!";
                SaveCurrentSettings();
                MessageBox.Show("¡Descarga completada exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        else
        {
            throw new Exception($"yt-dlp terminó con código de error: {process.ExitCode}");
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Seleccionar carpeta de destino",
            FileName = "Seleccionar carpeta",
            CheckFileExists = false,
            CheckPathExists = true,
            ValidateNames = false
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFolderTextBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
            SaveCurrentSettings();
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText())
        {
            UrlTextBox.Text = Clipboard.GetText();
        }
    }
}
