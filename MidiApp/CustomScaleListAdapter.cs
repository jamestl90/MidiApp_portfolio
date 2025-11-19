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
    public class CustomScaleListAdapter : ArrayAdapter<string>
    {
        private Context m_context;
        private List<string> m_scalesList;

        public CustomScaleListAdapter(Context context, int textViewResourceId, IList<string> objects)
            : base(context, textViewResourceId, objects)
        {
            m_context = context;
            m_scalesList = objects.ToList();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string scale = m_scalesList.ElementAt(position);

            LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.CustomListItemBlue, null);

            Button button = view.FindViewById<Button>(Resource.Id.button);
            button.Text = scale;
            button.Click += (sender, args) =>
            {
                ((ISlidingDrawerActivity)m_context).ScaleItemClicked(scale);
            };

            return view;
        }
    }
}