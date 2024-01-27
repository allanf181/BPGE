using Newtonsoft.Json;

namespace BPGE;

using System.Net;
using System.Net.Sockets;
using System.Text;

public class HttpReceiver
{
    BPGEView _bpgeView;
    
    public readonly Thread Thread;
    
    public HttpReceiver(BPGEView bpgeView)
    {
        _bpgeView = bpgeView;
        Thread = new Thread(Listener);
    }
    
    private void Listener()
    {
        _bpgeView.LogInfo("HTTP Receiver started");
        _bpgeView.httpReceiverStatusLabel.Text = "Running";
        TcpListener tcpListener = new TcpListener(IPAddress.Any, 23456);
        tcpListener.Start();
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] requestBytes = new byte[10240];

                        int readBytes = stream.Read(requestBytes, 0, requestBytes.Length);

                        var requestResult = Encoding.UTF8.GetString(requestBytes, 0, readBytes);
                        stream.Write(
                            Encoding.UTF8.GetBytes(
                                "HTTP/1.0 200 OK" + Environment.NewLine
                                                  + "Content-Length: " + 0 + Environment.NewLine
                                                  + "Content-Type: " + "text/plain" + Environment.NewLine
                                                  + Environment.NewLine + Environment.NewLine));
                        dynamic json =
                            JsonConvert.DeserializeObject(
                                requestResult.Split(Environment.NewLine + Environment.NewLine)[1]);
                        _bpgeView.VibrationManager.ProcessEvents(json);
                    }
                }
                catch (Exception e)
                {
                    _bpgeView.LogError($"Exception in HTTP Receiver: {Environment.NewLine}{e.ToString()}");
                }
            }
        });
    }
}