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
    public class MidiNotes
    {
        private static Dictionary<string, int> _notes;
        public static Dictionary<string, int> Notes
        {
            get
            {
                if (_notes == null)
                {
                    _notes = new Dictionary<string, int>();
                    GenerateNotes();
                }
                if (!Generated)
                {
                    GenerateNotes();
                }
                return _notes;
            }
        }

        private static bool _generated = false;
        public static bool Generated
        {
            get { return _generated; }
            private set { _generated = value; }
        }

        private static void GenerateNotes()
        {
            if (_notes == null)
                _notes = new Dictionary<string, int>();

            _notes["C"] = 0;
            _notes["C#"] = 1;
            _notes["Db"] = 1;
            _notes["D"] = 2;
            _notes["D#"] = 3;
            _notes["Eb"] = 3;
            _notes["E"] = 4;
            _notes["F"] = 5;
            _notes["F#"] = 6;
            _notes["Gb"] = 6;
            _notes["G"] = 7;
            _notes["G#"] = 8;
            _notes["Ab"] = 8;
            _notes["A"] = 9;
            _notes["A#"] = 10;
            _notes["Bb"] = 10;
            _notes["B"] = 11;            

            Generated = true;
        }

        public static byte GetLowestMidiNote(byte note)
        {
            int newNote = (int)note;
            while (newNote >= 0)
            {                
                newNote -= 12;
            }

            if (newNote < 0)
            {
                newNote += 12;
            }
            return (byte) newNote;
        }

        public static Dictionary<int, string> ReverseNotes(Dictionary<string, int> notes)
        {
            Dictionary<int, string> notesReverseLookup = new Dictionary<int, string>();
            foreach (var note in Notes)
            {
                if (!notesReverseLookup.ContainsKey(note.Value))
                {
                    notesReverseLookup[note.Value] = note.Key;
                }
                else
                {
                    notesReverseLookup[note.Value] += "/" + note.Key;
                }
            }
            return notesReverseLookup;
        }

        // safe way to get notes from the dictionary
        // remember to convert to byte 
        public static int GetNote(string note, int octave = 5)
        {
            if (!Generated)
            {
                // generate the dictionary if not already done
                GenerateNotes();
            }

            if (note.Contains("/"))
            {
                note = note.Split('/')[0];
            }

            int foundNote = Notes[note];
            int final = (foundNote + (12 * octave));
            
            if (final > 127 || final < 0)
            {
                return 255;
            }
            return final;
        }
    }
}