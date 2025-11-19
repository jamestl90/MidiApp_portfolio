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

namespace TestApp.Midi
{
    public class MidiDeviceInfoWrapper 
    {
        public MidiConnectionManager.MidiDeviceListItem Info { get; set; }

        public MidiDeviceInfoWrapper()
        {
            Info = null;
        }

        public MidiDeviceInfoWrapper(MidiConnectionManager.MidiDeviceListItem info)
        {
            Info = info;
        }

        public override string ToString()
        {
            if (Info == null)
                return "No midi devices found.";

            string type = Info.Type == MidiConnectionManager.ConnectionType.BluetoothLe ? "BluetoothLE" : Info.Type == MidiConnectionManager.ConnectionType.Usb ? "USB" : "Virtual";
            return Info.Name + " (" + type + ")";
        }
    }
}