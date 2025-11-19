using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Percent;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class TabHeadingVerticalLayout : PercentRelativeLayout
    {
        private Context m_context;

        public View VerticalBar;
        public TextView TextView;

        public View ContentView;

        public void Activate()
        {
            ContentView.Visibility = ViewStates.Visible;
            TextView.SetTextColor(Color.White);
            TextView.Alpha = 1f;
            VerticalBar.SetBackgroundColor(Color.ParseColor("#ff008a"));
            VerticalBar.Alpha = 1f;
        }

        public void Deactivate()
        {
            ContentView.Visibility = ViewStates.Invisible;
            TextView.SetTextColor(Color.ParseColor("#8f9494"));
            TextView.Alpha = 0.2f;
            VerticalBar.SetBackgroundColor(Color.ParseColor("#8f9494"));
            VerticalBar.Alpha = 0.0f;
        }

        public void Init(Context context)
        {
            m_context = context;

            LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.TabHeadingVerticalLayout, this);

            VerticalBar = view.FindViewById<View>(Resource.Id.verticalSeparator);
            TextView = view.FindViewById<TextView>(Resource.Id.tabHeadingText);
        }

        public TabHeadingVerticalLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init(null);
        }

        public TabHeadingVerticalLayout(Context context) : base(context)
        {
            Init(context);
        }

        public TabHeadingVerticalLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public TabHeadingVerticalLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }
    }
}