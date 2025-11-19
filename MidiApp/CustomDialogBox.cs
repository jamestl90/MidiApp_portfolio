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
    public class CustomDialogBox : Dialog, View.IOnClickListener
    {
        public EventHandler<EventArgs> PositiveButton;
        public EventHandler<EventArgs> NegativeButton;

        private bool _okOnly = false;
        private string _title = "";
        private string _body = "";

        public CustomDialogBox(Context context) : base(context)
        {
            _okOnly = false;
        }

        public CustomDialogBox(Context context, bool okOnly, string title, string body) : base(context)
        {
            _okOnly = okOnly;
            _title = title;
            _body = body;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature((int) WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.ConfirmDialogLayout);

            if (_title != "" && _body != "")
            {
                TextView title = FindViewById<TextView>(Resource.Id.title);
                TextView msg = FindViewById<TextView>(Resource.Id.Message);

                title.Text = _title;
                msg.Text = _body;
            }

            Button ok = FindViewById<Button>(Resource.Id.okButton);
            Button cancel = FindViewById<Button>(Resource.Id.cancelButton);

            if (_okOnly)
            {
                cancel.Visibility = ViewStates.Gone;
            }

            ok.Click += (sender, args) =>
            {
                if (PositiveButton != null)
                    PositiveButton.Invoke(sender, args);
                Dismiss();
            };
            cancel.Click += (sender, args) =>
            {
                if (NegativeButton != null)
                    NegativeButton.Invoke(sender, args);
                Dismiss();
            };
        }

        public void OnClick(View v)
        {

        }
    }
}