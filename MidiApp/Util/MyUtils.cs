using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using TestApp.Midi;

namespace TestApp.Util
{
    public static class MyUtils
    {
        public static ImageView CreateScaledImageView(float desiredWidth, float desiredHeight, Context ctx, int bitmapDrawableId, ViewGroup parent = null)
        {
            var bitmap = BitmapFactory.DecodeResource(ctx.Resources, bitmapDrawableId);

            int scaleWidth = (int)((desiredWidth / (float)bitmap.Width) * (float)bitmap.Width);
            int scaleHeight = (int)((desiredHeight / (float)bitmap.Height) * (float)bitmap.Height);

            Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(bitmap,
                scaleWidth,
                scaleHeight, false);

            ImageView imageView = new ImageView(ctx);
            imageView.SetImageBitmap(scaledBitmap);
            bitmap.Recycle();

            var relLayout = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            relLayout.AddRule(LayoutRules.CenterInParent);

            parent?.AddView(imageView, relLayout);
            return imageView;
        }

        public static void HideKeyboard(Context ctx, EditText editText)
        {
            InputMethodManager imm = (InputMethodManager)ctx.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(editText.WindowToken, 0);
        }

        public static void DumpMetrics(DisplayMetrics metrics)
        {
            var density = metrics.Density;
            var dpi = metrics.DensityDpi;
            var hPixels = metrics.HeightPixels;
            var wPixels = metrics.WidthPixels;
            var sDensity = metrics.ScaledDensity;
            var xdpi = metrics.Xdpi;
            var ydpi = metrics.Ydpi;

            var str =
                $"Density: {density}\nDensity Dpi: {dpi}\nHeight Pixels: {hPixels}\nWidth Pixels: {wPixels}\nScreen Density: {sDensity}\nxdpi: {xdpi}\nydpi: {ydpi}\n";

            Console.WriteLine(str);
        }

        public static void PopupMenu(Context context, string[] items, string title, EventHandler<DialogClickEventArgs> handler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title)
            .SetItems(items, handler)
            .Create().Show();
        }

        public static void ConfirmBox(Context ctx, string title, string message, EventHandler<DialogClickEventArgs> okHandler,
                                                                EventHandler<DialogClickEventArgs> cancelHandler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ctx);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton("Ok", okHandler);
            builder.SetNegativeButton("Cancel", cancelHandler);
            builder.Create().Show();
        }

        public static void MultiChoicePopupMenu(Context ctx, string[] items, EventHandler<DialogMultiChoiceClickEventArgs> handler,
            EventHandler<DialogClickEventArgs> okHandler, EventHandler<DialogClickEventArgs> cancelHandler)
        {
            bool[] checkedItems = new bool[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                checkedItems[i] = true;
            }
            checkedItems[checkedItems.Length - 1] = false;
            AlertDialog.Builder builder = new AlertDialog.Builder(ctx);
            builder.SetTitle("Configure");
            builder.SetMultiChoiceItems(items, checkedItems, handler);
            builder.SetPositiveButton("Ok", okHandler);
            builder.SetNegativeButton("Cancel", cancelHandler);
            builder.Create().Show();
        }

        public static void MessageBox(Context context, string message)
        {
            var alert = new AlertDialog.Builder(context);
            alert.SetMessage(message);
            alert.Create().Show();
        }

        public static float ConvertPixelsToDp(Context ctx, float px)
        {
            Android.Content.Res.Resources resources = ctx.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            float dp = px / ((float)metrics.DensityDpi / (float)DisplayMetricsDensity.Default);
            return dp;
        }

        public static float ConvertDpToPixel(Context ctx, float dp)
        {
            Android.Content.Res.Resources resources = ctx.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            float px = dp * ((float)metrics.DensityDpi / (float)DisplayMetricsDensity.Default);
            return px;
        }

        public static void SimpleButtonPressScaleAnimation(View v, int totalDuration, bool bounceUp, float amount, Action onAnimEnd)
        {
            amount = bounceUp ? 1.0f + amount : 1.0f - amount;

            AnimatorSet animSet = new AnimatorSet();
            ObjectAnimator scale1 = ObjectAnimator.OfPropertyValuesHolder(v, PropertyValuesHolder.OfFloat("scaleX", amount), PropertyValuesHolder.OfFloat("scaleY", amount));
            scale1.SetDuration(totalDuration / 2);
            ObjectAnimator scale2 = ObjectAnimator.OfPropertyValuesHolder(v, PropertyValuesHolder.OfFloat("scaleX", 1f), PropertyValuesHolder.OfFloat("scaleY", 1f));
            scale2.SetDuration(totalDuration / 2);
            scale2.AnimationEnd += (sender, args) =>
            {
                onAnimEnd();
            };
            animSet.Play(scale1).Before(scale2);
            animSet.Start();
        }

        public static void SimpleRotateAnimation(View v, int totalDuration)
        {
            var kf0 = Keyframe.OfFloat(0, 0);
            var kf1 = Keyframe.OfFloat(0.5f, -360f);
            var kf2 = Keyframe.OfFloat(1f, -720f);
            var pvhRot = PropertyValuesHolder.OfKeyframe("rotation", kf0, kf1, kf2);
            var rotAnim = ObjectAnimator.OfPropertyValuesHolder(v, pvhRot);
            rotAnim.SetDuration(totalDuration);
            rotAnim.Start();
        }

        public static Point GetScreenSize(Activity activity)
        {
            Display display = activity.WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);            
            return size;
        }

        public static bool IsScrolling(Activity activity, float x1, float y1, float x2, float y2, float threshold)
        {
            var screenSize = GetScreenSize(activity);
            float widthVal = screenSize.X*threshold;
            float heightVal = screenSize.Y*threshold;

            //Console.WriteLine(widthVal + "  h: " + heightVal);

            var xdiff = Math.Abs(x1 - x2);
            var ydiff = Math.Abs(y1 - y2);

            if (xdiff > widthVal || ydiff > heightVal)
                return true;
            return false;
        }

        public static bool IsViewInBounds(View view, int x, int y, Rect outRect, int[] location)
        {
            if (location == null)
            {
                location = new int[2];
            }
            if (location.Length != 2)
            {
                location = new int[2];
            }

            view.GetDrawingRect(outRect);
            view.GetLocationOnScreen(location);
            outRect.Offset(location[0], location[1]);
            return outRect.Contains(x, y);
        }

        public static MotionEvent ObtainDefaultMotionEvent(MotionEventActions action)
        {
            MotionEvent me = MotionEvent.Obtain(0, 100, action, 0, 0, 0);
            return me;
        }


        public static View GetViewAtLoc(int x, int y, List<View> views)
        {
            Rect rect = new Rect();
            int[] location = new int[2];

            foreach (var v in views)
            {
                if (IsViewInBounds(v, x, y, rect, location))
                {
                    return v;
                }
            }
            return null;
        }
    }
}