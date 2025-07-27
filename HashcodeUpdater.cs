using System;
using System.Threading;

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
                    Console.WriteLine("Updating hashcodes...");
                    _manager.UpdateHashcodes();
                    Console.WriteLine($"Update complete. Waiting for {_intervalSeconds / 60} minutes.");
                }
                catch (Exception e)
                {
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