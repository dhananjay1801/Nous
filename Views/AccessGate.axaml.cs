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

namespace Nous.Views
{
    public partial class AccessGate : Window
    {
        private ObservableCollection<string> _results = new ObservableCollection<string>();

        public AccessGate()
        {
            InitializeComponent();
            ResultsList.ItemsSource = _results;
        }

        private void AddHash_Click(object? sender, RoutedEventArgs e)
        {
            var box = new TextBox
            {
                Width = 300,
                Background = Avalonia.Media.Brushes.DimGray,
                Foreground = Avalonia.Media.Brushes.White
            };
            HashListPanel.Children.Add(box);
        }

        private void RemoveHash_Click(object? sender, RoutedEventArgs e)
        {
            if (HashListPanel.Children.Count > 0)
                HashListPanel.Children.RemoveAt(HashListPanel.Children.Count - 1);
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            HashInput.Text = string.Empty;
            HashListPanel.Children.Clear();
            PromptInput.Text = string.Empty;
            _results.Clear();
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

            var prompt = PromptInput.Text?.Trim() ?? "";

            if (hashcodes.Count == 0)
            {
                AddResult("Please enter at least one hash code!");
                return;
            }
            if (string.IsNullOrWhiteSpace(prompt))
            {
                AddResult("Please enter the prompt!");
                return;
            }

            string command = await GenerateCommand(prompt);
            if (string.IsNullOrEmpty(command))
            {
                AddResult("No command generated. Check backend.py");
                return;
            }

            //var manager = new IpHashManager();

            //foreach (var hashcode in hashcodes)
            //{
            //    _ = Task.Run(async () =>
            //    {
            //        try
            //        {
            //            string? ip = manager.GetIpByHashcode(hashcode);
            //            if (string.IsNullOrEmpty(ip))
            //            {
            //                AddResult($"{hashcode} : No IP found.");
            //                return;
            //            }

            //            var rawOutput = await SendCommand(ip, command);
            //            var explained = await ProcessWithAI(prompt, rawOutput);
            //            AddResult($"{hashcode} : {explained}");
            //        }
            //        catch (Exception ex)
            //        {
            //            AddResult($"{hashcode} : Error - {ex.Message}");
            //        }
            //    });
            //}
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
                            return;
                        }

                        var rawOutput = await SendCommand(ip, command);
                        var explained = await ProcessWithAI(prompt, rawOutput);
                        AddResult($"{hashcode} : {explained}");
                    }
                    catch (Exception ex)
                    {
                        AddResult($"{hashcode} : Error - {ex.Message}");
                    }
                });
            });

        }

        private void AddResult(string text)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _results.Add(text);
            });
        }

        private async Task<string> GenerateCommand(string prompt)
        {
            return await RunPython("backend.py", $"\"{prompt}\"", @"E:\College Project\Additions\Nous\Nous\");
        }

        private async Task<string> SendCommand(string ip, string command)
        {
            return await RunPython("client_bridge.py", $"\"{ip}\" \"{command}\"", AppContext.BaseDirectory);
        }

        private async Task<string> ProcessWithAI(string query, string output)
        {
            string input = $"{query}~~{output}";
            return await RunPython("explainer.py", $"\"{input}\"", AppContext.BaseDirectory);
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
