using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Com.Acs.Smartcard;
using Android.Content;
using Android.Hardware.Usb;
using System;
using Android.Support.V4.View;
using Android.Arch.Lifecycle;
using Java.Math;
using Java.Lang;
using System.Runtime.CompilerServices;
using Java.IO;
using Android.Icu.Text;
using System.Diagnostics;
using Android.Support.V4.App;
using Android;
using Android.Provider;
using System.IO;
using Android.Graphics;
using Android.Nfc;
using static NfcBindingTest.MainActivity;

namespace NfcBindingTest
{
    public class stateChangeListner : Java.Lang.Object, Com.Acs.Smartcard.Reader.IOnStateChangeListener
    {
        private static ReadKeyOption mReadKeyOption = new ReadKeyOption();
        private static Com.Acs.Smartcard.Reader mReader;
        MainActivity main;
        public stateChangeListner(Com.Acs.Smartcard.Reader _reader, ReadKeyOption _mReadKeyOption, MainActivity _main)
        {
            main = _main;
            //   main = new MainActivity();
            mReader = _reader;
            mReadKeyOption = _mReadKeyOption;
        }

        public void OnStateChange(int p0, int p1, int p2)
        {
            System.Console.WriteLine("Implement Interface");
            logMsg(DateTime.Now + ":: Line:42 READER IS OPEN " + mReader.IsOpened);
            try
            {
                logMsg(DateTime.Now + ":: Line:45 GET STATE-" + mReader.GetState(0));
                if (mReader.GetState(0) == 2)
                {
                    logMsg(DateTime.Now + ":: Line:47 GET STATE IF");
                    try
                    {
                        logMsg(DateTime.Now + ":: Line:50 TRY1");
                        mReader.Power(0, Com.Acs.Smartcard.Reader.CardWarmReset);
                        logMsg(DateTime.Now + ":: Line:52 TRY2");
                        mReader.SetProtocol(0, Com.Acs.Smartcard.Reader.ProtocolT0);
                        logMsg(DateTime.Now + ":: Line:53 TRY3");
                        byte[] command = { (byte)0xFF, (byte)0xCA, (byte)0x00, (byte)0x00, (byte)0x00 };
                        byte[] rcvbuffer = new byte[300];
                        mReader.Control(0, Com.Acs.Smartcard.Reader.IoctlCcidEscape, command, command.Length, rcvbuffer, rcvbuffer.Length);
                        byte[] response = new byte[300];
                        string commandString = "FF CA 00 00 04";
                        string cmdStr = toHexString(mReadKeyOption.ToByteArray());
                        byte[] command1 = toByteArray(commandString.Substring(0));
                        int key = Com.Acs.Smartcard.Reader.IoctlCcidEscape;
                        int responseLength;
                        if (key < 0)
                        {
                            responseLength = mReader.Transmit(0, command, command.Length, response, response.Length);
                        }
                        else
                        {
                            responseLength = mReader.Control(0, key, command, command.Length, response, response.Length);
                        }
                        ResponseResult result = this.parseResponse(response, responseLength);
                        logMsg(DateTime.Now + ":: Line: 73, GET STATUS=" + result.status);

                        if (result.status)
                        {
                            logMsg(DateTime.Now + ":: Line: 77 RESULT NOT NULL");
                            logMsg("Card HEX ID:" + result.hex_id);
                            logMsg("Reverse Card HEX ID:" + result.reverse_hex_id);
                            logMsg("Card ID:" + result.id);
                            logMsg("Reverse Card ID:" + result.revers_id);
                            string cardId = result.id;
                            logMsg(DateTime.Now + ":: Line:88 Object calling method");



                            main.RunOnUiThread(() =>
                            {
                                logMsg(DateTime.Now + ":: Line:95 RunOnUiThread");
                                main.printCardId(cardId);
                            });

                            logMsg(DateTime.Now + ":: Line:89 PRINT DONE");
                            logMsg("************");
                        }
                    }
                    catch (ReaderException ex)
                    {
                        logMsg(DateTime.Now + ":: Line:94 GET READER EXCEPTION-" + ex.Message);
                    }
                }
                else
                {
                    logMsg(DateTime.Now + ":: Line:92 GET STATE ELSE");
                }
                logMsg(DateTime.Now + ":: Line:94 END OnStateChange");

            }
            catch (Java.Lang.Exception ex)
            {
                logMsg(DateTime.Now + ":: Line:99 EXCEPTION-" + ex.Message);
                throw new NotImplementedException();
            }
            logMsg(DateTime.Now + ":: Line:102 END OnStateChange");
        }

