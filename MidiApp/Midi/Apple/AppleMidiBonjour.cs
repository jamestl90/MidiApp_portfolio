using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net.Nsd;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp.Midi.Apple
{
    public class ResolveServiceListener : Java.Lang.Object, NsdManager.IResolveListener
    {
        public EventHandler<ResolveFailArgs> OnResolveFailedHandler;
        public EventHandler<ResolveSuccessArgs> OnServiceResolvedHandler;

        public class ResolveFailArgs : EventArgs
        {
            public NsdServiceInfo Info { get; set; }
            public NsdFailure ErrorCode { get; set; }
        }

        public class ResolveSuccessArgs : EventArgs
        {
            public NsdServiceInfo Info { get; set; }
        }

        public void OnResolveFailed(NsdServiceInfo serviceInfo, NsdFailure errorCode)
        {
            OnResolveFailedHandler?.Invoke(this, new ResolveFailArgs { ErrorCode = errorCode, Info = serviceInfo });
        }

        public void OnServiceResolved(NsdServiceInfo serviceInfo)
        {
            OnServiceResolvedHandler?.Invoke(this, new ResolveSuccessArgs { Info = serviceInfo });
        }
    }

    public class DiscoveryServiceListener : Java.Lang.Object, NsdManager.IDiscoveryListener
    {
        public EventHandler<DiscoveryEventArgs> OnDiscoveryStartedHandler;
        public EventHandler<DiscoveryEventArgs> OnDiscoveryStoppedHandler;
        public EventHandler<ServiceEventArgs> OnServiceFoundHandler;
        public EventHandler<ServiceEventArgs> OnServiceLostHandler;
        public EventHandler<DiscoveryStartStopEventArgs> OnStartDiscoveryFailedHandler;
        public EventHandler<DiscoveryStartStopEventArgs> OnStopDiscoveryFailedHandler;

        public class ServiceEventArgs : EventArgs
        {
            public NsdServiceInfo Info { get; set; }
        }

        public class DiscoveryEventArgs : EventArgs
        {
            public string ServiceType { get; set; }
        }

        public class DiscoveryStartStopEventArgs : EventArgs
        {
            public string ServiceType { get; set; }
            public NsdFailure ErrorCode { get; set; }
        }

        public void OnDiscoveryStarted(string serviceType)
        {
            OnDiscoveryStartedHandler?.Invoke(this, new DiscoveryEventArgs { ServiceType = serviceType });
        }

        public void OnDiscoveryStopped(string serviceType)
        {
            OnDiscoveryStoppedHandler?.Invoke(this, new DiscoveryEventArgs { ServiceType = serviceType });
        }

        public void OnServiceFound(NsdServiceInfo serviceInfo)
        {
            OnServiceFoundHandler?.Invoke(this, new ServiceEventArgs { Info = serviceInfo });
        }

        public void OnServiceLost(NsdServiceInfo serviceInfo)
        {
            OnServiceLostHandler?.Invoke(this, new ServiceEventArgs { Info = serviceInfo });
        }

        public void OnStartDiscoveryFailed(string serviceType, NsdFailure errorCode)
        {
            OnStartDiscoveryFailedHandler?.Invoke(this, new DiscoveryStartStopEventArgs { ErrorCode = errorCode, ServiceType = serviceType });
        }

        public void OnStopDiscoveryFailed(string serviceType, NsdFailure errorCode)
        {
            OnStopDiscoveryFailedHandler?.Invoke(this, new DiscoveryStartStopEventArgs { ErrorCode = errorCode, ServiceType = serviceType });
        }
    }

    public class RegisterServiceListener : Java.Lang.Object, NsdManager.IRegistrationListener
    {
        public EventHandler<FailureEventArgs> OnRegistrationFailedHandler;
        public EventHandler<RegisterEventArgs> OnServiceRegisteredHandler;
        public EventHandler<RegisterEventArgs> OnServiceUnregisteredHandler;
        public EventHandler<FailureEventArgs> OnUnregistrationFailedHandler;

        public class FailureEventArgs : EventArgs
        {
            public NsdServiceInfo Info { get; set; }
            public NsdFailure ErrorCode { get; set; }
        }

        public class RegisterEventArgs : EventArgs
        {
            public NsdServiceInfo Info { get; set; }
        }

        public void OnRegistrationFailed(NsdServiceInfo serviceInfo, NsdFailure errorCode)
        {
            OnRegistrationFailedHandler?.Invoke(this, new FailureEventArgs { ErrorCode = errorCode, Info = serviceInfo });
        }

        public void OnServiceRegistered(NsdServiceInfo serviceInfo)
        {
            OnServiceRegisteredHandler?.Invoke(this, new RegisterEventArgs { Info = serviceInfo });
        }

        public void OnServiceUnregistered(NsdServiceInfo serviceInfo)
        {
            OnServiceUnregisteredHandler?.Invoke(this, new RegisterEventArgs { Info = serviceInfo });
        }

        public void OnUnregistrationFailed(NsdServiceInfo serviceInfo, NsdFailure errorCode)
        {
            OnUnregistrationFailedHandler?.Invoke(this, new FailureEventArgs { ErrorCode = errorCode, Info = serviceInfo });
        }
    }
}