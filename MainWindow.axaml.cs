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

            this.Icon = new WindowIcon("E:/Coding/Avalonia/Nous/Nous.ico");

        }

        private async void OnSubmitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string ip = IpInput?.Text?.Trim() ?? string.Empty;
            string prompt = PromptInput?.Text?.Trim() ?? string.Empty;
            string[] ipArray = ip.Split(',');


            if (string.IsNullOrEmpty(ip) || (ipArray.Length == 0))
            {
                OutputBox.Text = "Please enter at least 1 IP!";
                Logger.EWrite("NO VALID IP, terminating program....");
                return;
            }
            if (string.IsNullOrEmpty(prompt))
            {
                OutputBox.Text = "Please enter the prompt!";
                Logger.EWrite("NO VALID prompt, terminating program....");
                return;
            }

            Logger.SWrite($"Working for the IPs: {ip} ; PROMPT: {prompt}");
            // Generate command using backend.py (Stratos logic)
            string command = await GenerateCommand(prompt);

            if (string.IsNullOrEmpty(command))
            {
                OutputBox.Text = "Check backend.py! No command generated";
                Logger.EWrite("Error: No command generated from backend.py");
                return;
            }
            // ----------FOR LOOP, FCFS, NOT GUD FOR MULTIPLE EXECUTIONS------------


            //string finalOutput = "";
            //string result;
            ////for loop here, append multiple IP's output in the result 
            //for (int i = 0; i < ipArray.Length; i++)
            //{
            //    result = await SendCommand(ipArray[i], command);
            //    finalOutput += ipArray[i] + " : " + await ProcessWithAI(prompt, result) + "\n\n";
            //}
            //OutputBox.Text = finalOutput;
            // Multithreaded broadcast and AI processing


            //----------FOR LOOP END, FCFS, NOT GUD FOR MULTIPLE EXECUTIONS------------

            //------------Now using multi threading to process parallely and print at once------------
            //List<Task<string>> tasks = new();

            //foreach (var iterator_ip in ipArray)
            //{
            //    tasks.Add(Task.Run(async () =>
            //    {
            //        try
            //        {
            //            var rawOutput = await SendCommand(iterator_ip, command);
            //            var explained = await ProcessWithAI(prompt, rawOutput);
            //            Logger.SWrite($"Generated for IP :{iterator_ip} --> {explained}");
            //            return $"{iterator_ip} : {explained}";
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.EWrite($"Multi-threading error for {iterator_ip} : {ex.Message}");
            //            return $"{iterator_ip} : Error - {ex.Message}. Check logs.";
            //        }
            //    }));
            //}

            //var results = await Task.WhenAll(tasks);
            //OutputBox.Text = string.Join("\n\n", results);


            //------------Now proper multithreading + parallel output in the outputbox as well------------
      
            void AppendOutput(string text)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Logger.SWrite($"Output generated in the box: {text}");
                    OutputBox.Text += text + "\n\n";
                });
            }

            foreach (var iterator_ip in ipArray)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        //Logger.SWrite($"Sending {iterator_ip.ToString()}");
                        var rawOutput = await SendCommand(iterator_ip, command);
                        var explained = await ProcessWithAI(prompt, rawOutput);
                        Logger.SWrite($"Appending output: {iterator_ip} : {explained}");
                        AppendOutput($"{iterator_ip} : {explained}");
                    }
                    catch (Exception ex)
                    {
                        Logger.EWrite($"ERROR, exception in task handling for the IP ({iterator_ip}): {ex.Message}");
                        AppendOutput($"{iterator_ip} : Error - {ex.Message}");
                    }
                });
            }
            //------------Multithreading closed------------


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
