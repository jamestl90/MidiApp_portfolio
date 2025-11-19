using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using ResourceId = System.Int32;

namespace TestApp
{
    public class TutorialImages
    {
        private readonly List<ResourceId> m_drawables = new List<ResourceId>();
        private Bitmap m_currDrawable;

        private int m_current = 0;

        private Activity m_activity;

        public TutorialImages(Activity a)
        {
            m_activity = a;
        }

        public void Add(int d)
        {
            m_drawables.Add(d);
        }

        public Bitmap Current()
        {
            if (m_currDrawable != null)
                return m_currDrawable;

            int resId = m_drawables[m_current];
            m_currDrawable = BitmapFactory.DecodeResource(m_activity.Resources, resId);
            return m_currDrawable;
        }

        public Bitmap MoveNext()
        {
            if (m_currDrawable != null)
            {
                m_currDrawable.Recycle();
                m_currDrawable = null;
            }

            int newIndex = ++m_current;

            if (newIndex == m_drawables.Count)
                return null;
            return m_currDrawable = BitmapFactory.DecodeResource(m_activity.Resources, m_drawables[newIndex]); 
        }

        public bool IsLast()
        {
            return m_current == (m_drawables.Count - 1);
        }

        public void ClearAll()
        {
            m_currDrawable?.Recycle();

            m_drawables.Clear();
            m_current = 0;
        }
    }
}