        //public void OnStateChange(int p0, int p1, int p2)
        //{
        //    throw new NotImplementedException();
        //}
        private ResponseResult parseResponse(byte[] buffer, int bufferLength)
        {
            try
            {
                logMsg(DateTime.Now + ":: Line:320 " + ",parseResponse");
                ResponseResult result = new ResponseResult();
                string bufferString = BuildConfig.Flavor;

                logMsg(DateTime.Now + ":: Line:305 " + ",bufferLength-" + bufferLength);
                for (int i = 0; i < bufferLength; i++)
                {
                    string hexChar = Integer.ToHexString(buffer[i] & MotionEventCompat.ActionMask);
                    if (hexChar.Length == 1)
                    {
                        hexChar = "0" + hexChar;
                    }
                    if (i % 16 == 0 && bufferString != BuildConfig.Flavor)
                    {
                        result.status = false;
                        break;
                    }
                    bufferString = bufferString + hexChar.ToUpper() + ":";
                }
                logMsg(DateTime.Now + ":: Line:320 " + ",bufferString-" + bufferString);
                string[] separated = bufferString.Split(':');
                bool z = separated[4].Equals("90") && separated[5].Equals("00");
                result.status = z;
                logMsg(DateTime.Now + ":: Line:320 " + ",RESULT.STATUS=" + result.status);
                if (result.status)
                {
                    result.hex_id = separated[0] + ":" + separated[1] + ":" + separated[2] + ":" + separated[3];

                    logMsg(DateTime.Now + ":: Line:324 " + ",RESULT.HEX ID=" + result.hex_id);
                    result.reverse_hex_id = separated[3] + ":" + separated[2] + ":" + separated[1] + ":" + separated[0];
                    //  result.id = new BigInteger(result.hex_id.Replace(":", BuildConfig.Flavor));
                    result.id = result.hex_id.Replace(":", BuildConfig.Flavor);

                    logMsg(DateTime.Now + ":: Line:324 " + ",RESULT.ID=" + result.id);
                    // result.revers_id = new BigInteger(result.reverse_hex_id.Replace(":", BuildConfig.Flavor));
                    result.revers_id = result.reverse_hex_id.Replace(":", BuildConfig.Flavor);
                }
                //Integer num = this.card_count;
                //this.card_count = Integer.valueOf(this.card_count.intValue() + 1);
                return result;
            }
            catch (Java.Lang.Exception ex)
            {
                logMsg(DateTime.Now + ":: Line:141 parseResponse-" + ex.Message);
                return null;
            }
        }

        private static byte[] toByteArray(string hexString)
        {

            int hexStringLength = hexString.Length;
            byte[] byteArray = null;
            int count = 0;
            char c;
            int i;

            // Count number of hex characters
            for (i = 0; i < hexStringLength; i++)
            {

                c = hexString[i];
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a'
                        && c <= 'f')
                {
                    count++;
                }
            }

            byteArray = new byte[(count + 1) / 2];
            bool first = true;
            int len = 0;
            int value;
            for (i = 0; i < hexStringLength; i++)
            {

                c = hexString[i];
                if (c >= '0' && c <= '9')
                {
                    value = c - '0';
                }
                else if (c >= 'A' && c <= 'F')
                {
                    value = c - 'A' + 10;
                }
                else if (c >= 'a' && c <= 'f')
                {
                    value = c - 'a' + 10;
                }
                else
                {
                    value = -1;
                }

                if (value >= 0)
                {

                    if (first)
                    {

                        byteArray[len] = (byte)(value << 4);

                    }
                    else
                    {

                        byteArray[len] |= (byte)value;
                        len++;
                    }

                    first = !first;
                }
            }

