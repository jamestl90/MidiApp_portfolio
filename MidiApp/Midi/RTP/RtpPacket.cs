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
    /*
        0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |V=2|P|X|  CC   |M|     PT      |       sequence number         |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |                           timestamp                           |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
       |           synchronization source (SSRC) identifier            |
       +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
       |            contributing source (CSRC) identifiers             |
       |                             ....                              |
       +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   */

    public class RtpPacket : IBufferGenerator, IBufferReceiver
    {
        public const int HeaderSize = 12; // 12 bytes
        private const int PayloadType = 0x61; // 7 bits
        private const int Version = 2; // 2 bits
        private bool m_padding = false; // 1 bit
        private bool m_extension = false; // 1 bit
        private int m_csrcCount = 0;    // 4 bits
        private bool m_marker = false;  // 1 bit
        public SequenceNumber SequenceNumber; // 16 bits
        public uint m_timestamp = 0; // 32 bits
        public uint m_ssrc = 0; // 32 bits

        private byte[] m_payloadBytes = null;
        private byte[] m_journalBytes = null;
        private readonly byte[] m_headerBytes = new byte[HeaderSize];

        public RtpPacket(ushort seqNum, uint timestamp, uint ssrc)
        {
            SequenceNumber = new SequenceNumber(seqNum);
            m_timestamp = timestamp;
            m_ssrc = ssrc;
        }

        public void SetPayload(byte[] payloadBytes)
        {
            m_payloadBytes = payloadBytes;
        }

        public void SetJournalBytes(byte[] journalBytes)
        {
            m_journalBytes = journalBytes;
        }

        public byte[] GetPayload()
        {
            return m_payloadBytes;
        }

        public byte[] GetBuffer()
        {
            bool hasJournal = m_journalBytes != null;

            if (m_payloadBytes == null)
            {
                throw new Exception("No payload specified");
            }

            byte firstByte = 0;
            firstByte |= Version << 6;
            if (m_padding)
            {
                firstByte |= 0x20;
            }
            if (m_extension)
            {
                firstByte |= 0x10;
            }
            if (m_csrcCount > 0)
            {
                firstByte |= (byte)(m_csrcCount & 0x0F);
            }

            byte secondByte = 0;
            if (m_marker)
            {
                secondByte |= 0x80;
            }
            secondByte |= (PayloadType & 0x7F);
            //secondByte |= 0x80; // no marker

            int offset = 0;
            BufferHelpers.PutByte(m_headerBytes, offset, firstByte); // 1
            offset += 1;
            BufferHelpers.PutByte(m_headerBytes, offset, secondByte); // 1
            offset += 1;
            BufferHelpers.PutShort(m_headerBytes, offset, SequenceNumber.Short()); // 2
            offset += 2;
            BufferHelpers.PutInt(m_headerBytes, offset, m_timestamp); // 4
            offset += 4;
            BufferHelpers.PutInt(m_headerBytes, offset, m_ssrc);  // 4

            int length = (hasJournal ? m_journalBytes.Length : 0) + m_headerBytes.Length + m_payloadBytes.Length;

            var buffer = new byte[length];
            offset = 0;
            m_headerBytes.CopyTo(buffer, offset);
            offset += m_headerBytes.Length;
            m_payloadBytes.CopyTo(buffer, offset);            

            if (hasJournal)
            {
                buffer[offset] |= 0x40; // set the journal bit

                offset += m_payloadBytes.Length;
                m_journalBytes.CopyTo(buffer, offset);
            }

            return buffer;
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}