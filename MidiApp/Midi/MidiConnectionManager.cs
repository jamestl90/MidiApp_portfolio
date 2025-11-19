using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Hardware.Input;
using Android.Media.Midi;
using Android.Net.Wifi;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using TestApp.Bluetooth;
using TestApp.Midi.Apple;
using TestApp.Midi.Concrete;
using TestApp.Util;
using TestApp.Wifi;
using Formatter = Android.Text.Format.Formatter;

namespace TestApp.Midi
{
    public class MidiConnectionManager : Java.Lang.Object, MidiManager.IOnDeviceOpenedListener
    {
        private readonly Activity m_activity;

        private AppleMidiSession m_appleMidiSession;

        private MidiDevice m_device;
        private BluetoothDevice m_bluetoothDevice;
        private BluetoothAdapter m_btAdapter;
        private readonly MidiManager m_midiManager;
        private MidiInputPort m_midiInputPort;

        private string m_ipAdd;

        private ConnectionType m_currentMode;
        private ConnectionType m_lastMode;

        private readonly Dictionary<string, MidiDeviceListItem> m_listItems = new Dictionary<string, MidiDeviceListItem>();
        private readonly Dictionary<string, CleanupMidi> m_deviceItems = new Dictionary<string, CleanupMidi>();

        public bool IsShutdown = false;

        public class CleanupMidi
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public MidiInputPort Port;
            public MidiDevice Device;
        }

        public class MidiDeviceListItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public ConnectionType Type { get; set; }  
            public Object Tag { get; set; }

