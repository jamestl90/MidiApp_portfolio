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
using Android.Icu.Util;
using Javax.Microedition.Khronos.Egl;
using TestApp.Midi;
using TestApp.Util;

namespace TestApp
{
    public class FaderControl : View, ControlInfo
    {
        public EventHandler<FaderValueChangedEventArgs> OnValueChanged;

        public class FaderValueChangedEventArgs : EventArgs
        {
            public float Value { get; set; }
        }

        private static int MIN_CLICK_DURATION = 500;
        private bool longPress = false;
        private long startClickTime;
        private float lastX;
        private float lastY;

        private GestureDetector m_gestureDetector;

        private bool m_init = false;
        private Context m_context;
        private Bitmap m_background;
        private Bitmap m_foreground;
        private Bitmap m_overlay;
        private Bitmap m_thumb;

        private int _thumbHalfHeight;

        private float _x;
        private float _y;

        public int Max { get; set; }
        public int Min { get; set; }

        private int minX = 0;
        private int maxX = 0;
        private int minY = 0;
        private int maxY = 0;

        private float m_value;

        public float Value
        {
            get
            {
                return 1-m_value;
            }
            set
            {
                m_value = value;
                _y = minY + ((1 - m_value) * maxY);
                Invalidate();
            }
        }

        #region ctor
        public FaderControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public FaderControl(Context context) : base(context)
        {
            Init(context);
        }

        public FaderControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public FaderControl(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public FaderControl(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }
#endregion

        private void Init(Context context)
        {
            if (!m_init)
            {
                m_context = context;
                //m_gestureDetector = new GestureDetector(this);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InScaled = false;
                var bitmap = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.verticalSliderBackground, options);
                int width = (int)Resources.GetDimension(Resource.Dimension.fader_width);
                int height = (int)Resources.GetDimension(Resource.Dimension.fader_height);
                //int scaleWidth = (int)((width / (float)bitmap.Width) * (float)bitmap.Width);
                //int scaleHeight = (int)((height / (float)bitmap.Height) * (float)bitmap.Height);
                var scaled = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
                bitmap.Recycle();
                bitmap = null;

                m_background = scaled;

                bitmap = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.verticalSliderProgress, options);              
                var scaledProgressVar = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
                bitmap.Recycle();
                bitmap = null;

                m_foreground = scaledProgressVar;

                bitmap = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.vertical_slider_new_bg, options);
                var overlay = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
                bitmap.Recycle();
                bitmap = null;

                m_overlay = overlay;

                bitmap = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.vertical_slider_new_thumb, options);
                height = (int)Resources.GetDimension(Resource.Dimension.fader_thumb_height);
                //scaleWidth = (int)((width / (float)bitmap.Width) * (float)bitmap.Width);
                _thumbHalfHeight = (int)(height*0.5f);
                var scaledThumb = Bitmap.CreateScaledBitmap(bitmap, width, (int)height, true);
                bitmap.Recycle();
                bitmap = null;

                m_thumb = scaledThumb;                


                m_init = true;
                //Invalidate();
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            if (!changed) return;

            base.OnLayout(changed, left, top, right, bottom);
            minX = PaddingLeft;
            maxX = Width - PaddingLeft - PaddingRight;
            minY = PaddingTop;
            maxY = Height;

            _y = minY + ((1 - m_value) * maxY);
            Invalidate();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {        
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                
            int desiredWidth = (int)Resources.GetDimension(Resource.Dimension.fader_width); 
            int desiredHeight = (int)Resources.GetDimension(Resource.Dimension.fader_height);

            MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            int heightSize = MeasureSpec.GetSize(heightMeasureSpec);

            int width;
            int height;

            //Measure Width
            if (widthMode == MeasureSpecMode.Exactly)
            {
                //Must be this size
                width = widthSize;
            }
            else if (widthMode == MeasureSpecMode.AtMost)
            {
                //Can't be bigger than...
                width = Math.Min(desiredWidth, widthSize);
            }
            else
            {
                //Be whatever you want
                width = desiredWidth;
            }

            //Measure Height
            if (heightMode == MeasureSpecMode.Exactly)
            {
                //Must be this size
                height = heightSize;
            }
            else if (heightMode == MeasureSpecMode.AtMost)
            {
                //Can't be bigger than...
                height = Math.Min(desiredHeight, heightSize);
            }
            else
            {
                //Be whatever you want
                height = desiredHeight;
            }

            //SetMeasuredDimension(width, height);
        }

        protected override void OnDraw(Canvas canvas)
        {
            //canvas.ClipRect(minX, minY, maxX, maxY, Region.Op.Replace);

            base.OnDraw(canvas);            

            Rect dest = new Rect(minX, minY, maxX, minY + maxY);
            Paint paint = new Paint();
            paint.AntiAlias = true;
            paint.Dither = true;
            paint.FilterBitmap = true;

            //canvas.DrawBitmap(m_background, null, dest, paint);

            Rect progressRect = new Rect(minX, maxY, maxX, (int)_y);
            //paint.Alpha = 255;
            canvas.DrawBitmap(m_foreground, null, progressRect, paint);

            paint.Alpha = 255;
            canvas.DrawBitmap(m_overlay, null, dest, paint);

            Rect thumbRect = new Rect(minX, (int)_y - _thumbHalfHeight, maxX, (int)_y + _thumbHalfHeight);
            canvas.DrawBitmap(m_thumb, null, thumbRect, paint);
        }

        private void ProcessUpdate()
        {
            if (_y > ((maxY - _thumbHalfHeight)))
                _y = (maxY - _thumbHalfHeight);
            if (_y < minY + _thumbHalfHeight)
                _y = minY + _thumbHalfHeight;

            m_value = (_y - _thumbHalfHeight) / (maxY);

            if (m_value > 0.93f) m_value = 1.0f;
            if (m_value < 0.00f) m_value = 0.0f;

            Invalidate();

            //Console.WriteLine(Value);

            OnValueChanged?.Invoke(this, new FaderValueChangedEventArgs
            {
                Value = Value
            });
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            //Console.WriteLine(e.ActionIndex + "   " + e.PointerCount);

            //m_gestureDetector.OnTouchEvent(e);

            switch (e.Action)
            {           
                case MotionEventActions.Down:
                    if (longPress == false)
                    {
                        //Console.WriteLine("TOUCH DOWN");
                        longPress = true;                        
                        startClickTime = Calendar.Instance.TimeInMillis;
                        lastX = e.GetX();
                        lastY = e.GetY();
                    }
                    return true;              
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
                        _x = e.GetX();
                        _y = e.GetY();
                        ProcessUpdate();
                    }

                    return false;
                case MotionEventActions.Up:

                    //Console.WriteLine("TOUCH UP");

                    longPress = false;
                    _x = e.GetX();
                    _y = e.GetY();
                    ProcessUpdate();

                    return false;
            }
            return false;
        }

        public byte ControllerNumber { get; set; }

        public void OnLongPress(MotionEvent e)
        {
            if (MidiHelper.NoteStatus.Any(x => x.Value == NoteState.NoteOn))
                return;

            int px = 0;
            int py = 0;

            int[] loc = new int[2];
            this.GetLocationOnScreen(loc);

            px = loc[0];
            py = loc[1];

            MidiControlsHelper.ChannelPickerPopup(m_context, this, 0, 0, px, py + (Height/2), GravityFlags.NoGravity, this, ControllerNumber);
        }
    }
}