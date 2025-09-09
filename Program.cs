using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Nous;
using System;
using System.Linq;
using Nous.Utils;
using System.IO;
namespace Nous
{
    class Program
    {
        //private static Logger _logger = new Logger(@"E:\College Project\Min_Nous\Nous\Log.txt");

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Load environment variables from .env at startup (no external deps)
                try { LoadEnv(); } catch { }
                Logger.SWrite("Application Started");

                if (args.Length > 0)
                {
                    var manager = new IpHashManager();
                    if (args[0] == "insert" && args.Length >= 2)
                    {
                        if (args.Length == 2)
                            manager.InsertIp(args[1]);
                        else
                            manager.InsertIps(args.Skip(1));
                    }
                    else if (args[0] == "update")
                    {
                        manager.UpdateHashcodes();
                    }
                    else if (args[0] == "autoupdate")
                    {
                        var updater = new HashcodeUpdater(manager);
                        updater.Start();
                    }
                    else
                    {
                        Console.WriteLine("Usage: Nous insert <ip1> [<ip2> ...] | update | autoupdate");
                    }
                }
                else
                {
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }

                Logger.SWrite("Application Closed Normally");
            }
            catch (Exception ex)
            {
                Logger.EWrite($"Application Crashed with Exception: {ex.Message}\n{ex.StackTrace}");
                throw; // rethrow after logging
            }
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .AfterSetup(_ =>
                {
                    // Hook lifetime events
                    if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Exit += (_, __) => Logger.SWrite("Application Exit Event Triggered");
                    }
                });
        }

        private static void LoadEnv()
        {
            string? envFile = FindEnvFile();
            if (envFile == null) return;

            foreach (var rawLine in File.ReadAllLines(envFile))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var key = line.Substring(0, eq).Trim();
                var value = line.Substring(eq + 1).Trim();

                // Remove optional surrounding quotes
                if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (key.Length == 0) continue;
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        private static string? FindEnvFile()
        {
            // Search upward from the executable directory for a .env file
            string? dir = AppContext.BaseDirectory;
            for (int i = 0; i < 5 && dir != null; i++)
            {
                var candidate = Path.Combine(dir, ".env");
                if (File.Exists(candidate)) return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return null;
        }
    }
}
