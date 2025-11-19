using System.Collections.Generic;
using System.Linq;

namespace TestApp.Midi
{
    public static class MidiScales
    {
        public static byte[] GetScaleFromString(string scale, string key, int octave)
        {
            switch (scale)
            {
                case "Major":
                    return GetMajorScale(key, octave);
                case "Minor":
                    return GetMinorScale(key, octave);
                case "Major Pentatonic":
                    return GetMajorPentatonic(key, octave);                    
                case "Minor Pentatonic":
                    return GetMinorPentatonic(key, octave);                    
                case "Blues":
                    return GetBluesScale(key, octave);
                case "Minor Harm/Mel":
                    return GetMinorsScaleHarmMel(key, octave);
                case "Chromatic":
                    return GetChromaticScale(key, octave);
                default:
                    return null;
            }
        }

        public static byte[] GetMajorScale(string key, int octave)
        {
            byte note = (byte)MidiNotes.GetNote(key, octave);
            if (note == 255)
                return null;
            int[] notes =
            {
                note, note + 2, note + 4, note + 5, note + 7, note + 9, note + 11
            };
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static byte[] GetMinorScale(string key, int octave)
        {
            byte note = (byte)MidiNotes.GetNote(key, octave);
            if (note == 255)
                return null;
            int[] notes =
            {
                note, note + 2, note + 3, note + 5, note + 7, note + 8, note + 10
            };
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static byte[] GetMajorPentatonic(string key, int octave)
        {
            var bytes = GetMajorScale(key, octave);
            if (bytes == null)
                return null;
            int[] notes = new int[] {bytes[0], bytes[1], bytes[2], bytes[4], bytes[5]};
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static byte[] GetMinorPentatonic(string key, int octave)
        {
            var bytes = GetMinorScale(key, octave);
            if (bytes == null)
                return null;
            int[] notes = new int[] {bytes[0], bytes[2], bytes[3], bytes[4], bytes[6]};
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static byte[] GetBluesScale(string key, int octave)
        {
            byte note = (byte)MidiNotes.GetNote(key, octave);
            if (note == 255)
                return null;
            int[] notes =
            {
                note, note + 3, note + 5, note + 6, note + 7, note + 10
            };
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        private static void BoundaryCheck(int[] noteArray)
        {
            for (int i = 0; i < noteArray.Length; i++)
            {
                if (noteArray[i] > 127)
                {
                    //noteArray[i] = -1; // set an invalid value
                }
            }
        }

        public static byte[] GetMinorsScaleHarmMel(string key, int octave)
        {
            byte note = (byte)MidiNotes.GetNote(key, octave);
            if (note == 255)
                return null;

            int[] notes = 
            {                               
                note, note + 2, note + 3,
                note + 5, note + 7, note + 8,
                note + 10, note + 11
            };
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static byte[] GetChromaticScale(string key, int octave)
        {
            byte note = (byte)MidiNotes.GetNote(key, octave);
            if (note == 255)
                return null;
            int[] notes =
            {
                note, note + 1, note + 2, note + 3, note + 4, note + 5, note + 6,
                note + 7, note + 8, note + 9, note + 10, note + 11
            };
            BoundaryCheck(notes);
            return notes.Select(x => (byte)x).ToArray();
        }

        public static string GetNoteAsString(byte note, int currentOctave)
        {
            var reverseLookupNotes = MidiNotes.ReverseNotes(MidiNotes.Notes);

            int noteBase = 0;
            int octave = currentOctave;

            if (currentOctave != 0)
            {
                noteBase = note - (12 * currentOctave);
            }
            else
            {
                noteBase = note;
            }

            while (noteBase >= 12)
            {
                noteBase -= (int)MidiOctave.Interval.P8;
                octave++;
            }

            return reverseLookupNotes[noteBase] + octave;
        }

        public static List<string> GetNotesAsString(byte[] scale, int currentOctave)
        {
            List<string> newNotes = new List<string>();
            var reverseLookupNotes = MidiNotes.ReverseNotes(MidiNotes.Notes);

            for (int i = 0; i < scale.Length; ++i)
            {                
                int note = scale[i];
                int noteBase = 0;
                int octave = currentOctave;

                if (currentOctave != 0)
                {
                    noteBase = note - (12 * currentOctave);
                }    
                else
                {
                    noteBase = note;
                }

                if (noteBase > 11)
                {
                    noteBase -= (int)MidiOctave.Interval.P8;
                    octave++;
                }    
                newNotes.Add(reverseLookupNotes[noteBase] + octave);
            }
            return newNotes;
        }

        // scale must be multiple of size
        public static byte[] ArrangeForButtons(byte[] scale, int size)
        {
            if (scale == null) return null;

            byte[] arrangedNotes = new byte[scale.Length];
            int count = 0;
            for (int i = size - 1; i >= 0; i--)
            {
                for (int j = 0; j < size; j++)
                {
                    int index = i + (j*size);
                    arrangedNotes[count] = scale[index];
                    count++;
                }
            }
            return arrangedNotes;
        }

        public static byte[] ExtendScale(byte[] scale, int num)
        {
            if (num == 0) return scale;

            byte[] extension = new byte[scale.Length + num];
            for (int i = 0; i < scale.Length; i++)
            {
                extension[i] = scale[i];
            }
            for (int i = 0; i < num; i++)
            {
                extension[scale.Length + i] = extension[i];
                extension[scale.Length + i] += (byte)MidiOctave.Interval.P8;
            }
            return extension;
        }

        public static byte[] GetBlackPianoKeys(byte[] chromaticScale)
        {
            return new []
            {
                chromaticScale[1],
                chromaticScale[3],
                chromaticScale[6],
                chromaticScale[8],
                chromaticScale[10]
            };
        }

        public static byte[] GetWhitePianoKeys(byte[] chromaticScale)
        {
            return new[]
            {
                chromaticScale[0],
                chromaticScale[2],
                chromaticScale[4],
                chromaticScale[5],
                chromaticScale[7],
                chromaticScale[9],
                chromaticScale[11]
            };
        }

        public static byte[] AddOctaveUpToEnd(this byte[] scale)
        {
            byte first = scale[0];
            byte octaveUp = (byte)(first + MidiOctave.Interval.P8);

            byte[] newScale = new byte[scale.Length + 1];

            for (int i = 0; i < scale.Length; ++i)
            {
                newScale[i] = scale[i];
            }
            newScale[scale.Length] = octaveUp;
            return newScale;
        }
    }
}