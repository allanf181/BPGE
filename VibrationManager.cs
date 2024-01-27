using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Timer = System.Timers.Timer;

namespace BPGE;

public class EventConfig
{
    public int Intensity { get; set; }
    public double Duration { get; set; }
}
public class GameConfig
{
    public string? Mode { get; set; }
    public Dictionary<String, EventConfig> Events { get; set; }
}

public class VibrationManager
{
    private BPGEView _bpgeView;
    
    public ButtplugClient Client;

    private ButtplugWebsocketConnector _connector;
    
    private Timer _timer;

    private int _gameId = 0;
    
    private static readonly int IntensitySize = 10 * 60 * 30;
    
    private int _counter = 0;

    private int[] _intensityArray = new int[IntensitySize];
    
    private int _currentVibration = 0;
    
    private Dictionary<string, EventConfig> _intensities = new();
    
    private static string GetFilePath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }
    
    private static GameConfig GetGameConfig(string fileName)
    {
        return new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build().Deserialize<GameConfig>(File.ReadAllText(GetFilePath(fileName)));
    } 
    
    private void ResetConfig()
    {
        _intensities = new Dictionary<string, EventConfig>();
    }
    
    private bool LoadGlobalConfig()
    {
        ResetConfig();
        GameConfig globalConfig = null;
        if(File.Exists(GetFilePath("global.yaml")))
        {
            _bpgeView.LogDebug("Loading file global.yaml");
            globalConfig = GetGameConfig("global.yaml");
        }
        if (File.Exists(GetFilePath("global.yml")))
        {
            _bpgeView.LogDebug("Loading file global.yml");
            globalConfig = GetGameConfig("global.yml");
        }
        if(globalConfig != null)
        {
            foreach (var (key, value) in globalConfig.Events)
            {
                _intensities.Add(key, value);
            }

            return true;
        }
        
        _bpgeView.LogInfo("No global config found, using empty config");
        return false;
    }
    
    private void LoadGameConfig(int gameId)
    {
        var exists = LoadGlobalConfig();
        GameConfig gameConfig = null;
        if(File.Exists(GetFilePath($"{gameId}.yaml")))
        {
            _bpgeView.LogDebug($"Loading file {gameId}.yaml");
            gameConfig = GetGameConfig($"{gameId}.yaml");
            
        }else if (File.Exists(GetFilePath($"{gameId}.yml")))
        {
            _bpgeView.LogDebug($"Loading file {gameId}.yml");
            gameConfig = GetGameConfig($"{gameId}.yml");
        }

        if (gameConfig != null)
        {
            switch (gameConfig.Mode)
            {
                case "override":
                    ResetConfig();
                    break;
                case "append":
                    break;
                default:
                    if (exists)
                    {
                        _bpgeView.LogInfo($"Unknown mode {gameConfig.Mode}, using global config");
                    }
                    else
                    {
                        _bpgeView.LogInfo($"Unknown mode {gameConfig.Mode}, global config does not exist, no events will be triggered");
                    }
                    return;
            }
            foreach (var (key, value) in gameConfig.Events)
            {
                _intensities.Add(key, value);
            }
        }else if(exists)
        {
            _bpgeView.LogInfo($"No config found for game {gameId}, using global config");
        }else
        {
            _bpgeView.LogInfo($"No config found for game {gameId}, global config does not exist, no events will be triggered");
        }
    }
    
    private static int CircularIndex(int index)
    {
        return index % IntensitySize;
    }

    private int CurrentIndex()
    {
        return CircularIndex(_counter);
    }
    
    public VibrationManager(BPGEView bpgeView)
    {
        _bpgeView = bpgeView;
    }
    
    public async void Init()
    {
        try
        {
            _timer = new Timer(100);
            Client = new ButtplugClient("BPGE");
            _connector = new ButtplugWebsocketConnector(new Uri($"ws://127.0.0.1:{_bpgeView.IntifacePort}"));
            await Client.ConnectAsync(_connector);
            _timer.Elapsed += (sender, args) =>
            {
                if(_intensityArray[CurrentIndex()] != _currentVibration)
                {
                    var newIntensity = _intensityArray[CurrentIndex()];
                    _bpgeView.LogDebug($"Intensity changed from {_currentVibration} to {newIntensity}");
                    _currentVibration = newIntensity;
                    VibrateAll(_currentVibration);
                }
                _counter++;
            };
            Client.ServerDisconnect += (sender, args) =>
            {
                _bpgeView.LogInfo("Buttplug Client disconnected");
                _bpgeView.bpStatusLabel.Text = "Disconnected";
                _bpgeView.btnTest.Enabled = false;
            };
            Client.DeviceAdded += (sender, args) =>
            {
                _bpgeView.LogInfo($"Device added: {args.Device.Name}");
            };
            Client.DeviceRemoved += (sender, args) =>
            {
                _bpgeView.LogInfo($"Device removed: {args.Device.Name}");
            };
            _timer.Start();
            _bpgeView.LogInfo("Buttplug Client connected");
            _bpgeView.bpStatusLabel.Text = "Connected";
            _bpgeView.btnTest.Enabled = true;
            if (Client.Devices.Length == 0)
            {
                _bpgeView.LogInfo("No devices found, please manage your devices at Intiface\u00ae Central");
            }
            LoadGlobalConfig();
        }
        catch (Exception e)
        {
            _bpgeView.LogError($"Exception in BPManager: {Environment.NewLine}{e.ToString()}");
        }
    }
    
    public async void Reconnect()
    {
        await Client.DisconnectAsync();
        Init();
    }
    
    public void ProcessEvents(dynamic json)
    {
        foreach (dynamic item in json)
        {
            if (item.gameId != _gameId)
            {
                _bpgeView.LogInfo($"Game changed to {item.gameName} ({item.gameId})");
                _gameId = item.gameId;
                LoadGameConfig(_gameId);
                _bpgeView.LogDebug("Current config:");
                foreach (KeyValuePair<string, EventConfig> kvp in _intensities)
                {
                    _bpgeView.LogDebug($"Event: {kvp.Key} Intensity: {kvp.Value.Intensity}% Duration: {kvp.Value.Duration}s");
                }
            }
            if (item.type == "event")
            {
                foreach (dynamic ev in item.data.events)
                {
                    if (!_intensities.ContainsKey(ev.name.ToString())) continue;
                    EventConfig eventConfig = _intensities[ev.name.ToString()];
                    _bpgeView.LogInfo($"Game: {item.gameName} Event: {ev.name} Intensity: {eventConfig.Intensity}% Duration: {eventConfig.Duration}s");
                    AddToArray(CurrentIndex(), eventConfig.Intensity, eventConfig.Duration);
                }
            }
        }
    }
    
    private void AddToArray(int index, int intensity, double duration)
    {
        if (duration == 0)
        {
            duration = 5 * 60;
        }
        if(intensity == 0)
        {
            ClearArray();
        } else
        {
            for (int i = index; i <= index+(duration * 10); i++)
            {
                if (_intensityArray[CircularIndex(i)] < intensity)
                {
                    _intensityArray[CircularIndex(i)] = intensity;
                }
            }
        }
    }
    
    public void ClearArray()
    {
        _intensityArray = new int[IntensitySize];
    }
    
    private void VibrateAll(int intensity)
    {
        if (Client.Connected)
        {
            foreach (var buttplugClientDevice in Client.Devices)
            {
                buttplugClientDevice.VibrateAsync((double)intensity/100);
            }    
        }
    }
}