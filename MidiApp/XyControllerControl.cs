using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using TestApp.Midi;
using TestApp.Util;

namespace TestApp
{
    public interface MultiControlsInfo
    {
        byte ControllerNumber1 { get; set; }
        byte ControllerNumber2 { get; set; }
    }

    public class XyControllerControl : FrameLayout, MultiControlsInfo, GestureDetector.IOnGestureListener
    {        
        private Context m_context;

        //private ImageView m_fingerTracker;

        public EventHandler<XyControllerValueChangedEventArgs> OnValueChanged;

        private XyControllerGridView m_xyControllerGrid;
        private LinearLayout m_containerLayout;

        private TextView m_xValText;
        private TextView m_yValText;

        private GestureDetector m_gestureDetector;

        public class XyControllerValueChangedEventArgs : EventArgs
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float LastX { get; set; }
            public float LastY { get; set; }
            public float XPercent { get; set; }
            public float YPercent { get; set; }
        }

        public XyControllerControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            //Init(null);
        }

        public XyControllerControl(Context context) : base(context)
        {
            Init(context);
        }

        public XyControllerControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public XyControllerControl(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public XyControllerControl(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            m_context = context;
            m_gestureDetector = new GestureDetector(this);            

            Background = m_context.GetDrawable(Resource.Drawable.xycontroller_bg);                        
            //SetBackgroundColor(Color.Red);

            var inflater = (LayoutInflater) m_context.GetSystemService(Context.LayoutInflaterService);
            var view = inflater.Inflate(Resource.Layout.XyControllerLayout, null);

            m_containerLayout = (LinearLayout)view;
            m_xyControllerGrid = view.FindViewById<XyControllerGridView>(Resource.Id.xyControllerGridView);
            m_xyControllerGrid.GridParent = this;

            m_xValText = view.FindViewById<TextView>(Resource.Id.xValText);
            m_yValText = view.FindViewById<TextView>(Resource.Id.yValText);

            Button bindX = view.FindViewById<Button>(Resource.Id.bindXButton);
            Button bindY = view.FindViewById<Button>(Resource.Id.bindYButton);

            bindX.Click += (sender, args) =>
            {
                IMidiSender midisender = MainActivity.MidiSender;
                midisender.SendControlChange(MidiHelper.Channel, ControllerNumber1, (byte)127/2);
                //MainActivity.InputPort.Flush();

            };
            bindY.Click += (sender, args) =>
            {
                IMidiSender midisender = MainActivity.MidiSender;
                midisender.SendControlChange(MidiHelper.Channel, ControllerNumber2, (byte)127 / 2);
                //MainActivity.InputPort.Flush();
            };

            AddView(view);            
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);       
        }

        public void XyValueChanged(XyControllerValueChangedEventArgs args)
        {
            OnValueChanged?.Invoke(this, args);

            m_xValText.Text = "" + args.XPercent * 100.0f;
            m_yValText.Text = "" + args.YPercent * 100.0f;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            m_gestureDetector.OnTouchEvent(e);
            return true;
        }

        public byte ControllerNumber1 { get; set; }
        public byte ControllerNumber2 { get; set; }

        public bool OnDown(MotionEvent e)
        {
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
            int px = 0;
            int py = 0;

            int[] loc = new int[2];
            this.GetLocationOnScreen(loc);

            px = loc[0];
            py = loc[1];

            MidiControlsHelper.ChannelPickerXYPopup(m_context, this, 0, 0, px + (Width / 4), py + (Height / 4), GravityFlags.NoGravity, this, ControllerNumber1, ControllerNumber2);
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
             
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
    }
}