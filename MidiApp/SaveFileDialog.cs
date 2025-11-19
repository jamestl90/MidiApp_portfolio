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
    public class SaveFileDialog : Dialog
    {
        public class SaveFileEventArgs : EventArgs
        {
            public string FileName { get; set; }
        }

        public EventHandler<SaveFileEventArgs> OnSave;

        private Context _context;
        private readonly string _title;

        public SaveFileDialog(Context context, string title)
            : base(context)
        {
            _title = title;
            _context = context;
        }

        public SaveFileDialog(Context context)
            : base(context)
        {
            _title = "";
            _context = context;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int) WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.SaveFileDialogLayout);

            SetCanceledOnTouchOutside(true);

            if (_title != "")
            {
                var title = FindViewById<TextView>(Resource.Id.title);
                title.Text = _title;
            }

            var saveFileText = FindViewById<EditText>(Resource.Id.saveFileEditText);

            var saveFileButton = FindViewById<Button>(Resource.Id.saveButton);
            saveFileButton.Click += (sender, args) =>
            {
                string fileName = saveFileText.Text;

                OnSave?.Invoke(this, new SaveFileEventArgs { FileName = fileName });
            };
        }
    }
}