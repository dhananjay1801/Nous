using System;
using System.Diagnostics;
using Avalonia.Controls;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nous
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Icon = new WindowIcon("E:/Coding/Avalonia/Nous/Nous.ico");
        }

        private async void OnSubmitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string ip = IpInput?.Text?.Trim() ?? string.Empty;
            string prompt = PromptInput?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(prompt))
            {
                OutputBox.Text = "Please enter both IP and prompt!";
                return;
            }

            // Generate command using backend.py (Stratos logic)
            string command = await GenerateCommand(prompt);

            if (string.IsNullOrEmpty(command))
            {
                OutputBox.Text = "Failed to generate command!";
                return;
            }
            string result = await SendCommand(ip, command);
            string finalOutput = await ProcessWithAI(prompt,result);
            OutputBox.Text = finalOutput;
        }

        private async Task<string> GenerateCommand(string prompt)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"backend.py \"{prompt}\"",
                    WorkingDirectory = AppContext.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return $"[Python error] {error}";

                return output.Trim();
            }
            catch (Exception ex)
            {
                return $"[Exception] {ex.Message}";
            }
            //return "whoami";
        }

        private async Task<string> SendCommand(string ip, string command)
        {
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                await client.ConnectAsync(ip, 8080);

                using var stream = client.GetStream();
                byte[] bytesToSend = Encoding.UTF8.GetBytes(command);
                await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return response.Trim();
            }
            catch (Exception ex)
            {
                return $"Error sending command: {ex.Message}";
            }
        }

        private async Task<string> ProcessWithAI(string query,string output)
        {
            output = query.Trim()+ "~~" + output;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"explainer.py \"{output}\"",
                    WorkingDirectory = AppContext.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return $"[Python error] {error}";

                return output.Trim();
            }

            catch (Exception ex)
            {
                return $"[Exception] {ex.Message}";
            }
        }
    }
}
