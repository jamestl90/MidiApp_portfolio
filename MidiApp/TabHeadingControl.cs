using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Percent;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace TestApp
{    
    public class TabHeadingControl : PercentRelativeLayout
    {
        private Context m_context;

        public View HorizontalBar;
        public TextView TextView;

        public View ContentView;

        public void Activate(View viewToHide)
        {
            viewToHide?.Animate()
                .TranslationY(viewToHide.Height)
                .Alpha(0.0f)
                .SetDuration(300)
                .SetListener(new AnimationListener(animator1 =>
                {
                    viewToHide.Animate().SetListener(null).ScaleX(1);

                    viewToHide.Visibility = ViewStates.Invisible;
                    ContentView.Visibility = ViewStates.Visible;

                    if (ContentView.TranslationY == 0)
                    {
                        ContentView.TranslationY = ContentView.Height;
                    }

                    ContentView.Animate()
                        .TranslationY(0)
                        .Alpha(1.0f)
                        .SetDuration(300);

                }, AnimationListener.AnimationType.End));

            TextView.SetTextColor(Color.White);
            TextView.Alpha = 1f;
            HorizontalBar.SetBackgroundColor(Color.ParseColor("#e2dcdf"));
            HorizontalBar.Alpha = 1f;
        }

        public void Deactivate()
        {
            //ContentView.Visibility = ViewStates.Invisible;
            TextView.SetTextColor(Color.ParseColor("#555555"));
            TextView.Alpha = 1f;
            HorizontalBar.SetBackgroundColor(Color.ParseColor("#8f9494"));
            HorizontalBar.Alpha = 0.0f;
        }

        public void Init(Context context)
        {
            m_context = context;

            LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.TabHeadingLayout, this);

            HorizontalBar = view.FindViewById<View>(Resource.Id.horizontalSeparator);
            TextView = view.FindViewById<TextView>(Resource.Id.tabHeadingText);
        }

        public TabHeadingControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init(null);
        }

        public TabHeadingControl(Context context) : base(context)
        {
            Init(context);
        }

        public TabHeadingControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public TabHeadingControl(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }
    }
}