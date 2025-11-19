using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class AnimationListener : AnimatorListenerAdapter
    {
        public Action<Animator> OnCancel;
        public Action<Animator> OnStart;
        public Action<Animator> OnEnd;
        public Action<Animator> OnRepeat;

        public enum AnimationType
        {
            Cancel,
            Start,
            End,
            Repeat
        }

        public AnimationListener(Action<Animator> action, AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Cancel:
                    OnCancel = action;
                    break;
                case AnimationType.Start:
                    OnStart = action;
                    break;
                case AnimationType.End:
                    OnEnd = action;
                    break;
                case AnimationType.Repeat:
                    OnRepeat = action;
                    break;
            }
        }

        public AnimationListener(Action<Animator> start, Action<Animator> end, Action<Animator> repeat,
            Action<Animator> cancel)
        {
            OnCancel = cancel;
            OnStart = start;
            OnEnd = end;
            OnRepeat = repeat;
        }

        public class AnimationListenerEventArgs : EventArgs
        {
            public Animator Animation { get; set; }
        }

        public override void OnAnimationCancel(Animator animation)
        {
            base.OnAnimationCancel(animation);
            OnCancel?.Invoke(animation);
            //AnimationCancel?.Invoke(this, new AnimationListenerEventArgs { Animation = animation });
        }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);
            OnEnd?.Invoke(animation);
            //AnimationEnd?.Invoke(this, new AnimationListenerEventArgs { Animation = animation });
        }

        public override void OnAnimationRepeat(Animator animation)
        {
            base.OnAnimationRepeat(animation);
            OnRepeat?.Invoke(animation);
            //AnimationRepeat?.Invoke(this, new AnimationListenerEventArgs { Animation = animation });
        }

        public override void OnAnimationStart(Animator animation)
        {
            base.OnAnimationStart(animation);
            OnStart?.Invoke(animation);
            //AnimationStart?.Invoke(this, new AnimationListenerEventArgs { Animation = animation });
        }
    }
}