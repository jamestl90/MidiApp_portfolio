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
using TestApp.Midi.RTP.Journal.Chapters;

namespace TestApp.Midi.RTP.Journal
{
    public class RtpChannelJournal : IBufferGenerator, IBufferReceiver
    {
        /*
            0                   1                   2                   3
            0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |S| CHAN  |H|      LENGTH       |P|C|M|W|N|E|T|A|  Chapters ... |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */

        #region Channel Journal Header

        public bool S { get; private set; }
        public ushort Chan { get; private set; }
        public bool H { get; private set; }
        public ushort Len { get; private set; } // 10 bits

        private bool P; // program change 
        public bool C { get; private set; } // channel change (need this)
        private bool M; // parameter system
        public bool W { get; private set; } // pitch wheel (need this)
        public bool N { get; private set; } // note off (need this)
        private bool E; // note command extras
        private bool T; // channel aftertouch
        private bool A; // poly aftertouch

        private readonly byte[] m_header = new byte[3];

        #endregion

        #region Channel Journal Chapters

        private JournalChapterControlChange m_jcControlChange;
        private JournalChapterNote m_jcNote;
        private JournalChapterPitchWheel m_jcPitchWheel;

        #endregion

        public RtpChannelJournal(ushort channel)
        {
            SetH(false);
            SetChan(channel);
            SetLen(0);
            SetS(true);
            
            SetC(false);
            SetN(false);
            SetW(false);

            P = false;
            M = false;
            E = false;
            T = false;
            A = false;
        }

        #region Setters

        public void SetS(bool val)
        {
            S = val;
            byte mask = (1 << 7);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetChan(ushort chan)
        {
            Chan = chan;
            m_header[0] |= (byte)((Chan & 0xF) << 3);
        }

        public void SetLen(ushort len)
        {
            Len = len;
            m_header[0] |= (byte)((Len >> 8) & 0x3);
            m_header[1] = (byte)(Len & 0xFF);
        }

        public void SetH(bool val)
        {
            H = val;
            byte mask = (1 << 2);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetC(bool val)
        {
            C = val;
            byte mask = (1 << 6);
            BufferHelpers.SetBit(m_header, val, mask, 2);
        }

        public void SetW(bool val)
        {
            W = val;
            byte mask = (1 << 4);
            BufferHelpers.SetBit(m_header, val, mask, 2);
        }

        public void SetN(bool val)
        {
            N = val;
            byte mask = (1 << 3);
            BufferHelpers.SetBit(m_header, val, mask, 2);
        }

        #endregion

        public void UpdateWith(Dictionary<ushort, List<RtpMidiMessage>> newData)
        {
            foreach (var pair in newData)
            {
                ushort seqNum = pair.Key;

                foreach (var msg in newData[seqNum])
                {
                    byte status = msg.GetStatus();

                    if (status == MidiConstants.NoteOn || status == MidiConstants.NoteOff)
                    {
                        if (m_jcNote == null)
                        {
                            SetN(true);
                            m_jcNote = new JournalChapterNote();
                        }
                        m_jcNote.UpdateWith(msg);
                    }
                    else if (status == MidiConstants.PitchBendChange)
                    {
                        if (m_jcPitchWheel == null)
                        {
                            SetW(true);
                            m_jcPitchWheel = new JournalChapterPitchWheel();
                        }
                        m_jcPitchWheel.UpdateWith(msg);
                    }
                    else if (status == MidiConstants.ControlChange)
                    {
                        if (m_jcControlChange == null)
                        {
                            SetC(true);
                            m_jcControlChange = new JournalChapterControlChange();
                        }
                        m_jcControlChange.UpdateWith(msg);
                    }
                }
            }
        }

        public byte[] GetBuffer()
        {
            var header = m_header;        

            return header;
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}