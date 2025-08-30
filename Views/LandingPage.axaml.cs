using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nous.Utils;
namespace Nous.Views
{
    public partial class LandingPage : UserControl
    {
        private DispatcherTimer _timer;
        private int _remainingSeconds = 3600; // 1 hour in seconds
        private readonly IpHashManager _hashManager;

        public LandingPage()
        {
            InitializeComponent();
            _hashManager = new IpHashManager();
            Logger.SWrite("LandingPage initialized.");
            StartCountdown();
        }

        private void StartCountdown()
        {
            Logger.SWrite("Countdown timer started.");
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                _remainingSeconds--;
                if (_remainingSeconds < 0)
                {
                    _remainingSeconds = 3600;
                    Logger.SWrite("Countdown reached 0. Updating hashcodes...");
                    UpdateHashcodes();
                }

                var hours = _remainingSeconds / 3600;
                var minutes = (_remainingSeconds % 3600) / 60;
                var seconds = _remainingSeconds % 60;
                TimerText.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            };
            _timer.Start();
        }

        private void UpdateHashcodes()
        {
            try
            {
                _hashManager.UpdateHashcodes();
                Logger.SWrite("Hashcodes updated successfully.");
                Debug.WriteLine("Hashcodes updated successfully.");
            }
            catch (Exception ex)
            {
                Logger.EWrite($"Error updating hashcodes: {ex.Message}");
                Debug.WriteLine($"Error updating hashcodes: {ex.Message}");
            }
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            Logger.SWrite("StartServer button clicked.");
            // Run Python server in background
            string scriptPath = @"D:\Project stuff\Nous\api_server.py";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C python \"{scriptPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                });
                Logger.SWrite("Python server process started.");
            }
            catch (Exception ex)
            {
                Logger.EWrite($"Failed to start Python server: {ex.Message}");
            }

            // Show popup for 5 seconds
            var popup = new StartServerWindow();
            popup.Show();
            Logger.SWrite("StartServer popup displayed.");
            await Task.Delay(5000);
            popup.Close();
            Logger.SWrite("StartServer popup closed.");
        }

        private void AccessGate_Click(object? sender, RoutedEventArgs e)
        {
            Logger.SWrite("AccessGate button clicked.");
            var accessGateWindow = new AccessGate();
            accessGateWindow.Show();
            Logger.SWrite("AccessGate window opened.");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Logger.SWrite("About button clicked.");
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog((Window)this.VisualRoot);
            Logger.SWrite("About window opened as dialog.");
        }
    }
}
