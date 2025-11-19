using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public static class Monitor
    {
        public static int LastTotalElapsedTime = 0;

        public static void RunAndPrintElapsedTime(string name, Action action)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            action();
            sw.Stop();
            Console.WriteLine(name + " - " + sw.Elapsed.Milliseconds);
            LastTotalElapsedTime += sw.Elapsed.Milliseconds;
        }
    }
}