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

namespace TestApp.Midi.RTP.Journal.Chapters
{
    #region Control Change
    public class JournalChapterControlChange : IBufferGenerator, IBufferReceiver
    {
        // assume its control change message passed from channel journal
        public void UpdateWith(RtpMidiMessage message)
        {
            
        }

        public byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Pitch Wheel
    public class JournalChapterPitchWheel : IBufferGenerator, IBufferReceiver
    {
        // assume its pitch wheel message passed from channel journal
        public void UpdateWith(RtpMidiMessage message)
        {

        }

        public byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Notes
    public class JournalChapterNote : IBufferGenerator, IBufferReceiver
    {
        private byte m_header;

        // assume its note on/off message passed from channel journal
        public void UpdateWith(RtpMidiMessage message)
        {

        }



        public byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }

        public void ParseBuffer(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}