            return byteArray;
        }
        private string toHexString(int i)
        {

            string hexString = Integer.ToHexString(i);
            if (hexString.Length % 2 != 0)
            {
                hexString = "0" + hexString;
            }

            return hexString.ToUpper();
        }
        private static string toHexString(byte[] buffer)
        {

            string bufferString = "";

            for (int i = 0; i < buffer.Length; i++)
            {

                string hexChar = Integer.ToHexString(buffer[i] & 0xFF);
                if (hexChar.Length == 1)
                {
                    hexChar = "0" + hexChar;
                }

                bufferString += hexChar.ToUpper() + " ";
            }

            return bufferString;
        }
        private void logBuffer(byte[] buffer, int bufferLength)
        {

            string bufferString = "";

            for (int i = 0; i < bufferLength; i++)
            {

                string hexChar = Integer.ToHexString(buffer[i] & 0xFF);
                if (hexChar.Length == 1)
                {
                    hexChar = "0" + hexChar;
                }

                if (i % 16 == 0)
                {

                    if (bufferString != "")
                    {
                        bufferString = "";
                    }
                }

                bufferString += hexChar.ToUpper() + " ";
            }

            if (bufferString != "")
            {
            }
        }

        private static void logMsg(string text)
        {
            string path = "/storage/emulated/0/logCsharp.txt";
            Java.IO.File logFile = new Java.IO.File(path);
            //File logFile = new File("sdcard/log.file");
            if (!logFile.Exists())
            {
                try
                {
                    // logFile.Mkdir();
                    logFile.CreateNewFile();
                }
                catch (Java.IO.IOException e)
                {
                    // TODO Auto-generated catch block
                    e.PrintStackTrace();
                }
            }
            try
            {
                //BufferedWriter for performance, true to set append to file flag
                BufferedWriter buf = new BufferedWriter(new FileWriter(logFile, true));
                buf.Append(text);
                buf.NewLine();
                buf.Close();
            }
            catch (Java.IO.IOException e)
            {
                // TODO Auto-generated catch block
                e.PrintStackTrace();
            }
        }
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        //  private State state;

        //   private OnStateChangeListener onStateChangeListener;
        private static UsbManager mManager;
        private static Com.Acs.Smartcard.Reader mReader;
        private static ArrayAdapter<string> mReaderAdapter;
        private static ArrayAdapter<string> mSlotAdapter;
        private static Features mFeatures = new Features();
        private static PendingIntent mPermissionIntent;
        private static Spinner mReaderSpinner;
        StackFrame CallStack;
        private static Spinner mSlotSpinner;
        private static ReadKeyOption mReadKeyOption = new ReadKeyOption();
        private NfcAdapter _nfcAdapter;
        TextView txtview;
        public class ResponseResult
        {
            public string hex_id;
            public string id;
            public string revers_id;
            public string reverse_hex_id;
            public bool status;

