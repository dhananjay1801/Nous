using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
            StartCountdown();
        }

        private void StartCountdown()
        {
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
                Debug.WriteLine("Hashcodes updated successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating hashcodes: {ex.Message}");
            }
        }

        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            // Run Python server in background
            string scriptPath = @"E:\College Project\Min_Nous\Nous\api_server.py";

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C python \"{scriptPath}\"",
                UseShellExecute = true,
                CreateNoWindow = false
            });

            // Show popup for 5 seconds
            var popup = new StartServerWindow();
            popup.Show();
            await Task.Delay(5000);
            popup.Close();
        }

        private void AccessGate_Click(object? sender, RoutedEventArgs e)
        {
            var accessGateWindow = new AccessGate();
            accessGateWindow.Show();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog((Window)this.VisualRoot);
        }
    }
}
