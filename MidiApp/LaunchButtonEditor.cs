using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class LaunchButtonEditor : LinearLayout
    {
        public class UpdateLaunchButtonEventArgs : EventArgs
        {
            public LaunchButton LaunchButton { get; set; }
        }
        public EventHandler<UpdateLaunchButtonEventArgs> OnUpdate;

        private LaunchButton m_launchButton;

        public CustomItemPicker BindingNumText;
        public EditText NameText;
        public Android.Support.V7.Widget.GridLayout GridLayout;
        public Button UpdateButton;

        public LaunchButtonEditor(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LaunchButtonEditor(Context context) : base(context)
        {
            Initialise(context);
        }

        public LaunchButtonEditor(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialise(context);
        }

        public LaunchButtonEditor(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialise(context);
        }

        public LaunchButtonEditor(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialise(context);
        }

        private void Initialise(Context ctx)
        {
            LayoutInflater inflater = (LayoutInflater)ctx.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.LaunchButtonEditorLayout, this);

            BindingNumText = layout.FindViewById<CustomItemPicker>(Resource.Id.controlNumPicker);
            BindingNumText.IsDataNumeric = true;
            BindingNumText.Editable = false;
            BindingNumText.Items = Enumerable.Range(0, 127).
                Select(x => new CustomItemPicker.CustomItem
                {
                    Index = x - 1,
                    Text = (x).ToString()
                }).ToList();
            BindingNumText.CurrentItem = 0;

            NameText = layout.FindViewById<EditText>(Resource.Id.launchButtonNameEditText);
            //GridLayout = layout.FindViewById<Android.Support.V7.Widget.GridLayout>(Resource.Id.colorsLayout);

            UpdateButton = layout.FindViewById<Button>(Resource.Id.updateButton);
            UpdateButton.Click += (sender, args) =>
            {
                m_launchButton.ControlNum = (byte) (BindingNumText.CurrentItem + 1);
                m_launchButton.Name = NameText.Text;
                m_launchButton.Text = NameText.Text.Equals(BindingNumText.CurrentItem.ToString()) ? BindingNumText.CurrentItem.ToString() : NameText.Text;

                OnUpdate?.Invoke(this, new UpdateLaunchButtonEventArgs {LaunchButton = m_launchButton});
            };
        }

        public void SetTargetButton(LaunchButton lb)
        {
            m_launchButton = lb;

            BindingNumText.CurrentItem = m_launchButton.ControlNum - 1;
            NameText.Text = m_launchButton.Name;
        }

        public LaunchButtonData GetCurrData()
        {
            return new LaunchButtonData
            {
                ControlNum = (byte)BindingNumText.CurrentItem,
                Name = NameText.Text
            };
        }  
    }
}