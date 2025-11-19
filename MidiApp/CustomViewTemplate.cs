public class CustomViewPicker : View, GestureDetector.IOnGestureListener
    {
        private bool m_init = false;
        private Context m_context;
        private GestureDetector m_gestureDetector;

        #region ctor
        public CustomNumberPicker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomNumberPicker(Context context) : base(context)
        {
            Initialise(context);
        }

        public CustomNumberPicker(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialise(context);
        }

        public CustomNumberPicker(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialise(context);
        }

        public CustomNumberPicker(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialise(context);
        }
#endregion

        private void Initialise(Context ctx)
        {
            if (!m_init)
            {
                m_context = ctx;
                m_gestureDetector = new GestureDetector(this);

                m_init = true;
            }
        }
		
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(MeasuredWidth, MeasuredHeight);        
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            m_gestureDetector.OnTouchEvent(e);
            return true;
        }

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