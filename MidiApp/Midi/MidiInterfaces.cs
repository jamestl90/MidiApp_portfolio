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
    public abstract class MidiBase
    {
        protected byte RunningStatus { get; set; }
    }

    public interface IMidiSender
    {
        bool Suspend { get; set; }

        void SendChordOn(MidiChord chord, int channel, byte velocity);
        void SendChordOff(MidiChord notes, int channel);
        void Send(byte status, byte data1, byte data2);
        void SendNoteOn(int channel, byte pitch, byte velocity);
        void SendNoteOff(int channel, byte pitch, byte velocity = 0x40);
        void SendControlChange(int channel, ChannelModeMessageType messageType, byte extra = 0);
        void SendControlChange(int channel, byte controllerNum, byte controllerVal);
        void SendPitchBendChange(int channel, byte lsb, byte msb);
        void Flush();
    }
}