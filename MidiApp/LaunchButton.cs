using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TestApp.Midi;

namespace TestApp
{
    public class LaunchButtonData
    {
        public byte ControlNum { get; set; }
        public Color Color { get; set; }
        public string Name { get; set; }

        public LaunchButtonData(LaunchButton lb)
        {
            ControlNum = lb.ControlNum;
            Name = lb.Name;
        }

        public LaunchButtonData()
        {
            
        }

        public void Set(LaunchButton lb)
        {
            ControlNum = lb.ControlNum;
            Name = lb.Name;
        }
    }

    public class LaunchButton : Button
    {
        public byte ControlNum { get; set; }
        public Color Color { get; set; }
        public string Name { get; set; }
        public int LoopCount { get; set; }

        public void UpdateWith(LaunchButtonData data)
        {
            ControlNum = data.ControlNum;
            Color = data.Color;
            Name = data.Name;
        }

        public void Activate(IMidiSender sender)
        {
            sender.SendControlChange(MidiHelper.Channel, ControlNum, 1);
        }

        public LaunchButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LaunchButton(Context context) : base(context)
        {
        }

        public LaunchButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public LaunchButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public LaunchButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }
    }
}