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
    public enum ChannelModeMessageType
    {
        AllSoundOff = 120,
        ResetAllControllers = 121,
        LocalControl = 122,
        AllNotesOff = 123,
        OmniModeOff = 124,
        OmniModeOn = 125,
        MonoModeOn = 126,
        PolyModeOn = 127,
    }

    public enum NoteState
    {
        NoteOn,
        NoteOff
    }

    public class MidiConstants
    {
        public static readonly byte NoteOn = 0x90;
        public static readonly byte NoteOff = 0x80;
        public static readonly byte ControlChange = 0xB0;
        public static readonly byte PitchBendChange = 0xE0;
    }    
}