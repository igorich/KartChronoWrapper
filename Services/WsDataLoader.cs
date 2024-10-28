using System.Text.Json;
using KartChronoWrapper.Models;
using WebSocketSharp;

namespace KartChronoWrapper.Services
{
    public class WsDataLoader
    {
        private bool _firstMessageReceived = false;
        private bool _binaryMessageReceived = false;
        private readonly string _trackId;
        private Dictionary<int, List<string>>? _lapsTime;
        private WebSocket _webSocket;

        public  List<PilotProfile>? _pilots;

        public WsDataLoader() : this("1f3e81fc98c56b12aaeed4a1a4eb91cb")
        {
            Console.WriteLine("No track id, failback to id jekabpils/1f3e81fc98c56b12aaeed4a1a4eb91cb");
        }

        public WsDataLoader(string trackId)
        {
            _trackId = trackId;
        }

        public void LoadData()
        {            
            _webSocket = new WebSocket("wss://kartchrono.com:9180");
            
            _webSocket.OnOpen += (sender, e) =>
            {
                var payload = new { trackId = _trackId };
                _webSocket.Send(JsonSerializer.Serialize(payload));
            };
            _webSocket.OnMessage += this.Ws_OnMessage;
            _webSocket.OnError += (sender, e) => { Console.WriteLine("Ошибка: " + e.Message); };
            _webSocket.OnClose += (sender, e) => { Console.WriteLine("Соединение закрыто."); };

            _webSocket.Connect();            
        }

        private void Close()
        {
            Console.WriteLine("Соединение закрыто.");
            _webSocket.Close();
        }

        private void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            if (e.Data == null && !_binaryMessageReceived)
            {
                _lapsTime = this.ProcessBinaryMessage(e.RawData);
                _binaryMessageReceived = true;
            }
            else if (!_firstMessageReceived)
            {
                _pilots = this.ProcessFirstMessage(e.Data);
                _firstMessageReceived = true;
            }

            if (_firstMessageReceived && _binaryMessageReceived)
            {
                foreach (var session in _lapsTime)
                {
                    if (_pilots is not null && _pilots.Any(i => i.Id == session.Key.ToString()))
                    {
                        var p = _pilots.Where(i => i.Id == session.Key.ToString()).First();
                        p.Laps = session.Value;
                    }
                }

                new S3FilesService().SaveCurrentSession(_pilots);

                this.Close();
            }
        }

        private List<PilotProfile> ProcessFirstMessage(string jsonString)
        {
            var pilots = new List<PilotProfile>();
            //results -> [] -> 3, 4, 6
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;

                var results = root.GetProperty("results");
                foreach (JsonProperty node in results.EnumerateObject())
                {
                    var p = new PilotProfile()
                    {
                        Name = node.Value.GetProperty("4").ToString(),
                        Id = node.Name,
                        BestLap = node.Value.GetProperty("6").ToString(),
                        KartNo = node.Value.GetProperty("3").ToString(),
                    };
                    pilots.Add(p);
                }
            }

            return pilots;
        }

        private Dictionary<int, List<string>> ProcessBinaryMessage(byte[] byteData)
        {
            var data = this.ByteArrayToIntArray(byteData);
            var resuls = new Dictionary<int, List<string>>();
            int offset = 11;

            for (int i = offset; i < data.Length - 12; i += 13)
            {
                var user_id = data[i + 4];
                if (!resuls.ContainsKey(user_id))
                    resuls.Add(user_id, new List<string>());

                var lapnumber = data[i + 6];
                var laptime = data[i + 12];
                Console.WriteLine($"{user_id} : {laptime}");

                resuls[user_id].Add(this.ConvertIntToLapTime(laptime));
            }

            return resuls;
        }

        private string ConvertIntToLapTime(int time)
        {
            if (time == 0)
                return string.Empty;

            return time.ToString().Insert(2, ",");
        }

        private int[] ByteArrayToIntArray(byte[] byteArray)
        {
            if (byteArray.Length % 4 != 0)
                throw new ArgumentException("Длина массива байт должна быть кратна 4.");

            int[] intArray = new int[byteArray.Length / 4];

            for (int i = 0; i < intArray.Length; i++)
                intArray[i] = BitConverter.ToInt32(byteArray, i * 4);

            return intArray;
        }

    }
}
