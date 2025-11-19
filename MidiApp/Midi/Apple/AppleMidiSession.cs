using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net.Nsd;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using Java.Net;
using TestApp.Midi.Concrete;
using TestApp.Midi.RTP;
using TestApp.Midi.RTP.Journal;
using TestApp.Wifi;
using ChannelNum = System.Int32;
using StatusByte = System.Byte;

namespace TestApp.Midi.Apple
{
    public class AppleMidiSession : Java.Lang.Object
    {
        public RtpJournal Journal = new RtpJournal();

        public EventHandler<SessionEstablishedEventArgs> OnSessionEstablished;
        public EventHandler<SessionEndedEventArgs> OnSessionEnded;

        public EventHandler<SyncInfoEventArgs> OnConnectionUpdated;

        public class SyncInfoEventArgs : EventArgs
        {
            public string Name { get; set; }
            public uint Ssrc { get; set; }
            public IPEndPoint Endpoint { get; set; }
            public string Status { get; set; }
        }

        public class SessionEstablishedEventArgs : EventArgs
        {
            public NetworkMidiSender MidiSender { get; set; }
            public RtpMidiSession MidiSession { get; set; }
        }

        public class SessionEndedEventArgs : EventArgs
        {
            public string Name { get; set; }
            public string Endpoint { get; set; }
            public uint Ssrc { get; set; }
        }

        private readonly Dictionary<ChannelNum, KeyValuePair<StatusByte, RtpMidiMessage>> m_messageHistory = new Dictionary<ChannelNum, KeyValuePair<StatusByte, RtpMidiMessage>>();

        public bool DoingSync = false;
        public bool SyncEstablished = false;

        public bool AcceptSessions = true;

        private int m_port;
        private int m_rate = 10000;
        public uint m_ssrc;
        private uint m_receiverSsrc;
        private long m_startTime;
        private int m_count = 0;

        private string CurrName = "";

        private List<RtpMidiSession> m_sessions = new List<RtpMidiSession>();
        private Handler m_handler;

        private RtpPort m_controlPort;
        private RtpPort m_dataPort;

        public string BonjourName { get; set; }
        private string serviceName;

        public IPEndPoint EstablishedEndPoint;

        private Random m_rand;

        private bool m_doingInvitationSeq = false;

        private string m_host;

        private bool m_amInitiator;

        public uint MyToken;

        private Context m_context;

        private uint m_token = 0;
        private uint Token
        {
            get
            {
                if (m_token == 0)
                {
                    m_token = (uint)Math.Round(m_rand.NextDouble() * Math.Pow(2, 32));
                }
                return m_token;
            }
            set { m_token = value; }
        }

        //public AppleMidiSession(Handler handler)
        //{
        //    m_handler = handler;
        //    Init("192.168.0.7", 5004);
        //}

        private NsdManager m_NsdManager;
        private ResolveServiceListener m_resolveServiceListener;
        private RegisterServiceListener m_registerServiceListener;
        private DiscoveryServiceListener m_discoveryServiceListener;

        public AppleMidiSession(Context context, string bonjourName, string ip, int port, Handler handler)
        {
            m_context = context;
            m_NsdManager = (NsdManager)context.GetSystemService(Context.NsdService);
            m_handler = handler;
            Init(ip, bonjourName, port); 
        }

        private void Init(string addr, string bonjour, int port = 0)
        {
            m_amInitiator = false;
            m_host = addr;
            m_port = port == 0 ? 5008 : port;
            BonjourName = bonjour;

            //m_startTicks = DateTime.UtcNow.Ticks;
            m_startTime = Stopwatch.GetTimestamp();
            m_rand = new Random(DateTime.Now.Millisecond);
            m_ssrc = (uint)Math.Round(m_rand.NextDouble() * 0xFFFF * 0xFFFF);

            MyToken = (uint)Math.Round(m_rand.NextDouble() * Math.Pow(2, 32));

            if (!Stopwatch.IsHighResolution)
            {
                //Console.WriteLine("Stopwatch isn't high res");
            }

            SetupPorts();
        }

        public NsdManager GetNsdManager()
        {
            return m_NsdManager;
        }

        public int GetPort()
        {
   ;         return m_port;
        }

        public List<RtpMidiSession> GetSessions()
        {
            return m_sessions;            
        }

