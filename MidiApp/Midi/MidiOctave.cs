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

namespace TestApp.Midi
{
    public class MidiOctave
    {
        public const int MaxPitchBend = 0x3FFF; // 16383
        public const int DefaultPitchBend = 0x2000; // 8192
        public const int MinPitchBend = 0x0;

        public static int ShiftAmount = 12;

        public enum ShiftDirection
        {
            Up,
            Down
        }

        public enum Interval
        {
            Unison = 0,
            m2 = 1,
            M2 = 2,
            m3 = 3,
            M3 = 4,
            P4 = 5,
            d5 = 6,
            P5 = 7,
            m6 = 8,
            M6 = 9,
            m7 = 10,
            M7 = 11,
            P8 = 12,
            m9 = 13,
            M9 = 14,
            m10 = 15,
            M10 = 16,
            P11 = 17,
            d12 = 18,
            P12 = 19,
            m13 = 20
        }

        public static string[] GetIntervalsAsStrings()
        {
            var values = Enum.GetValues(typeof(Interval)).Cast<Interval>();
            return values.ToList().Select(x => x.ToString()).ToArray();
        }

        public static byte[] ShiftOctave(byte[] notes, ShiftDirection shiftDir)
        {
            for (int i = 0; i < notes.Length; ++i)
            {
                if (shiftDir == ShiftDirection.Up)
                {
                    notes[i] += 12;
                }
                else
                {
                    notes[i] -= 12;
                }               
            }
            return notes;
        }

        public static byte[] ShiftNotes(byte[] notes, ShiftDirection shiftDir, Interval amount)
        {
            for (int i = 0; i < notes.Length; ++i)
            {
                if (shiftDir == ShiftDirection.Up)
                {
                    notes[i] += (byte)amount;
                }
                else
                {
                    notes[i] -= (byte)amount;
                }
            }
            return notes;
        }
    }
}