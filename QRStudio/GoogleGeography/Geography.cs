using Newtonsoft.Json;
using QRStudio.Google;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace QRStudio.Google
{
    static class Geography
    {
        public static async Task<GeographyResult> Search(string word)
        {
            string url = $"{"http://"}maps.googleapis.com/maps/api/geocode/json?sensor=false&language=ko&address={HttpUtility.UrlEncode(word)}";

            var req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "GET";
            req.Accept = "json";

            var res = await req.GetResponseAsync() as HttpWebResponse;

            using (var sr = new StreamReader(res.GetResponseStream()))
            {
                return await Task.FromResult(JsonConvert.DeserializeObject<GeographyResult>(sr.ReadToEnd()));
            }
        }
    }
}
