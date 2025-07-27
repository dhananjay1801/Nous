using Avalonia;
using Nous;
using System;
using System.Linq;

namespace Nous;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
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
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
