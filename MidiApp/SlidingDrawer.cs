using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.Animation;
using Java.Lang;

namespace TestApp
{
    public class SlidingDrawer : FrameLayout
    {
        internal class AnimationListener : Java.Lang.Object, Animator.IAnimatorListener
        {
            private readonly Action m_action;

            public AnimationListener(Action onEndAction)
            {
                m_action = onEndAction;
            }

            public void OnAnimationCancel(Animator animation)
            {
            }

            public void OnAnimationEnd(Animator animation)
            {
                m_action.Invoke();
            }

            public void OnAnimationRepeat(Animator animation)
            {
            }

            public void OnAnimationStart(Animator animation)
            {
            }
        }

        public interface IOnStateChangedListener
        {
            void StateChanged(SlideState newState);
        }

        public enum Stick
        {
            ToBottom = 1,
            ToLeft = 2,
            ToRight = 3,
            ToTop = 4
        }

        public enum SlideState
        {
            Open = 1,
            Closed = 2
        }

        public enum ScrollState
        {
            Vertical = 1,
            Horizontal = 2
        }

        public static readonly string TAG = "SlidingDrawer";

        private static readonly int MAX_CLICK_DURATION = 1000;
        private static readonly int MAX_CLICK_DISTANCE = 5;

        private static readonly int DEFAULT_SLIDING_LAYER_OFFSET = 200;
        private static readonly int TRANSLATION_ANIM_DURATION = 300;
        private static readonly SlideState DEFAULT_SLIDE_STATE = SlideState.Closed;

        private float m_initialCoordinate;
        private int m_touchSlop;

        private int m_delta;
        private int m_lastCoordinate;
        private long m_pressStartTime;
        private bool m_isAnimating = false;

        private int m_offsetDistance;
        private Stick m_stickTo;
        private ScrollState m_scrollOrientation;
        private SlideState m_slideState = DEFAULT_SLIDE_STATE;

        private IOnStateChangedListener m_onStateChangedListener;

        private bool m_init;

        #region ctor

        public SlidingDrawer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { 
        }

        public SlidingDrawer(Context context) : base(context)
        {
        }

        public SlidingDrawer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public SlidingDrawer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(MeasuredHeight, MeasuredWidth);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            var vars = context.Theme.ObtainStyledAttributes(attrs,
                Resource.Styleable.SlidingLayer,
                0, 0);

            try
            {
                m_stickTo = (Stick)vars.GetInteger(Resource.Styleable.SlidingLayer_stickTo, (int)Stick.ToBottom);
                m_offsetDistance = vars.GetDimensionPixelSize(Resource.Styleable.SlidingLayer_offsetDistance,
                    DEFAULT_SLIDING_LAYER_OFFSET);
            }
            finally
            {
                vars.Recycle();
            }

            m_touchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;

            m_init = true;

            switch (m_stickTo)
            {
                case Stick.ToBottom:
                case Stick.ToTop:
                    m_scrollOrientation = ScrollState.Vertical;
                    break;
                case Stick.ToRight:
                case Stick.ToLeft:
                    m_scrollOrientation = ScrollState.Horizontal;
                    break;
            }
        }

        #endregion

        #region Helpers
        public static bool IsClicked(Context context, float diff, long pressDuration)
        {
            return pressDuration < MAX_CLICK_DURATION &&
                    Distance(context, diff) < MAX_CLICK_DISTANCE;
        }

        private static float Distance(Context context, float diff)
        {
            float distanceInPx = (float)System.Math.Sqrt(diff * diff);
            return PxToDp(context, distanceInPx);
        }

        public static float PxToDp(Context context, float px)
        {
            return px / context.Resources.DisplayMetrics.Density;
        }

        public static int GetLocationInYAxis(View v)
        {
            int[] globalPos = new int[2];
            v.GetLocationInWindow(globalPos);
            return globalPos[1];
        }

        public static int GetLocationInXAxis(View v)
        {
            int[] globalPos = new int[2];
            v.GetLocationInWindow(globalPos);
            return globalPos[0];
        }
        #endregion

        public void SetOnStateChangedListener(IOnStateChangedListener listener)
        {
            m_onStateChangedListener = listener;
        }

