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
    public class CustomDialogCheckListBox : Dialog, AdapterView.IOnItemClickListener
    {
        public EventHandler<CheckedItemsEventArgs> PositiveButton;
        public EventHandler<EventArgs> NegativeButton;

        public class CheckedItemsEventArgs
        {
            public List<CheckListDataItem> CheckedItems { get; set; }
        }

        private readonly List<CheckListDataItem> m_items;
        private readonly Context m_context;

        public CustomDialogCheckListBox(Context context, IList<CheckListDataItem> items) : base(context)
        {
            m_items = items.ToList();
            m_context = context;
        }        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature((int)WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.ConfirmDialogListLayout);

            ListView listView = FindViewById<ListView>(Resource.Id.settingsList);
            Button ok = FindViewById<Button>(Resource.Id.okButton);
            Button cancel = FindViewById<Button>(Resource.Id.cancelButton);

            SetCanceledOnTouchOutside(true);

            ok.Click += (sender, args) =>
            {
                PositiveButton?.Invoke(sender, new CheckedItemsEventArgs
                {
                    CheckedItems = ((CustomCheckListAdapter)listView.Adapter).GetItems()
                });
                Dismiss();
            };
            cancel.Click += (sender, args) =>
            {
                NegativeButton?.Invoke(sender, args);
                Dismiss();
            };

            ArrayAdapter adapter1 = new CustomCheckListAdapter(m_context, 0, m_items);
            listView.Adapter = adapter1;
            listView.OnItemClickListener = this;
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            CustomCheckListAdapter.ViewHolder holder = view.Tag as CustomCheckListAdapter.ViewHolder;
            ListView listView = FindViewById<ListView>(Resource.Id.settingsList);
            var obj = listView.Adapter.GetItem(position) as CheckListDataItem;
            
            holder.CheckBox.Checked = !holder.CheckBox.Checked;
            obj.Checked = holder.CheckBox.Checked;
        }
    }
}