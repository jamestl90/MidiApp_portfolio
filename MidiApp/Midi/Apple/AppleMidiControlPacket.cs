using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TestApp.Midi.RTP;

namespace TestApp.Midi.Apple
{
    /*
     *  https://developer.apple.com/library/content/documentation/Audio/Conceptual/MIDINetworkDriverProtocol/MIDI/MIDI.html
     *  https://en.wikipedia.org/wiki/RTP-MIDI#AppleMIDI
     */

    public class AppleMidiControlPacket : IBufferReceiver, IBufferGenerator
    {        
        public AppleMidiCommands Command;

        public uint Version;
        public uint Token;
        public uint Ssrc;
        private bool m_valid;
        public string Name;

        public uint Count;
        private uint m_padding;
        public ulong Timestamp1;
        public ulong Timestamp2;
        public ulong Timestamp3;
        public SequenceNumber SequenceNum;
        public uint ReceiverSeqNum;

        private byte[] m_buffer;

        public bool IsError { get; private set; }

        public AppleMidiControlPacket()
        {
            IsError = false;
            Version = (uint) AppleMidiConstants.Version;
            m_padding = 0;
        }

        public static AppleMidiControlPacket CreateInvitationCommand(AppleMidiCommands command, string name, uint token, uint ssrc)
        {            
            AppleMidiControlPacket packet = new AppleMidiControlPacket
            {
                Name = name,
                Command = command,
                Token = token,
                Ssrc = ssrc
            };
            return packet;
        }

        public static AppleMidiControlPacket CreateSynchronizeCommand(AppleMidiCommands command, uint ssrc, uint count, ulong time1, ulong time2, ulong time3)
        {
            AppleMidiControlPacket packet = new AppleMidiControlPacket
            {
                Command = command,
                Count = count,
                Timestamp1 = time1,
                Timestamp2 = time2,
                Timestamp3 = time3,
                Ssrc = ssrc
            };
            return packet;
        }

        public void ParseBuffer(byte[] buffer)
        {
            m_buffer = buffer;

            m_valid = AppleMidiHelper.IsAppleMidiPacket(m_buffer);

            if (!m_valid)
            {
                IsError = true;
                return;
            }

            Command = AppleMidiHelper.GetAppleMidiPacketCommand(m_buffer);

            switch (Command)
            {
                case AppleMidiCommands.Invitation:
                case AppleMidiCommands.InvitationRejected:
                case AppleMidiCommands.InvitationAccepted:
                case AppleMidiCommands.EndSession:

                    Version = BufferHelpers.Get32(m_buffer, 4);
                    Token = BufferHelpers.Get32(m_buffer, 8);
                    Ssrc = BufferHelpers.Get32(m_buffer, 12);
                    Name = BufferHelpers.GetString(m_buffer, 16);

                    break;
                case AppleMidiCommands.Synchronize:

                    Ssrc = BufferHelpers.Get32(m_buffer, 4);
                    Count = m_buffer[8];
                    m_padding = BufferHelpers.Get24(buffer, 9);

                    Timestamp1 = BufferHelpers.Get64(buffer, 12);
                    Timestamp2 = BufferHelpers.Get64(buffer, 20);
                    Timestamp3 = BufferHelpers.Get64(buffer, 28);

                    break;
                case AppleMidiCommands.ReceiverFeedback:

                    Ssrc = BufferHelpers.Get32(m_buffer, 4);
                    ReceiverSeqNum = BufferHelpers.Get32(m_buffer, 8);

                    break;
                default:
                    IsError = true;
                    return;
            }
        }

        public byte[] GetBuffer()
        {
            byte[] buffer = null;

            ushort cmd = AppleMidiHelper.GetAppleMidiPacketCommand(Command);

            switch (Command)
            {
                case AppleMidiCommands.Invitation:
                case AppleMidiCommands.InvitationRejected:
                case AppleMidiCommands.InvitationAccepted:
                case AppleMidiCommands.EndSession:

                    buffer = new byte[16 + 1 + Name.Length];
                    BufferHelpers.PutShort(buffer, 0, (ushort)AppleMidiConstants.PacketStart);
                    BufferHelpers.PutShort(buffer, 2, cmd);
                    BufferHelpers.PutInt(buffer, 4, Version);
                    BufferHelpers.PutInt(buffer, 8, Token);
                    BufferHelpers.PutInt(buffer, 12, Ssrc);
                    BufferHelpers.PutString(buffer, 16, Name);

                    break;

                case AppleMidiCommands.Synchronize:

                    uint size = 12 + (3*8);

                    buffer = new byte[size];
                    BufferHelpers.PutShort(buffer, 0, (ushort)AppleMidiConstants.PacketStart);
                    BufferHelpers.PutShort(buffer, 2, cmd);
                    BufferHelpers.PutInt(buffer, 4, Ssrc);
                    BufferHelpers.PutByte(buffer, 8, (byte)Count);

                    // 3 padding octets
                    BufferHelpers.PutByte(buffer, 9, (byte)m_padding);
                    BufferHelpers.PutByte(buffer, 10, (byte)m_padding);
                    BufferHelpers.PutByte(buffer, 11, (byte)m_padding);

                    var timestamps = new [] {Timestamp1, Timestamp2, Timestamp3};

                    int offset = 12;

                    for (int i = 0; i < 3; i++)
                    {
                        BufferHelpers.PutLong(buffer, offset, timestamps[i]);
                        offset += 8; // int64 / long
                    }

                    return buffer;
                case AppleMidiCommands.ReceiverFeedback:

                    buffer = new byte[12];
                    BufferHelpers.PutShort(buffer, 0, (ushort)AppleMidiConstants.PacketStart);
                    BufferHelpers.PutShort(buffer, 2, cmd);
                    BufferHelpers.PutInt(buffer, 4, Ssrc);
                    BufferHelpers.PutInt(buffer, 8, (uint)SequenceNum.Long());

                    break;
                default:
                    return null;
            }
            return buffer;
        }
    }
}
 