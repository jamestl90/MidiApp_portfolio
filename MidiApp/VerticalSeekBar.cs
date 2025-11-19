using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gestures;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public delegate void VerticalSeekBarStartTrackingTouchEventHandler(object sender, SeekBar.StartTrackingTouchEventArgs args);
    public delegate void VerticalSeekBarStopTrackingTouchEventHandler(object sender, SeekBar.StopTrackingTouchEventArgs args);

    /// <summary>
    /// Loosely based on implementation from http://stackoverflow.com/questions/4892179/how-can-i-get-a-working-vertical-seekbar-in-android
    /// </summary>
    public class VerticalSeekBar : SeekBar, GestureDetector.IOnGestureListener, ControlInfo
    {
        private GestureDetector m_gestureDetector;

        private Context m_context;

        #region ctor

        protected VerticalSeekBar(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Init(null);
        }

        public VerticalSeekBar(Context context)
            : base(context)
        {
            Init(context);
        }

        public VerticalSeekBar(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init(context);
        }

        public VerticalSeekBar(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            m_gestureDetector = new GestureDetector(this);
            m_gestureDetector.IsLongpressEnabled = true;
            m_context = context;
        }

        #endregion

        #region fields

        private int m_min;

        #endregion

        #region properties

        public int Min
        {
            get { return m_min; }
            set
            {
                if (Min > Progress)
                    Progress = Min;
                m_min = value;
                OnSizeChanged(Width, Height, 0, 0);
            }
        }

        public override int Progress
        {
            get
            {
                return base.Progress <= Min ? Min : base.Progress;
            }
            set
            {
                if (value <= Min)
                    base.Progress = Min;
                else if (value >= Max)
                    base.Progress = Max;
                else
                    base.Progress = value;

                OnSizeChanged(Width, Height, 0, 0);
            }
        }

        #endregion

        #region events

        public new event VerticalSeekBarStartTrackingTouchEventHandler StartTrackingTouch;
        public new event VerticalSeekBarStopTrackingTouchEventHandler StopTrackingTouch;

        #endregion

        public override void Draw(Android.Graphics.Canvas canvas)
        {
            canvas.Rotate(-90);
            canvas.Translate(-Height, 0);
            base.OnDraw(canvas);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(heightMeasureSpec, widthMeasureSpec);
            SetMeasuredDimension(MeasuredHeight, MeasuredWidth);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(h, w, oldh, oldw);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            m_gestureDetector.OnTouchEvent(e);

            if (!Enabled)
                return false;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    StartTrackingTouch?.Invoke(this, new StartTrackingTouchEventArgs(this));
                    Selected = true;
                    Pressed = true;
                    Progress = Max - (int)(Max * e.GetY() / Height);
                    break;
                case MotionEventActions.Move:
                    Progress = Max - (int)(Max * e.GetY() / Height);
                    break;
                case MotionEventActions.Up:
                    StopTrackingTouch?.Invoke(this, new StopTrackingTouchEventArgs(this));
                    Selected = false;
                    Pressed = false;
                    Progress = Max - (int)(Max * e.GetY() / Height);
                    break;
                case MotionEventActions.Cancel:
                    Selected = false;
                    Pressed = false;
                    break;
            }

            return true;
        }

        public byte ControllerNumber { get; set; }
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
            //MidiControlsHelper.ChannelPickerPopup(m_context, this, 0, 0, this, ControllerNumber);
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