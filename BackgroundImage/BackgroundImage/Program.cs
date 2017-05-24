using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using System.Windows.Forms;

namespace BackgroundImage
{
    class Program
    {
        const string BASE = "http://www.bing.com";

        static string getImageURL()
        {
            HttpClient client = new HttpClient();
            string resource = "/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US";
            Task<HttpResponseMessage> task = client.GetAsync(BASE + resource);
            task.Wait();
            HttpResponseMessage resp = task.Result;
            Task<string> stringtask = resp.Content.ReadAsStringAsync();
            stringtask.Wait();
            string result = stringtask.Result;

            JObject resultJson = JObject.Parse(result);
            string imageResource = resultJson["images"][0]["url"].ToString();

            return imageResource;
        }

        static void setWallpaper(string url)
        {
            Uri uri = new Uri(BASE + url);
            Wallpaper.Set(uri, Wallpaper.Style.Stretched);
        }

        static void Main(string[] args)
        {
            string imageURL = getImageURL();
            System.Diagnostics.Process.Start(BASE + imageURL);
            System.Threading.Thread.Sleep(2000);
            DialogResult result = MessageBox.Show(
                "Do you want to change the background?",
                "Background Downloader",
                MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                setWallpaper(imageURL);
            }

        }
    }
}
