using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace TestApp
{
    public class PortInputFilter : Java.Lang.Object, IInputFilter
    {
        public PortInputFilter(Context ctx)
        {
            m_ctx = ctx;
        }

        private Context m_ctx;
        public const int MaxLength = 5;
        public const int MinLength = 1;

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            Java.Lang.String defaultText = new Java.Lang.String("");

            short n;

            for (int i = start; i < end; i++)
            {
                if (!Int16.TryParse(source.CharAt(i).ToString(), out n))
                {
                    Toast.MakeText(m_ctx, "Invalid port.", ToastLength.Short).Show();
                    return defaultText;
                }
            }
            return null;
        }
    }
}