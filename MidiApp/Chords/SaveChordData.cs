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
using TestApp.Midi;

namespace TestApp.Chords
{
    public class SaveChordData
    {
        public string CurrentScale { get; set; }
        public string CurrentKey { get; set; }
        public int CurrentOctave { get; set; }
        public List<MidiChord> Chords { get; set; }
    }
}