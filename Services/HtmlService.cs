using KartChronoWrapper.Models;
using System.Text;

namespace KartChronoWrapper.Services
{
    public class HtmlService
    {
        public string SaveCurrentSession(List<PilotProfile> data)
        {
            var template = File.ReadAllText("WebPages/sessionResultTemplate.html");
            var body = new StringBuilder();

            for (int i = 0; i < data.Count(); ++i)
            {
                var laps = data[i].Laps;
                if (laps is null)
                    continue;
                var bestLapAsInt = int.Parse(data[i].BestLap);
                var bestLapAsTime = TimeSpan.FromMilliseconds(bestLapAsInt).ToString(@"mm\:ss\.fff");
                var lastLap = TimeSpan.FromMilliseconds(laps.Last()).ToString(@"mm\:ss\.fff");
                var str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                   $"    <div id=\"pos\" class=\"resultsCell totalBest\">{i}</div>\r\n" +
                   $"    <div id=\"num\" class=\"resultsCell\">#{data[i].KartNo}</div>\r\n" +
                    "    <div class=\"resultsCell compositeNameSectors \">\r\n" +
                    "        <div class=\"compositeNameSectorsInner\">\r\n" +
                    "            <div class=\"innerNameSectorsCell\" id=\"teamContainer\">\r\n" +
                    "                <div id=\"team\" class=\"innerNameValue hidden\">&nbsp;</div>\r\n" +
                    "            </div>\r\n" +
                    "            <div class=\"innerNameSectorsCell\" id=\"nameContainer\">\r\n" +
                   $"                <div id=\"name\" class=\"innerNameValue\">{data[i].Name ?? "Прокат"}</div>\r\n" +
                    "            </div>\r\n" +
                    "        </div>\r\n" +
                    "        <div id=\"marker\" class=\"markerClass\"></div>\r\n" +
                    "    </div>\r\n" +
                   $"    <div id=\"best_lap_time\" class=\"resultsCell \">{bestLapAsTime}</div>\r\n" +
                   $"    <div id=\"laps\" class=\"resultsCell \">{laps.Count(i => i > 0)}</div>\r\n" +
                   $"    <div id=\"last_lap_time_1\" class=\"lapTime resultsCell\">{lastLap}</div>\r\n" +
                    "    <div id=\"gap\" class=\"diffTime resultsCell\"></div>\r\n" +
                    "</div>";
                body.Append(str);
                for (int j = 0; j < laps.Count(); ++j)
                {
                    if (laps[j] == 0)
                        continue;
                    var style = (bestLapAsInt == laps[j]) ? "personalBest" : string.Empty;
                    var lapAsTime = TimeSpan.FromMilliseconds(laps[j]).ToString(@"mm\:ss\.fff");
                    str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                        "    <div id=\"pos\" class=\"resultsCell totalBest\"></div>\r\n" +
                       $"    <div id=\"num\" class=\"\">{j + 1}</div>\r\n" +
                        "    <div id=\"name\" class=\" \"></div>\r\n" +
                       $"    <div id=\"best_lap_time\" class=\"lapTime  \"></div>\r\n" +
                        "    <div id=\"laps\" class=\" \"></div>\r\n" +
                       $"    <div id=\"last_lap_time_1\" class=\"lapTime {style}\">{lapAsTime}</div>\r\n" +
                        "    <div id=\"gap\" class=\"diffTime resultsCell\"></div>\r\n" +
                        "</div>";
                    body.Append(str);
                }
            }

            return string.Format(template, body.ToString());
        }

        public async Task<string> WrapToPage(IEnumerable<string> sessions, DateTime selectedDate)
        {
            var template = await File.ReadAllTextAsync("WebPages/sessionListTemplate.html");
            var body = new StringBuilder();

            for (int i = 0; i < sessions.Count(); ++i)
            {
                var fileNamePosition = sessions.ElementAt(i).LastIndexOf('/');
                var sessionTitle = (fileNamePosition != -1)
                    ? sessions.ElementAt(i).Substring(fileNamePosition + 1)
                    : sessions.ElementAt(i);
                var str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                        $"    <div id=\"pos\" class=\"resultsCell totalBest\">{i+1}</div>\r\n" +
                        $"    <div id=\"num\" class=\"resultsCell\"></div>\r\n" +
                        "    <div class=\"resultsCell compositeNameSectors \">\r\n" +
                        "        <div class=\"compositeNameSectorsInner\">\r\n" +
                        "            <div class=\"innerNameSectorsCell\" id=\"teamContainer\">\r\n" +
                        "                <div id=\"team\" class=\"innerNameValue hidden\">&nbsp;</div>\r\n" +
                        "            </div>\r\n" +
                        "            <div class=\"innerNameSectorsCell\" id=\"nameContainer\">\r\n" +
                        $"                <div id=\"name\" class=\"innerNameValue\">" +
                        $"                     <a class=\"innerNameLink\" " +
                        $"                         href=\"GetSession?name={sessions.ElementAt(i)}&strDate={selectedDate.ToString("yyyy-MM-dd")}\">{sessionTitle}</a>" +
                        "                </div>\r\n" +
                        "            </div>\r\n" +
                        "        </div>\r\n" +
                        "        <div id=\"marker\" class=\"markerClass\"></div>\r\n" +
                        "    </div>\r\n" +
                        $"    <div id=\"best_lap_time\" class=\"lapTime resultsCell personalBest bestLapClass totalBest\"></div>\r\n" +
                        "    <div id=\"laps\" class=\"resultsCell \">10</div>\r\n" +
                        $"    <div id=\"last_lap_time_1\" class=\"lapTime resultsCell\"></div>\r\n" +
                        "    <div id=\"gap\" class=\"diffTime resultsCell\"></div>\r\n" +
                        "</div>";
                body.Append(str);
            }
            return template.Replace("{0}", body.ToString());
        }
    }
}