        public bool EndSession(uint ssrc)
        {
            var session = m_sessions.FirstOrDefault(x => x.PartnerSsrc == ssrc);

            if (session != null)
            {
                if (OnSessionEnded != null)
                    m_handler.Post(() => OnSessionEnded.Invoke(this, new SessionEndedEventArgs { Ssrc = session.PartnerSsrc, Name = session.Name, Endpoint = session.EndPoint.ToString() }));

                var packet = AppleMidiControlPacket.CreateInvitationCommand(AppleMidiCommands.EndSession, BonjourName,
                    session.SessionToken, session.Ssrc);
                var buffer = packet.GetBuffer();
                //m_dataPort.Send(buffer, buffer.Length);
                m_dataPort.Send(buffer, buffer.Length, session.EndPoint);

                session.Shutdown();
                m_sessions.Remove(session);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetupPorts()
        {
            IPAddress ipAddress = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Any, m_port);
            IPEndPoint ipLocalEndPoint2 = new IPEndPoint(IPAddress.Any, m_port+1);

            m_controlPort = new RtpPort(m_port);
            m_controlPort.OnReceived += OnControlReceived;
            m_controlPort.StartListening();

            m_dataPort = new RtpPort(m_port+1);
            m_dataPort.OnReceived += OnDataReceived;
            m_dataPort.StartListening();           
        }

        // cannot be re-opened!
        public void Shutdown()
        {
            UnregisterServices();

            m_sessions.ForEach(x => x.Shutdown());

            m_controlPort.Close();
            m_dataPort.Close();

            OnConnectionUpdated = null;
            OnSessionEnded = null;
            OnSessionEstablished = null;            
        }

        //public void CloseSession()
        //{
        //    var packet = AppleMidiControlPacket.CreateInvitationCommand(AppleMidiCommands.EndSession, "AndroidPhone",
        //        Token, m_ssrc);
        //    var buffer = packet.GetBuffer();
        //    m_controlPort.Send(buffer, buffer.Length);
        //
        //    SyncEstablished = false;
        //}

        public void InitiateSession(IPEndPoint endPoint)
        {
            var packet = AppleMidiControlPacket.CreateInvitationCommand(AppleMidiCommands.Invitation, BonjourName, MyToken, m_ssrc);

            var buffer = packet.GetBuffer();
            m_controlPort.Send(buffer, buffer.Length, endPoint);
            //m_dataPort.Send(buffer, buffer.Length, endPoint);
        }

        public uint GetCurrentTime()
        {
            var ticks = Stopwatch.GetTimestamp() - m_startTime;
            var seconds = (double)ticks / Stopwatch.Frequency;

            return (uint)Math.Floor(seconds * m_rate); // convert it to units of 100 microseconds
        }

        private void OnControlReceived(object sender, UdpReceiveResult result)
        {
            AppleMidiControlPacket packet = new AppleMidiControlPacket();
            packet.ParseBuffer(result.Buffer);

            if (packet.IsError) return;

            //Console.WriteLine("Control port rec: " + packet.Command + "  From: " + result.RemoteEndPoint);                       

            if (packet.Command == AppleMidiCommands.Invitation)
            {
                if (!AcceptSessions) return;

                m_amInitiator = false;
                Token = packet.Token;
                m_receiverSsrc = packet.Ssrc;
                CurrName = packet.Name;

                m_handler.Post(() => OnConnectionUpdated?.Invoke(this, new SyncInfoEventArgs { Name = CurrName, Endpoint = result.RemoteEndPoint, Ssrc = m_receiverSsrc, Status = "Accepting..."}));

                // send accept on control port
                var packetToSend = AppleMidiControlPacket.CreateInvitationCommand(AppleMidiCommands.InvitationAccepted, BonjourName, Token, m_ssrc);
                //Console.WriteLine("Control port send: " + packetToSend.Command + "  To: " + result.RemoteEndPoint);
                var buffer = packetToSend.GetBuffer();                
                m_controlPort.Send(buffer, buffer.Length, result.RemoteEndPoint);
            }
            if (packet.Command == AppleMidiCommands.InvitationAccepted)
            {
                m_amInitiator = true;
            }
            if (packet.Command == AppleMidiCommands.EndSession)
            {
                bool hasSessions = m_sessions.Count > 0;

                DoingSync = false;

                if (hasSessions)
                {
                    var session = m_sessions.FirstOrDefault(x => x.PartnerSsrc == packet.Ssrc);

                    if (session != null)
                    {
                        session.Shutdown();

                        if (OnSessionEnded != null)
                            m_handler.Post(() => OnSessionEnded.Invoke(this, new SessionEndedEventArgs { Ssrc = session.PartnerSsrc, Name = session.Name, Endpoint = session.EndPoint.ToString() }));

                        m_sessions.Remove(session);
                    }                    
                }                        
            }
            if (packet.Command == AppleMidiCommands.ReceiverFeedback)
            {
                var session = m_sessions.FirstOrDefault(x => x.PartnerSsrc == packet.Ssrc);
                session?.SetLastReceivedSeqNum(packet.ReceiverSeqNum);
                //Console.WriteLine("SeqNum: " + packet.ReceiverSeqNum + " ssrc: " + packet.Ssrc);
            }
        }

        private void OnDataReceived(object sender, UdpReceiveResult result)
        {
            AppleMidiControlPacket packet = new AppleMidiControlPacket();
            packet.ParseBuffer(result.Buffer);

            if (packet.IsError) return;

            //Console.WriteLine("Data port rec: " + packet.Command + "  From: " + result.RemoteEndPoint);

            if (packet.Command == AppleMidiCommands.Invitation)
            {
                // send accept on data port
                var packetToSend = AppleMidiControlPacket.CreateInvitationCommand(AppleMidiCommands.InvitationAccepted, BonjourName, packet.Token, m_ssrc);
                //Console.WriteLine("Data port send: " + packetToSend.Command + "  To: " + result.RemoteEndPoint);
                var buffer = packetToSend.GetBuffer();
                m_dataPort.Send(buffer, buffer.Length, result.RemoteEndPoint);
            }
            if (packet.Command == AppleMidiCommands.InvitationAccepted)
            {
                m_amInitiator = true;
            }
            if (packet.Command == AppleMidiCommands.Synchronize)
            {
                DoingSync = true;

                m_handler.Post(() => OnConnectionUpdated?.Invoke(this, new SyncInfoEventArgs { Ssrc = m_receiverSsrc, Name = CurrName, Endpoint = result.RemoteEndPoint, Status = "Synchronizing" }));

                /*
                    Item        Value
                    
                    SSRC        The sender's synchronization source identifier.
                    count       The count is the number of valid timestamps in the packet minus 1.
                    timestamp   A 64-bit number indicating a time, relative to an arbitrary and unknown point in the past, in units of 100 microseconds.
 
                    Initiator sends sync packet with count = 0, current time in timestamp 1
                    Responder sends sync packet with count = 1, current time in timestamp 2, timestamp 1 copied from received packet
                    Initiator sends sync packet with count = 2, current time in timestamp 3, timestamps 1 and 2 copied from received packet
                */

                //Console.WriteLine("sync count: " + packet.Count);

                if (packet.Count == 3)
                    m_count = -1;
                else
                {
                    m_count = (int) packet.Count;
                }

                AppleMidiControlPacket newPacket =
                    AppleMidiControlPacket.CreateSynchronizeCommand(AppleMidiCommands.Synchronize, m_ssrc,
                        (uint) m_count, packet.Timestamp1, 0, 0);

                if (m_count == -1)
                {
                    // dont do anything, invalid count number
                }
                if (packet.Count == 0)
                {
                    //Console.WriteLine("NOW: " + now);

                    newPacket.Count++;
                    newPacket.Timestamp1 = (ulong) packet.Timestamp1;
                    newPacket.Timestamp2 = (ulong) GetCurrentTime(); 

                    //Console.WriteLine("sending count: " + newPacket.Count + "  ts1: " + newPacket.Timestamp1 + " ts2: " +
                    //                  newPacket.Timestamp2);
                    var buff = newPacket.GetBuffer();
                    m_dataPort.Send(buff, buff.Length, result.RemoteEndPoint);
                }
                if (packet.Count == 1)
                {
                    // Initiator sends sync packet with count = 2, current time in timestamp 3, timestamps 1 and 2 copied from received packet
                    newPacket.Count++;
                    newPacket.Timestamp1 = packet.Timestamp1;
                    newPacket.Timestamp2 = packet.Timestamp2;
                    newPacket.Timestamp3 = GetCurrentTime();

                    var buff = newPacket.GetBuffer();
                    m_dataPort.Send(buff, buff.Length, result.RemoteEndPoint);
                }
                if (packet.Count == 2)
                {
                    m_handler.Post(() => OnConnectionUpdated?.Invoke(this, new SyncInfoEventArgs { Ssrc = m_receiverSsrc, Name = CurrName, Endpoint = result.RemoteEndPoint, Status = "Sync Complete." }));

                    EstablishedEndPoint = result.RemoteEndPoint;

                    bool exists = m_sessions.Any(x => x.PartnerSsrc == m_receiverSsrc);

                    if (!exists)
                    {
                        var session = new RtpMidiSession(this, Token, m_ssrc, m_receiverSsrc, EstablishedEndPoint, CurrName);
                        CurrName = "";
                        m_sessions.Add(session);
                        Token = 0;

                        if (OnSessionEstablished != null)
                        {
                            // run on main thread
                            var networkSender = new NetworkMidiSender(session);
                            networkSender.Suspend = false;
                            m_handler.Post(() => OnSessionEstablished.Invoke(this, new SessionEstablishedEventArgs
                            {
                                MidiSender = networkSender,
                                MidiSession = session
                            }));
                        }
                    }                                      
                }
            }
            else
            {
                DoingSync = false;
            }
        }

        public void SendPacket(RtpMidiCommands commands, RtpMidiSession midiSession, uint esn)
        {
            //if (!SyncEstablished) return;

            var payload = commands.GetBuffer();

            // final packet to send

            var packet = new RtpPacket(midiSession.SequenceNumber.Short(), GetCurrentTime(), midiSession.Ssrc);

            // update session's history
            //Journal.UpdateWith(packet, commands.MidiMessages);
            
            packet.SetPayload(payload);                  
            //packet.SetJournalBytes(Journal.GetJournal(esn, Convert.ToUInt16(midiSession.FirstStreamSeqNum & 0xFFFF)));

            var buff = packet.GetBuffer();
            m_dataPort.Send(buff, buff.Length, midiSession.EndPoint);

            Journal.MostRecentSeqNum = packet.SequenceNumber.Short();

            commands.ClearCommands();
        }

        public void RegisterService(string ip, Context ctx)
        {
            m_handler.PostDelayed(() =>
            {
                NsdServiceInfo serviceInfo = new NsdServiceInfo();

                serviceInfo.ServiceName = BonjourName;
                serviceInfo.ServiceType = "_apple-midi._udp";
                serviceInfo.Port = m_port;
                serviceInfo.Host = InetAddress.GetByName(ip);

                if (WifiHelpers.IsWifiConnected(m_context))
                {
                    CreateRegisterServiceListener(m_NsdManager);
                    CreateDiscoverServiceListener(m_NsdManager);

                    // register service 
                    m_NsdManager.RegisterService(serviceInfo, NsdProtocol.DnsSd, m_registerServiceListener);

                    // find services
                    m_NsdManager.DiscoverServices("_apple-midi._udp", NsdProtocol.DnsSd, m_discoveryServiceListener);
                }
                
            }, 500);
        }

        private ResolveServiceListener CreateResolveListener()
        {
            var  resolveListener = new ResolveServiceListener();
            resolveListener.OnResolveFailedHandler += (sender, args) =>
            {

            };
            resolveListener.OnServiceResolvedHandler += (sender, args) =>
            {
                if (args.Info.ServiceName == BonjourName) return; // found this service

                // new service discovered!
                //Console.WriteLine(args.Info.ServiceType + " " + args.Info.Host + " " + args.Info.Port + " " + args.Info.ServiceName);
            };
            return resolveListener;
        }

        public void UnregisterServices()
        {
            if (m_discoveryServiceListener != null && m_registerServiceListener != null)
            {                
                m_NsdManager.UnregisterService(m_registerServiceListener);
                m_NsdManager.StopServiceDiscovery(m_discoveryServiceListener);
            }            
        }

        public void CreateRegisterServiceListener(NsdManager mgr)
        {
            if (m_resolveServiceListener != null && m_discoveryServiceListener != null)
            {
                m_NsdManager.UnregisterService(m_registerServiceListener);
                m_NsdManager.StopServiceDiscovery(m_discoveryServiceListener);
            }

            m_registerServiceListener = new RegisterServiceListener();
            m_registerServiceListener.OnRegistrationFailedHandler += (sender, args) =>
            {

            };
            m_registerServiceListener.OnServiceRegisteredHandler += (sender, args) =>
            {
                serviceName = args.Info.ServiceName;
            };
            m_registerServiceListener.OnServiceUnregisteredHandler += (sender, args) =>
            {

            };
            m_registerServiceListener.OnUnregistrationFailedHandler += (sender, args) =>
            {

            };
        }

        public void CreateDiscoverServiceListener(NsdManager mgr)
        {
            if (m_resolveServiceListener != null && m_discoveryServiceListener != null)
            {
                m_NsdManager.UnregisterService(m_registerServiceListener);
                m_NsdManager.StopServiceDiscovery(m_discoveryServiceListener);
            }

            m_discoveryServiceListener = new DiscoveryServiceListener();
            m_discoveryServiceListener.OnDiscoveryStartedHandler += (sender, args) =>
            {

            };
            m_discoveryServiceListener.OnDiscoveryStoppedHandler += (sender, args) =>
            {

            };
            m_discoveryServiceListener.OnServiceFoundHandler += (sender, args) =>
            {
                m_NsdManager.ResolveService(args.Info, CreateResolveListener());
            };
            m_discoveryServiceListener.OnServiceLostHandler += (sender, args) =>
            {

            };
            m_discoveryServiceListener.OnStartDiscoveryFailedHandler += (sender, args) =>
            {
                m_NsdManager.StopServiceDiscovery(m_discoveryServiceListener);
            };
            m_discoveryServiceListener.OnStopDiscoveryFailedHandler += (sender, args) =>
            {
                m_NsdManager.StopServiceDiscovery(m_discoveryServiceListener);
            };
        }
    }
}
 