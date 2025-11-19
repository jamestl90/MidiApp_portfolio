using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.Percent;
using Java.Interop;
using TestApp.Midi;
using TestApp.Util;
using Android.Support.V7.Widget;
using TestApp.Resources.layout;

namespace TestApp
{
    [Activity(Label = "LaunchActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class LaunchActivity : Activity
    {
        private byte m_lastNote = 0x0;

        private LaunchButtonData[] m_allButtons = new LaunchButtonData[127];
        private readonly List<LaunchButton> m_buttons = new List<LaunchButton>();

        public static string CurrentKey = "C";
        public static int CurrentOctave = 4;
        public static string CurrentScale = "Minor Harm/Mel";
        public byte CurrentRoot = 60;

        public static Color UnPressedColour = Color.GhostWhite;
        public static Color PressedColour = Color.LightBlue;

        public static Android.Graphics.Drawables.Drawable UnPressedDrawable;
        public static Android.Graphics.Drawables.Drawable PressedDrawable;
        public static Android.Graphics.Drawables.Drawable EditButtonDrawable;

        private SlidingDrawerFragment m_fragment;

        private LaunchButtonEditor m_launchButtonEditor;
        private LaunchButton m_lastButton;

        private bool EditMode;

        public void KeyItemClicked(string item)
        {
            if (item != CurrentKey)
            {
                CurrentKey = item;
                ScaleItemClicked(CurrentScale);
            }
        }

        public void ScaleItemClicked(string item)
        {
            CurrentScale = item;

            if (GlobalSettings.GetDefaultLaunchScreenSize() == 4)
            {

            }
            else
            {

            }

            for (int i = 0; i < m_buttons.Count; ++i)
            {                
                if (i > 127)
                {
                    m_buttons[i].Enabled = false;
                    m_buttons[i].Text = "#";
                }
                else
                {
                    m_buttons[i].Enabled = true;
                    m_buttons[i].Text = MidiScales.GetNoteAsString((byte)i, CurrentOctave);
                }
            }
            //else
            //{
            //    for (int i = 0; i < m_buttons.Count; ++i)
            //    {
            //        m_buttons[i].Enabled = false;
            //        m_buttons[i].Text = "#";
            //    }
            //}
        }

        private void OnPadDown(LaunchButton b)
        {
            if (EditMode)
            {
                         
            }
            else
            {
                IMidiSender sender = MainActivity.MidiSender;
                b.Background = UnPressedDrawable;

                m_lastNote = (byte)b.Tag;

                sender.SendControlChange(MidiHelper.Channel, b.ControlNum, 127);
            }
        }

        private void OnPadUp(LaunchButton b)
        {
            if (EditMode)
            {
                if (m_lastButton != null)
                {
                    m_lastButton.Background = UnPressedDrawable;
                }

                if (m_lastButton == b)
                {
                    m_lastButton = null;
                    return;
                }

                b.Background = EditButtonDrawable;
                m_lastButton = b;
                m_launchButtonEditor.SetTargetButton(b);
            }
            else
            {
                IMidiSender sender = MainActivity.MidiSender;
                b.Background = UnPressedDrawable;

                m_lastNote = (byte)b.Tag;

                sender.SendNoteOff(MidiHelper.Channel /* channel 1 */,
                    (byte)b.Tag /* middle C */);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.LaunchLayout);
            SetupTrayMenu(savedInstanceState);

            CurrentOctave = MidiHelper.Octave;
            CurrentKey = MidiHelper.Key;
            CurrentScale = GlobalSettings.GetDefaultPadScreenScaleName();
            CurrentRoot = (byte)MidiNotes.GetNote(CurrentKey, CurrentOctave);

            var bitmap1 = BitmapFactory.DecodeResource(Resources, Resource.Drawable.pad_down);
            var bitmap2 = BitmapFactory.DecodeResource(Resources, Resource.Drawable.pad_up);
            var bitmap3 = BitmapFactory.DecodeResource(Resources, Resource.Drawable.pad_edit);

            float size = (int)Resources.GetDimension(Resource.Dimension.pad_size);
            int scaleWidth = (int)((size / (float)bitmap1.Width) * (float)bitmap1.Width);
            int scaleHeight = (int)((size / (float)bitmap1.Height) * (float)bitmap1.Height);

            Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(bitmap1,
                scaleWidth,
                scaleHeight, false);
            Bitmap scaledBitmap2 = Bitmap.CreateScaledBitmap(bitmap2,
                scaleWidth,
                scaleHeight, false);
            Bitmap scaledBitmap3 = Bitmap.CreateScaledBitmap(bitmap3,
                scaleWidth,
                scaleHeight, false);
            bitmap1.Recycle();
            bitmap2.Recycle();
            bitmap3.Recycle();

            EditMode = false;

            PressedDrawable = new BitmapDrawable(scaledBitmap);
            UnPressedDrawable = new BitmapDrawable(scaledBitmap2);
            EditButtonDrawable = new BitmapDrawable(scaledBitmap3);

            m_buttons.Clear();
            //ActionBar.Hide();
            SetupProgressBars();
            SetupLockButton();
            SetupDefault();
            //SetupTouchStrip();

            //byte[] notes = MidiScales.GetScaleFromString(CurrentScale, CurrentKey, CurrentOctave);

            m_launchButtonEditor.OnUpdate += (sender, args) =>
            {
                m_allButtons[args.LaunchButton.LoopCount].Set(args.LaunchButton);
            };

            //m_allButtons = m_allButtons.Reverse().ToArray();

            Android.Support.V7.Widget.GridLayout gridLayout = FindViewById<Android.Support.V7.Widget.GridLayout>(Resource.Id.padsLayout);
            if (gridLayout == null) return;
            gridLayout.RemoveAllViews();


            gridLayout.RowCount = 4;
            gridLayout.ColumnCount = 3;

            int flipRow = gridLayout.RowCount - 1;

            gridLayout.UseDefaultMargins = true;

            int loopCount = CurrentRoot;
            for (int i = 0; i < gridLayout.RowCount; i++) // row
            {
                for (int j = 0; j < gridLayout.ColumnCount; j++) // column
                {
                    //var note = notes[j + (i * gridLayout.RowCount)];

                    LaunchButton b = new LaunchButton(this);
                    b.UpdateWith(m_allButtons[loopCount]);
                    b.LoopCount = loopCount;
                    b.Text = b.Name;
                    b.Background = UnPressedDrawable;
                    b.Touch += (sender, args) =>
                    {
                        if (args.Event.Action == MotionEventActions.Down)
                        {
                            OnPadDown(b);
                            b.Background = PressedDrawable;
                        }
                        else if (args.Event.Action == MotionEventActions.Up)
                        {
                            OnPadUp(b);
                        }                        
                    };

                    b.SetTextColor(Color.Gray);
                    b.TextSize = (int)Resources.GetDimension(Resource.Dimension.text_size);

                    Android.Support.V7.Widget.GridLayout.LayoutParams gridParams = MidiControlsHelper.CreatePadLayoutParamsSupportV7(j, (flipRow - i));
                    gridParams.SetGravity((int)GravityFlags.Fill);
                    int margin = (int)MyUtils.ConvertDpToPixel(this, 4);
                    gridParams.SetMargins(margin, margin, margin, margin);
                    gridLayout.AddView(b, gridParams);
                    m_buttons.Add(b);
                    loopCount++;
                }
            }
            var firstButton = m_buttons.First();
            //firstButton.Background = EditButtonDrawable;
            //m_launchButtonEditor.SetTargetButton(firstButton);

            m_lastButton = firstButton;
        }

        private void SetupDefault()
        {
            List<LaunchButtonData> launchList = GlobalSettings.GetDefaultLaunch();

            if (launchList.Count == 0)
            {
                for (int i = 0; i < 127; i++)
                {
                    m_allButtons[i] = new LaunchButtonData();
                    m_allButtons[i].ControlNum = (byte)(i + 1);
                    m_allButtons[i].Name = m_allButtons[i].ControlNum.ToString();
                    m_allButtons[i].Color = UnPressedColour;
                }
                //UpdateCurrentButtonSet();
            }
            else
            {
                for (int i = 0; i < m_allButtons.Length; i++)
                {
                    m_allButtons[i] = new LaunchButtonData();
                    m_allButtons[i].Name = launchList[i].Name;
                    m_allButtons[i].ControlNum = launchList[i].ControlNum;
                }
                UpdateCurrentButtonSet();
            }            
        }

        private void SetupButtons()
        {
            var controlView = m_fragment.View;

            var defaultButton = controlView.FindViewById<Button>(Resource.Id.defaultButton);
            var saveButton = controlView.FindViewById<Button>(Resource.Id.saveButton);
            var loadButton = controlView.FindViewById<Button>(Resource.Id.loadButton);
            var clearButton = controlView.FindViewById<Button>(Resource.Id.clearButton);

            defaultButton.Click += (sender, args) =>
            {
                List<LaunchButtonData> launchData = m_allButtons.ToList();
                GlobalSettings.SetDefaultLaunch(launchData);

                Toast.MakeText(this, "New default launch screen was set.", ToastLength.Short).Show();
            };

            saveButton.Click += (sender, args) =>
            {
                var sfd = new SaveFileDialog(this, "Save Launch Layout");
                sfd.OnSave += (o, eventArgs) =>
                {
                    GlobalSettings.SaveLaunchLayout(eventArgs.FileName, m_allButtons.ToList());

                    Toast.MakeText(this, "Launch layout was saved.", ToastLength.Short).Show();

                    sfd.Dismiss();
                };
                sfd.Show();
            };

            loadButton.Click += (sender, args) =>
            {
                var items = GlobalSettings.GetSavedLaunch();
                var ofd = new OpenFileDialog(this, items);
                ofd.OnLoad += (o, eventArgs) =>
                {
                    var launchLayout = GlobalSettings.GetLaunchLayout(eventArgs.FileName);
                    for (int i = 0; i < m_allButtons.Length; i++)
                    {
                        m_allButtons[i].Name = launchLayout[i].Name;
                        m_allButtons[i].ControlNum = launchLayout[i].ControlNum;
                    }
                    if (m_lastButton != null)
                    {
                        m_launchButtonEditor.SetTargetButton(m_lastButton);
                    }

                    UpdateCurrentButtonSet();

                    Toast.MakeText(this, "Launch layout was loaded.", ToastLength.Short).Show();

                    ofd.Dismiss();
                };
                ofd.Show();
            };

            clearButton.Click += (sender, args) =>
            {
                for (int i = 0; i < 127; i++)
                {
                    m_allButtons[i] = new LaunchButtonData();
                    m_allButtons[i].ControlNum = (byte)(i + 1);
                    m_allButtons[i].Name = m_allButtons[i].ControlNum.ToString();
                    m_allButtons[i].Color = UnPressedColour;
                }
                UpdateCurrentButtonSet();
            };
        }

        private void SetupTrayMenu(Bundle savedInstanceState)
        {
            if (savedInstanceState == null)
            {
                FragmentManager fragmentManager = FragmentManager;
                int stickTo = Intent.GetIntExtra("stickTo", 0);
                m_fragment = SlidingDrawerFragment.New("LaunchScreenSliderMenu");
                m_fragment.OnViewCreatedFinished += (sender, args) => { OnFragmentInit(); };
                fragmentManager.BeginTransaction().Replace(
                        Resource.Id.content_fragment,
                        m_fragment,
                        SlidingDrawerFragment.TAG)
                        .Commit();
                /*ViewGroup.LayoutParams parms = m_fragment.View.LayoutParameters;
                parms.Height = 900;
                m_fragment.View.LayoutParameters = parms;*/
            }
        }

        public void OnFragmentInit()
        {
            SetupButtons();
        }

        private void SetupLockButton()
        {
            //Button defaultButton = FindViewById<Button>(Resource.Id.defaultButton);
            ToggleButton button = FindViewById<ToggleButton>(Resource.Id.unlockButton);
            

            m_launchButtonEditor = FindViewById<LaunchButtonEditor>(Resource.Id.launchButtonEditor);

            button.Toggle();
            button.Text = "Edit";
            button.CheckedChange += (sender, args) =>
            {
                if (args.IsChecked)
                {
                    m_launchButtonEditor.Visibility = ViewStates.Gone;
                    //ClearHighlights();

                    //defaultButton.Visibility = ViewStates.Gone;
                    EditMode = false;
                    button.Text = "Edit";
                    button.TextOff = "Edit";
                    button.TextOn = "Edit";

                    IMidiSender midiSender = MainActivity.MidiSender;
                    midiSender.SendControlChange(MidiHelper.Channel, ChannelModeMessageType.AllNotesOff);

                    if (m_lastButton != null)
                    {
                        m_lastButton.Background = UnPressedDrawable;
                        m_lastButton = null;
                    }
                }
                else
                {
                    m_launchButtonEditor.Visibility = ViewStates.Visible;
                    //defaultButton.Visibility = ViewStates.Visible;
                    EditMode = true;
                    button.Text = "Lock";
                    button.TextOff = "Lock";
                    button.TextOn = "Lock";

                    var firstButton = m_buttons.First();
                    firstButton.Background = EditButtonDrawable;
                    m_launchButtonEditor.SetTargetButton(firstButton);

                    m_lastButton = firstButton;
                }
            };
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            IMidiSender sender = MainActivity.MidiSender;
            if (keyCode == Keycode.Back)
            {
                sender.SendControlChange(MidiHelper.Channel, ChannelModeMessageType.AllNotesOff);
                Finish();
                return true;
            }
            if (keyCode == Keycode.VolumeUp)
            {
                OctaveDown(null, null);
                return true;
            }
            if (keyCode == Keycode.VolumeDown)
            {
                OctaveUp(null, null);
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private void SetupProgressBars()
        {
            IMidiSender sender = MainActivity.MidiSender;

            bool showSlider1 = true;
            bool showSlider2 = true;

            if (!showSlider1)
            {
                FaderControl fader1 = FindViewById<FaderControl>(Resource.Id.fader1);
                fader1.Visibility = ViewStates.Gone;
            }
            else
            {
                FaderControl fader1 = FindViewById<FaderControl>(Resource.Id.fader1);
                fader1.ControllerNumber = 13;
                fader1.OnValueChanged += (o, a) =>
                {
                    //Console.WriteLine(a.Value);
                    sender.SendControlChange(MidiHelper.Channel, fader1.ControllerNumber, (byte)(a.Value * 127.0f));
                };
                fader1.Value = 0.85f;
            }

            if (!showSlider2)
            {
                FaderControl fader2 = FindViewById<FaderControl>(Resource.Id.fader2);
                fader2.Visibility = ViewStates.Gone;
            }
            else
            {
                FaderControl fader2 = FindViewById<FaderControl>(Resource.Id.fader2);
                fader2.ControllerNumber = 14;
                fader2.OnValueChanged += (o, a) =>
                {
                    sender.SendControlChange(MidiHelper.Channel, fader2.ControllerNumber, (byte)(a.Value * 127.0f));
                };
                fader2.Value = 0.85f;
            }
        }

        private void SetupTouchStrip()
        {
            IMidiSender midisender = MainActivity.MidiSender;
            bool useTouchStrip = true;
            if (!useTouchStrip)
            {
                TouchStrip strip = FindViewById<TouchStrip>(Resource.Id.touchStrip);
                strip.Visibility = ViewStates.Gone;
            }
            else
            {
                TouchStrip strip = FindViewById<TouchStrip>(Resource.Id.touchStrip);
                strip.StartTrackingTouch += (sender, e) =>
                {
                    var touchStrip = sender as TouchStrip;
                    CalcPitchBend(e, touchStrip);
                };
                strip.StopTrackingTouch += (sender, e) =>
                {
                    midisender.SendPitchBendChange(MidiHelper.Channel, 0x0, 0x40);
                };
                strip.UpdateTrackingTouch += (sender, e) =>
                {
                    var touchStrip = sender as TouchStrip;
                    CalcPitchBend(e, touchStrip);
                };
            }
        }

        private void CalcPitchBend(MotionEvent e, TouchStrip touchStrip)
        {
            //float screenX = e.GetX();
            float screenY = e.GetY();
            //float viewX = screenX - touchStrip.Left;
            float viewY = screenY - touchStrip.Top;
            if (viewY < 0)
            {
                viewY = 0;
            }
            if (viewY > screenY)
            {
                viewY = screenY;
            }

            float halfHeight = touchStrip.HalfHeight;
            byte lsb = 0x0;
            byte msb = 0x0;
            int valueToBend = 0;

            if (viewY < halfHeight)
            {
                float percent = 1 - (viewY / halfHeight);
                valueToBend = (MidiOctave.DefaultPitchBend + (int)(MidiOctave.DefaultPitchBend * percent));
            }
            else if (viewY > halfHeight)
            {
                float percent = 1 - ((halfHeight - (viewY - halfHeight)) / halfHeight);
                valueToBend = (MidiOctave.DefaultPitchBend - (int)(MidiOctave.DefaultPitchBend * percent));
            }
            else
            {
                valueToBend = MidiOctave.DefaultPitchBend;
            }

            msb = MidiHelper.PitchBendCalcMsb(valueToBend);
            lsb = MidiHelper.PitchBendCalcLsb(valueToBend);

            IMidiSender midisender = MainActivity.MidiSender;
            midisender.SendPitchBendChange(MidiHelper.Channel, lsb, msb);
        }

        public void OctaveUp(object o, EventArgs e)
        {
            if (CurrentOctave == 10) return;

            try
            {
                IMidiSender midisender = MainActivity.MidiSender;
                midisender.SendControlChange(MidiHelper.Channel,
                    ChannelModeMessageType.AllNotesOff);

                var futureNotes = m_buttons.Select(x => x.ControlNum).Select(x => x + MidiOctave.ShiftAmount);

                if (CurrentRoot + (byte)MidiOctave.ShiftAmount > 127)
                {
                    return;
                }
                else
                {
                    CurrentRoot += (byte)MidiOctave.ShiftAmount;
                }

                CurrentOctave = (int)(CurrentRoot / 12.0f);

                int loopCount = CurrentRoot;
                foreach (var b in m_buttons)
                {
                    byte tag = (byte)b.ControlNum;

                    int newNote = tag + (byte)MidiOctave.ShiftAmount;

                    if (newNote > 127)
                    {
                        b.Enabled = false;
                        b.Text = "#";
                    }
                    else
                    {
                        b.UpdateWith(m_allButtons[loopCount]);
                        b.Text = b.Name;
                        b.Enabled = true;
                    }
                    loopCount++;
                }
            }
            catch (Exception)
            {

            }
        }

        public void OctaveDown(object o, EventArgs e)
        {
            try
            {
                IMidiSender midisender = MainActivity.MidiSender;
                midisender.SendControlChange(MidiHelper.Channel,
                    ChannelModeMessageType.AllNotesOff);

                if ((CurrentRoot - (byte)MidiOctave.ShiftAmount) >= 0)
                {
                    CurrentRoot -= (byte)MidiOctave.ShiftAmount;
                }
                else
                {
                    return;
                }

                CurrentOctave = (int)(CurrentRoot / 12.0f);

                int loopCount = CurrentRoot;
                foreach (var b in m_buttons)
                {
                    byte tag = (byte)b.ControlNum;

                    int newNote = tag - (byte)MidiOctave.ShiftAmount;

                    if (newNote > 127)
                    {
                        b.Enabled = false;
                        b.Text = "#";
                    }
                    else
                    {
                        b.UpdateWith(m_allButtons[loopCount]);
                        b.Text = b.Name;
                        b.Enabled = true;
                    }
                    loopCount++;
                }
            }
            catch (Exception)
            {

            }
        }

        private void UpdateCurrentButtonSet()
        {
            int loopCount = CurrentRoot;
            foreach (var b in m_buttons)
            {
                var bd = m_allButtons[loopCount];
                b.UpdateWith(bd);
                b.Text = bd.Name;
                b.Enabled = true;

                loopCount++;
            }
        }

        public async void DoButtonStartupAnimation()
        {
            int delayMillis = 70;
            EnableAllButtons(false);
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < m_buttons.Count; i++)
                {
                    Button b = m_buttons[i];
                    SetSingleButtonColour(PressedColour, b);
                    await Task.Delay(delayMillis);
                }
            }
            SetAllButtonsToColour(UnPressedColour);
            EnableAllButtons(true);
        }

        private void SetAllButtonsToColour(Color color)
        {
            foreach (var b in m_buttons)
            {
                b.SetBackgroundColor(color);
                b.Invalidate();
            }
        }

        private void SetSingleButtonColour(Color colour, Button b)
        {
            foreach (var button in m_buttons)
            {
                if ((byte)button.Tag == (byte)b.Tag)
                {
                    button.SetBackgroundColor(colour);
                }
                else
                {
                    button.SetBackgroundColor(UnPressedColour);
                }
                button.Invalidate();
            }
        }

        private void EnableAllButtons(bool val)
        {
            m_buttons.ForEach(x => x.Enabled = val);
        }
    }
}