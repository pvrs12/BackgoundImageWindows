using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackgroundImage
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            string b = "http://www.bing.com";
            string resource = "/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US";
            Task<HttpResponseMessage> task = client.GetAsync(b + resource);
            task.Wait();
            HttpResponseMessage resp = task.Result;
            Task<string> stringtask = resp.Content.ReadAsStringAsync();
            stringtask.Wait();
            string result = stringtask.Result;

            JObject resultJson = JObject.Parse(result);
            string imageResource = resultJson["images"][0]["url"].ToString();

            Uri uri = new Uri(b + imageResource);
            Wallpaper.Set(uri, Wallpaper.Style.Stretched);

            //Console.ReadKey();
        }
    }
}
