using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TestApp.Bluetooth
{
    public class BluetoothLeScanCallback : ScanCallback
    {
        public EventHandler<BatchScanResultsArgs> OnBatchScanResultsHandler { get; set; }
        public EventHandler<ScanFailedArgs> OnScanFailedHandler { get; set; }
        public EventHandler<ScanResultArgs> OnScanResultHandler { get; set; }

        public class BatchScanResultsArgs
        {
            public IList<ScanResult> Results { get; set; }
        }

        public class ScanFailedArgs
        {
            public ScanFailure ErrorCode { get; set; }
        }

        public class ScanResultArgs
        {
            public ScanCallbackType CallbackType { get; set; }
            public ScanResult ScanResult { get; set; }
        }

        public BluetoothLeScanCallback()
        {       
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);

            OnBatchScanResultsHandler.Invoke(this, new BatchScanResultsArgs { Results = results });
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);

            OnScanFailedHandler.Invoke(this, new ScanFailedArgs {ErrorCode = errorCode});
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            OnScanResultHandler.Invoke(this, new ScanResultArgs
            {
                CallbackType = callbackType,
                ScanResult = result
            });
        }
    }
}