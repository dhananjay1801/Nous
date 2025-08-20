using Avalonia.Logging;
using System;
using System.Threading;
using Nous.Utils;

using RLogger = Nous.Utils.Logger;

namespace Nous
{
    public class HashcodeUpdater
    {
        private readonly IpHashManager _manager;
        private readonly int _intervalSeconds;
        private bool _running = false;

        public HashcodeUpdater(IpHashManager manager, int intervalSeconds = 3600)
        {
            _manager = manager;
            _intervalSeconds = intervalSeconds;
        }

        public void Start()
        {
            _running = true;
            while (_running)
            {
                try
                {
                    //Console.WriteLine("Updating hashcodes...");
                    RLogger.SWrite("UPDATING HASHCDODES");
                   
                    _manager.UpdateHashcodes();
                    //Console.WriteLine($"Update complete. Waiting for {_intervalSeconds / 60} minutes.");
                    RLogger.SWrite("Update complete. Waiting for next");
                }
                catch (Exception e)
                {
                    RLogger.EWrite($"Error during updating: {e.Message}");
                    Console.WriteLine($"Error during update: {e.Message}");
                }
                Thread.Sleep(_intervalSeconds * 1000);
            }
        }

        public void Stop()
        {
            _running = false;
        }
    }
} 