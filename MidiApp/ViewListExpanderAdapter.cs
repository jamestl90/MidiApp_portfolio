using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class ViewListExpanderAdapter : BaseExpandableListAdapter
    {
        public class CustomExpandableViewData : Java.Lang.Object
        {
            public int LayoutId { get; set; }
        }

        private Dictionary<string, List<CustomExpandableViewData>> _listDataChild;
        private List<string> _listDataHeader;
        private Activity _activity;

        public ViewListExpanderAdapter(Activity activity, List<string> listDataHeader, Dictionary<string, List<CustomExpandableViewData>> listChildData)
        {
            _listDataChild = listChildData;
            _listDataHeader = listDataHeader;
            _activity = activity; 
        }

        public override int GroupCount
        {
            get
            {
                return _listDataHeader.Count;
            }
        }

        public override bool HasStableIds
        {
            get
            {
                return false;
            }
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return _listDataChild[_listDataHeader[groupPosition]][childPosition];
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return _listDataChild[_listDataHeader[groupPosition]].Count;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            CustomExpandableViewData child = (CustomExpandableViewData)GetChild(groupPosition, childPosition);

            if (convertView == null)
            {
                LayoutInflater infalInflater = (LayoutInflater)_activity.GetSystemService(Context.LayoutInflaterService);
                convertView = infalInflater.Inflate(child.LayoutId, null);
            }

            return convertView;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return _listDataHeader[groupPosition];
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            string headerTitle = (string)GetGroup(groupPosition);
            if (convertView == null)
            {
                LayoutInflater infalInflater = (LayoutInflater)_activity.GetSystemService(Activity.LayoutInflaterService);
                convertView = infalInflater.Inflate(Resource.Layout.list_group, null);
            }

            TextView lblListHeader = (TextView)convertView.FindViewById(Resource.Id.lblListHeader);
            lblListHeader.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            lblListHeader.Text = headerTitle;

            return convertView;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }
}