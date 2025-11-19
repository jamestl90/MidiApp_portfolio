using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TestApp.Midi.Apple;
using TestApp.Midi.RTP;

namespace TestApp.Midi.Concrete
{
    public class NetworkMidiSender : IMidiSender
    {
        private readonly ConcurrentQueue<RtpMidiMessage> m_midiMessageQueue = new ConcurrentQueue<RtpMidiMessage>();

        private readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();

        private readonly RtpMidiSession m_session;

        public const int HeartBeatRate = 3500; // send heartbeat every 3.5 seconds
        public const int BatchMessageRate = 15; // batch and send messages every 15 millisseconds

        public NetworkMidiSender(RtpMidiSession session)
        {
            m_session = session;
            m_session.MidiSender = this;

            uint timeSinceLastSend = 0;

            // task to batch midi messages 
            Task.Run(() =>
            {
                long lastTime = 0;

                while (true)
                {
                    if (m_tokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    
                    var currTime = Stopwatch.GetTimestamp();
                    var elapsed = currTime - lastTime;
                    var ms = ((double)elapsed / Stopwatch.Frequency) * 1000.0; // convert to milliseconds

                    if (ms >= BatchMessageRate) // every 15 milliseconds grab all available messages to batch and send
                    {
                        lastTime = currTime;

                        var count = m_midiMessageQueue.Count;

                        if (count == 0)
                        {
                            timeSinceLastSend += BatchMessageRate; // 15 ms more
                        }
                        if (timeSinceLastSend >= HeartBeatRate)   // send an empty "heartbeat" packet to keep session alive
                        {
                            RtpMidiCommands singleCommandList = new RtpMidiCommands(false);
                            byte status = (byte)MidiConstants.NoteOn;
                            RtpMidiMessage msg = new RtpMidiMessage(status, 0, 0);
                            singleCommandList.AddMidiMessage(0, msg);
                            SendBulk(singleCommandList);
                            timeSinceLastSend = 0;
                        }

                        RtpMidiCommands cmdList = new RtpMidiCommands(false);

                        while (count > 0) // get commands from the queue
                        {
                            RtpMidiMessage msg;
                            if (m_midiMessageQueue.TryDequeue(out msg))
                            {
                                cmdList.AddMidiMessage(0, msg);
                                --count;
                            }
                        }
                        if (cmdList.MidiMessages.Count != 0)
                        {
                            SendBulk(cmdList);
                            timeSinceLastSend = 0;
                        }
                    }
                }
                
            }, m_tokenSource.Token); 
        }

        public void Shutdown()
        {
            m_tokenSource.Cancel();
        }

        public void SendChordOn(MidiChord chord, int channel, byte velocity)
        {
            byte status = (byte)(MidiConstants.NoteOn + (channel - 1));
            RtpMidiMessage msg = new RtpMidiMessage(status, chord.Root, chord.Intervals.Cast<int>().ToArray(), velocity);
            msg.UsesRunningStatus = false;    
            Send(msg);     
        }

        public void SendChordOff(MidiChord chord, int channel)
        {
            byte status = (byte)(MidiConstants.NoteOn + (channel - 1));
            RtpMidiMessage msg = new RtpMidiMessage(status, chord.Root, chord.Intervals.Cast<int>().ToArray(), 0);
            msg.UsesRunningStatus = false;
            Send(msg);
        }

        public void Send(byte status, byte data1, byte data2)
        {
            RtpMidiMessage msg = new RtpMidiMessage(status, data1, data2);
            Send(msg);
        }

        private void Send(RtpMidiMessage msg)
        {     
            m_midiMessageQueue.Enqueue(msg);
        }

        private void SendBulk(RtpMidiCommands commands)
        {
            m_session.SendCommands(commands);
        }

        public void SendNoteOn(int channel, byte pitch, byte velocity)
        {
            byte status = (byte)(MidiConstants.NoteOn + (channel - 1));
            RtpMidiMessage msg1 = new RtpMidiMessage(status, pitch, velocity);
            
            Send(msg1);
        }

        public void SendNoteOff(int channel, byte pitch, byte velocity = 0x40)
        {
            byte status = (byte)(MidiConstants.NoteOff + (channel - 1)); // note off
            RtpMidiMessage msg1 = new RtpMidiMessage(status, pitch, velocity);

            Send(msg1);
        }

        public void SendControlChange(int channel, ChannelModeMessageType messageType, byte extra)
        {
            byte status = (byte)(MidiConstants.ControlChange + (channel - 1)); // control change

            byte data1 = 0;            

            switch (messageType)
            {
                case ChannelModeMessageType.AllSoundOff:
                    extra = 0;
                    data1 = 120;
                    break;
                case ChannelModeMessageType.ResetAllControllers:
                    extra = 0;
                    data1 = 121;
                    break;
                case ChannelModeMessageType.LocalControl:
                    data1 = 122;
                    break;
                case ChannelModeMessageType.AllNotesOff:
                    data1 = 123;
                    break;
                case ChannelModeMessageType.OmniModeOff:
                    data1 = 124;
                    extra = 0;
                    break;
                case ChannelModeMessageType.OmniModeOn:
                    data1 = 125;
                    extra = 0;
                    break;
                case ChannelModeMessageType.MonoModeOn:
                    data1 = 126; // extra is number of channels (Omni Off) or 0 (Omni On)
                    break;
                case ChannelModeMessageType.PolyModeOn:
                    extra = 0;
                    data1 = 127;
                    break;
            }

            RtpMidiMessage msg1 = new RtpMidiMessage(status, data1, extra);
            Send(msg1);
        }

        public void SendControlChange(int channel, byte controllerNum, byte controllerVal)
        {
            byte status = (byte)(MidiConstants.ControlChange + (channel - 1)); // control change

            RtpMidiMessage msg1 = new RtpMidiMessage(status, controllerNum, controllerVal);
            Send(msg1);
        }

        public void SendPitchBendChange(int channel, byte lsb, byte msb)
        {
            byte status = (byte)(MidiConstants.PitchBendChange + (channel - 1)); // pitch bend change

            RtpMidiMessage msg1 = new RtpMidiMessage(status, lsb, msb);
            Send(msg1);
        }

        public void Flush()
        {
            for (int i = 0; i < 16; i++)
            {
                //SendControlChange(i, ChannelModeMessageType.AllSoundOff, 0);
            }
        }

        public bool Suspend { get; set; }
    }
}