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
using Android.Widget;

namespace TestApp
{
    public class SlidingDrawerFragment : Fragment, SlidingDrawer.IOnStateChangedListener
    {
        public static readonly string TAG = "SlidingDrawerFragment";

        public EventHandler<EventArgs> OnViewCreatedFinished;

        private ImageView m_imageView;
        private SlidingDrawer m_slidingDrawer;

        public static SlidingDrawerFragment New(string fragmentType)
        {
            SlidingDrawerFragment fragment = new SlidingDrawerFragment();
            Bundle args = new Bundle();
            args.PutString("fragmentType", fragmentType);
            fragment.Arguments = args;
            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Bundle args = Arguments;
            int resource = 0;
            switch (args.GetString("fragmentType"))
            {
                case "KeysScales":
                    resource = Resource.Layout.fragment_sliding_drawer_left;
                    break;
                case "KeysScalesWithSaveLoad":
                    resource = Resource.Layout.FragmentSaveLoad_left;
                    break;
                case "LaunchScreenSliderMenu":
                    resource = Resource.Layout.FragmentSaveLoadClear_left;
                    break;
            }
            var view = inflater.Inflate(resource, container, false);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            m_imageView = (ImageView) view.FindViewById(Resource.Id.slidingImage);
            m_slidingDrawer = (SlidingDrawer) view.FindViewById(Resource.Id.slidingDrawer);

            m_slidingDrawer.SetOnStateChangedListener(this);

            OnViewCreatedFinished?.Invoke(this, new EventArgs());
        }

        public SlidingDrawer GetSlidingDrawer()
        {
            return m_slidingDrawer;
        }

        public void StateChanged(SlidingDrawer.SlideState newState)
        {
            switch (newState)
            {
                case SlidingDrawer.SlideState.Open:
                    m_imageView.SetImageResource(Resource.Drawable.leftArrow);
                    break;
                case SlidingDrawer.SlideState.Closed:
                    m_imageView.SetImageResource(Resource.Drawable.rightArrow);
                    break;
            }
        }
    }
}