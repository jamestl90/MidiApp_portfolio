using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp
{
	public class UpdatableListAdapter : ArrayAdapter<UpdatableListAdapter.WifiMidiListItem>
	{
	    private Context m_context;

	    public class WifiMidiListItem
	    {
	        public string Name;
	        public string Status;
	        public bool Established;
	        public bool IsSelected;
	        public string Host;
	        public uint Ssrc;
	        public int Position;
	    }

	    public class ViewHolder : Java.Lang.Object
	    {
	        public TextView NameTextView;
	        public TextView AddrTextView;
	        public TextView StatusTextView;
	    }

	    public UpdatableListAdapter(Context context, int resId, WifiMidiListItem[] items)
            : base(context, resId, items)
	    {
	        m_context = context;
	    }

	    public UpdatableListAdapter(Context context, int textViewResourceId, IList<WifiMidiListItem> objects) : base(context, textViewResourceId, objects)
	    {
	        m_context = context;
	    }

	    public override View GetView(int position, View convertView, ViewGroup parent)
	    {
            ViewHolder holder = null;

	        WifiMidiListItem item = GetItem(position);
	        item.Position = position;
            View view = convertView;

            if (view != null)
            {
                holder = view.Tag as ViewHolder;
            }

            if (holder == null)
            {
                holder = new ViewHolder();
                LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
                view = inflater.Inflate(Resource.Layout.UpdatableListItem, parent, false);
                holder.NameTextView = view.FindViewById<TextView>(Resource.Id.bonjourName);
                holder.AddrTextView = view.FindViewById<TextView>(Resource.Id.hostAddress);
                holder.StatusTextView = view.FindViewById<TextView>(Resource.Id.status);
                view.Tag = holder;
            }

	        holder.NameTextView.Text = item.Name;
	        holder.AddrTextView.Text = item.Host;
	        holder.StatusTextView.Text = item.Status;

	        if (item.IsSelected)
	        {
	            view.SetBackgroundColor(Color.ParseColor("#2cffa9d8"));
	        }
	        else
	        {
	            view.SetBackgroundColor(Color.ParseColor("#00000000"));
	        }

            return view;
        }
	}
}