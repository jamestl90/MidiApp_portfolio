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
    public class MidiChord : Java.Lang.Object
    {
        public string Name { get; set; }

        public byte Root { get; set; }

        public MidiOctave.Interval[] Intervals { get; set; }

        public MidiChord()
        {
            Intervals = new MidiOctave.Interval[18];
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MidiChordSerialize
    {
        public int Octave { get; set; }

        public string Name { get; set; }

        public byte Root { get; set; }

        public int[] Intervals { get; set; }

        public MidiChordSerialize() { }

        public MidiChordSerialize(MidiChord chord, int currOctave)
        {
            Octave = currOctave;
            Name = chord.Name;
            Root = chord.Root;
            Root = (byte) (chord.Root - (12 * currOctave));
            Intervals = Array.ConvertAll(chord.Intervals, x => (int)x);
        }

        public MidiChord ToChord(int currOctave)
        {
            MidiChord chord = new MidiChord();
            chord.Name = Name;
            chord.Root = (byte)(Root + (12 * currOctave));
            chord.Intervals = new MidiOctave.Interval[Intervals.Length];
            for (int i = 0; i < Intervals.Length; i++)
            {
                chord.Intervals[i] = (MidiOctave.Interval) Intervals[i];
            }
            return chord;
        }
    }

    public static class MidiChords
    {
        public static MidiChord MajorTriad(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "Triad(Maj)",
                Intervals = new []
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.P5
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MinorTriad(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "Triad(Min)",
                Intervals = new []
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.P5
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MinorSeven(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "m7",
                Intervals = new[]
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.m7
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MajorSeven(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "M7",
                Intervals = new[]
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.M7
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MinorNinth(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "m9",
                Intervals = new[]
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.m7,
                    MidiOctave.Interval.M9
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord DomNinth(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "M9",
                Intervals = new[]
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.m7,
                    MidiOctave.Interval.M9
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord Augmented(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "Aug",
                Intervals = new[]
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.m6,
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord Diminished(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "Dim",
                Intervals = new[]
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.d5,
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord DominantSeventh(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "D7",
                Intervals = new[]
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.m7
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MajorSixth(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "M6",
                Intervals = new[]
                {
                    MidiOctave.Interval.M3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.M6
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MinorSixth(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "m6",
                Intervals = new[]
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.M6
                },
                Root = root
            };
            return chord;
        }

        public static MidiChord MinorMajorSeventh(byte root)
        {
            MidiChord chord = new MidiChord
            {
                Name = "mM7",
                Intervals = new[]
                {
                    MidiOctave.Interval.m3,
                    MidiOctave.Interval.P5,
                    MidiOctave.Interval.M7
                },
                Root = root
            };
            return chord;
        }
    }
}