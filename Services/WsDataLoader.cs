using KartChronoWrapper.Models;
using Serilog;
using System.Net;
using System.Text.Json;
using WebSocketSharp;

namespace KartChronoWrapper.Services
{
    public class WsDataLoader : ISaveSessionService
    {
        private const string _dataSourceUrl = "wss://kartchrono.com:9180";

        private bool _firstMessageReceived = false;
        private bool _binaryMessageReceived = false;
        private string _trackId;
        private Dictionary<int, List<int>>? _lapsTime;
        private WebSocket? _webSocket = null;

        public  List<PilotProfile>? _pilots;

        public WsDataLoader()
        {
            _trackId = Environment.GetEnvironmentVariable("TRACK_ID") ?? "1f3e81fc98c56b12aaeed4a1a4eb91cb";
            //Log.Warning("No track id, failback to id jekabpils/1f3e81fc98c56b12aaeed4a1a4eb91cb");
        }

        public Task SaveSession()
        {
            this.LoadData();

            return Task.CompletedTask;
        }

        public void LoadData()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                _webSocket = new WebSocket(_dataSourceUrl);
                _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
                _webSocket.SslConfiguration.CheckCertificateRevocation = false;

                _webSocket.SslConfiguration.ServerCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => true;

                _webSocket.OnOpen += (sender, e) =>
                {
                    try
                    {
                        var payload = new { trackId = _trackId };
                        _webSocket.Send(JsonSerializer.Serialize(payload));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "WebSocket failed OnOpen");
                    }
                };
                _webSocket.OnMessage += this.Ws_OnMessage;
                _webSocket.OnError += (sender, e) => { Log.Error("WebSocket error: " + e.Message); };
                _webSocket.OnClose += (sender, e) => { Log.Debug("WebSocket connection closed."); };

                _webSocket.Connect();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "WebSocket failed");
            }
        }

        private void Close()
        {
            Log.Debug("Wss connection closed.");
            if(_webSocket is not null)
                _webSocket.Close();
        }

        private void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            try
            {
                if (e.Data == null && !_binaryMessageReceived)
                {
                    try
                    {
                        _lapsTime = this.ProcessBinaryMessage(e.RawData);
                    }
                    catch { }
                    _binaryMessageReceived = true;
                }
                else if (!_firstMessageReceived)
                {
                    try
                    {
                        _pilots = this.ProcessFirstMessage(e.Data);
                    }
                    catch { }
                    _firstMessageReceived = true;
                }

                if (_firstMessageReceived && _binaryMessageReceived)
                {
                    foreach (var session in _lapsTime!)
                    {
                        if (_pilots is not null && _pilots.Any(i => i.Id == session.Key.ToString()))
                        {
                            var p = _pilots.Where(i => i.Id == session.Key.ToString()).First();
                            p.Laps = session.Value;
                        }
                    }

                    if (_pilots is not null)
                        _ = new S3FilesService().SaveCurrentSession(_pilots);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "WebSocket OnMessage");
            }
        }

        private List<PilotProfile> ProcessFirstMessage(string? jsonString)
        {
            var pilots = new List<PilotProfile>();
            if (string.IsNullOrEmpty(jsonString))
                return pilots;

            //results -> [] -> 3, 4, 6
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;

                var results = root.GetProperty("results");
                foreach (JsonProperty node in results.EnumerateObject())
                {
                    var p = new PilotProfile()
                    {
                        Name = string.IsNullOrEmpty(node.Value.GetProperty("4").ToString()) 
                            ? node.Value.GetProperty("21").ToString()
                            : node.Value.GetProperty("4").ToString(),
                        Id = node.Name,
                        BestLap = node.Value.GetProperty("6").ToString(),
                        KartNo = node.Value.GetProperty("3").ToString(),
                    };
                    pilots.Add(p);
                }
                if (pilots.Count > 0 && pilots[0] is not null)
                    pilots[0].RaceTitle = root.GetProperty("104").ToString();
            }

            return pilots;
        }

        private Dictionary<int, List<int>> ProcessBinaryMessage(byte[] byteData)
        {
            var data = this.ByteArrayToIntArray(byteData);
            var resuls = new Dictionary<int, List<int>>();
            int offset = 11;

            for (int i = offset; i < data.Length - 12; i += 13)
            {
                var user_id = data[i + 4];
                if (!resuls.ContainsKey(user_id))
                    resuls.Add(user_id, new List<int>());

                var lapnumber = data[i + 6];
                var laptime = data[i + 12];
                Log.Debug($"ProcessBinaryMessage (user:laptime): {user_id} : {laptime}");

                resuls[user_id].Add(laptime);
            }

            return resuls;
        }

        private int[] ByteArrayToIntArray(byte[] byteArray)
        {
            if (byteArray.Length % 4 != 0)
                throw new ArgumentException("The length of the byte array must be a multiple of 4.");

            int[] intArray = new int[byteArray.Length / 4];

            for (int i = 0; i < intArray.Length; i++)
                intArray[i] = BitConverter.ToInt32(byteArray, i * 4);

            return intArray;
        }

    }
}
