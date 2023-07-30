using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;

namespace YoutubeDownloader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // playlisty
            while(true)
            {
                Console.WriteLine("Zadej adresu videa");
                string? link = Console.ReadLine();
                var validUri = Uri.TryCreate(link, UriKind.Absolute, out var link2);
                if (!validUri) continue;

                YoutubeClient client = new();
                Playlist playlistInfo = client.Playlists.GetAsync(link2.ToString()).Result;
                IReadOnlyList<PlaylistVideo> playlistVideos = client.Playlists.GetVideosAsync(link2.ToString()).GetAwaiter().GetResult();
                foreach (var item in playlistVideos)
                {
                    var t = client.Videos.DownloadAsync(item.Url, item.Title.Replace('/',' ').Replace('\\',' ').Replace('?',' ') + ".mp4");
                    t.AsTask().Wait();
                }
            }
            // videa
            
            while (false)
            {
                Console.WriteLine("Zadej adresu videa");
                string? link = Console.ReadLine();
                //string? link = "https://www.youtube.com/watch?v=egkqDwQuh8E";
                var validUri = Uri.TryCreate(link, UriKind.Absolute, out var link2);
                if (!validUri) continue;

                YoutubeClient client = new();
                Video video = client.Videos.GetAsync(link2!.ToString()).Result;
                StreamManifest streamManifest = client.Videos.Streams.GetManifestAsync(link2.ToString()).Result;
                List<VideoOnlyStreamInfo> vsechnyVidea = streamManifest.GetVideoOnlyStreams().ToList();
                List<AudioOnlyStreamInfo> vsechnyAudia = streamManifest.GetAudioOnlyStreams().ToList();
                IVideoStreamInfo nejlepsiVideo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                IStreamInfo nejlepsiAudio = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                IStreamInfo[] mixedStreams = new IStreamInfo[] { nejlepsiVideo, nejlepsiAudio };

                var velikost = mixedStreams.Select(x => x.Size.KiloBytes).Sum();
                var p = new Progress<double>(x =>
                {
                    Console.Write("\r" + (x * 100).ToString("N2") + " % z " + (velikost / 1024).ToString("N2") + " MB");
                });

                if (!ExistsOnPath("ffmpeg")) continue;

                var t = Task.Run(async () =>
                {
                    await client.Videos.DownloadAsync(mixedStreams, new ConversionRequestBuilder($"video.{nejlepsiVideo.Container}").Build(), p);
                });
                t.Wait();


                Console.WriteLine("\nStazeno");

            }
        }

        public static bool ExistsOnPath(string exeName)
        {
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = "where";
                    p.StartInfo.Arguments = exeName;
                    p.Start();
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch (Win32Exception)
            {
                throw new Exception("'where' command is not on path");
            }
        }
    }
}