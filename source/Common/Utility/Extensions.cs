using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Trellis.Utility
{
    public static class Extensions
    {
        public static void Stop(this Stopwatch sw, string message = "")
        {
            if(sw.IsRunning)
                sw.Stop();
            
            Console.WriteLine($"[Th:{Thread.CurrentThread.ManagedThreadId}] {message} : {sw.Elapsed.TotalMilliseconds} ms");
            sw.Reset();
        }

        public static string ToIso8601String(this DateTime dt) => dt.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        public static bool TryParseIso8601String(this DateTime dt, string isoString, out DateTime date)
        {
            return DateTime.TryParseExact(
                isoString,   //"2010-08-20T15:00:00Z"
                @"yyyy-MM-dd\THH:mm:ss.fff\Z",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal, 
                out date);
        }
    }
}