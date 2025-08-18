using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Nous;
using System;
using System.Linq;
using Nous.Utils;
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
    }
}
