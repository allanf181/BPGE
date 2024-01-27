using System.Net;

namespace BPGE {
    using System;
    using Terminal.Gui;

    public partial class BPGEView : Window {
        private Label httpReceiverLabel;
        public Label httpReceiverStatusLabel;
        public Label serverLabel;
        private Label bpLabel;
        public Label bpStatusLabel;
        public Button btnTest;
        public Button btnReconnect;
        public Button btnStopVibration;
        private FrameView frameView;
        private TextView logView;
        
        public void init()
        {
            this.Title = "ButtPlug Game Events (Ctrl+Q to quit)";
            
            this.httpReceiverLabel = new Label () { 
                Text = "HTTP Receiver Status:",
            };
            
            this.httpReceiverStatusLabel = new Label () {
                Text = "Stopped",
                X = Pos.Right (httpReceiverLabel) + 1,
            };
            
            this.serverLabel = new Label () {
                Text = $"Server (click to copy): {Dns.GetHostName().ToLower()}:23456",
            };
            serverLabel.X = Pos.AnchorEnd(serverLabel.Text.Length);
            
            this.bpLabel = new Label () {
                Text = "Intiface Status:",
                X = Pos.Left (httpReceiverLabel),
                Y = Pos.Bottom (httpReceiverLabel) + 1,
            };
            
            this.bpStatusLabel = new Label () {
                Text = "Disconnected",
                X = Pos.Right (bpLabel) + 1,
                Y = Pos.Bottom (httpReceiverStatusLabel) + 1,
            };
            
            this.btnTest = new Button () {
                Text = "Test Devices",
                Y = Pos.Bottom(httpReceiverLabel) + 1,
                X = Pos.Percent(33),
                Enabled = false,
            };
            
            this.btnReconnect = new Button () {
                Text = "Reconnect",
                Y = Pos.Bottom(httpReceiverLabel) + 1,
                X = Pos.Percent(60),
            };
            
            this.btnStopVibration = new Button () {
                Text = "Stop Vibration",
                Y = Pos.Bottom(httpReceiverLabel) + 1,
            };
            this.btnStopVibration.X = Pos.AnchorEnd(btnStopVibration.Text.Length + 4);
            
            this.frameView = new FrameView ("Log") {
                X = Pos.Left (httpReceiverLabel),
                Y = Pos.Bottom (btnTest) + 1,
                Width = Dim.Fill (),
                Height = Dim.Fill (),
            };
            
            this.logView = new TextView () {
                Width = Dim.Fill (),
                Height = Dim.Fill (),
                ReadOnly = true,
                Text = ""
            };
            
            this.frameView.Add (logView);
            
            Add (httpReceiverLabel, httpReceiverStatusLabel, serverLabel, bpLabel, bpStatusLabel, btnTest, btnReconnect, btnStopVibration, frameView);
        }
    }
}

