using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Icu.Util;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TestApp.Midi;
using TestApp.Util;

namespace TestApp
{
    public interface ControlInfo
    {
         byte ControllerNumber { get; set; }
    }

    public class RotaryKnobControl : RelativeLayout, ControlInfo
    {
        public EventHandler<OnRotationValueChangedEventArgs> OnRotationValueChanged;

        public class OnRotationValueChangedEventArgs : EventArgs
        {
            public float Angle { get; set; }
            public PointF Point { get; set; }
            public double Percentage { get; set; }
        }

        private double m_percent;

        private bool m_isInit = false;

        private float m_angle;

        private static int MIN_CLICK_DURATION = 500;
        private bool longPress = false;
        private long startClickTime;

        private float lastX;
        private float lastY;

        private Context m_context;

        private ImageView m_image;

        private PointF m_point;

        public RotaryKnobControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public RotaryKnobControl(Context context) : base(context)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public RotaryKnobControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public RotaryKnobControl(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public RotaryKnobControl(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec); 
            SetMeasuredDimension(MeasuredWidth, MeasuredHeight);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(h, w, oldh, oldw);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            if (!changed) return;


            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InScaled = true;

            float size = (int)Resources.GetDimension(Resource.Dimension.rotary_knob_size);

            var bitmap = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.knob_bg_new2, options);
            var bg = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.knob_new2, options);

            int scaleWidth = (int)((size / (float)bg.Width) * (float)bg.Width);
            int scaleHeight = (int)((size / (float)bg.Height) * (float)bg.Height);

            Bitmap backgroundBitmap = Bitmap.CreateScaledBitmap(bg,
                Width,
                Height, false);

            scaleWidth = (int)((size / (float)bitmap.Width) * (float)bitmap.Width);
            scaleHeight = (int)((size / (float)bitmap.Height) * (float)bitmap.Height);

            Bitmap scaledBm = Bitmap.CreateScaledBitmap(bitmap,
                Width,
                Height, false);

            m_image = new ImageView(m_context);
            //m_image.SetScaleType(ImageView.ScaleType.FitCenter);
            m_image.SetImageBitmap(scaledBm);
            m_image.Background = new BitmapDrawable(backgroundBitmap);
            bitmap.Recycle();
            bg.Recycle();

            var relLayout = new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            relLayout.AddRule(LayoutRules.CenterInParent);

            AddView(m_image, relLayout);
        }

        private void Initialise(Context context)
        {
            m_context = context;
            //m_gestureDetector = new GestureDetector(this);

            

            m_isInit = true;
        }

        public void DoRotate(float deg)
        {
            if (deg > -150 && deg < 150)
            {
                Matrix matrix = new Matrix();
                m_image.SetScaleType(ImageView.ScaleType.Matrix);
                matrix.PostRotate((float)deg, Width / 2.0f,
                                              Height / 2.0f);
                m_image.ImageMatrix = matrix;
                m_angle = deg;

                m_percent = getPercentage();

                OnRotationValueChanged?.Invoke(this, new OnRotationValueChangedEventArgs
                {
                    Angle = m_angle,
                    Point = m_point,
                    Percentage = m_percent
                });
            }                      
        }

        private double getPercentage()
        {
            double pc = (m_angle + 150.0)/300.0;

            if (pc < 0.01)
            {
                pc = 0.0;
            }
            if (pc > 0.995)
            {
                pc = 1.0f;
            }

            return pc;
        }

        public double GetPercentage()
        {
            return m_percent;
        }

        private float CartesianToPolar(float x, float y)
        {
            // (180.0 / Math.Pi) used to convert radians to degrees
            return (float)(Math.Atan2(x, y) * (180.0 / Math.PI));
        }

        private PointF GetAdjustedCoordinates(MotionEvent me)
        {
            float x = me.GetX() / (float)Width;
            float y = me.GetY() / (float)Height;
            x = x - 0.5f; // make it useable for atan2
            y = (1 - y) - 0.5f; // flip y coordinate and make it usable for atan2
            return new PointF(x, y);
        }

        public void OnLongPress(MotionEvent e)
        {
            // are there any notes that are playing currently
            if (MidiHelper.NoteStatus.Any(x => x.Value == NoteState.NoteOn))
                return;

            int px = 0;
            int py = 0;

            int[] loc = new int[2];
            this.GetLocationOnScreen(loc);

            px = loc[0];
            py = loc[1];

            MidiControlsHelper.ChannelPickerPopup(m_context, this, 0, 0, px, py - Height, GravityFlags.NoGravity, this, ControllerNumber);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            //Console.WriteLine(e.ActionIndex + "   " + e.PointerCount);

            //m_gestureDetector.OnTouchEvent(e);
            
            var angle = 0.0f;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (longPress == false)
                    {
                        longPress = true;
                        startClickTime = Calendar.Instance.TimeInMillis;
                        lastX = e.GetX();
                        lastY = e.GetY();
                    }
                    return true; 
                case MotionEventActions.Up:
                    longPress = false;

                    m_point = GetAdjustedCoordinates(e);
                    angle = CartesianToPolar(m_point.X, m_point.Y);

                    DoRotate(angle);
                    return false;
                case MotionEventActions.Move:

                    if (longPress == true)
                    {
                        if (MyUtils.IsScrolling((Activity)m_context, lastX, lastY, e.GetX(), e.GetY(), 0.02f))
                        {
                            //Console.WriteLine("SCROLLING");
                            longPress = false;
                        }
                        else
                        {
                            long clickDuration = Calendar.Instance.TimeInMillis - startClickTime;
                            if (clickDuration >= MIN_CLICK_DURATION)
                            {
                                OnLongPress(e);
                                longPress = false;
                            }
                        }
                    }
                    else
                    {
                        m_point = GetAdjustedCoordinates(e);
                        angle = CartesianToPolar(m_point.X, m_point.Y);

                        DoRotate(angle);
                    }
                    return false;
            }
            return false;
        }

        public byte ControllerNumber { get; set; }
    }
}