using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;

namespace TestApp.Wifi
{
    public class WifiHelpers
    {
        public static string LocalIpToString(Context ctx)
        {
            WifiManager wm = (WifiManager)ctx.GetSystemService(Context.WifiService);
            return Formatter.FormatIpAddress(wm.ConnectionInfo.IpAddress);
        }

        public static bool IsWifiConnected(Context ctx)
        {
            ConnectivityManager connManager = (ConnectivityManager)ctx.GetSystemService(Context.ConnectivityService);
            NetworkInfo mWifi = connManager.GetNetworkInfo(ConnectivityType.Wifi);
            return mWifi.IsConnected;
        }

        public static bool IsWifiEnabled(Context ctx)
        {
            WifiManager wm = (WifiManager)ctx.GetSystemService(Context.WifiService);
            return wm.IsWifiEnabled;
        }
    }
}