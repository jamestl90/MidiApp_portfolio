using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class SequenceNumber
    {
        private ushort m_maxSeq = 65535;
        private uint m_badSeq = 0;

        //private uint m_wrapAroundCount;
        private ulong m_val;
        private Random m_rand;

        public const int MaxMisorder = 100;
        public const int MaxDropout = 3000;

        public SequenceNumber(SequenceNumber other)
        {
            m_val = other.Long();
        }

        public SequenceNumber()
        {
            //m_wrapAroundCount = 0;
            GenerateNew();
        }

        public SequenceNumber(Random rand)
        {
            //m_wrapAroundCount = 0;
            m_rand = rand;
            GenerateNew();
        }

        public SequenceNumber(ushort seqNum)
        {
            m_val = seqNum;
        }

        public void Set(ulong other)
        {
            m_val = other;
        }

        public void GenerateNew()
        {
            if (m_rand == null)
            {
                m_rand = new Random(DateTime.Now.Millisecond);
            }

            //m_wrapAroundCount = 0;
            m_val = (uint)Math.Floor(m_rand.NextDouble() * 0xFFFF);
        }

        public ushort Short()
        {
            return Convert.ToUInt16(m_val & 0xFFFF);
        }

        public ulong Long()
        {
            return m_val;
        }

        public override string ToString()
        {
            return m_val.ToString();
        }

        public void Print()
        {
            Console.WriteLine($"Seq num: {m_val}");
        }

        public void PrintShort()
        {
            Console.WriteLine($"Seq num: {Short()}");
        }

        public ulong Increment()
        {
            ++m_val;
            return m_val;

            /*ushort seq = Short();
            ushort udelta = (ushort)(seq - m_maxSeq);

            if (udelta < MaxDropout)
            {
                // wrap around shouldn't happen very often anyway. 
                if (seq < m_maxSeq)
                {
                    m_wrapAroundCount++;
                }
                m_maxSeq = seq;
            }
            else if (udelta <= (0xFFFF - MaxMisorder))
            {
                // seq num made a large jump :(
                if (seq == m_badSeq)
                {
                    // two sequential packets received 
                }
                else
                {
                    m_badSeq = (uint) (seq + 1);
                }
            }
            else
            {
                // duplicate or misordered packet
            }

            m_val = seq + (0xFFFF*m_wrapAroundCount);
            */
        }
    }
}