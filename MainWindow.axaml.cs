using System;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Globalization;
using System.Collections.Generic;
using Avalonia.Threading;


namespace Nous
{
    public partial class MainWindow : Window
    {




        public MainWindow()
        {
            Logger.SWrite("---------- PROGRAM STARTED ----------");
            InitializeComponent();

            this.Icon = new WindowIcon("D:/Project stuff/Nous/Nous.ico");

        }

        private async void OnSubmitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string hashcodes = IpInput?.Text?.Trim() ?? string.Empty;
            string prompt = PromptInput?.Text?.Trim() ?? string.Empty;
            string[] hashcodeArray = hashcodes.Split(',');

            if (string.IsNullOrEmpty(hashcodes) || (hashcodeArray.Length == 0))
            {
                OutputBox.Text = "Please enter at least 1 hashcode!";
                Logger.EWrite("NO VALID HASHCODE, terminating program....");
                return;
            }
            if (string.IsNullOrEmpty(prompt))
            {
                OutputBox.Text = "Please enter the prompt!";
                Logger.EWrite("NO VALID prompt, terminating program....");
                return;
            }

            Logger.SWrite($"Working for the Hashcodes: {hashcodes} ; PROMPT: {prompt}");
            string command = await GenerateCommand(prompt);

            if (string.IsNullOrEmpty(command))
            {
                OutputBox.Text = "Check backend.py! No command generated";
                Logger.EWrite("Error: No command generated from backend.py");
                return;
            }

            var manager = new IpHashManager();

            void AppendOutput(string text)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Logger.SWrite($"Output generated in the box: {text}");
                    OutputBox.Text += text + "\n\n";
                });
            }

            foreach (var hashcode in hashcodeArray)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string? ip = manager.GetIpByHashcode(hashcode.Trim());
                        if (string.IsNullOrEmpty(ip))
                        {
                            AppendOutput($"{hashcode} : Error - No IP found for this hashcode");
                            return;
                        }
                        var rawOutput = await SendCommand(ip, command);
                        var explained = await ProcessWithAI(prompt, rawOutput);
                        Logger.SWrite($"Appending output: {hashcode} : {explained}");
                        AppendOutput($"{hashcode} : {explained}");
                    }
                    catch (Exception ex)
                    {
                        Logger.EWrite($"ERROR, exception in task handling for the hashcode ({hashcode}): {ex.Message}");
                        AppendOutput($"{hashcode} : Error - {ex.Message}");
                    }
                });
            }
        }

        private async Task<string> GenerateCommand(string prompt)
        {
            Logger.SWrite($"Command Generation initiated for: {prompt}");

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
                {
                    Logger.EWrite($"PROGRAM TERMINATED. Python error: {error}");
                    return $"[Python error] {error}";
                }
                Logger.SWrite($"Command Generated: {output.Trim()}");
                return output.Trim();
            }
            catch (Exception ex)
            {
                Logger.EWrite($"PROGRAM TERMINATED. Exception recieved: {ex.Message}");
                return $"[Exception] {ex.Message}";
            }
            //return "whoami";
        }

        //no encrpytion send command for c# listener
        //private async Task<string> SendCommand(string ip, string command)
        //{
        //    try
        //    {
        //        using var client = new System.Net.Sockets.TcpClient();
        //        await client.ConnectAsync(ip, 8080);

        //        using var stream = client.GetStream();
        //        byte[] bytesToSend = Encoding.UTF8.GetBytes(command);
        //        await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

        //        byte[] buffer = new byte[4096];
        //        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        //        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //        return response.Trim();
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Error sending command: {ex.Message}";
        //    }
        //}


        //encrypted sendcommand for c# listener, use this in future versions to remove use of client bridge in python

        //private async Task<string> SendCommand(string ip, string command)
        //{
        //    try
        //    {
        //        using var client = new TcpClient();
        //        await client.ConnectAsync(ip, 8080);

        //        using var sslStream = new SslStream(client.GetStream(), false,
        //            (sender, cert, chain, errors) => true); // DEV ONLY: trust any cert

        //        await sslStream.AuthenticateAsClientAsync("nous.local"); // <- CN from your cert

        //        byte[] bytesToSend = Encoding.UTF8.GetBytes(command);
        //        await sslStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

        //        byte[] buffer = new byte[4096];
        //        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
        //        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        //        return response.Trim();
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Error sending command: {ex.Message}";
        //    }
        //}

        private async Task<string> SendCommand(string ip, string command)
        {
            Logger.SWrite($"Forwading the command: {command} TO: {ip}");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"client_bridge.py \"{ip}\" \"{command}\"",
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
                {
                    Logger.EWrite($"PROGRAM TERMINATED. Python error: {error}");
                    return $"[Python error] {error}";
                }
                Logger.SWrite($"Recieved output from {ip}: {output.Trim()}");
                return output.Trim();
            }
            catch (Exception ex)
            {
                Logger.EWrite($"PROGRAM TERMINATED. Exception recieved: {ex.Message}");
                return $"[Exception] {ex.Message}";
            }
        }

        

        private async Task<string> ProcessWithAI(string query,string output)
        {

            output = query.Trim()+ "~~" + output;
            Logger.SWrite("Processing the output with AI:" + output);
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
                {
                    Logger.EWrite($"PROGRAM TERMINATED. Python error: {error}");
                    return $"[Python error] {error}";
                }
                Logger.SWrite($"Sucessfully processed with AI:{output.Trim()}");
                return output.Trim();
            }

            catch (Exception ex)
            {
                Logger.EWrite($"PROGRAM TERMINATED. Exception recieved: {ex.Message}");
                return $"[Exception] {ex.Message}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Logger.EWrite("---------- PROGRAM TERMINATED ----------");
        }


    }
    //now logger class YAS
    public static class Logger
    {
        public static string Path = @"E:\Coding\Avalonia\Nous\Log.txt";

        public static void SWrite(string message)
        {
            var culture = new CultureInfo("en-GB");
            try
            {
                if (!File.Exists(Path))
                    File.Create(Path).Dispose();

                using (StreamWriter sw = File.AppendText(Path))
                {
                    sw.WriteLine($"[+] [{DateTime.Now.ToString(culture)}] {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logger Error: {ex.Message}");
            }
        }
        public static void EWrite(string message)
        {
            var culture = new CultureInfo("en-GB");
            try
            {
                if (!File.Exists(Path))
                    File.Create(Path).Dispose();

                using (StreamWriter sw = File.AppendText(Path))
                {
                    sw.WriteLine($"[-] [{DateTime.Now.ToString(culture)}] {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logger Error: {ex.Message}");
            }
        }
    }




}
