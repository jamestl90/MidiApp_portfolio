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
using Java.Lang;
using TestApp.Midi.RTP;

namespace TestApp.Midi.RTP
{
    /*
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  Delta Time 0     (1-4 octets long, or 0 octets if Z = 0)     |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  MIDI Command 0   (1 or more octets long)                     |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  Delta Time 1     (1-4 octets long)                           |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  MIDI Command 1   (1 or more octets long)                     |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |                              ...                              |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  Delta Time N     (1-4 octets long)                           |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          |  MIDI Command N   (0 or more octets long)                     |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    */

    public class RtpMidiCommands : IBufferGenerator, IBufferReceiver
    {
        private readonly List<byte[]> m_buffers = new List<byte[]>();
        
        public List<RtpMidiMessage> MidiMessages = new List<RtpMidiMessage>();

        private bool m_b = false; // header is 1 octet long (true = 2 octets)
        private bool m_j = false; // no journal
        private bool m_z = false; // no delta time 0 field
        private bool m_p = false; // not sure?
        private int m_maxLen = 15; // bytes/octets

        public RtpMidiCommands(bool journal)
        {            
            // store journal logs in here of midi messages?
            m_j = journal;
        }

        public void AddMidiMessage(uint deltaTime, RtpMidiMessage msg) // midiPacket.GetBuffer()
        {
            MidiMessages.Add(msg);

            if (m_buffers.Count == 0)
            {
                // check for running status
                if (msg.UsesRunningStatus)
                {
                    m_p = true; // phantom bit is true if first command is relying on running status of previous command
                }
            }

            var deltaTimeBuffer = BufferHelpers.EncodeVlv(deltaTime);

            if (msg.NumMessages > 1)
            {                
                for (int i = 0; i < msg.NumMessages; i++)
                {
                    m_buffers.Add(deltaTimeBuffer);
                    m_buffers.Add(msg.GetBuffer(i * 3, 3));
                }                
            }
            else
            {
                m_buffers.Add(deltaTimeBuffer);
                m_buffers.Add(msg.GetBuffer());
            }     
        }

        public void ClearCommands()
        {
            m_buffers.Clear();
            MidiMessages.Clear();
        }

        private byte[] GetHeaderBuffer(int commandLength)
        {
            byte[] header = null;

            byte firstByte = 0;
            byte secondByte = 0;

            m_b = commandLength > 15; // in bytes
            m_maxLen = m_b ? 4095 : 15; // in bytes

            if (commandLength > m_maxLen)
                throw new System.Exception("Command is greater than 4095");

            firstByte = BufferHelpers.SetByteFlags(m_b, m_j, m_z, m_p, false, false, false, false);

            if (m_b)
            {
                firstByte |= (byte) ((commandLength >> 8) & 0x0F);
                secondByte |= (byte) (commandLength & 0xFF);
                header = new []
                {
                    firstByte, secondByte
                };                
            }
            else
            {
                firstByte |= (byte) (commandLength & 0x0F);
                header = new []
                {
                    firstByte
                };                
            }
            return header;
        }

        public byte[] GetBuffer()
        {
            m_z = true;
            bool deltaTimeZero = (m_buffers[0].Length == 1 && m_buffers[0].ElementAt(0) == 0x0);
            if (deltaTimeZero)
            {
                m_buffers.RemoveAt(0);
                m_z = false;
            }

            int commandLength = m_buffers.Sum(x => x.Length); // in bytes

            byte[] messageListHeader = GetHeaderBuffer(commandLength);

            byte[] finalBuffer = new byte[commandLength + messageListHeader.Length];
            int offset = 0;

            messageListHeader.CopyTo(finalBuffer, 0);
            offset += messageListHeader.Length;

            foreach (var buffer in m_buffers)
            {
                buffer.CopyTo(finalBuffer, offset);
                offset += buffer.Length;
            }

            return finalBuffer;
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}