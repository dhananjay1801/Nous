using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nous.Utils;
namespace Nous.Views
{
    public partial class AccessGate : Window
    {
        private ObservableCollection<string> _results = new ObservableCollection<string>();

        public AccessGate()
        {
            InitializeComponent();
            ResultsList.ItemsSource = _results;
            Logger.SWrite("AccessGate window initialized.");
        }

        private void AddHash_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var box = new TextBox
                {
                    Width = 300,
                    Background = Avalonia.Media.Brushes.DimGray,
                    Foreground = Avalonia.Media.Brushes.White
                };
                HashListPanel.Children.Add(box);
                Logger.SWrite("Hash textbox added.");
            }
            catch (Exception ex)
            {
                Logger.EWrite($"AddHash_Click error: {ex.Message}");
            }
        }

        private void RemoveHash_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (HashListPanel.Children.Count > 0)
                {
                    HashListPanel.Children.RemoveAt(HashListPanel.Children.Count - 1);
                    Logger.SWrite("Hash textbox removed.");
                }
            }
            catch (Exception ex)
            {
                Logger.EWrite($"RemoveHash_Click error: {ex.Message}");
            }
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                HashInput.Text = string.Empty;
                HashListPanel.Children.Clear();
                PromptInput.Text = string.Empty;
                CommandBox.Text = string.Empty;
                _results.Clear();
                Logger.SWrite("Inputs and results cleared.");
            }
            catch (Exception ex)
            {
                Logger.EWrite($"Clear_Click error: {ex.Message}");
            }
        }

        private async void Generate_Click(object? sender, RoutedEventArgs e)
        {
            string prompt = PromptInput.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(prompt))
            {
                AddResult("Please enter the prompt first!");
                Logger.EWrite("Generate_Click failed: prompt was empty.");
                return;
            }

            Logger.SWrite($"Generate_Click received prompt: {prompt}");
            string command = await RunPython("backend.py", $"\"{prompt}\"", @"E:\College Project\Additions\Nous\Nous\");
            if (string.IsNullOrEmpty(command))
            {
                Logger.EWrite("backend.py returned empty command.");
                return;
            }

            string sanitized = await RunPython("sanitizer.py", $"\"{command}\"", @"E:\College Project\Min_Nous\Nous\");
            if (string.IsNullOrEmpty(sanitized))
            {
                Logger.EWrite("sanitizer.py returned empty output.");
                return;
            }

            CommandBox.Text = sanitized;
            Logger.SWrite($"Command generated and sanitized: {sanitized}");
        }

        private async void Send_Click(object? sender, RoutedEventArgs e)
        {
            _results.Clear();

            var hashcodes = new List<string>();
            if (!string.IsNullOrWhiteSpace(HashInput.Text))
                hashcodes.Add(HashInput.Text.Trim());

            foreach (var child in HashListPanel.Children.OfType<TextBox>())
                if (!string.IsNullOrWhiteSpace(child.Text))
                    hashcodes.Add(child.Text.Trim());

            if (hashcodes.Count == 0)
            {
                AddResult("Please enter at least one hash code!");
                Logger.EWrite("Send_Click failed: no hashcodes provided.");
                return;
            }

            string command = CommandBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(command))
            {
                AddResult("Please generate or enter a command before sending.");
                Logger.EWrite("Send_Click failed: command is empty.");
                return;
            }

            var manager = new IpHashManager();
            Parallel.ForEach(hashcodes, hashcode =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string? ip = manager.GetIpByHashcode(hashcode);
                        if (string.IsNullOrEmpty(ip))
                        {
                            AddResult($"{hashcode} : No IP found.");
                            Logger.EWrite($"{hashcode} : No IP found.");
                            return;
                        }

                        var rawOutput = await SendCommand(ip, command);
                        var explained = await ProcessWithAI(command, rawOutput);
                        AddResult($"{hashcode} : {explained}");
                        Logger.SWrite($"{hashcode} : Command executed successfully.");
                    }
                    catch (Exception ex)
                    {
                        AddResult($"{hashcode} : Error - {ex.Message}");
                        Logger.EWrite($"{hashcode} : Exception - {ex.Message}");
                    }
                });
            });
        }

        private void AddResult(string text)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _results.Add(text);
                Logger.SWrite($"Result added: {text}");
            });
        }

        private async Task<string> SendCommand(string ip, string command)
        {
            Logger.SWrite($"Sending command '{command}' to IP: {ip}");
            var result = await RunPython("client_bridge.py", $"\"{ip}\" \"{command}\"", AppContext.BaseDirectory);
            Logger.SWrite($"SendCommand result for {ip}: {result}");
            return result;
        }

        private async Task<string> ProcessWithAI(string query, string output)
        {
            Logger.SWrite($"Processing output with AI for query: {query}");
            string input = $"{query}~~{output}";
            var result = await RunPython("explainer.py", $"\"{input}\"", AppContext.BaseDirectory);
            Logger.SWrite("AI processing completed.");
            return result;
        }

        private async Task<string> RunPython(string script, string args, string workingDir)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"{script} {args}",
                    WorkingDirectory = workingDir,
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
                {
                    Logger.EWrite($"RunPython error in {script}: {error}");
                    return $"[Python error] {error}";
                }

                Logger.SWrite($"RunPython success in {script}: {output.Trim()}");
                return output.Trim();
            }
            catch (Exception ex)
            {
                Logger.EWrite($"RunPython exception in {script}: {ex.Message}");
                return $"[Exception] {ex.Message}";
            }
        }
    }
}
