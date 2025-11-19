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
    public class XyControllerGridView : View, GestureDetector.IOnGestureListener
    {
        private bool m_init = false;
        private Context m_context;

        private bool loaded = false;

        private Bitmap m_tracker;
        private Bitmap m_background;

        private float _x;
        private float _y;

        private float _lastX;
        private float _lastY;

        private GestureDetector m_gestureDetector;

        public XyControllerControl GridParent { get; set; }

        private Paint m_linePaint;

        #region ctor
        public XyControllerGridView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public XyControllerGridView(Context context) : base(context)
        {
            Init(context);
        }

        public XyControllerGridView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public XyControllerGridView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public XyControllerGridView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }
#endregion

        private void Init(Context context)
        {
            if (!m_init)
            {
                m_context = context;
                m_gestureDetector = new GestureDetector(this);

                var tracker = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.finger_tracker);

                float size = (int)Resources.GetDimension(Resource.Dimension.finger_tracker_size);
                int scaleWidth = (int)((size / (float)tracker.Width) * (float)tracker.Width);
                int scaleHeight = (int)((size / (float)tracker.Height) * (float)tracker.Height);

                m_tracker = Bitmap.CreateScaledBitmap(tracker, scaleWidth, scaleHeight, false);
                tracker.Recycle();

                m_init = true;

                m_linePaint = new Paint();
                m_linePaint.Color = new Color(242, 15, 83, 255);
                m_linePaint.StrokeWidth = 5;
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            BitmapFactory.Options options = new BitmapFactory.Options();

            //options.InScaled = false;
            if (!loaded)
            {
                var background = BitmapFactory.DecodeResource(m_context.Resources, Resource.Drawable.xycontroller_fg,
                    options);
                int scaleWidth = (int) ((Width/(float) background.Width)*(float) background.Width);
                int scaleHeight = (int) ((Height/(float) background.Height)*(float) background.Height);
                m_background = Bitmap.CreateScaledBitmap(background, scaleWidth, scaleHeight, false);
                background.Recycle();

                _x = Width/2f;
                _y = Height/2f;
                Invalidate();
                loaded = true;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            Rect dest = new Rect(0, 0, Width, Height);
            Paint paint = new Paint();
            paint.FilterBitmap = true;           
            canvas.DrawBitmap(m_background, null, dest, paint);

            canvas.Save();
            Rect dest2 = new Rect(0, 0, m_tracker.Width, m_tracker.Height);
            canvas.Translate(_x, _y);
            canvas.DrawBitmap(m_tracker, null, dest2, paint);
            canvas.Restore();

            PointF vertTop = new PointF(_x + (m_tracker.Width * 0.5f), 0);
            PointF vertBot = new PointF(_x + (m_tracker.Width * 0.5f), Height);
            PointF horLeft = new PointF(0, _y + (m_tracker.Width * 0.5f));
            PointF horRight = new PointF(Width, _y + (m_tracker.Width * 0.5f));

            canvas.DrawLine(vertTop.X, vertTop.Y, vertTop.X, _y, m_linePaint);
            canvas.DrawLine(vertBot.X, vertBot.Y, vertBot.X, _y + (m_tracker.Width), m_linePaint);

            canvas.DrawLine(horLeft.X, horLeft.Y, _x, horLeft.Y, m_linePaint);
            canvas.DrawLine(horRight.X, horRight.Y, _x + (m_tracker.Width), horRight.Y, m_linePaint);
        }

        public void DoUpdate(MotionEvent e)
        {
            float halfWidth = m_tracker.Width / 2f;
            float halfHeight = m_tracker.Height / 2f;

            float x = e.GetX();
            float y = e.GetY();

            bool needInvalidate = false;

            if (x > 0 && x < Width)
            {
                _x = e.GetX() - halfWidth;
                needInvalidate = true;
            }
            else
            {
                if (x < 0)
                {
                    _x = 0 - halfWidth;
                }
                if (x > Width)
                {
                    _x = Width - halfWidth;
                }
            }
            if (y > 0 && y < Height)
            {
                _y = e.GetY() - halfHeight;
                needInvalidate = true;
            }
            else
            {
                if (y < 0)
                {
                    _y = 0 - halfHeight;
                }
                if (y > Height)
                {
                    _y = Height - halfHeight;
                }
            }
            if (needInvalidate)
            {
                Invalidate();
                GridParent.XyValueChanged(new XyControllerControl.XyControllerValueChangedEventArgs
                {
                    LastX = _lastX,
                    LastY = _lastY,
                    X = _x,
                    Y = _y,
                    XPercent = CalcPercent(_x, halfWidth, Width, false),
                    YPercent = CalcPercent(_y, halfHeight, Height, true)
                });
            }

            _lastX = _x;
            _lastY = _y;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            m_gestureDetector.OnTouchEvent(e);
            return true;
        }

        private float CalcPercent(float val, float halfSize, float dimension, bool flip)
        {
            float pc = (val + halfSize)/dimension;
            if (pc <= 0.01f)
            {
                pc = 0f;
            }
            if (pc >= 0.99f)
            {
                pc = 1.0f;
            }
            if (flip)
                return (float) Math.Round(1.0f - pc, 2);
            else
            {
                return (float) Math.Round(pc, 2);
            }
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

            MidiControlsHelper.ChannelPickerXYPopup(m_context, this, 0, 0, px + (Width/4), py + (Height/4), GravityFlags.NoGravity, GridParent, GridParent.ControllerNumber1, GridParent.ControllerNumber2);
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            DoUpdate(e2);
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
            
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            DoUpdate(e);
            return false;
        }
    }
}