        public void ToggleDrawer()
        {
            if (!m_isAnimating)
            {
                var newState = IsOpened() ? SlideState.Closed : SlideState.Open;
                NotifyActionAndAnimateForState(newState, GetLength() - m_offsetDistance, true);
                m_slideState = newState;
            }            
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {           
            base.OnLayout(changed, left, top, right, bottom);

            if (m_init)
            {
                Post(() =>
                {
                    NotifyActionForState(m_slideState, false);
                });
                m_init = false;
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    switch (m_stickTo)
                    {
                        case Stick.ToBottom:
                        case Stick.ToTop:
                            m_initialCoordinate = ev.GetY();
                            break;
                        case Stick.ToLeft:
                        case Stick.ToRight:
                            m_initialCoordinate = ev.GetX();
                            break;
                    }
                    break;
                case MotionEventActions.Move:
                    float coordinate = 0;
                    switch (m_stickTo)
                    {
                        case Stick.ToBottom:
                        case Stick.ToTop:
                            coordinate = ev.GetY();
                            break;
                        case Stick.ToLeft:
                        case Stick.ToRight:
                            coordinate = ev.GetX();
                            break;
                    }
                    int diff = (int)System.Math.Abs(coordinate - m_initialCoordinate);

                    if (diff > m_touchSlop)
                    {
                        // drag captured
                        return true;
                    }
                    break;
            }

            return base.OnInterceptTouchEvent(ev);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            //var frag = ControlFragment.FragmentControlView;
            //this.LayoutParameters = new RelativeLayout.LayoutParams(300, ViewGroup.LayoutParams.MatchParent);

            View parent = (View) Parent;
            int coordinate;
            int distance = GetDistance();
            int tapCoordinate;
            int heightPixels = Context.Resources.DisplayMetrics.HeightPixels;            

            switch (m_stickTo)
            {
                case Stick.ToBottom:
                    coordinate = (int) e.RawY;
                    tapCoordinate = (int) e.RawY;
                break;

                case Stick.ToLeft:
                    coordinate = parent.Width - (int) e.RawX;
                    tapCoordinate = (int) e.RawX;
                break;

                case Stick.ToRight:
                    coordinate = (int) e.RawX;
                    tapCoordinate = (int) e.RawX;
                break;

                case Stick.ToTop: 
                    coordinate = heightPixels - (int) e.RawY;
                    tapCoordinate = (int) e.RawY;
                break;
                default:
                    throw new IllegalStateException("Failed to initialize coordinates.");
            }

            var layoutParams = (RelativeLayout.LayoutParams) LayoutParameters;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    switch (m_stickTo)
                    {
                        case Stick.ToBottom:
                            m_delta = coordinate - layoutParams.TopMargin;
                            break;
                        case Stick.ToLeft:
                            m_delta = coordinate - layoutParams.RightMargin;
                            break;
                        case Stick.ToRight:
                            m_delta = coordinate - layoutParams.LeftMargin;
                            break;
                        case Stick.ToTop:
                            m_delta = coordinate - layoutParams.BottomMargin;
                            break;                       
                    }
                    m_lastCoordinate = coordinate;
                    m_pressStartTime = JavaSystem.CurrentTimeMillis();
                    break;
                case MotionEventActions.Move:

                    int farMargin = coordinate - m_delta;
                    int closeMargin = distance - farMargin;

                    switch (m_stickTo)
                    {
                        case Stick.ToBottom:
                            if (farMargin > distance &&
                                closeMargin > m_offsetDistance - Height)
                            {
                                layoutParams.BottomMargin = closeMargin;
                                layoutParams.TopMargin = farMargin;
                            }
                            break;
                        case Stick.ToLeft:
                            if (farMargin > distance &&
                                closeMargin > m_offsetDistance - Width)
                            {
                                layoutParams.LeftMargin = closeMargin;
                                layoutParams.RightMargin = farMargin;
                            }
                            break;
                        case Stick.ToRight:
                            if (farMargin > distance &&
                                closeMargin > m_offsetDistance - Width)
                            {
                                layoutParams.RightMargin = closeMargin;
                                layoutParams.LeftMargin = farMargin;
                            }
                            break;
                        case Stick.ToTop:
                            if (farMargin > distance &&
                                closeMargin > m_offsetDistance - Height)
                            {
                                layoutParams.BottomMargin = farMargin;
                                layoutParams.TopMargin = closeMargin;
                            }
                            break;
                    }
                    break;
                case MotionEventActions.Up:

                    int diff = coordinate - m_lastCoordinate;
                    long pressDuration = JavaSystem.CurrentTimeMillis() - m_pressStartTime;

                    switch (m_stickTo)
                    {
                        case Stick.ToBottom:
                            if (IsClicked(Context, diff, pressDuration))
                            {
                                if (tapCoordinate > parent.Height - m_offsetDistance &&
                                    m_slideState == SlideState.Closed)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Open, Height - m_offsetDistance, true);
                                }
                                else if (System.Math.Abs(heightPixels - tapCoordinate - Height) < 
                                    m_offsetDistance && m_slideState == SlideState.Open)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Closed, Height - m_offsetDistance, true);
                                }
                            }
                            else
                            {
                                SmoothScrollToAndNotify(diff);
                            }

                            break;
                        case Stick.ToLeft:
                            if (IsClicked(Context, diff, pressDuration))
                            {
                                if (tapCoordinate <= m_offsetDistance && m_slideState == SlideState.Closed)
                                {
                                    //var frag = ControlFragment.FragmentControlView;
                                    
                                    NotifyActionAndAnimateForState(SlideState.Open, Width - m_offsetDistance, true);
                                }
                                else if (tapCoordinate > Width - m_offsetDistance &&
                                         m_slideState == SlideState.Open)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Closed, Width - m_offsetDistance, true);
                                }
                            }
                            else
                            {
                                SmoothScrollToAndNotify(diff);
                            }

                            break;
                        case Stick.ToRight:
                            if (IsClicked(Context, diff, pressDuration))
                            {
                                if (parent.Width - tapCoordinate <= m_offsetDistance &&
                                    m_slideState == SlideState.Closed)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Open, Width - m_offsetDistance, true);
                                }
                                else if (parent.Width - tapCoordinate > Width - m_offsetDistance &&
                                         m_slideState == SlideState.Open)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Closed, Width - m_offsetDistance, true);
                                }
                            }
                            else
                            {
                                SmoothScrollToAndNotify(diff);
                            }

                            break;
                        case Stick.ToTop:
                            if (IsClicked(Context, diff, pressDuration))
                            {
                                int y = GetLocationInYAxis(this);
                                if (tapCoordinate - System.Math.Abs(y) <= m_offsetDistance &&
                                    m_slideState == SlideState.Closed)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Open, Height - m_offsetDistance, true);
                                }
                                else if (Height - (tapCoordinate - System.Math.Abs(y)) <
                                         m_offsetDistance && m_slideState == SlideState.Open)
                                {
                                    NotifyActionAndAnimateForState(SlideState.Closed, Height - m_offsetDistance, true);
                                }
                            }
                            else
                            {
                                SmoothScrollToAndNotify(diff);
                            }

                            break;
                    }
                    break;
            }
            return true;

        }

        private void NotifyActionAndAnimateForState(SlideState stateToApply, int translation, bool notify)
        {
            Action animationEnd1 = () =>
            {
                m_isAnimating = false;
                NotifyActionForState(stateToApply, notify);
                TranslationY = 0;           
            };
            Action animationEnd2 = () =>
            {
                m_isAnimating = false;
                NotifyActionForState(stateToApply, notify);
                TranslationX = 0;
            };
            Animator.IAnimatorListener listener1 = new AnimationListener(animationEnd1);
            Animator.IAnimatorListener listener2 = new AnimationListener(animationEnd2);

            m_isAnimating = true;
            m_slideState = stateToApply;

            switch (m_stickTo)
            {
                case Stick.ToBottom:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            Animate()
                            .TranslationY(-translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener1);
                            break;
                        case SlideState.Closed:
                            Animate()
                            .TranslationY(translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener1);
                            break;
                    }
                    break;

                case Stick.ToTop:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            Animate()
                            .TranslationY(translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener1);
                            break;
                        case SlideState.Closed:
                            Animate()
                            .TranslationY(-translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener1);
                            break;
                    }
                    break;

                case Stick.ToLeft:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            Animate()
                            .TranslationX(translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener2);
                            break;
                        case SlideState.Closed:
                            Animate()
                            .TranslationX(-translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener2);
                            break;
                    }
                    break;

                case Stick.ToRight:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            Animate()
                            .TranslationX(-translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener2);
                            break;
                        case SlideState.Closed:
                            Animate()
                            .TranslationX(translation)
                            .SetDuration(TRANSLATION_ANIM_DURATION)
                            .SetInterpolator(new DecelerateInterpolator())
                            .SetListener(listener2);
                            break;
                    }
                    break;
            }
        }

        private void NotifyActionForState(SlideState stateToApply, bool notify)
        {
            int distance = GetDistance(); 
            RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)LayoutParameters;

            switch (m_stickTo)
            {
                case Stick.ToBottom:

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            layoutParams.BottomMargin = 0;
                            layoutParams.TopMargin = distance;
                            break;
                        case SlideState.Closed:
                            layoutParams.BottomMargin = m_offsetDistance - Height;
                            layoutParams.TopMargin = distance - (m_offsetDistance - Height);
                            break;
                    }
                    break;

                case Stick.ToLeft:

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            layoutParams.LeftMargin = 0;
                            layoutParams.RightMargin = distance;
                            break;
                        case SlideState.Closed:
                            layoutParams.LeftMargin = m_offsetDistance - Width;
                            layoutParams.RightMargin = distance - (m_offsetDistance - Width);
                            break;
                    }
                    break;

                case Stick.ToRight:

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            layoutParams.RightMargin = 0;
                            layoutParams.LeftMargin = distance;
                            break;
                        case SlideState.Closed:
                            layoutParams.RightMargin = m_offsetDistance - Width;
                            layoutParams.LeftMargin = distance - (m_offsetDistance - Width);
                            break;
                    }
                    break;

                case Stick.ToTop:

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            layoutParams.TopMargin = 0;
                            layoutParams.BottomMargin = distance;
                            break;
                        case SlideState.Closed:
                            layoutParams.TopMargin = m_offsetDistance - Height;
                            layoutParams.BottomMargin = distance - (m_offsetDistance - Height);
                            break;
                    }
                    break;
            }
            if (notify)
            {
                m_onStateChangedListener?.StateChanged(stateToApply);
            }

            LayoutParameters = layoutParams;
        }

        private int GetDistance()
        {
            View parent = (View)Parent;

            switch (m_scrollOrientation)
            {
                case ScrollState.Vertical:
                    return parent.Height -
                            parent.PaddingTop -
                            parent.PaddingBottom -
                            Height;
                case ScrollState.Horizontal:
                    return parent.Width -
                            parent.PaddingLeft -
                            parent.PaddingRight -
                            Width;
            }
            throw new IllegalStateException("Scroll orientation is not initialized.");
        }

        private int GetLength()
        {
            switch (m_scrollOrientation)
            {
                case ScrollState.Vertical:
                    return Height;
                case ScrollState.Horizontal:
                    return Width;
            }
            throw new IllegalStateException("Scroll orientation is not initialized.");
        }

        private void SmoothScrollToAndNotify(int diff)
        {
            int length = GetLength();

            SlideState stateToApply;
            if (diff > 0)
            {
                if (diff > length/2.5f)
                {
                    stateToApply = SlideState.Closed;
                    NotifyActionAndAnimateForState(stateToApply, GetTranslationFor(stateToApply), true);
                }
                else if (m_slideState == SlideState.Open)
                {
                    stateToApply = SlideState.Open;
                    NotifyActionAndAnimateForState(stateToApply, GetTranslationFor(stateToApply), false);
                }
            }
            else
            {
                if (System.Math.Abs(diff) > length/2.5f)
                {
                    stateToApply = SlideState.Open;
                    NotifyActionAndAnimateForState(stateToApply, GetTranslationFor(stateToApply), true);
                }
                else if (m_slideState == SlideState.Closed)
                {
                    stateToApply = SlideState.Closed;
                    NotifyActionAndAnimateForState(stateToApply, GetTranslationFor(stateToApply), false);
                }
            }
        }

        private int GetTranslationFor(SlideState stateToApply)
        {
            int heightPixels = Context.Resources.DisplayMetrics.HeightPixels;
            int widthPixels = Context.Resources.DisplayMetrics.WidthPixels;

            switch (m_stickTo)
            {
                case Stick.ToBottom:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            return Height - (heightPixels - GetLocationInYAxis(this));                            
                        case SlideState.Closed:
                            return heightPixels - GetLocationInYAxis(this) - m_offsetDistance;                      
                    }
                    break;
                case Stick.ToLeft:
                    int x = GetLocationInXAxis(this) + Width;

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            return Width - x;                            
                        case SlideState.Closed:
                            return x - m_offsetDistance;                            
                    }
                    break;
                case Stick.ToRight:
                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            return Width - (widthPixels - GetLocationInXAxis(this));                            
                        case SlideState.Closed:
                            return widthPixels - GetLocationInXAxis(this) - m_offsetDistance;
                    }
                    break;
                case Stick.ToTop:
                    int actionBarDiff = heightPixels - ((View) Parent).Height;
                    int y = GetLocationInYAxis(this) + Height;

                    switch (stateToApply)
                    {
                        case SlideState.Open:
                            return Height - y + actionBarDiff;
                        case SlideState.Closed:
                            return y - m_offsetDistance - actionBarDiff;
                    }
                    break;
            }
            throw new IllegalStateException("Failed to return translation for drawer.");
        }

        public bool IsOpened()
        {
            return m_slideState == SlideState.Open;
        }

        public bool IsClosed()
        {
            return m_slideState == SlideState.Closed;
        }
    }
}