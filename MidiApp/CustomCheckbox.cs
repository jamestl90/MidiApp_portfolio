using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Percent;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace TestApp
{
	public class CustomCheckbox : PercentRelativeLayout
	{
        public CheckBox CheckBox { get; set; }

	    public CustomCheckbox(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	    {
	    }

	    public CustomCheckbox(Context context) : base(context)
	    {
            Init(context);
        }

	    public CustomCheckbox(Context context, IAttributeSet attrs) : base(context, attrs)
	    {
	        Init(context);
	    }

	    public CustomCheckbox(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
	    {
            Init(context);
	    }

	    private void Init(Context ctx)
	    {
            LayoutInflater inflater = (LayoutInflater)ctx.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.CustomCheckbox, this);

	        CheckBox = view.FindViewById<CheckBox>(Resource.Id.checkbox);
	    }
	}
}