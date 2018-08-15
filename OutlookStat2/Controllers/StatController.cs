using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutlookTest;
using Newtonsoft.Json;

namespace OutlookStat2.Controllers
{
    [Route("api/unread")]
    public class StatController : Controller
    {
        public class Datapoint {
            public string x;
            public int y;
        }
        public class Dataset {
            public string label;
            public IList<int> data = new List<int>();
            public string backgroundColor;
        }

        public class Data {
            public IList<string> labels = new List<string>();
            public IList<Dataset> datasets = new List<Dataset>();
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = new Data(); 

            var unreadsByFolder = await Task.Run(() => OutlookAPI.FetchUnreads().ToList());
            var colors = new Colours.ColourGenerator();

            foreach (var unreadsInFolder in unreadsByFolder)
            {
                var dataset = new Dataset { label = unreadsInFolder.Key, backgroundColor =  "#" + colors.NextColour() };

                var unreadInFolderByDate = unreadsInFolder.GroupBy(item => item.Received.Date);

                foreach (var d in unreadInFolderByDate)
                {
                    //dataset.data.Add(new Datapoint { x = d.Key.Date.ToString("yyyy-MM-dd"), y = d.Count() });
                    dataset.data.Add(d.Count());
                }
                data.datasets.Add(dataset);
            }

            var dates = unreadsByFolder.SelectMany(i => i.Select(t => t.Received.Date)).Distinct();
            data.labels = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            
            return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public struct DataPoint {
            public string Folder;
            public int Count;
        }
    }
}
