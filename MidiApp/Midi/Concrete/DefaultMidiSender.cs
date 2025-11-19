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

namespace TestApp.Midi.Concrete
{
    public class DefaultMidiSender : IMidiSender
    {
        public void SendChordOn(MidiChord chord, int channel, byte velocity)
        {

        }

        public void SendChordOff(MidiChord notes, int channel)
        {

        }

        public void Send(byte status, byte data1, byte data2)
        {

        }

        public void SendNoteOn(int channel, byte pitch, byte velocity)
        {

        }

        public void SendNoteOff(int channel, byte pitch, byte velocity = 0x40)
        {

        }

        public void SendControlChange(int channel, ChannelModeMessageType messageType, byte extra = 0)
        {

        }

        public void SendControlChange(int channel, byte controllerNum, byte controllerVal)
        {

        }

        public void SendPitchBendChange(int channel, byte lsb, byte msb)
        {

        }

        public void Flush()
        {

        }

        public bool Suspend { get; set; }
    }
}