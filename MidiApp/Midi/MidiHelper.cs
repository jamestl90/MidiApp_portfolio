using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media.Midi;
using Android.Text.Method;
using Android.Util;
using TestApp.Midi;

/* Midi specs taken from below website
 * 
 * https://www.midi.org/specifications/item/table-1-summary-of-midi-message
 * 
 */
namespace TestApp.Midi
{
    public static class MidiHelper
    {        
        public static string MidiOverBtleUuid = "03B80E5A-EDE8-4B33-A751-6CE34EC4C700";

        public static MidiDevice OpenedDevice;
        public static int Channel;
        public static byte Velocity;
        public static int Octave;
        public static string Key;

        public static Dictionary<byte, NoteState> NoteStatus = new Dictionary<byte, NoteState>();        

        public static void SendChordOn(IMidiSender sender, int channel, byte velocity, MidiChord chord)
        {
            // send root on
            sender.SendNoteOn(channel, chord.Root, velocity);

            foreach (var interval in chord.Intervals)
            {
                byte note = (byte) (interval + chord.Root);
                if (note <= 127)
                {
                    sender.SendNoteOn(channel, note, velocity);
                }
            }
        }

        public static void SendChordOff(IMidiSender sender, int channel, MidiChord chord)
        {
            // send root off
            sender.SendNoteOff(channel, chord.Root);

            foreach (var interval in chord.Intervals)
            {
                byte note = (byte)(interval + chord.Root);
                if (note <= 127)
                {
                    sender.SendNoteOff(channel, note);
                }
            }
        }

        public static void DumpProperties()
        {
            Console.WriteLine("Channel: {0}\n", Channel);
            Console.WriteLine("Velocity: {0}\n", Velocity);
            Console.WriteLine("Octave: {0}\n", Octave);
            Console.WriteLine("Key: {0}\n", Key);
        }

        // value should range from 0 to 16383 (14 bit value)
        public static byte PitchBendCalcLsb(int value)
        {
            return (byte) (value & 0x7f);
        }

        // value should range from 0 to 16383 (14 bit value)
        public static byte PitchBendCalcMsb(int value)
        {
            return (byte) ((value >> 7) & 0x7f);
        }

        public static List<byte> GetNotesFromButtons(List<NoteButton> buttons, int offset)
        {
            return buttons.Select(x => (byte)((byte)x.Tag + offset)).ToList();
        }

        public static List<byte> GetNotesFromChordButtons(List<NoteButton> buttons, int offset)
        {
            return buttons.Select(x => (byte)(((MidiChord)x.Tag).Root + offset)).ToList();
        }
    }

    public static class MidiControlsHelper
    {
        public static void ChannelPickerPopup(Context context, View parent, int width, int height, 
            int px, int py, GravityFlags flags, ControlInfo controlInfo, int currControllerNumber)
        {
            if (width == 0)
            {
                width = (int)context.Resources.GetDimension(Resource.Dimension.channel_picker_popup_width);
            }
            if (height == 0)
            {
                height = (int)context.Resources.GetDimension(Resource.Dimension.channel_picker_popup_height);
            }

            var layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            var view = layoutInflater.Inflate(Resource.Layout.ChannelNumberPickerLayout, null);

            PopupWindow window = new PopupWindow(view, width, height, true);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                window.Elevation = 2.0f;
            }

            var numberPicker = view.FindViewById<CustomItemPicker>(Resource.Id.customItemPicker);
            numberPicker.IsDataNumeric = true;
            numberPicker.Editable = true;

            List<CustomItemPicker.CustomItem> items = new List<CustomItemPicker.CustomItem>();
            for (int i = 0; i < 100; i++)
            {
                items.Add(new CustomItemPicker.CustomItem
                {
                    Index = i,
                    Text = (i + 1).ToString()
                });
            }
            numberPicker.Items = items;
            numberPicker.CurrentItem = currControllerNumber - 1;

            numberPicker.OnValueChanged += (sender, args) =>
            {
                controlInfo.ControllerNumber = (byte)(args.Item.Index + 1);
            };

            var okButton = view.FindViewById<Button>(Resource.Id.okButton);
            okButton.Click += (sender, args) =>
            {
                window.Dismiss();
            };

            //window.ShowAtLocation(this, GravityFlags.Center, 0, 0);
            //window.ShowAsDropDown(parent, width / 2, height);
            window.ShowAtLocation(parent, flags, px, py);
        }

        public static void ChannelPickerXYPopup(Context context, View parent, int width, int height,
            int px, int py, GravityFlags flags, MultiControlsInfo controlInfo, int controllerNumberX, int controllerNumberY)
        {
            if (width == 0)
            {
                width = (int)context.Resources.GetDimension(Resource.Dimension.channel_picker_popup_width);
            }
            if (height == 0)
            {
                height = (int)context.Resources.GetDimension(Resource.Dimension.channel_picker_popupXY_height);
            }

            var layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            var view = layoutInflater.Inflate(Resource.Layout.XyChannelNumberPickerLayout, null);

            PopupWindow window = new PopupWindow(view, width, height, true);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                window.Elevation = 2.0f;
            }

            List<CustomItemPicker.CustomItem> items = new List<CustomItemPicker.CustomItem>();
            for (int i = 0; i < 100; i++)
            {
                items.Add(new CustomItemPicker.CustomItem
                {
                    Index = i,
                    Text = (i + 1).ToString()
                });
            }

