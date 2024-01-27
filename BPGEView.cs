using System.Net;

namespace BPGE 
{
    using Terminal.Gui;

    public partial class BPGEView : Window
    {
        private bool _debug;
        public int IntifacePort;
        
        public HttpReceiver HttpReceiver;
        public VibrationManager VibrationManager;

        public BPGEView(bool debug, int intifacePort)
        {
            _debug = debug;
            IntifacePort = intifacePort;
            init();
            serverLabel.Clicked += () => {
                Clipboard.Contents = $"{Dns.GetHostName().ToLower()}:23456";
                MessageBox.Query ("Copied", "Copied to clipboard", "Ok");
            };
            btnTest.Clicked += async () => {
                if (VibrationManager is { Client.Connected: true })
                {
                    new Thread(async () =>
                    {
                        foreach (var buttplugClientDevice in VibrationManager.Client.Devices)
                        {
                            await buttplugClientDevice.VibrateAsync(1);
                        }

                        Thread.Sleep(1000);
                        foreach (var buttplugClientDevice in VibrationManager.Client.Devices)
                        {
                            await buttplugClientDevice.VibrateAsync(0);
                        }
                    }).Start();
                }
            };
            btnReconnect.Clicked += async () =>
            {
                VibrationManager?.Reconnect();
            };
            btnStopVibration.Clicked += async () =>
            {
                VibrationManager?.ClearArray();
                new Thread(async () =>
                {
                    if (VibrationManager?.Client?.Devices == null) return;
                    foreach (var buttplugClientDevice in VibrationManager.Client.Devices)
                    {
                        await buttplugClientDevice.VibrateAsync(0);
                    }
                }).Start();
            };
            Initialized += (s,e) => {
                LogDebug(Directory.GetCurrentDirectory());
                HttpReceiver = new HttpReceiver(this);
                HttpReceiver.Thread.Start();
                VibrationManager = new VibrationManager(this);
                VibrationManager.Init();
            };
        }
        
        private void Log(string text)
        {
            logView.Text += text + "\n";
            logView.MoveEnd();
        }
        
        public void LogInfo(string text)
        {
            Log($"[{DateTime.Now.ToString("HH:mm:ss")}] [INFO] {text}");
        }
        
        public void LogDebug(string text)
        {
            if (_debug)
            {
                Log($"[{DateTime.Now.ToString("HH:mm:ss")}] [DEBUG] {text}");
            }
        }
        
        public void LogError(string text)
        {
            Log($"[{DateTime.Now.ToString("HH:mm:ss")}] [ERROR] {text}");
        }
    }
}

