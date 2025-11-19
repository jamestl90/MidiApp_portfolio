using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TestApp.Midi;
using TestApp.Util;

namespace TestApp
{
    public class NotePickerControl : LinearLayout
    {
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        private bool m_isInit = false;

        private bool playing = false;

        private View m_notePickerLayout;

        public byte Root { get; set; }

        #region ctor
        public NotePickerControl(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public NotePickerControl(Context context) : base(context)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public NotePickerControl(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }            
        }

        public NotePickerControl(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }

        public NotePickerControl(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            if (!m_isInit)
            {
                Initialise(context);
            }
        }
        #endregion

        public List<ToggleButton> GetToggleButtons()
        {
            List<ToggleButton> toggleButtons = new List<ToggleButton>();
            var leftColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.leftColumnLayout);
            var rightColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.rightColumnLayout);

            for (int i = 0; i < leftColumn.ChildCount; i++)
            {
                var view = leftColumn.GetChildAt(i);
                if (view.GetType() == typeof(ToggleButton))
                {
                    toggleButtons.Add((ToggleButton)view);
                }
            }
            for (int i = 0; i < rightColumn.ChildCount; i++)
            {
                var view = rightColumn.GetChildAt(i);
                if (view.GetType() == typeof(ToggleButton))
                {
                    toggleButtons.Add((ToggleButton)view);
                }
            }
            return toggleButtons;
        }

        public List<MidiOctave.Interval> GetSelectedIntervals()
        {
            List<MidiOctave.Interval> intervals = new List<MidiOctave.Interval>();
            var leftColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.leftColumnLayout);
            var rightColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.rightColumnLayout);

            for (int i = 0; i < leftColumn.ChildCount; i++)
            {
                var view = leftColumn.GetChildAt(i);
                if (view.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = view as ToggleButton;
                    if (tb.Checked)
                    {
                        intervals.Add((MidiOctave.Interval)((int)tb.Tag));
                    }
                }
            }

            for (int i = 0; i < rightColumn.ChildCount; i++)
            {
                var view = rightColumn.GetChildAt(i);
                if (view.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = view as ToggleButton;
                    if (tb.Checked)
                    {
                        intervals.Add((MidiOctave.Interval)((int)tb.Tag));
                    }
                }
            }
            return intervals;
        }

        public MidiChord GetChord(byte root)
        {
            return new MidiChord
            {
                Intervals = GetSelectedIntervals().ToArray(),
                Name = "Custom",
                Root = root
            };
        }

        public void ClearSelections()
        {
            playing = false;
            var toggleButtons = GetToggleButtons();

            foreach (var button in toggleButtons)
            {
                button.Checked = false;
            }
            playing = true;
        }

        public void SetChord(MidiChord presetChord)
        {
            ClearSelections();

            var toggleButtons = GetToggleButtons();

            playing = false;

            foreach (var interval in presetChord.Intervals)
            {
                foreach (var button in toggleButtons)
                {
                    if ((int) button.Tag == (int) interval)
                    {
                        button.Checked = true;
                    }
                }
            }
            //PlayChord();
            playing = true;
        }

        private void Initialise(Context context)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            m_notePickerLayout = inflater.Inflate(Resource.Layout.NotePickerLayout, this);

            var leftColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.leftColumnLayout);
            var rightColumn = m_notePickerLayout.FindViewById<LinearLayout>(Resource.Id.rightColumnLayout);
            int margin = (int)MyUtils.ConvertDpToPixel(context, 4);
            leftColumn.SetPadding(2,2,2,2);
            rightColumn.SetPadding(2, 2, 2, 2);

            var intervalsEnum = Enum.GetValues(typeof(MidiOctave.Interval)).Cast<MidiOctave.Interval>().ToArray();
            var intervals = MidiOctave.GetIntervalsAsStrings();

            var background = context.GetDrawable(Resource.Drawable.list_item_background);
            var backgroundPressed = context.GetDrawable(Resource.Drawable.list_item_background_down);

            for (int i = 1; i < 10; i++)
            {
                ToggleButton b = new ToggleButton(context);
                b.Tag = (int)intervalsEnum[10 - i];
                string text = intervals[10 - i];
                b.SetAllCaps(false);
                b.SetText(text, TextView.BufferType.Normal);
                b.TextOff = text;
                b.TextOn = text;
                b.Background = background;
                b.SetTextColor(Color.White);
                var lay = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                lay.Height = 0;
                lay.Weight = 1;
                lay.Gravity = GravityFlags.Center;                
                lay.SetMargins(2,2,2,2);                             
                b.LayoutParameters = lay;
                b.CheckedChange += (sender, args) =>
                {
                    if (args.IsChecked)
                    {
                        b.Background = backgroundPressed;
                        if (playing)
                            PlayChord();
                    }
                    else
                    {
                        b.Background = background;
                    }
                };
                leftColumn.AddView(b);
            }

            for (int i = 12; i < 21; i++)
            {                
                ToggleButton b = new ToggleButton(context);
                b.Tag = (int)intervalsEnum[20 - (i - 10)];
                b.SetAllCaps(false);
                string text = intervals[20 - (i - 10)];
                b.SetText(text, TextView.BufferType.Normal);
                b.TextOff = text;
                b.TextOn = text;
                b.Background = background;
                b.SetTextColor(Color.White);                
                var lay = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                lay.Height = 0;
                lay.Weight = 1;
                lay.Gravity = GravityFlags.Center;
                lay.SetMargins(2, 2, 2, 2);
                b.LayoutParameters = lay;
                b.CheckedChange += (sender, args) =>
                {
                    if (args.IsChecked)
                    {
                        b.Background = backgroundPressed;
                        if (playing)
                            PlayChord();
                    }
                    else
                    {
                        b.Background = background;
                    }                        
                };
                
                rightColumn.AddView(b);
            }

            m_isInit = true;
        }

        private async void PlayChord()
        {

                IMidiSender sender = MainActivity.MidiSender;
                      
                var channel = MidiHelper.Channel;
                var oct = MidiHelper.Octave;
                var key = MidiHelper.Key;
                var vel = MidiHelper.Velocity;
                //var chord = GetChord((byte) MidiNotes.GetNote(key, oct));
                var chord = GetChord(Root);
                MidiHelper.SendChordOn(sender, channel, vel, chord);
                //MainActivity.InputPort.Flush();

                await Task.Delay(800);
                
                MidiHelper.SendChordOff(sender, channel, chord);
                //MainActivity.InputPort.SendControlChange(MidiHelper.Channel,
                //    MidiHelper.ChannelModeMessageType.AllNotesOff);
                //MainActivity.InputPort.Flush();
        }
    }
}