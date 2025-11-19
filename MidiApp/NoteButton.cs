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
    public class NoteButton : Button
    {
        public bool IsPointerInside { get; set; }

        public string Tag2 { get; set; }

        public NoteButton(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer)
        {
        }

        public NoteButton(Context context) 
            : base(context)
        {
            Init();
        }

        public NoteButton(Context context, IAttributeSet attrs) 
            : base(context, attrs)
        {
            Init();
        }

        public NoteButton(Context context, IAttributeSet attrs, int defStyleAttr) 
            : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public NoteButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) 
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        private void Init()
        {
            IsPointerInside = false;
            // set on click here
        }
    }
}