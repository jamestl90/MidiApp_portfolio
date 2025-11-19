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
using TestApp.Midi.RTP;

namespace TestApp.Midi.Apple
{
    public enum AppleMidiCommands
    {
        Invitation = 0x494E,            // IN
        InvitationRejected = 0x4E4F,    // NO
        InvitationAccepted = 0x4F4B,    // OK
        EndSession = 0x4259,            // BY
        Synchronize = 0x434B,           // CK
        ReceiverFeedback = 0x5253,      // RS
        BitrateReceiveLimit = 0x524C,   // RL
        Error
    }

    public enum AppleMidiConstants
    {        
        Version = 2,
        PacketStart = 0xFFFF
    }

    public static class AppleMidiHelper
    {
        public static RtpPacket GuardPacket()
        {
            //RtpPacket packet = new RtpPacket();
            return null;
        }
         
        public static bool IsAppleMidiPacket(byte[] packet)
        {
            var sub = new byte[] {packet[1], packet[0]};
     
            ushort val = BitConverter.ToUInt16(sub, 0);

            return val == (ushort)AppleMidiConstants.PacketStart;
        }

        public static AppleMidiCommands GetAppleMidiPacketCommand(byte[] packet)
        {
            var sub = new byte[] { packet[3], packet[2] };

            ushort val = BitConverter.ToUInt16(sub, 0);

            switch (val)
            {
                case 0x494E:
                    return AppleMidiCommands.Invitation;
                case 0x4E4F:
                    return AppleMidiCommands.InvitationRejected;
                case 0x4F4B:
                    return AppleMidiCommands.InvitationAccepted;
                case 0x4259:
                    return AppleMidiCommands.EndSession;
                case 0x434B:
                    return AppleMidiCommands.Synchronize;
                case 0x5253:
                    return AppleMidiCommands.ReceiverFeedback;
                case 0x524C:
                    return AppleMidiCommands.BitrateReceiveLimit;
                default:
                    return AppleMidiCommands.Error;
            }
        }

        public static ushort GetAppleMidiPacketCommand(AppleMidiCommands command)
        {
            switch (command)
            {
                case AppleMidiCommands.Invitation:
                    return 0x494E;
                case AppleMidiCommands.InvitationRejected:
                    return 0x4E4F;
                case AppleMidiCommands.InvitationAccepted:
                    return 0x4F4B;
                case AppleMidiCommands.EndSession:
                    return 0x4259;
                case AppleMidiCommands.Synchronize:
                    return 0x434B;
                case AppleMidiCommands.ReceiverFeedback:
                    return 0x5253;
                case AppleMidiCommands.BitrateReceiveLimit:
                    return 0x524C;
                default:
                    throw new Exception("Unknown midi command");
            }
        }         
    }
}
 