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

namespace TestApp
{
    public class Keypad
    {
        private int m_numKeys;
        private int m_numOctaves;
        private byte[] notes;

        public Keypad(int width, int height)
        {

        }

        public void Update(int octave, string key, string scale)
        {
            // update notes array
            MidiScales.GetScaleFromString(scale, key, octave);

            ArrangeNotes();
        }

        protected virtual void ArrangeNotes()
        {
            // arrange notes for button layout
        }
    }

    public class Keyboard
    {
        private byte[] m_blackNotes;
        private byte[] m_whiteNotes;

        private int m_numOctaves;

        public Keyboard(int numOctaves, string key)
        {
            m_numOctaves = numOctaves;
            m_whiteNotes = new byte[numOctaves * 8];
            m_blackNotes = new byte[numOctaves * 5];

            Update(0, key);
        }

        public byte[] WhiteKeys() { return m_whiteNotes; }
        public byte[] BlackKeys() { return m_blackNotes; }

        public void Update(int octave, string key)
        {
            var whiteNotes = MidiScales.GetWhitePianoKeys(MidiScales.GetChromaticScale(key, octave)).AddOctaveUpToEnd();
            var blackNotes = MidiScales.GetBlackPianoKeys(MidiScales.GetChromaticScale(key, octave)).AddOctaveUpToEnd();

            for (int i = 0; i < 8; ++i)
            {
                m_whiteNotes[i] = whiteNotes[i];
            }
            for (int i = 8; i < 16; ++i)
            {
                m_whiteNotes[i] = (byte)(whiteNotes[i - 8] + (byte)12);
            }

            for (int i = 0; i < 5; ++i)
            {
                m_blackNotes[i] = blackNotes[i];
            }
            for (int i = 5; i < 10; ++i)
            {
                m_blackNotes[i] = (byte)(blackNotes[i - 5] + (byte)12);
            }
        }
    }
}