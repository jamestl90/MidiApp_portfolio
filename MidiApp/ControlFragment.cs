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
using TestApp.Midi;

namespace TestApp
{
    interface ISlidingDrawerActivity
    {
        void ScaleItemClicked(string item);
        void KeyItemClicked(string item);
    }

    public class ControlFragment : Fragment, AdapterView.IOnItemClickListener
    {
        public static View FragmentControlView = null;

        public ControlFragment()
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
            View inflated =  inflater.Inflate(Resource.Layout.fragment_control, container, false);
            FragmentControlView = inflated;
            
            ListView keylist = inflated.FindViewById<ListView>(Resource.Id.keyList);
            keylist.Tag = "Keys";

            var notes = MidiNotes.ReverseNotes(MidiNotes.Notes).Select(x => x.Value).ToList();

            ArrayAdapter adapter1 = new CustomKeyListAdapter(Activity, 0, notes);            

            // Assign adapter to ListView
            keylist.Adapter = adapter1;
            keylist.OnItemClickListener = this;
            
            if (Activity.GetType() == typeof(PianoActivity))
            {
                LinearLayout scaleList = inflated.FindViewById<LinearLayout>(Resource.Id.slidingDrawerScaleList);
                scaleList.Visibility = ViewStates.Gone;

                LinearLayout keyList = inflated.FindViewById<LinearLayout>(Resource.Id.slidingDrawerKeyList);
                keyList.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent,
                                                                         LinearLayout.LayoutParams.WrapContent, 
                                                                         0.1f);
            }
            else
            {
                var scales = Resources.GetStringArray(Resource.Array.scales_array).ToList();

                ListView scaleList = inflated.FindViewById<ListView>(Resource.Id.scaleList);
                scaleList.Tag = "Scales";
                ArrayAdapter adapter2 = new CustomScaleListAdapter(Activity, 0, scales);
                scaleList.OnItemClickListener = this;
                scaleList.Adapter = adapter2;
            }

            return inflated;
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            ListView listView = parent as ListView;

            string item = (string)parent.GetItemAtPosition(position);

            if ((string) listView.Tag == "Scales")
            {
                ((ISlidingDrawerActivity)Activity).ScaleItemClicked(item);
            }
            else if ((string)listView.Tag == "Keys")
            {
                ((ISlidingDrawerActivity)Activity).KeyItemClicked(item);
            }
        }
    }
}


/*
 * 
            var expListView = (ExpandableListView)inflated.FindViewById(Resource.Id.midiSettingsListView);
            var listDataHeader = new List<string>();
            var listDataChild = new Dictionary<string, List<ViewListExpanderAdapter.CustomExpandableViewData>>();

            listDataHeader.Add("Interface");
            listDataHeader.Add("Sound");
            listDataHeader.Add("Key");

            ViewListExpanderAdapter.CustomExpandableViewData data1 = new ViewListExpanderAdapter.CustomExpandableViewData();
            data1.LayoutId = Resource.Layout.interface_settings;

            ViewListExpanderAdapter.CustomExpandableViewData data2 = new ViewListExpanderAdapter.CustomExpandableViewData();
            data2.LayoutId = Resource.Layout.interface_settings;

            ViewListExpanderAdapter.CustomExpandableViewData data3 = new ViewListExpanderAdapter.CustomExpandableViewData();
            data3.LayoutId = Resource.Layout.interface_settings;

            listDataChild.Add(listDataHeader[0], new List<ViewListExpanderAdapter.CustomExpandableViewData> { data1 });
            listDataChild.Add(listDataHeader[1], new List<ViewListExpanderAdapter.CustomExpandableViewData> { data2 });
            listDataChild.Add(listDataHeader[2], new List<ViewListExpanderAdapter.CustomExpandableViewData> { data3 });

            var listAdapter = new ViewListExpanderAdapter(Activity, listDataHeader, listDataChild);
            expListView.SetAdapter(listAdapter);
            expListView.Clickable = true;

    */