using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp
{
    public class RepeatableTask
    {
        private readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();
        private readonly Action m_action;
        private readonly int m_millis;
        public bool Cancelled { get; private set; }
        public string LastExceptionMessage { get; private set; }

        public RepeatableTask(int milliseconds, Action action)
        {
            Cancelled = false;
            m_action = action;
            m_millis = milliseconds;            
        }

        public static RepeatableTask StartNew(int milliseconds, Action action)
        {
            RepeatableTask task = new RepeatableTask(milliseconds, action);
            task.Start();
            return task;
        }

        public async Task Start()
        {
            try
            {
                await Task.Run(() =>
                {
                    long lastTime = 0;

                    while (true)
                    {
                        if (m_tokenSource.Token.IsCancellationRequested)
                        {
                            Cancelled = true;
                            break;
                        }

                        var currTime = Stopwatch.GetTimestamp();
                        var elapsed = currTime - lastTime;
                        var ms = ((double) elapsed/Stopwatch.Frequency)*1000.0;

                        if (ms >= m_millis)
                        {
                            lastTime = currTime;

                            m_action();
                        }
                    }
                }, m_tokenSource.Token);
            }
            catch (Exception ex)
            {
                LastExceptionMessage = ex.Message;
                Cancelled = true;
            }
        }

        public void Stop()
        {
            m_tokenSource.Cancel();                        
        }
    }
}