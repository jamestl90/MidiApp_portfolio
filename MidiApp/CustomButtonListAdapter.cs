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
    public class CustomButtonListAdapter : ArrayAdapter<MidiChord>
    {
        private Context m_context;
        private List<MidiChord> m_midiChords;

        private ToggleButton m_unlockButton;
        private NotePickerControl m_notePicker;

        public CustomButtonListAdapter(Context context, int textViewResourceId, ToggleButton togButton, NotePickerControl notepicker, IList<MidiChord> objects)
            : base(context, textViewResourceId, objects)
        {
            m_context = context;
            m_midiChords = objects.ToList();

            m_notePicker = notepicker;
            m_unlockButton = togButton;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            MidiChord chord = m_midiChords.ElementAt(position);

            LayoutInflater inflater = (LayoutInflater) m_context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.CustomListItemBlue, null);

            Button button = view.FindViewById<Button>(Resource.Id.button);
            button.Text = chord.Name;
            button.Click += (sender, args) =>
            {
                ((ChordModeActivity)m_context).ChordButtonPressed(chord);
            };

            return view;
        }        
    }
}