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

namespace TestApp
{
    public static class BufferHelpers
    {
        public const byte MASK_8 = 0x80;
        public const byte MASK_7 = 0x40;
        public const byte MASK_6 = 0x20;
        public const byte MASK_5 = 0x10;
        public const byte MASK_4 = 0x08;
        public const byte MASK_3 = 0x04;
        public const byte MASK_2 = 0x02;
        public const byte MASK_1 = 0x01;
        public const byte ZERO_BYTE = 0x0;

        public static void PutByte(byte[] buffer, int offset, byte val)
        {
            buffer[offset] = val;
        }

        public static void SetBit(byte[] buffer, bool val, byte mask, int index)
        {
            if (val)
            {
                buffer[index] |= mask;
            }
            else
            {
                buffer[index] &= (byte)~mask;
            }
        }

        public static void PutShort(byte[] buffer, int offset, ushort val)
        {
            buffer[offset] = (byte)((val >> 8) & 0xFF);
            buffer[offset + 1] = (byte)(val & 0xFF);
        }

        public static void Put24(byte[] buffer, int offset, int val)
        {
            buffer[offset] = (byte)((val >> 16) & 0xFF);
            buffer[offset + 1] = (byte)((val >> 8) & 0xFF);
            buffer[offset + 2] = (byte)(val & 0xFF); ;
        }

        public static void PutInt(byte[] buffer, int offset, uint val)
        {
            buffer[offset] = (byte)((val >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((val >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((val >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(val & 0xFF);
        }

        public static void PutLong(byte[] buffer, int offset, ulong val)
        {
            buffer[offset] = (byte)(val >> 56);
            buffer[offset + 1] = (byte)(val >> 48);
            buffer[offset + 2] = (byte)(val >> 40);
            buffer[offset + 3] = (byte)(val >> 32);
            buffer[offset + 4] = (byte)(val >> 24);
            buffer[offset + 5] = (byte)(val >> 16);
            buffer[offset + 6] = (byte)(val >> 8);
            buffer[offset + 7] = (byte)val;
        }

        public static uint Get24(byte[] buffer, int offset)
        {
            byte firstByte = buffer[offset];
            byte secondByte = buffer[offset + 1];
            byte thirdByte = buffer[offset + 2];

            uint val = (uint)((firstByte << 16) |
                       (secondByte << 8) |
                       (thirdByte) & 
                       0xFFFFFF);
            return val;
        }

        public static uint Get32(byte[] buffer, int offset)
        {
            byte firstByte = buffer[offset];
            byte secondByte = buffer[offset + 1];
            byte thirdByte = buffer[offset + 2];
            byte fourthByte = buffer[offset + 3];

            uint val = (uint) ((firstByte << 24) |
                               (secondByte << 16) |
                               (thirdByte << 8) |
                               (fourthByte)) & 0xffffffff;
            return val;
        }

        public static ulong Get64(byte[] buffer, int offset)
        {
            uint upper = Get32(buffer, offset);
            uint lower = Get32(buffer, offset + 4);

            ulong val = upper;
            val <<= 32;
            val += lower;
            val &= 0xFFFFFFFFFFFFFFFFL;
            return val;
        }

        public static string GetString(byte[] buffer, int offset)
        {
            return Encoding.UTF8.GetString(buffer, offset, buffer.Length - offset);
        }

        public static void PutString(byte[] buffer, int offset, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var nullTerminatedString = new byte[bytes.Length + 1];
            bytes.CopyTo(nullTerminatedString, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                buffer[offset + i] = bytes[i];
            }
        }

        public static ushort Get16(byte[] buffer, int offset)
        {
            byte firstByte = buffer[offset];
            byte secondByte = buffer[offset + 1];

            ushort val = (ushort)(((firstByte << 8) |
                                   secondByte) & 0xFFFF);
            return val;
        }

        public static void PrintOctetBits(byte b)
        {
            Console.WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
        }

        // left to right
        public static byte SetByteFlags(bool p1, bool p2, bool p3, bool p4,
                                        bool p5, bool p6, bool p7, bool p8)
        {
            byte val = 0;
            val |= p1 ? MASK_8 : ZERO_BYTE;
            val |= p2 ? MASK_7 : ZERO_BYTE;
            val |= p3 ? MASK_6 : ZERO_BYTE;
            val |= p4 ? MASK_5 : ZERO_BYTE;
            val |= p5 ? MASK_4 : ZERO_BYTE;
            val |= p6 ? MASK_3 : ZERO_BYTE;
            val |= p7 ? MASK_2 : ZERO_BYTE;
            val |= p8 ? MASK_1 : ZERO_BYTE;
            return val;
        }

        /*
         * encode variable length value
         * http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
         */
        public static byte[] EncodeVlv(uint timestamp)
        {
            uint tempTimestamp = timestamp & 0x0FFFFFFF;

            byte[] buffer = new byte[4];

            int i = 0;
            int count = 1;
            for (i = 0; i < 4; ++i)
            {
                if (i == 0)
                    buffer[i] = 0x00;
                else
                    buffer[i] = 0x80;

                buffer[i] |= (byte)(tempTimestamp & 0x7F);

                tempTimestamp = tempTimestamp >> 7;

                if (tempTimestamp == 0 || i == 3)
                {
                    break;
                }
                count++;
            }

            if (count == 4)
            {
                return new [] { buffer[3], buffer[2], buffer[1], buffer[0] };
            }
            if (count == 3)
            {
                return new [] { buffer[2], buffer[1], buffer[0] };
            }
            if (count == 2)
            {
                return new [] { buffer[1], buffer[0] };
            }
            return new [] { buffer[0] };
        }
    }
}