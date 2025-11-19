using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TestApp.Midi;

namespace TestApp
{
    class LaunchControlFragment : Fragment, AdapterView.IOnItemClickListener
    {
        public View FragmentControlView = null;

        public LaunchControlFragment()
        {

        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View inflated = inflater.Inflate(Resource.Layout.launchControlFragment, container, false);
            FragmentControlView = inflated;

            return inflated;
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {

        }
    }
}