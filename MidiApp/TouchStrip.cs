using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class TouchStrip : ImageView
    {
        private Paint m_paint;        
        private Rect m_stripSegRect;
        private StripSegment m_touchedSegment = null;
        private bool m_lightsGenerated = false;

        public Bitmap TrackerLight { get; set; }
        public int NumLights { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        private int minX = 0;
        private int maxX = 0;
        private int minY = 0;
        private int maxY = 0;

        public int HalfHeight => SizeY/2;

        public class StripSegment
        {
            public Matrix Transform { get; set; }
            public Point HeightRange { get; set; }
            public float Alpha { get; set; }
        }

        private bool m_isInit = false;

        private List<StripSegment> m_segments;
        public List<StripSegment> Segments => m_segments;

        #region ctor
        public TouchStrip(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public TouchStrip(Context context) : base(context)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public TouchStrip(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public TouchStrip(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public TouchStrip(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }
        #endregion

        private void Initialise(Context context)
        {           
            TrackerLight = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.touch_strip_light);

            Bitmap bm = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.vertical_slider_new_bg);
            if (bm != null)
            {
                Background = new BitmapDrawable(bm);
            }
            m_isInit = true;            
        }

        public EventHandler<MotionEvent> StartTrackingTouch { get; set; }
        public EventHandler<MotionEvent> StopTrackingTouch { get; set; }
        public EventHandler<MotionEvent> UpdateTrackingTouch { get; set; }

        private void GenerateLights()
        {
            if (m_lightsGenerated) return;

            // 8 lights in the strip
            NumLights = 8;
            // paint brush
            m_paint = new Paint();
            m_paint.AntiAlias = true;
            m_paint.Dither = true;
            m_paint.FilterBitmap = true;

            m_segments = new List<StripSegment>(NumLights);
            int segHeight = SizeY / NumLights;
            int startHeight = 0;
            m_stripSegRect = new Rect(0, 0, SizeX, segHeight);
            for (int i = 0; i < m_segments.Capacity; ++i)
            {
                Matrix transform = new Matrix();                
                transform.SetTranslate(0, startHeight);
                m_segments.Add(new StripSegment { Alpha = 0f, Transform = transform, HeightRange = new Point(startHeight, startHeight + m_stripSegRect.Height())});
                startHeight += segHeight;
            }
            SetAlphaAllSegments(0.1f);

            DoStartAnimation();

            Invalidate();
            m_lightsGenerated = true;
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            minX = PaddingLeft;
            maxX = Width - PaddingLeft - PaddingRight;
            minY = PaddingTop;
            maxY = Height - PaddingTop - PaddingBottom;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(heightMeasureSpec, widthMeasureSpec);
            SetMeasuredDimension(MeasuredHeight, MeasuredWidth);        
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(h, w, oldh, oldw);
            SizeX = MeasuredWidth;
            SizeY = MeasuredHeight;
            GenerateLights();
        }

        public async void DoStartAnimation()
        {
            Enabled = false;
            int currentDelay = 10;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < m_segments.Count; i++)
                {
                    SetAlphaAllSegments(0.1f);
                    m_segments[i].Alpha = 1.0f;
                    Invalidate();
                    await Task.Delay(currentDelay);
                }
                for (int i = m_segments.Count - 1; i >= 0; i--)
                {
                    SetAlphaAllSegments(0.1f);
                    m_segments[i].Alpha = 1.0f;
                    Invalidate();
                    await Task.Delay(currentDelay);
                }
                currentDelay += 8;
            }
            for (int j = 0; j < 6; j++)
            {
                float alpha = j%2 == 0 ? 0f : 1f;
                SetAlphaAllSegments(alpha);
                await Task.Delay(150);
            }
            SetAlphaAllSegments(0.1f);
            Enabled = true;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {            
            if (!Enabled)
                return false;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    StartTrackingTouch?.Invoke(this, e);
                    Selected = true;
                    Pressed = true;
                    m_touchedSegment = GetTouchedSegment(e);
                    break;
                case MotionEventActions.Move:
                    m_touchedSegment = GetTouchedSegment(e);
                    UpdateTrackingTouch?.Invoke(this, e);
                    break;
                case MotionEventActions.Up:
                    StopTrackingTouch?.Invoke(this, e);
                    Selected = false;
                    Pressed = false;
                    m_touchedSegment = null;
                    break;
                case MotionEventActions.Cancel:
                    Selected = false;
                    Pressed = false;
                    m_touchedSegment = null;
                    break;
            }

            if (m_touchedSegment != null)
            {
                SetAlphaAllSegments(0.1f);
                m_touchedSegment.Alpha = 1.0f;
                LightEitherSideOfTouched();
                Invalidate();
            }
            else
            {
                SetAlphaAllSegments(0.1f);
            }

            return true;
        }

        public void LightEitherSideOfTouched()
        {
            for (int i = 0; i < m_segments.Count; ++i)
            {
                if (m_segments[i].Alpha == m_touchedSegment.Alpha)
                {
                    if (i != 0)
                    {
                        m_segments[i - 1].Alpha = 0.5f;
                    }
                    if (i != m_segments.Count - 1)
                    {
                        m_segments[i + 1].Alpha = 0.5f;
                    }
                }
            }
        }

        private StripSegment GetTouchedSegment(MotionEvent e)
        {
            float touchHeight = e.GetY();
            foreach (var seg in m_segments)
            {
                int segLower = seg.HeightRange.X;
                int segUpper = seg.HeightRange.Y;

                if (touchHeight > segLower && touchHeight < segUpper)
                {
                    return seg;
                }
            }
            return null;
        }

        public void SetAlphaAllSegments(float alpha)
        {
            m_segments.ForEach(x => x.Alpha = alpha);
            Invalidate();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (m_segments?.Count > 0)
            {
                foreach (var seg in m_segments)
                {
                    var transform = seg.Transform;
                    canvas.Save();
                    canvas.Concat(transform);
                    m_paint.Alpha = (int)(255 * seg.Alpha);
                    canvas.DrawBitmap(TrackerLight, null, m_stripSegRect, m_paint);
                    canvas.Restore();
                }                
            }
        }       
    }
}