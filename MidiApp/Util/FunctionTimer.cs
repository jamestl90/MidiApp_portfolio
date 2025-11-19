using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Diagnostics;

namespace TestApp.Util
{
    public static class FunctionTimer
    {
        private static Stopwatch _stopwatch;
        private static string _functionName;

        public static void StartProfile(string functionName)
        {
            _functionName = functionName;
            _stopwatch = Stopwatch.StartNew();
        }

        public static void StopAndReport()
        {
            _stopwatch.Stop();
            TimeSpan timespan = _stopwatch.Elapsed;
            var output = _functionName + " : " + String.Format("{0:00}:{1:00}:{2:00}\n", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10);
            Console.WriteLine(output);
        }
    }
}