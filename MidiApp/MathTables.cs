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
    public static class MathTables
    {
        public enum AngleMeasurement
        {
            Degrees,
            Radians
        }

        public enum ConstAnglesDegrees
        {
            Deg30,
            Deg45,
            Deg60,
            Deg90,
            Deg180,
            Deg270,
            Deg360
        }

        public static double GetAngleRadian(ConstAnglesDegrees angle)
        {
            switch (angle)
            {
                case ConstAnglesDegrees.Deg30:
                    return Math.PI/6.0;
                case ConstAnglesDegrees.Deg45:
                    return Math.PI / 4.0;
                case ConstAnglesDegrees.Deg60:
                    return Math.PI / 3.0;
                case ConstAnglesDegrees.Deg90:
                    return Math.PI / 2.0;
                case ConstAnglesDegrees.Deg180:
                    return Math.PI;
                case ConstAnglesDegrees.Deg270:
                    return (3*Math.PI)/2.0;
                case ConstAnglesDegrees.Deg360:
                    return Math.PI*2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(angle), angle, null);
            }
        }
    }
}