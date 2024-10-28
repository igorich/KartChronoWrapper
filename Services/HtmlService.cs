using KartChronoWrapper.Models;
using System.Text;

namespace KartChronoWrapper.Services
{
    public class HtmlService
    {
        public string SaveCurrentSession1(List<PilotProfile> data)
        {
            var template = File.ReadAllText("WebPages/SessionResults.html");
            var body = new StringBuilder();
            //foreach (var i in data)
            for (int i = 0; i < data.Count(); ++i)
            {
                var str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                    $"    <div id=\"pos\" class=\"resultsCell totalBest\">{i}</div>\r\n" +
                    $"    <div id=\"num\" class=\"resultsCell\">{data[i].KartNo}</div>\r\n" +
                    "    <div class=\"resultsCell compositeNameSectors \">\r\n" +
                    "        <div class=\"compositeNameSectorsInner\">\r\n" +
                    "            <div class=\"innerNameSectorsCell\" id=\"teamContainer\">\r\n" +
                    "                <div id=\"team\" class=\"innerNameValue hidden\">&nbsp;</div>\r\n" +
                    "            </div>\r\n" +
                    "            <div class=\"innerNameSectorsCell\" id=\"nameContainer\">\r\n" +
                    $"                <div id=\"name\" class=\"innerNameValue\">{data[i].Name}</div>\r\n" +
                    "            </div>\r\n" +
                    "        </div>\r\n" +
                    "        <div id=\"marker\" class=\"markerClass\"></div>\r\n" +
                    "    </div>\r\n" +
                    $"    <div id=\"best_lap_time\" class=\"lapTime resultsCell personalBest bestLapClass totalBest\">{data[i].BestLap}</div>\r\n" +
                    "    <div id=\"laps\" class=\"resultsCell \">10</div>\r\n" +
                    $"    <div id=\"last_lap_time_1\" class=\"lapTime resultsCell\">{data[i].Laps[data[i].Laps.Count() - 1]}</div>\r\n" +
                    "    <div id=\"gap\" class=\"diffTime resultsCell\"></div>\r\n" +
                    "</div>";
                body.Append(str);
                var laps = data[i].Laps;
                for (int j = 0; j < laps.Count(); ++j)
                {
                    str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                        "    <div id=\"pos\" class=\"resultsCell totalBest\"></div>\r\n" +
                       $"    <div id=\"num\" class=\"resultsCell\">{j}</div>\r\n" +
                        "    <div id=\"name\" class=\"resultsCell \"></div>\r\n" +
                       $"    <div id=\"best_lap_time\" class=\"lapTime resultsCell personalBest bestLapClass totalBest\">{laps[j]}</div>\r\n" +
                        "    <div id=\"laps\" class=\"resultsCell \"></div>\r\n" +
                        "    <div id=\"last_lap_time_1\" class=\"lapTime resultsCell\">48.794</div>\r\n" +
                        "    <div id=\"gap\" class=\"diffTime resultsCell\"></div>\r\n" +
                        "</div>";
                    body.Append(str);
                }
            }

            return string.Format(template, body.ToString());
        }

        public async Task<string> WrapToPage(IEnumerable<string> sessions)
        {
            var template = await File.ReadAllTextAsync("WebPages/sessionsTemplate.html");
            var body = new StringBuilder();

            for (int i = 0; i < sessions.Count(); ++i)
            {
                var str = "<div id=\"dataRow\" class=\"dataRow compid-1001 oddRow totalBest\" style=\"display: flex; position: static\" ontransitionend=\"resetZIndex(this);\">\r\n" +
                        $"    <div id=\"pos\" class=\"resultsCell totalBest\">{i}</div>\r\n" +
                        $"    <div id=\"num\" class=\"resultsCell\"></div>\r\n" +
                        "    <div class=\"resultsCell compositeNameSectors \">\r\n" +
                        "        <div class=\"compositeNameSectorsInner\">\r\n" +
                        "            <div class=\"innerNameSectorsCell\" id=\"teamContainer\">\r\n" +
                        "                <div id=\"team\" class=\"innerNameValue hidden\">&nbsp;</div>\r\n" +
                        "            </div>\r\n" +
                        "            <div class=\"innerNameSectorsCell\" id=\"nameContainer\">\r\n" +
                        $"                <div id=\"name\" class=\"innerNameValue\">{sessions.ElementAt(i)}</div>\r\n" +
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
            return string.Format(template, body.ToString());
        }
    }
}
