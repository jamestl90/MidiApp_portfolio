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
    public class MyBroadcastReceiver : BroadcastReceiver
    {
        public class BroadcastEvent : EventArgs
        {
            public Context Context { get; set; }
            public Intent Intent { get; set; }
        }

        public EventHandler<BroadcastEvent> OnReceiveEvent;

        public override void OnReceive(Context context, Intent intent)
        {
            OnReceiveEvent?.Invoke(this, new BroadcastEvent
            {
                Context = context,
                Intent = intent
            });
        }
    }
}