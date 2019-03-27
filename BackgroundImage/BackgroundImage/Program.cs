using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BackgroundImage
{
    class Program
    {
        const string BASE = "http://www.bing.com";

        private static string selected_url = "";
        private static List<PictureBox> images;

        private static WallpaperSelector ws;

        static List<string> getImageURLs(int number)
        {
            HttpClient client = new HttpClient();
            string resource = String.Format("/HPImageArchive.aspx?format=js&n={0}&mkt=en-US", number);
            Task<HttpResponseMessage> t1 = client.GetAsync(BASE + resource);
            t1.Wait();
            Task<string> t2= t1.Result.Content.ReadAsStringAsync();
            t2.Wait();

            JObject resultJson = JObject.Parse(t2.Result);

            List<string> images = new List<string>();
            foreach(JObject image in resultJson["images"])
            {
                images.Add(String.Format("{0}{1}", BASE, image["url"].ToString()));
            }

            return images;
        }

        static void setWallpaper(string url)
        {
            Uri uri = new Uri(url);
            Wallpaper.Set(uri, Wallpaper.Style.Stretched);
        }

        private static WallpaperSelector run()
        {
            WallpaperSelector ws = new WallpaperSelector();

            ws.previewButton.Click += PreviewButton_Click;
            ws.setWallpaperButton.Click += SetWallpaperButton_Click;

            List<string> imageURLs = getImageURLs(8);

            // download the images
            
            List<Tuple<Task<HttpResponseMessage>, string>> httpTasks = new List<Tuple<Task<HttpResponseMessage>, string>>();
            foreach (string imageURL in imageURLs)
            {
                HttpClient client = new HttpClient();
                httpTasks.Add(Tuple.Create(client.GetAsync(imageURL), imageURL));
            }

            // load the streams
            List<Tuple <Task<Stream>, string>> streamTasks = new List<Tuple<Task<Stream>, string>>();
            foreach(Tuple < Task<HttpResponseMessage>, string> t in httpTasks)
            {
                t.Item1.Wait();
                streamTasks.Add(Tuple.Create(t.Item1.Result.Content.ReadAsStreamAsync(), t.Item2));
            }

            // read from the streams
            images = new List<PictureBox>();
            foreach(Tuple<Task<Stream>, string> stream in streamTasks)
            {
                PictureBox pb = new PictureBox();
                stream.Item1.Wait();
                pb.Width = 250;
                pb.Height = (int)(pb.Width * (1080.0 / 1920.0));

                pb.Image = Image.FromStream(stream.Item1.Result).GetThumbnailImage(pb.Width, pb.Height, null, IntPtr.Zero);
                pb.Parent = ws.flowLayoutPanel1;

                pb.Name = stream.Item2;

                pb.Click += Pb_Click;
                images.Add(pb);
            }
            
            return ws;
        }

        private static void SetWallpaperButton_Click(object sender, EventArgs e)
        {
            setWallpaper(selected_url);
        }

        private static void PreviewButton_Click(object sender, EventArgs e)
        {
            //open the image in a web browser
            System.Diagnostics.Process.Start(selected_url);
        }

        private static void clear_selections()
        {
            foreach(PictureBox pb in images)
            {
                pb.BorderStyle = BorderStyle.None;
            }
        }

        private static void Pb_Click(object sender, EventArgs e)
        {
            ws.previewButton.Enabled = true;
            ws.setWallpaperButton.Enabled = true;

            clear_selections();

            PictureBox pb = (PictureBox)sender;
            pb.BorderStyle = BorderStyle.Fixed3D;

            selected_url = pb.Name;
        }

        static void Main(string[] args)
        {
            ws = run();
            ws.ShowDialog();
        }
    }
}
