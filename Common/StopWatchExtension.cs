using System;
using System.Diagnostics;
using System.Threading;

namespace Program
{
    public static class StopWatchExtension
    {
        public static void Stop(this Stopwatch sw, string message = "")
        {
            if(sw.IsRunning)
                sw.Stop();
            
            Console.WriteLine($"[Th:{Thread.CurrentThread.ManagedThreadId}] {message} : {sw.Elapsed.TotalMilliseconds} ms");
            sw.Reset();
        }

        public static string ToIsoString(this DateTime dt) => dt.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
    }
}