            public ResponseResult()
            {
            }
        }
        public void printCardId(string id)
        {
            try
            {
                logMsg("Line:360 printCardId==" + id);
                logMsg(DateTime.Now + ":: Line:362 PRINTING TEXT1" + txtview);
                if (txtview != null)
                {
                    txtview.Text = id;
                    logMsg(DateTime.Now + ":: Line:362 PRINTING TEXT2");
                }
                else
                {
                    logMsg(DateTime.Now + ":: Line:374 TEXT VIEW IS NULL");
                }
            }
            catch (Java.Lang.Exception ex)
            {
                logMsg(DateTime.Now + ":: Line:362 Exception=" + ex.Message);
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            CallStack = new StackFrame(1, true);
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            mManager = (UsbManager)this.Application.GetSystemService(Context.UsbService);
            // Initialize reader
            mReader = new Com.Acs.Smartcard.Reader(mManager);


            // Initialize reader spinner
            mReaderAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);

            foreach (UsbDevice device in mManager.DeviceList.Values)
            {
                if (mReader.IsSupported(device))
                {
                    mReaderAdapter.Add(device.DeviceName);
                }
            }
            stateChangeListner change = new stateChangeListner(mReader, mReadKeyOption, this);
            // Com.Acs.Smartcard.Reader.IOnStateChangeListener onStateChangeListener =new ;

            mReader.SetOnStateChangeListener(change);


            mReaderSpinner = FindViewById<Spinner>(Resource.Id.main_spinner_reader);
            mReaderSpinner.Adapter = mReaderAdapter;

            mPermissionIntent = PendingIntent.GetBroadcast(this, 0, new Intent(
                ACTION_USB_PERMISSION), 0);
            //IntentFilter filter = new IntentFilter();
            //filter.AddAction(ACTION_USB_PERMISSION);
            //filter.AddAction(UsbManager.ActionUsbDeviceDetached);
            //filter.AddAction(UsbManager.ActionUsbDeviceAttached);
            txtview = FindViewById<TextView>(Resource.Id.txtview);
            //RegisterReceiver(receiver, intent);
            //TextView txt = FindViewById<TextView>(Resource.Id.txtview);
            //txt.Text = "";
            try
            {
                mReceiver _receiver = new mReceiver();
                IntentFilter filter = new IntentFilter();
                filter.AddAction(ACTION_USB_PERMISSION);
                filter.AddAction(UsbManager.ActionUsbDeviceDetached);
                filter.AddAction(UsbManager.ActionUsbDeviceAttached);

                RegisterReceiver(_receiver, filter);


                //RegisterReceiver(_receiver, new IntentFilter(ACTION_USB_PERMISSION));
                //RegisterReceiver(_receiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
                //RegisterReceiver(_receiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));

                ////var intent = new Intent(this, typeof(mReceiver));
                //var intent = new Intent(this, receiver.Class);
                //intent.SetAction(ACTION_USB_PERMISSION);
                //intent.SetAction(UsbManager.ActionUsbDeviceDetached);
                //intent.SetAction(UsbManager.ActionUsbDeviceAttached);
                // Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);
                //SendBroadcast(intent);
                // txt.Text = "done";
                // ****** OnStateChangeListener ******
                // this.onStateChange(0);

            }
            catch (SecurityException ex)
            {
                txtview.Text = ex.Message.ToString();
            }

            mSlotAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            mSlotSpinner = FindViewById<Spinner>(Resource.Id.main_spinner_slot);
            mSlotSpinner.Adapter = mSlotAdapter;

            string deviceName = (string)mReaderSpinner.SelectedItem;
            if (deviceName != null)
            {
                foreach (UsbDevice device in mManager.DeviceList.Values)
                {
                    if (deviceName.Equals(device.DeviceName))
                    {
                        mManager.RequestPermission(device, mPermissionIntent);
                        break;
                    }
                }
            }
        }

        public void onStateChange(int slotNum)
        {
            System.Console.WriteLine("onStateChange=" + " " + slotNum);
            logMsg(DateTime.Now + ":: Line:152 " + "slotNum-" + slotNum + " IS OPEN " + mReader.IsOpened);
            try
            {
                logMsg(DateTime.Now + ":: Line:156 GET STATE-" + mReader.GetState(0));
                if (mReader.GetState(0) == 1)
                {

                    try
                    {
                        mReader.Power(0, Com.Acs.Smartcard.Reader.CardWarmReset);
                        mReader.SetProtocol(0, Com.Acs.Smartcard.Reader.ProtocolT0);
                        byte[] command = { (byte)0xFF, (byte)0xCA, (byte)0x00, (byte)0x00, (byte)0x00 };
                        byte[] rcvbuffer = new byte[300];
                        mReader.Control(0, Com.Acs.Smartcard.Reader.IoctlCcidEscape, command, command.Length, rcvbuffer, rcvbuffer.Length);
                        byte[] response = new byte[300];
                        string commandString = "FF CA 00 00 04";
                        string cmdStr = toHexString(mReadKeyOption.ToByteArray());
                        byte[] command1 = toByteArray(commandString.Substring(0));
                        int key = Com.Acs.Smartcard.Reader.IoctlCcidEscape;
                        int responseLength;
                        if (key < 0)
                        {
                            responseLength = mReader.Transmit(0, command, command.Length, response, response.Length);
                        }
                        else
                        {
                            responseLength = mReader.Control(0, key, command, command.Length, response, response.Length);
                        }
                        ResponseResult result = this.parseResponse(response, responseLength);
                        logMsg(DateTime.Now + ":: Line: 181, GET RESULT-" + result);

                        if (result != null)
                        {

                        }
                    }
                    catch (ReaderException ex)
                    {
                        System.Console.WriteLine("ReaderException=" + " " + ex.Message);
                        logMsg(DateTime.Now + ":: Line:191 GET READER EXCEPTION-" + ex.Message);
                    }
                }

            }
            catch (Java.Lang.Exception ex)
            {

            }
        }

