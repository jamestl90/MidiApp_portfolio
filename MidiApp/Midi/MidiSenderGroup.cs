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
using TestApp.Midi.Concrete;
using SenderId = System.String;

namespace TestApp.Midi
{
    public class MidiSenderGroup : IMidiSender
    {
        private readonly Dictionary<SenderId, IMidiSender> m_senders;

        public MidiSenderGroup()
        {
            m_senders = new Dictionary<SenderId, IMidiSender>();

            //m_senders.Add("Default", new DefaultMidiSender());
        }

        public void DoSuspend(SenderId id, bool suspend)
        {
            var val = m_senders[id];

            if (val != null)
            {
                val.Suspend = suspend;
            }
        }

        public void DoSuspendBtle(bool suspend)
        {
            foreach (var bluetoothLeMidiSender in m_senders.Values.OfType<BluetoothLeMidiSender>())
                bluetoothLeMidiSender.Suspend = suspend;
        }

        public void DoSuspendWifi(bool suspend)
        {
            foreach (var networksender in m_senders.Values.OfType<NetworkMidiSender>())
                networksender.Suspend = suspend;
        }

        public void Add(SenderId id, IMidiSender sender)
        {
            if (m_senders.ContainsKey(id) && sender != null)
            {
                Remove(id);
            }
            m_senders.Add(id, sender);
        }

        public void Remove(SenderId id)
        {
            m_senders.Remove(id);
        }

        public void SendChordOn(MidiChord chord, int channel, byte velocity)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendChordOn(chord, channel, velocity);
            });
        }

        public void SendChordOff(MidiChord chord, int channel)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendChordOff(chord, channel);
            });
        }

        public void Send(byte status, byte data1, byte data2)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.Send(status, data1, data2);
            });
        }

        public void SendNoteOn(int channel, byte pitch, byte velocity)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendNoteOn(channel, pitch, velocity);
            });
        }

        public void SendNoteOff(int channel, byte pitch, byte velocity = 64)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendNoteOff(channel, pitch, velocity);
            });
        }

        public void SendControlChange(int channel, ChannelModeMessageType messageType, byte extra = 0)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendControlChange(channel, messageType, extra);
            });
        }

        public void SendControlChange(int channel, byte controllerNum, byte controllerVal)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendControlChange(channel, controllerNum, controllerVal);
            });
        }

        public void SendPitchBendChange(int channel, byte lsb, byte msb)
        {
            m_senders.Values.ToList().ForEach(x =>
            {
                if (x.Suspend) return;
                x.SendPitchBendChange(channel, lsb, msb);
            });
        }

        public void Flush()
        {
            m_senders.Values.ToList().ForEach(x => x.Flush());
        }

        public bool Suspend { get; set; }
    }
}