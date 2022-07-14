using System.Linq;
using VideoLibrary;

namespace YoutubeDownloader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Zadej adresu videa");
                string? link = Console.ReadLine();
                YouTube? youTube = YouTube.Default; // starting point for YouTube actions

                try
                {
                    Task<IEnumerable<YouTubeVideo>>? t = youTube.GetAllVideosAsync(link); // gets a Video object with info about the video
                    t.Wait();
                    IEnumerable<YouTubeVideo>? videos = t.Result;
                    YouTubeVideo? video = videos.OrderByDescending(x => x.Resolution).First();
                    Console.WriteLine("Stahuju: " + video.FullName);
                    Stream s = video.Stream();
                    using (var fs = File.Create(video.FullName))
                    {
                        int count = 0;
                        byte[] buffer = new byte[16 * 1024];
                        int read;
                        while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, read);
                            count = (count + read);
                            double progress = (double)(count / (double)video.ContentLength) * 100; 
                            //progress = Math.Round(progress, 2);
                            Console.Write("\r" + progress.ToString("N2") + " % z " + video.ContentLength / 1024 + " KB");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Neco se stalo");
                    Console.Write(e.Message);
                }
                Console.WriteLine("\nStazeno");
            }
        }
    }
}