        private static string ACTION_USB_PERMISSION = "com.android.example.USB_PERMISSION";

        //[BroadcastReceiver(Enabled = true, Exported = false)]
        [BroadcastReceiver]
        //[IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached, UsbManager.ActionUsbDeviceDetached })]
        //[MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
        //[MetaData(UsbManager.ActionUsbDeviceDetached, Resource = "@xml/device_filter")]

        //[IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached, UsbManager.ActionUsbDeviceDetached })]

        public class mReceiver : BroadcastReceiver
        {
            //[MethodImpl(MethodImplOptions.Synchronized)]
            public override void OnReceive(Context context, Intent intent)
            {
                //intent.SetAction(ACTION_USB_PERMISSION);
                //intent.SetAction(UsbManager.ActionUsbDeviceAttached);
                //intent.SetAction(UsbManager.ActionUsbDeviceDetached);
                // Do stuff here.
                string value = intent.GetStringExtra("key");
                string action = intent.Action;
                logMsg(DateTime.Now + " Line:219: Action name:" + action);
                if (ACTION_USB_PERMISSION.Equals(action))
                {
                    logMsg(DateTime.Now + " Line:219: ACTION_USB_PERMISSION1");
                    UsbDevice device = (UsbDevice)intent
                           .GetParcelableExtra(UsbManager.ExtraDevice);
                    logMsg(DateTime.Now + " Line:219: ACTION_USB_PERMISSION Device" + device);
                    if (intent.GetBooleanExtra(
                           UsbManager.ExtraPermissionGranted, false))
                    {
                        if (device != null)
                        {

                            mReader.Open(device);
                            logMsg(DateTime.Now + " Line:219: ACTION_USB_PERMISSION OPEN");
                            new OpenTask().Execute(device);
                            byte[] buffer = toByteArray("02");
                            mReadKeyOption.TimeOut = buffer[0] & 0xFF;
                        }
                    }
                }
                else if (UsbManager.ActionUsbDeviceDetached.Equals(action))
                {
                    logMsg(DateTime.Now + " Line:219: ActionUsbDeviceDetached");
                    //// Update reader list
                    mReaderAdapter.Clear();
                    foreach (UsbDevice device1 in mManager.DeviceList.Values)
                    {
                        if (mReader.IsSupported(device1))
                        {
                            mReaderAdapter.Add(device1.DeviceName);
                        }
                    }

                    UsbDevice device = (UsbDevice)intent
                            .GetParcelableExtra(UsbManager.ExtraDevice);
                    //logMsg(DateTime.Now + " Line:219: ActionUsbDeviceDetached::" + device);
                    if (device != null && device.Equals(mReader.Device))
                    {
                        // Clear slot items
                        mSlotAdapter.Clear();

                        // Close reader
                        System.Console.Write("Closing reader...");
                        new CloseTask().Execute();
                    }

                }
                else if (UsbManager.ActionUsbDeviceAttached.Equals(action))
                {
                    logMsg(DateTime.Now + " Line:268: ActionUsbDeviceAttached::");
                    //if (deviceName != null) {

                    // For each device
                    foreach (UsbDevice device1 in mManager.DeviceList.Values)
                    {
                        // Request permission
                        mManager.RequestPermission(device1,
                                mPermissionIntent);
                        //requested = true;
                        break;
                        // }
                    }

                    // }
                }
            }
        }
        //[Android.Runtime.Register("android/os/AsyncTask", DoNotGenerateAcw = true)]
        //private class OpenTask<UsbDevice, Void, Exception> : Object