            var numberPicker = view.FindViewById<CustomItemPicker>(Resource.Id.channelNumberPickerX);
            numberPicker.IsDataNumeric = true;
            numberPicker.Editable = true;            
            numberPicker.Items = items;
            numberPicker.CurrentItem = controllerNumberX - 1;

            numberPicker.OnValueChanged += (sender, args) =>
            {
                controlInfo.ControllerNumber1 = (byte)(args.Item.Index + 1);
            };

            var okButton = view.FindViewById<Button>(Resource.Id.okButton);
            okButton.Click += (sender, args) =>
            {
                window.Dismiss();
            };

            numberPicker = view.FindViewById<CustomItemPicker>(Resource.Id.channelNumberPickerY);
            numberPicker.IsDataNumeric = true;
            numberPicker.Editable = true;
            numberPicker.Items = items;
            numberPicker.CurrentItem = controllerNumberY - 1;

            numberPicker.OnValueChanged += (sender, args) =>
            {
                controlInfo.ControllerNumber2 = (byte)(args.Item.Index + 1);
            };

            window.ShowAtLocation(parent, flags, px, py);
        }

        public static NoteButton CreatePad(Context context,
                               byte tag,
                               Drawable initialStateDrawable,
                               Android.Graphics.Color textColor,
                               string text,
                               bool showNoteNames,
                               int currentOctave,
                               ITransformationMethod transformMethod,
                               Action<object, View.TouchEventArgs> touchMethod)
        {
            NoteButton b = new NoteButton(context);
            b.Tag = tag;
            if ((byte)b.Tag > 127)
            {
                b.Enabled = false;
                b.Text = "#";
            }
            else if (showNoteNames)
            {
                b.Text = MidiScales.GetNoteAsString((byte)b.Tag, currentOctave);
            }
            b.Background = initialStateDrawable;
            
            b.SetTextColor(textColor);
            b.TextSize = (int)context.Resources.GetDimension(Resource.Dimension.text_size);
            b.TransformationMethod = transformMethod;
            b.Touch += touchMethod.Invoke;

            return b;
        }

        public static NoteButton CreateChordPad(Context context,
                                       byte tag,
                                       Drawable initialStateDrawable,
                                       Android.Graphics.Color textColor,
                                       string text,
                                       ITransformationMethod transformMethod,
                                       Action<object, View.TouchEventArgs> touchMethod)
        {
            NoteButton b = new NoteButton(context);
            b.Tag = MidiChords.MajorTriad(tag);
            b.Text = text;
            b.Background = initialStateDrawable;
            b.SetTextColor(Color.Gray);
            b.TextSize = (int)context.Resources.GetDimension(Resource.Dimension.text_size); 
            b.TransformationMethod = null;
            b.Touch += touchMethod.Invoke;
            return b;
        }

        public static Action<object, View.TouchEventArgs> CreatePadTouchAction(Android.Graphics.Drawables.Drawable unpressed,
                                                                               Android.Graphics.Drawables.Drawable pressed,
                                                                               Action<NoteButton, object> onPadPress,
                                                                               Action<NoteButton, object> onPadRelease,
                                                                               Action<NoteButton, object> onPadMove)
        {
            return (o, e) =>
            {

                    NoteButton button = o as NoteButton;
                    if (e.Event.Action == MotionEventActions.Down)
                    {                        
                        onPadPress.Invoke(button, e);
                    }
                    else if (e.Event.Action == MotionEventActions.Up)
                    {
                        onPadRelease.Invoke(button, e);
                    }
                    else if (e.Event.Action == MotionEventActions.Move)
                    {
                        onPadMove.Invoke(button, e);
                    }
                    //MainActivity.InputPort.Flush();
            };
        }

        public static GridLayout.LayoutParams CreatePadLayoutParams(int i, int j)
        {
            GridLayout.LayoutParams gridParams = new GridLayout.LayoutParams();
            gridParams.SetGravity(GravityFlags.FillHorizontal);
            gridParams.ColumnSpec = GridLayout.InvokeSpec(i, 1f);
            gridParams.RowSpec = GridLayout.InvokeSpec(j, 1f);
            gridParams.SetMargins(2, 2, 2, 2);
            return gridParams;
        }


        public static Android.Support.V7.Widget.GridLayout.LayoutParams CreatePadLayoutParamsSupportV7(int i, int j)
        {
            Android.Support.V7.Widget.GridLayout.LayoutParams gridParams = new Android.Support.V7.Widget.GridLayout.LayoutParams();
            gridParams.SetGravity((int)GravityFlags.Fill);
            
            gridParams.ColumnSpec = Android.Support.V7.Widget.GridLayout.InvokeSpec(i, 1f);
            gridParams.RowSpec = Android.Support.V7.Widget.GridLayout.InvokeSpec(j, 1f);
            gridParams.SetMargins(2, 2, 2, 2);
            return gridParams;
        }

        public static void InitialiseFader(IMidiSender sender, FaderControl fader, byte controllerNum)
        {
            fader.ControllerNumber = controllerNum;
            fader.OnValueChanged += (o, a) =>
            {
                sender.SendControlChange(MidiHelper.Channel, fader.ControllerNumber, (byte)(a.Value * 127.0f));
                //MainActivity.InputPort.Flush();
            };
            fader.Value = 0.85f;
        }
    }
}
