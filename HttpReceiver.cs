using Newtonsoft.Json;

namespace BPGE;

using System.Net;
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
        
        using var listener = new HttpListener();
        listener.Prefixes.Add("http://+:80/Temporary_Listen_Addresses/");
        listener.Start();
        
        _bpgeView.LogInfo("HTTP Receiver started");
        _bpgeView.httpReceiverStatusLabel.Text = "Running";
        while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext ctx = listener.GetContext();
                    HttpListenerRequest req = ctx.Request;

                    var body = new StreamReader(req.InputStream).ReadToEnd();

                    using HttpListenerResponse resp = ctx.Response;

                    resp.StatusCode = (int) HttpStatusCode.OK;
                    resp.StatusDescription = "Status OK";
                    string data = "OK";
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    resp.ContentLength64 = buffer.Length;

                    using Stream ros = resp.OutputStream;
                    ros.Write(buffer, 0, buffer.Length);
                    
                    if(req.Headers["Origin"].Contains("iafnecpcfnepnifhkhbifmngngmpkbencicpfmmi"))
                    {
                        _bpgeView.LogDebug($"HTTP Request: {Environment.NewLine}{body}");
                        dynamic json = JsonConvert.DeserializeObject(body);
                        _bpgeView.VibrationManager.ProcessEvents(json);
                    }
                    else
                    {
                        _bpgeView.LogDebug($"Invalid Request Origin: {req.Headers["Origin"]}");
                    }
                }
                catch (Exception e)
                {
                    _bpgeView.LogError($"Exception in HTTP Receiver: {Environment.NewLine}{e.ToString()}");
                }
            }
    }
}