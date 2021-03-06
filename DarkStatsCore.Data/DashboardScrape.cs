using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DarkStatsCore.Data.Models;

namespace DarkStatsCore.Data
{
    public static class DashboardScrape
    {
        public static bool IsTaskActive => _scrapeTask == null ? false : true;
        public static EventHandler<DashboardEventArgs> DataGathered;
        private static List<HostDelta> _dashboardDeltas = new List<HostDelta>();
        private static bool _updateEvent;
        private static DateTime _lastGathered = DateTime.Now;
        private static Task _scrapeTask;

        public static void StartDashboardScrapeTask(string url, TimeSpan refreshTime, CancellationToken cancellationToken)
        {
            if (!IsTaskActive)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " - Starting dashboard scrape...");
                _updateEvent = false;
                _scrapeTask = ExecuteScrapeTask(url, refreshTime, cancellationToken);
            }
        }

        private async static Task ExecuteScrapeTask(string url, TimeSpan refreshTime, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Scrape(url);
                await Task.Delay(refreshTime);
            }
            _updateEvent = false;
            _dashboardDeltas = new List<HostDelta>();
            _scrapeTask = null;
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " - Dashboard scrape stopped.");
        }       

        private static void Scrape(string url)
        {
            try
            {
                var traffic = Scraper.ScrapeData(url);
                traffic.AdjustDeltas(_dashboardDeltas);
                var elapsedMs = DateTime.Now.Subtract(_lastGathered).TotalMilliseconds;
                _lastGathered = DateTime.Now;
                if (_updateEvent)
                {
                    var deltas = _dashboardDeltas.Where(h => h.LastCheckDeltaBytes > 0)
                        .OrderByDescending(h => h.LastCheckDeltaBytes)
                        .ToList();
                    DataGathered?.Invoke(null, new DashboardEventArgs(deltas, elapsedMs));
                }
                else
                {
                    _updateEvent = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error on Dashboard scrape: " + e.InnerException != null ? e.InnerException.Message : e.Message);
            }
        }
    }
}