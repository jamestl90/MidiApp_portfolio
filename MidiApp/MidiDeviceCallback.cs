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
using Android.Media.Midi;

namespace TestApp
{
    public class MidiDeviceCallback : MidiManager.DeviceCallback
    {
        public Action<MidiDeviceInfo> OnAdded_Device { get; set; }
        public Action<MidiDeviceInfo> OnRemoved_Device { get; set; }
        public Action<MidiDeviceStatus> OnStatusChanged_Device { get; set; }

        public override void OnDeviceAdded(MidiDeviceInfo device)
        {
            base.OnDeviceAdded(device);
            OnAdded_Device.Invoke(device);
        }

        public override void OnDeviceRemoved(MidiDeviceInfo device)
        {
            base.OnDeviceRemoved(device);
            OnRemoved_Device.Invoke(device);
        }

        public override void OnDeviceStatusChanged(MidiDeviceStatus status)
        {
            base.OnDeviceStatusChanged(status);
            OnStatusChanged_Device.Invoke(status);
        }
    }
}