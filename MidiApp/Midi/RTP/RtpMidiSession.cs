using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TestApp.Midi.Apple;
using TestApp.Midi.Concrete;

namespace TestApp.Midi.RTP
{
    public class RtpMidiSession
    {
        public NetworkMidiSender MidiSender;

        public AppleMidiSession AppleMidi;

        public IPEndPoint EndPoint;
        public SequenceNumber SequenceNumber;
        public SequenceNumber LastSeqNum;
        public uint Ssrc;
        public uint PartnerSsrc;
        public string Name;
        public uint SessionToken;

        public ulong StreamSendCount = 0;
        public ulong LastReceivedSeqNum = 0;
        public ulong FirstStreamSeqNum = 0;
        public ulong ReceivedCount = 0;

        public RtpMidiSession(AppleMidiSession sessionOwner, uint token, uint ssrc, uint partnerSsrc, IPEndPoint endpoint, string name = "")
        {
            SessionToken = token;
            EndPoint = endpoint;
            AppleMidi = sessionOwner;
            Ssrc = ssrc;
            PartnerSsrc = partnerSsrc;
            SequenceNumber = new SequenceNumber(new Random(DateTime.Now.Millisecond));
            LastSeqNum = new SequenceNumber(SequenceNumber);
            Name = name;
        }


        /*
            If the sender has received at least one feedback report from receiver
            k, M(k) is the most recent report of the highest RTP packet sequence
            number seen by the receiver, normalized to reflect the rollover count
            of the sender.

            If the sender has not received a feedback report from the receiver,
            M(k) is the extended sequence number of the last packet the sender
            transmitted before it became aware of the receiver.  If the sender
            became aware of this receiver before it sent the first packet in the
            stream, M(k) is the extended sequence number of the first packet in
            the stream.
        */

        public void Shutdown()
        {
            MidiSender?.Shutdown();
        }

        public void SendCommands(RtpMidiCommands commands)
        {
            LastSeqNum.Set(SequenceNumber.Long());
            SequenceNumber.Increment();

            if (StreamSendCount == 0)
            {
                FirstStreamSeqNum = LastSeqNum.Long();
            }

            uint esn = 0;

            if (ReceivedCount == 0 && StreamSendCount > 0)
            {
                esn = (uint)LastSeqNum.Long();
            }
            else if (ReceivedCount > 0 && StreamSendCount > 0)
            {
                esn = (uint)LastReceivedSeqNum;
            }
            else if (StreamSendCount == 0)
            {
                esn = (uint)FirstStreamSeqNum;
            }            

            AppleMidi.SendPacket(commands, this, esn);
            ++StreamSendCount;
        }

        public void SetLastReceivedSeqNum(uint seqNum)
        {
            ++ReceivedCount;

            LastReceivedSeqNum = seqNum;
        }
    }
}