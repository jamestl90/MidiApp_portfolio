using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class BluetoothAdvertiseCallback : AdvertiseCallback
    {
        public class StartFailureEventArgs : EventArgs
        {
            public AdvertiseFailure FailureCode { get; set; }
        }

        public class StartSuccessEventArgs : EventArgs
        {
            public AdvertiseSettings Settings { get; set; }
        }

        public EventHandler<StartFailureEventArgs> StartFailureEvent;
        public EventHandler<StartSuccessEventArgs> StartSuccessEvent;

        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);

            StartFailureEvent?.Invoke(this, new StartFailureEventArgs
            {
                FailureCode = errorCode
            });
        }

        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);

            StartSuccessEvent?.Invoke(this, new StartSuccessEventArgs
            {
                Settings = settingsInEffect
            });
        }
    }
}