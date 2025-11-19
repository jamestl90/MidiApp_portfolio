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
    public class ButtonListAdapter : ArrayAdapter<string>
    {
        public class ButtonClickEventArgs : EventArgs
        {
            public string ButtonText { get; set; }
        }

        public EventHandler<ButtonClickEventArgs> ItemClick;

        private readonly Context m_context;
        private readonly List<string> m_list;

        public ButtonListAdapter(Context context, int textViewResourceId, IList<string> objects)
            : base(context, textViewResourceId, objects)
        {
            m_context = context;
            m_list = objects.ToList();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var name = m_list.ElementAt(position);

            LayoutInflater inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.CustomListItemBlue, null);

            Button button = view.FindViewById<Button>(Resource.Id.button);
            button.Text = name;
            button.Click += (sender, args) =>
            {
                ItemClick?.Invoke(this, new ButtonClickEventArgs { ButtonText = button.Text });
            };

            return view;
        }
    }
}