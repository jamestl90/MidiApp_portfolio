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

using TestApp.Midi.RTP.Journal;

using ChannelNumber = System.UInt16;
using SeqNum = System.UInt16;
using MidiMessageList = System.Collections.Generic.List<TestApp.Midi.RTP.RtpMidiMessage>;

namespace TestApp.Midi.RTP.Journal
{
    public class RtpJournal : IBufferGenerator, IBufferReceiver
    {
        /*
             0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3
            +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            |S|Y|A|H|TOTCHAN|   Checkpoint Packet Seqnum    |
            +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */

        #region Recovery Journal Header Fields

        public bool S { get; private set; }
        public bool Y { get; private set; }
        public bool A { get; private set; }
        public bool H { get; private set; }

        public uint TotChan { get; private set; } // totchan + 1
        public ushort CheckPointPacketSeqNum { get; private set; }

        public ushort MostRecentSeqNum = 0;

        private readonly byte[] m_header = new byte[3];

        #endregion

        private Dictionary<ChannelNumber, Dictionary<SeqNum, MidiMessageList>> m_journalDic = new Dictionary<ChannelNumber, Dictionary<SeqNum, MidiMessageList>>();

        #region Channel Journals

        private readonly RtpChannelJournal[] m_channelJournals = new RtpChannelJournal[16];

        #endregion

        public RtpJournal()
        {
            // use setters 
            SetS(true);
            SetY(false);
            SetA(false);
            SetH(false);
            SetTotChan(0);
            SetCheckpointPacketSeqNum(0);
        }

        public MidiMessageList GetSessionHistory(ushort seqNum)
        {
            return m_journalDic.SelectMany(x => x.Value)
                               .Where(x => x.Key < MostRecentSeqNum && x.Key >= seqNum)
                               .SelectMany(x => x.Value)
                               .ToList();                        
        }

        public void UpdateWith(RtpPacket rtpPacket, List<RtpMidiMessage> messages)
        {            
            foreach (var cmd in messages)
            {
                ushort channel = cmd.GetChannel();

                if (!m_journalDic.ContainsKey(channel)) // Is this channel not logged yet?
                {
                    m_journalDic.Add(channel, new Dictionary<ushort, List<RtpMidiMessage>>());
                    m_journalDic[channel].Add(rtpPacket.SequenceNumber.Short(), new MidiMessageList(new [] { cmd }));                   
                }
                else
                {                    
                    if (!m_journalDic[channel].ContainsKey(rtpPacket.SequenceNumber.Short()))
                    {
                        m_journalDic[channel].Add(rtpPacket.SequenceNumber.Short(), new MidiMessageList(new [] { cmd }));
                    }
                    else
                    {
                        m_journalDic[channel][rtpPacket.SequenceNumber.Short()].Add(cmd);
                    }                    
                }
            }

            // update the channel journals
            for (int i = 0; i < m_journalDic.Keys.Count; i++)
            {
                if (m_channelJournals[i] == null)
                {
                    m_channelJournals[i] = new RtpChannelJournal((ushort)i);
                }
                m_channelJournals[i].UpdateWith(m_journalDic[m_journalDic.Keys.ElementAt(i)]);
            }

            SetA(m_channelJournals.Any(x => x != null));
        }

        public byte[] GetJournal(uint esn, ushort sessionFirstSeqNum)
        {
            var sessionHistory = GetSessionHistory(sessionFirstSeqNum);
            return m_header;
        }        

        #region Setters

        public void SetS(bool val)
        {
            S = val;
            byte mask = (1 << 7);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetY(bool val)
        {
            Y = val;
            byte mask = (1 << 6);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetA(bool val)
        {
            A = val;
            byte mask = (1 << 5);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetH(bool val)
        {
            H = val;
            byte mask = (1 << 4);
            BufferHelpers.SetBit(m_header, val, mask, 0);
        }

        public void SetTotChan(uint totChan)
        {
            TotChan = totChan;
            m_header[0] |= (byte) (TotChan & 0x0F);
        }

        public void SetCheckpointPacketSeqNum(ushort cpsn)
        {
            CheckPointPacketSeqNum = cpsn;
            BufferHelpers.PutShort(m_header, 1, CheckPointPacketSeqNum);
        }

        #endregion

        public byte[] GetBuffer()
        {
            var header = m_header;

            if (!A && !Y)
            {
                return header; // empty journal
            }

            if (Y) // system journal is present
            {
                throw new NotImplementedException();
            }
            if (A) // channel journal
            {
                
            }

            return header;
        }

        public void ParseBuffer(byte[] buffer)
        {

        }
    }
}