        private class OpenTask : AsyncTask
        {
            protected override void OnPreExecute()
            {
                logMsg(DateTime.Now + "::ON OPEN TASK");
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {

                return null;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                if (result == null)
                {
                    int numSlots = mReader.NumSlots;
                    mFeatures.Clear();
                    mSlotSpinner.Enabled = true;
                    mSlotAdapter.Clear();
                    for (int i = 0; i < numSlots; i++)
                    {
                        mSlotAdapter.Add(Integer.ToString(i));
                    }
                    mSlotSpinner.SetSelection(1);
                    int slotNum = 0;
                    if (slotNum != Spinner.InvalidPosition)
                    {
                        // Set parameters
                        TransmitParams params11 = new TransmitParams();
                        params11.slotNum = slotNum;
                        params11.controlCode = Com.Acs.Smartcard.Reader.IoctlAcr83ReadKey;
                        params11.commandString = toHexString(mReadKeyOption
                                .ToByteArray());

                        // Transmit control command
                        // new TransmitTask().Execute();
                        // Remove all control codes
                    }
                    /*** End READING KEY***/
                    mFeatures.Clear();
                }

            }
        }
        private class CloseTask : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                mReader.Close();
                return null;
            }
            protected override void OnPostExecute(Java.Lang.Object result)
            {
            }
        }
        private class TransmitTask : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                return null;
            }

            //protected override void OnProgressUpdate(Java.Lang.Object progress)
            //{                
            //}
        }
        private ResponseResult parseResponse(byte[] buffer, int bufferLength)
        {
            logMsg(DateTime.Now + ":: Line:320 " + ",parseResponse");
            ResponseResult result = new ResponseResult();
            string bufferString = BuildConfig.Flavor;

            logMsg(DateTime.Now + ":: Line:305 " + ",bufferLength-" + bufferLength);
            for (int i = 0; i < bufferLength; i++)
            {
                string hexChar = Integer.ToHexString(buffer[i] & MotionEventCompat.ActionMask);
                if (hexChar.Length == 1)
                {
                    hexChar = "0" + hexChar;
                }
                if (i % 16 == 0 && bufferString != BuildConfig.Flavor)
                {
                    result.status = false;
                    break;
                }
                bufferString = bufferString + hexChar.ToUpper() + ":";
            }
            logMsg(DateTime.Now + ":: Line:320 " + ",bufferString-" + bufferString);
            string[] separated = bufferString.Split(':');
            bool z = separated[4].Equals("90") && separated[5].Equals("00");
            result.status = z;
            logMsg(DateTime.Now + ":: Line:320 " + ",RESULT.STATUS=" + result.status);
            if (result.status)
            {
                result.hex_id = separated[0] + ":" + separated[1] + ":" + separated[2] + ":" + separated[3];
                //  TextView txtview = FindViewById<TextView>(Resource.Id.txtview);
                //  txtview.Text = result.hex_id;
                logMsg(DateTime.Now + ":: Line:324 " + ",RESULT.HEX ID=" + result.hex_id);
                result.reverse_hex_id = separated[3] + ":" + separated[2] + ":" + separated[1] + ":" + separated[0];
                //result.id = new BigInteger(result.hex_id.Replace(":", BuildConfig.Flavor));
                //result.revers_id = new BigInteger(result.reverse_hex_id.Replace(":", BuildConfig.Flavor));
                result.id = result.hex_id.Replace(":", BuildConfig.Flavor);
                result.revers_id = result.reverse_hex_id.Replace(":", BuildConfig.Flavor);
            }
            //Integer num = this.card_count;
            //this.card_count = Integer.valueOf(this.card_count.intValue() + 1);
            return result;
        }

        private class TransmitParams
        {

            public int slotNum;
            public int controlCode;
            public string commandString;
        }
        //private class TransmitProgress
        //{

        //    public int controlCode;
        //    public byte[] command;
        //    public int commandLength;
        //    public byte[] response;
        //    public int responseLength;
        //    public Java.Lang.Exception e;
        //}
        private static byte[] toByteArray(string hexString)
        {

            int hexStringLength = hexString.Length;
            byte[] byteArray = null;
            int count = 0;
            char c;
            int i;

            // Count number of hex characters
            for (i = 0; i < hexStringLength; i++)
            {

                c = hexString[i];
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a'
                        && c <= 'f')
                {
                    count++;
                }
            }

            byteArray = new byte[(count + 1) / 2];
            bool first = true;
            int len = 0;
            int value;
            for (i = 0; i < hexStringLength; i++)
            {

                c = hexString[i];
                if (c >= '0' && c <= '9')
                {
                    value = c - '0';
                }
                else if (c >= 'A' && c <= 'F')
                {
                    value = c - 'A' + 10;
                }
                else if (c >= 'a' && c <= 'f')
                {
                    value = c - 'a' + 10;
                }
                else
                {
                    value = -1;
                }

                if (value >= 0)
                {

                    if (first)
                    {

                        byteArray[len] = (byte)(value << 4);

                    }
                    else
                    {

                        byteArray[len] |= (byte)value;
                        len++;
                    }

                    first = !first;
                }
            }

            return byteArray;
        }
        private string toHexString(int i)
        {

            string hexString = Integer.ToHexString(i);
            if (hexString.Length % 2 != 0)
            {
                hexString = "0" + hexString;
            }

            return hexString.ToUpper();
        }
        private static string toHexString(byte[] buffer)
        {

            string bufferString = "";

            for (int i = 0; i < buffer.Length; i++)
            {

                string hexChar = Integer.ToHexString(buffer[i] & 0xFF);
                if (hexChar.Length == 1)
                {
                    hexChar = "0" + hexChar;
                }

                bufferString += hexChar.ToUpper() + " ";
            }

            return bufferString;
        }
        private void logBuffer(byte[] buffer, int bufferLength)
        {

            string bufferString = "";

            for (int i = 0; i < bufferLength; i++)
            {

                string hexChar = Integer.ToHexString(buffer[i] & 0xFF);
                if (hexChar.Length == 1)
                {
                    hexChar = "0" + hexChar;
                }

                if (i % 16 == 0)
                {

                    if (bufferString != "")
                    {
                        bufferString = "";
                    }
                }

                bufferString += hexChar.ToUpper() + " ";
            }

            if (bufferString != "")
            {
            }
        }

        private static void logMsg(string text)
        {

            //string path = "/storage/emulated/0/logsX.txt";
            //var path1 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            //System.IO.File.Create(path);

            //Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            //Java.IO.File dir = new Java.IO.File(sdCard.AbsolutePath);
            //dir.Mkdirs();
            //Java.IO.File file = new Java.IO.File(dir, "iootext.txt");
            //if (!file.Exists())
            //{
            //    file.CreateNewFile();
            //    file.Mkdir();
            //    FileWriter writer = new FileWriter(file);
            //    // Writes the content to the file
            //    writer.Write("");
            //    writer.Flush();
            //    writer.Close();
            //}

            string path = "/storage/emulated/0/logCsharp.txt";
            Java.IO.File logFile = new Java.IO.File(path);
            //File logFile = new File("sdcard/log.file");
            if (!logFile.Exists())
            {
                try
                {
                    // logFile.Mkdir();
                    logFile.CreateNewFile();
                }
                catch (Java.IO.IOException e)
                {
                    // TODO Auto-generated catch block
                    e.PrintStackTrace();
                }
            }
            try
            {
                //BufferedWriter for performance, true to set append to file flag
                BufferedWriter buf = new BufferedWriter(new FileWriter(logFile, true));
                buf.Append(text);
                buf.NewLine();
                buf.Close();
            }
            catch (Java.IO.IOException e)
            {
                // TODO Auto-generated catch block
                e.PrintStackTrace();
            }
        }
        /*public void setOnStateChangeListener(OnStateChangeListener listener)
          {
              this.onStateChangeListener = listener;
          }
          public State getState()
          {
              return state;
          }
          public interface OnStateChangeListener
          {
              void onStateChange(int slotNum, int prevState, int currState);
          }
          public enum State
          {
              COLLAPSED,
              EXPANDED,
              IDLE
          }*/
    }
}


