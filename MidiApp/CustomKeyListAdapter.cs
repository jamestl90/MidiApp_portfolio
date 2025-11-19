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
    public class CustomKeyListAdapter : ArrayAdapter<string>
    {
        private Context m_context;
        private List<string> m_keysList;

        public CustomKeyListAdapter(Context context, int textViewResourceId, IList<string> objects)
            : base(context, textViewResourceId, objects)
        {
            m_context = context;
            m_keysList = objects.ToList();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string key = m_keysList.ElementAt(position);

            LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.CustomListItemBlue, null);

            Button button = view.FindViewById<Button>(Resource.Id.button);
            button.Text = key;
            button.Click += (sender, args) =>
            {
                ((ISlidingDrawerActivity)m_context).KeyItemClicked(key);
            };

            return view;
        }
    }
}