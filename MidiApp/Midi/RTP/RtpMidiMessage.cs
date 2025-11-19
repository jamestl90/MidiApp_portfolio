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

namespace TestApp.Midi.RTP
{
    public class RtpMidiMessage : IBufferGenerator, IBufferReceiver
    {
        public int NumMessages = 1;

        public ushort GetChannel()
        {
            return (ushort)(m_bytes[0] & 0xF);
        }

        public byte GetStatus()
        {
            return (byte) (m_bytes[0] & 0xF0);
        }

        public bool UsesRunningStatus = false;

        private byte[] m_bytes = null;

        public RtpMidiMessage(byte status, byte data1, byte data2)
        {
            SetBytes(status, data1, data2);
        }

        public RtpMidiMessage(byte status, byte root, int[] intervals, byte velocity)
        {
            SetChordBytes(status, root, intervals, velocity);
        }

        private void SetChordBytes(byte status, byte root, int[] intervals, byte velocity)
        {
            if (intervals == null || intervals.Length == 0) return;            

            int numIntervals = 0;
            for (int i = 0; i < intervals.Length; ++i)
            {
                if ((byte)(intervals[i] + root) <= 127) // only send chord notes within the midi note range
                {
                    numIntervals++;
                }
            }

            NumMessages = numIntervals + 1;

            if (m_bytes == null)
                m_bytes = new byte[NumMessages * 3];            

            m_bytes[0] = status;
            m_bytes[1] = root;
            m_bytes[2] = velocity;

            int start = 3;
            for (int i = 0; i < NumMessages - 1; i++)
            {
                m_bytes[start++] = status;
                m_bytes[start++] = (byte)(root + intervals[i]);
                m_bytes[start++] = velocity;
            }
        }

        private void SetBytes(byte status, byte data1, byte data2)
        {
            if (m_bytes == null)
                m_bytes = new byte[3];

            int numBytes = 0;

            m_bytes[numBytes++] = status;
            m_bytes[numBytes++] = data1;
            m_bytes[numBytes++] = data2;
        }

        public byte[] GetBuffer()
        {
            return m_bytes;
        }

        public byte[] GetBuffer(int offset, int len)
        {
            return m_bytes.Skip(offset).Take(len).ToArray();
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}