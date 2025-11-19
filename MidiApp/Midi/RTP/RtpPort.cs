using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Exception = System.Exception;

namespace TestApp.Midi.RTP
{
    public class RtpPort
    {        
        private readonly UdpClient m_client;

        private readonly Socket m_socket;

        private int m_port;
        private string m_addr;
        private IPEndPoint m_endPoint;

        private CancellationTokenSource m_tokenSource = new CancellationTokenSource();

        public EventHandler<UdpReceiveResult> OnReceived;

        //public class OnReceivedEventArgs : EventArgs
        //{
        //    
        //}

        public RtpPort(int port)
        {
            m_endPoint = new IPEndPoint(IPAddress.IPv6Any, port);

            //m_socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            //m_socket.DualMode = true;
            //
            //m_socket.Bind(m_endPoint);

            m_client = new UdpClient(m_endPoint.Port, AddressFamily.InterNetworkV6);
        }

        public void Close()
        {
            m_tokenSource.Cancel();
            m_client.Close();
        }

        public void Send(byte[] buffer, int length)
        {
            Task.Factory.StartNew(async () =>
            {
                await DoSend(buffer, length);
            });
        }

        public void Send(byte[] buffer, int length, IPEndPoint endPoint)
        {
            Task.Factory.StartNew(async () =>
            {
                await DoSend(buffer, length, endPoint);
            });
        }

        private async Task DoSend(byte[] buffer, int length, IPEndPoint endPoint)
        {
            var res = await m_client.SendAsync(buffer, length, endPoint);
        }

        private async Task DoSend(byte[] buffer, int length)
        {
            var res = await m_client.SendAsync(buffer, length);
        }

        private async Task<UdpReceiveResult> DoReceive()
        {
            var result = await m_client.ReceiveAsync();
            return result;
        }

        public void StopListening()
        {
            m_tokenSource.Cancel();
            m_tokenSource = null;
        }

        public void StartListening()
        {
            if (m_tokenSource == null)
            {
                m_tokenSource = new CancellationTokenSource();
            }

            Task.Run(async () => 
            {
                while (true)
                {
                    if (m_tokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var received = await DoReceive();
                        OnReceived?.Invoke(this, received);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("***EXCEPTION***");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("***************");                                                                
                    }                    
                }
            }, m_tokenSource.Token);
        }
    }
}