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

namespace TestApp
{
    public class CheckListDataItem : Java.Lang.Object
    {
        public string Text { get; set; }
        public bool Checked { get; set; }
    }

    public class CustomCheckListAdapter : ArrayAdapter<CheckListDataItem>
    {
        public class ViewHolder : Java.Lang.Object
        {
            public TextView TextView { get; set; }
            public CheckBox CheckBox { get; set; }
        }

        private Context m_context;
        private List<CheckListDataItem> m_items;

        public List<CheckListDataItem> GetItems()
        {
            return m_items;
        }

        public CustomCheckListAdapter(Context context, int textViewResourceId, IList<CheckListDataItem> objects)
            : base(context, textViewResourceId, objects)
        {
            m_context = context;
            m_items = objects.ToList();
        }

        public void SetChecked(int position, bool val)
        {
            m_items[position].Checked = val;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder = null;

            CheckListDataItem item = m_items.ElementAt(position);
            View view = convertView;

            if (view != null)
            {
                holder = view.Tag as ViewHolder;
            }

            if (holder == null)
            {
                holder = new ViewHolder();
                LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
                view = inflater.Inflate(Resource.Layout.CustomCheckListItem, null);
                holder.CheckBox = view.FindViewById<CheckBox>(Resource.Id.checkbox);
                holder.TextView = view.FindViewById<TextView>(Resource.Id.text);
                view.Tag = holder;
            }

            holder.CheckBox.Checked = item.Checked;
            holder.TextView.Text = item.Text;

            return view;
        }
    }
}