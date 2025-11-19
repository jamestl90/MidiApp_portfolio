using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media.Midi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using TestApp.Midi;

namespace TestApp.Midi.Concrete
{
    public class UsbMidiSender : MidiBase, IMidiSender
    {
        private readonly MidiInputPort m_inputPort;

        public UsbMidiSender(MidiInputPort port)
        {
            m_inputPort = port;
        }

        public void SendChordOn(MidiChord chord, int channel, byte velocity)
        {
            if (m_inputPort == null) return;

            byte status = (byte)(MidiConstants.NoteOn + (channel - 1));

            int numIntervals = 0;
            for (int i = 0; i < chord.Intervals.Length; ++i)
            {
                if ((byte)(chord.Intervals[i] + chord.Root) <= 127)
                {
                    numIntervals++;
                }
            }

            int numNotes = numIntervals + 1;
            byte[] buffer = new byte[numNotes * 3];

            buffer[0] = status;
            buffer[1] = chord.Root;
            buffer[2] = velocity;

            int start = 3;
            for (int i = 0; i < numIntervals; i++)
            {
                buffer[start++] = status;
                buffer[start++] = (byte)(chord.Root + chord.Intervals[i]);
                buffer[start++] = velocity;
            }

            m_inputPort?.Send(buffer, 0, numNotes * 3);
        }

        public void SendChordOff(MidiChord chord, int channel)
        {
            if (m_inputPort == null) return;

            byte status = (byte)(MidiConstants.NoteOff + (channel - 1));

            int numNotes = chord.Intervals.Length + 1;
            byte[] buffer = new byte[numNotes * 3];

            buffer[0] = status;
            buffer[1] = chord.Root;
            buffer[2] = 0;

            int start = 3;
            for (int i = 0; i < chord.Intervals.Length; i++)
            {
                buffer[start++] = status;
                buffer[start++] = (byte)(chord.Root + chord.Intervals[i]);
                buffer[start++] = 0;
            }

            m_inputPort?.Send(buffer, 0, numNotes * 3);
        }

        public void Send(byte status, byte data1, byte data2)
        {
            if (m_inputPort == null) return;

            byte[] buffer = new byte[4];
            int numBytes = 0;

            if (RunningStatus != status) // new running status
            {
                buffer[numBytes++] = status;
                buffer[numBytes++] = data1;
                buffer[numBytes++] = data2;
                RunningStatus = status;
            }
            else // same running status
            {
                buffer[numBytes++] = data1;
                buffer[numBytes++] = data2;
            }

            m_inputPort.Send(buffer, 0, numBytes);
        }

        public void SendNoteOn(int channel, byte pitch, byte velocity)
        {
            byte status = (byte)(MidiConstants.NoteOn + (channel - 1));  // note on
            Send(status, pitch, velocity);
        }

        public void SendNoteOff(int channel, byte pitch, byte velocity = 0x40)
        {
            byte status = (byte)(MidiConstants.NoteOff + (channel - 1)); // note off
            Send(status, pitch, velocity);
        }

        public void SendControlChange(int channel, ChannelModeMessageType messageType, byte extra = 0)
        {
            byte status = (byte)(MidiConstants.ControlChange + (channel - 1)); // control change

            switch (messageType)
            {
                case ChannelModeMessageType.AllSoundOff:
                    Send(status, 120, 0);
                    break;
                case ChannelModeMessageType.ResetAllControllers:
                    Send(status, 121, extra); // should be 0
                    break;
                case ChannelModeMessageType.LocalControl:
                    Send(status, 122, extra); // 0 for off, 127 for on
                    break;
                case ChannelModeMessageType.AllNotesOff:
                    Send(status, 123, extra);
                    break;
                case ChannelModeMessageType.OmniModeOff:
                    Send(status, 124, 0);
                    break;
                case ChannelModeMessageType.OmniModeOn:
                    Send(status, 125, 0);
                    break;
                case ChannelModeMessageType.MonoModeOn:
                    Send(status, 126, extra); // extra is number of channels (Omni Off) or 0 (Omni On)
                    break;
                case ChannelModeMessageType.PolyModeOn:
                    Send(status, 127, 0);
                    break;
            }
        }       

        public void SendControlChange(int channel, byte controllerNum, byte controllerVal)
        {
            byte status = (byte)(MidiConstants.ControlChange + (channel - 1)); // control change
            Send(status, controllerNum, controllerVal);
        }

        public void SendPitchBendChange(int channel, byte lsb, byte msb)
        {
            byte status = (byte)(MidiConstants.PitchBendChange + (channel - 1)); // pitch bend change
            Send(status, lsb, msb);
        }

        public void Flush()
        {
            m_inputPort.Flush();
        }

        public bool Suspend { get; set; }
    }
}