            public override string ToString()
            {
                string type = Type == ConnectionType.BluetoothLe ? "BluetoothLE" : Type == MidiConnectionManager.ConnectionType.Usb ? "USB" : "Virtual";
                return Name + " (" + type + ")";
            }
        }

        public EventHandler<MidiDeviceAddedEventArgs> OnMidiDeviceAdded;
        public EventHandler<MidiDeviceWasOpenedEventArgs> OnMidiDeviceOpened;
        public EventHandler<MidiDeviceRemovedEventArgs> OnMidiDeviceRemoved;
        public EventHandler<MidiDeviceNotFoundEventArgs> OnDeviceNotFound;

        public class MidiDeviceAddedEventArgs : EventArgs
        {
            public MidiDeviceListItem MidiDeviceInfo { get; set; }
        }

        public class MidiDeviceNotFoundEventArgs : EventArgs
        {
            public DefaultMidiSender DummySender { get; set; }
        }

        public class MidiDeviceWasOpenedEventArgs : EventArgs
        {
            public string Name { get; set; }
            public ConnectionType Type { get; set; }
            public IMidiSender Sender { get; set; }
        }

        public class MidiDeviceRemovedEventArgs : EventArgs
        {
            public string Name { get; set; }
            public ConnectionType Type { get; set; }
            public int Id { get; set; }
        }

        public enum ConnectionType
        {
            Usb,
            BluetoothLe,
            Network,
            Virtual
        }

        public MidiConnectionManager(Activity activity, MidiManager midiManager)
        {
            m_activity = activity;
            m_midiManager = midiManager;
            m_currentMode = ConnectionType.Usb;

            if (m_midiManager == null) return;

            
        }

        public void Init()
        {
            //SetupBluetoothDevice();

            var devs = m_midiManager.GetDevices();

            MidiDeviceCallback callback = new MidiDeviceCallback
            {
                OnAdded_Device = (device) =>
                {
                    if (!m_listItems.ContainsKey(device.Id.ToString()))
                    {
                        ConnectionType type = device.Type == MidiDeviceType.Bluetooth ? ConnectionType.BluetoothLe : device.Type == MidiDeviceType.Usb ? ConnectionType.Usb : ConnectionType.Virtual;
                        var item = new MidiDeviceListItem
                        {
                            Name = device.Properties.GetString("name"),
                            Type = type,
                            Id = device.Id.ToString(),
                            Tag = device
                        };
                        m_listItems.Add(device.Id.ToString(), item);
                        OnMidiDeviceAdded?.Invoke(this, new MidiDeviceAddedEventArgs
                        {
                            MidiDeviceInfo = item
                        });
                    }
                },
                OnRemoved_Device = (device) =>
                {
                    var item = m_deviceItems.FirstOrDefault(x => x.Value.Id == device.Id);

                    if (!item.Equals(new KeyValuePair<string, CleanupMidi>()))
                    {
                        item.Value.Port.Close();
                        item.Value.Device.Close();
                        m_deviceItems.Remove(device.Properties.GetString(MidiDeviceInfo.PropertyName));
                    }

                    m_listItems.Remove(device.Id.ToString());

                    ConnectionType type = device.Type == MidiDeviceType.Bluetooth ? ConnectionType.BluetoothLe : device.Type == MidiDeviceType.Usb ? ConnectionType.Usb : ConnectionType.Virtual;
                    OnMidiDeviceRemoved?.Invoke(this, new MidiDeviceRemovedEventArgs
                    {
                        Id = device.Id,
                        Name = device.Properties.GetString("name"),
                        Type = type
                    });
                },
                OnStatusChanged_Device = (status) =>
                {
                    //Console.WriteLine("Status changed: Input port open? " + status.IsInputPortOpen(status.DeviceInfo.GetPorts()[0].PortNumber));
                    //Console.WriteLine("Status changed: Output port open count: " + status.GetOutputPortOpenCount(status.DeviceInfo.GetPorts()[0].PortNumber));
                    //Console.WriteLine(status.ToString());
                }
            };
            m_midiManager.RegisterDeviceCallback(callback, new Handler(Looper.MainLooper));

            OpenAppleMidiSession(GlobalSettings.GetDefaultBonjourName(), WifiHelpers.LocalIpToString(m_activity), GlobalSettings.GetDefaultPort());
        }

        public AppleMidiSession GetAppleMidi()
        {
            return m_appleMidiSession;
        }

        public void SetAppleMidiNull()
        {
            m_appleMidiSession = null;
        }

        public AppleMidiSession NewAppleMidiSession(string bonjour, string ip, int port)
        {
            m_ipAdd = ip;            
            m_appleMidiSession = null;
            OpenAppleMidiSession(bonjour, m_ipAdd, port);
            return m_appleMidiSession;
        }

        // default config
        public bool OpenAppleMidiSession(string bonjourName)
        {
            return OpenAppleMidiSession(bonjourName, WifiHelpers.LocalIpToString(m_activity), 5008);
        }

        public bool OpenAppleMidiSession(string bonjourName, string ip, int port)
        {
            if (WifiHelpers.IsWifiEnabled(m_activity) && WifiHelpers.IsWifiConnected(m_activity))
            {
                if (m_appleMidiSession != null) return true;
                
                m_ipAdd = WifiHelpers.LocalIpToString(m_activity);                

                m_appleMidiSession = new AppleMidiSession(((Context)m_activity), bonjourName, m_ipAdd, port, new Handler(Looper.MainLooper));
                m_appleMidiSession.OnSessionEstablished += (sender, args) =>
                {
                    var info = new MidiDeviceListItem
                    {
                        Id = m_appleMidiSession.m_ssrc.ToString(),
                        Name = args.MidiSession.Name,
                        Tag = args.MidiSession.EndPoint,
                        Type = ConnectionType.Network
                    };
                    OnMidiDeviceAdded?.Invoke(this, new MidiDeviceAddedEventArgs
                    {
                        MidiDeviceInfo = info
                    });

                    // auto connect to wifi, move this to TryOpenDevice when complete
                    OnMidiDeviceOpened?.Invoke(this, new MidiDeviceWasOpenedEventArgs
                    {
                        Name = args.MidiSession.Name + " " + args.MidiSession.EndPoint,
                        Sender = args.MidiSender,
                        Type = ConnectionType.Network
                    });
                };
                m_appleMidiSession.OnSessionEnded += (sender, args) =>
                {
                    OnMidiDeviceRemoved?.Invoke(this, new MidiDeviceRemovedEventArgs
                    {
                        Name = args.Name + " " + args.Endpoint,
                        Id = (int)m_appleMidiSession.m_ssrc,
                        Type = ConnectionType.Network
                    });
                };
                m_appleMidiSession.RegisterService(m_ipAdd, m_activity);
            }
            return false;
        }

        public MidiConnectionManager(Activity activity)
        {
            m_activity = activity;
            m_currentMode = ConnectionType.Network;
            // no midi manager = no bluetooth or usb :( 
        }

        public void SetMode(ConnectionType connection)
        {
            m_lastMode = m_currentMode;

            m_currentMode = connection;

            switch (m_currentMode)
            {
                case ConnectionType.Usb:
                case ConnectionType.BluetoothLe:
                    
                    break;
                case ConnectionType.Network:

                    break;
                default:
                    OnDeviceNotFound?.Invoke(this, new MidiDeviceNotFoundEventArgs
                    {
                        DummySender = new DefaultMidiSender()
                    });
                    break;
            }
        }

        private void SetupBluetoothDevice()
        {
            m_btAdapter = BluetoothAdapter.DefaultAdapter;
            if (m_btAdapter == null) return;

            if (!m_btAdapter.IsEnabled)
            {
                return;
            }

            m_btAdapter.StartDiscovery();
            BluetoothLeScanCallback btLeScanCallback = new BluetoothLeScanCallback();
            btLeScanCallback.OnBatchScanResultsHandler += (sender, args) =>
            {
                MyUtils.MessageBox(m_activity, "OnBatchScanResultsHandler");
            };
            btLeScanCallback.OnScanFailedHandler += (sender, args) =>
            {
                MyUtils.MessageBox(m_activity, "OnScanFailedHandler");
            };
            btLeScanCallback.OnScanResultHandler += (sender, args) =>
            {
                MyUtils.MessageBox(m_activity, "OnScanResultHandler");
            };

            ScanSettings scanSettings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.Balanced).Build();
            ScanFilter filter = new ScanFilter.Builder().SetServiceUuid(ParcelUuid.FromString(MidiHelper.MidiOverBtleUuid)).Build();
            List<ScanFilter> filters = new List<ScanFilter>();
            filters.Add(filter);
            m_btAdapter.BluetoothLeScanner.StartScan(filters, scanSettings, btLeScanCallback);

            var btDevices = m_btAdapter.BondedDevices;
            if (btDevices.Count > 0)
            {
                foreach (var btdev in btDevices)
                {
                    bool doContinue = false;
                    foreach (var uuid in btdev.GetUuids())
                    {
                        if (uuid.ToString().Equals(MidiHelper.MidiOverBtleUuid))
                        {
                            //doContinue = true;
                            break;
                        }
                    }
                    if (doContinue) continue;
                    if (!m_listItems.ContainsKey(btdev.Name + btdev.Address))
                    {
                        var listItem = new MidiDeviceListItem
                        {
                            Id = btdev.Name + btdev.Address,
                            Name = btdev.Name,
                            Type = ConnectionType.BluetoothLe,
                            Tag = btdev
                        };
                        var item = new MidiDeviceAddedEventArgs
                        {
                            MidiDeviceInfo = listItem
                        };
                        m_listItems.Add(btdev.Name + btdev.Address, listItem);
                        OnMidiDeviceAdded?.Invoke(this, item);
                    }
                }
            }
        }

        public void OpenBluetoothDev(BluetoothDevice btd)
        {
            var devs = m_midiManager.GetDevices();

            //devs.ToList().ForEach(x => Console.WriteLine(x.Properties.GetString(MidiDeviceInfo.PropertyName)));
            //Console.WriteLine(devs.Length);
            var existing = devs.FirstOrDefault(x => x.Properties.GetString(MidiDeviceInfo.PropertyName) == btd.Name);

            if (existing != null)
            {
                m_midiManager.OpenDevice(existing, this, new Handler(Looper.MainLooper));
            }
            else if (m_listItems.ContainsKey(btd.Name + btd.Address) == false)
            {
                m_midiManager.OpenBluetoothDevice(btd, this, new Handler(m_activity.MainLooper));
            }
        }

        public void AddBluetoothDev(BluetoothDevice item)
        {
            if (m_listItems.ContainsKey(item.Name + item.Address) == false)
            {
                var dli = new MidiDeviceListItem()
                {
                    Id = item.Name + item.Address,
                    Name = item.Name,
                    Type = ConnectionType.BluetoothLe,
                    Tag = item
                };

                var args = new MidiDeviceAddedEventArgs
                {
                    MidiDeviceInfo = dli
                };
                m_listItems.Add(item.Name + item.Address, dli);
                OnMidiDeviceAdded?.Invoke(this, args);
            }
        }

        public List<MidiDeviceListItem> FindAvailableDevices()
        {
            if (m_midiManager == null) return null;

            var devices = m_midiManager.GetDevices();

            if (devices.Length > 0)
            {
                foreach (var device in devices)
                {
                    ConnectionType type = device.Type == MidiDeviceType.Bluetooth ? ConnectionType.BluetoothLe : device.Type == MidiDeviceType.Usb ? ConnectionType.Usb : ConnectionType.Virtual;
                    if (!m_listItems.ContainsKey(device.Id.ToString()))
                    {
                        m_listItems.Add(device.Id.ToString(), new MidiDeviceListItem
                        {
                            Name = device.Properties.GetString("name"),
                            Type = type,
                            Id = device.Id.ToString(),
                            Tag = device
                        });                        
                    }
                }
            }
            return m_listItems.Values.ToList();
        }

        //public void TryOpenUsb(string name)
        //{
        //    var item = m_listItems.FirstOrDefault(x => x.Value.Name == name).Value;
        //
        //    if (item != null && item.Type == ConnectionType.Usb)
        //    {
        //        m_midiManager.OpenDevice((MidiDeviceInfo)item.Tag, this, new Handler(m_activity.MainLooper));
        //    }            
        //}

        //public void TryOpenBluetooth(string name)
        //{
        //    var item = m_listItems.FirstOrDefault(x => x.Value.Name == name).Value;
        //
        //    if (item != null && item.Type == ConnectionType.BluetoothLe)
        //    {
        //        if (item.Tag is BluetoothDevice)
        //        {                    
        //            m_midiManager.OpenBluetoothDevice((BluetoothDevice)m_listItems[item.Id].Tag, this, new Handler(m_activity.MainLooper));
        //        }
        //        else if (item.Tag is MidiDeviceInfo)
        //        {
        //            m_midiManager.OpenDevice((MidiDeviceInfo)m_listItems[item.Id].Tag, this, new Handler(m_activity.MainLooper));
        //        }
        //    }
        //}

        public void TryOpenUsb()
        {
            var item = m_listItems.FirstOrDefault(x => x.Value.Type == ConnectionType.Usb).Value.Tag;

            if (item != null)
            {
                m_midiManager.OpenDevice((MidiDeviceInfo)item, this, new Handler(m_activity.MainLooper));
            }
        }

        public void TryOpenDevice(MidiDeviceListItem item)
        {
            try
            {
                if (item.Type == ConnectionType.Usb)
                {
                    m_midiManager.OpenDevice((MidiDeviceInfo) m_listItems[item.Id].Tag, this,
                        new Handler(m_activity.MainLooper));
                }
                if (item.Type == ConnectionType.BluetoothLe)
                {
                    if (item.Tag is BluetoothDevice)
                    {
                        m_midiManager.OpenBluetoothDevice((BluetoothDevice) m_listItems[item.Id].Tag, this,
                            new Handler(m_activity.MainLooper));
                    }
                    else if (item.Tag is MidiDeviceInfo)
                    {
                        m_midiManager.OpenDevice((MidiDeviceInfo) m_listItems[item.Id].Tag, this,
                            new Handler(m_activity.MainLooper));
                    }
                }
                if (item.Type == ConnectionType.Network)
                {
                    //WifiManager wifiMgr = (WifiManager)m_activity.GetSystemService(Context.WifiService);
                    //if (wifiMgr.IsWifiEnabled)
                    //{
                    //    m_appleMidiSession?.InitiateSession(new IPEndPoint(IPAddress.Any, m_appleMidiSession.GetPort()));
                    //}
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(m_activity, "Error opening device, try restarting Beat Remote.", ToastLength.Long);
            }
        }

        public void OnDeviceOpened(MidiDevice device)
        {
            if (device == null) return; 

            m_device = device;

            int port = m_device.Info.GetPorts()[0].PortNumber;
            m_midiInputPort = m_device.OpenInputPort(port);

            string name = m_device.Info.Properties.GetString(MidiDeviceInfo.PropertyName);
            if (m_deviceItems.ContainsKey(name))
            {
                CloseDev(m_deviceItems[name]);
                m_deviceItems.Remove(name);
            }
                
            m_deviceItems.Add(name, new CleanupMidi { Device = m_device, Port = m_midiInputPort, Name = name, Id = m_device.Info.Id });

            if (device.Info.Type == MidiDeviceType.Usb)
            {
                var usbSender = new UsbMidiSender(m_midiInputPort);
                usbSender.Suspend = false;
                OnMidiDeviceOpened?.Invoke(this, new MidiDeviceWasOpenedEventArgs
                {
                    Name = device.Info.Properties.GetString("name"),
                    Sender = usbSender,
                    Type = ConnectionType.Usb
                });
            }
            if (device.Info.Type == MidiDeviceType.Bluetooth)
            {
                var btSender = new BluetoothLeMidiSender(m_midiInputPort);
                btSender.Suspend = false;
                OnMidiDeviceOpened?.Invoke(this, new MidiDeviceWasOpenedEventArgs
                {
                    Name = device.Info.Properties.GetString("name"),
                    Sender = btSender,
                    Type = ConnectionType.BluetoothLe
                });
            }                                    
        }

        public void Shutdown()
        {
            m_appleMidiSession?.Shutdown();

            foreach (var item in m_deviceItems)
            {
                item.Value.Port?.Close();
                item.Value.Device?.Close();
            }
            IsShutdown = true;
        }

        public void CloseDev(MidiDeviceInfo info)
        {
            var item = m_deviceItems[info.Properties.GetString("name")];
            item?.Port.Close();            
            item?.Device.Close();
        }

        private void CloseDev(CleanupMidi midi)
        {
            midi.Port?.Close();
            midi.Device?.Close();
        }
    }
}