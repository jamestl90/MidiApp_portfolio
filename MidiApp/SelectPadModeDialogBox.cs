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
    public class SelectPadModeDialogBox : Dialog, View.IOnClickListener
    {
        public EventHandler<EventArgs> NotesButton;
        public EventHandler<EventArgs> LaunchButton;

        public SelectPadModeDialogBox(Context context) : base(context)
        {
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature((int)WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.SelectPadModeLayout);

            Button ok = FindViewById<Button>(Resource.Id.noteButton);
            Button cancel = FindViewById<Button>(Resource.Id.launchButton);

            ok.Click += (sender, args) =>
            {
                if (NotesButton != null)
                    NotesButton.Invoke(sender, args);
                Dismiss();
            };
            cancel.Click += (sender, args) =>
            {
                if (LaunchButton != null)
                    LaunchButton.Invoke(sender, args);
                Dismiss();
            };
        }

        public void OnClick(View v)
        {

        }
    }
}