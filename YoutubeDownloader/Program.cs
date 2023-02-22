using System;
using System.Diagnostics;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

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
                //string? link = "https://www.youtube.com/watch?v=egkqDwQuh8E";
                var k = Uri.TryCreate(link, UriKind.Absolute, out var link2);
                if (!k) return;

                YoutubeClient client = new();
                Video video = client.Videos.GetAsync(link2.ToString()).Result;
                StreamManifest streamManifest = client.Videos.Streams.GetManifestAsync(link2.ToString()).Result;
                List<IVideoStreamInfo> vsechnyVidea = streamManifest.GetVideoStreams().ToList();
                List<IAudioStreamInfo> vsechnyAudia = streamManifest.GetAudioStreams().ToList();
                IVideoStreamInfo nejlepsiVideo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                IStreamInfo nejlepsiAudio = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                IStreamInfo[] mixedStreams = new IStreamInfo[] { nejlepsiVideo, nejlepsiAudio };

                var velikost = mixedStreams.Select(x => x.Size.KiloBytes).Sum();
                var p = new Progress<double>(x =>
                {
                    Console.Write("\r" + (x * 100).ToString("N2") + " % z " + velikost / 1024 + " MB");
                });
                var t = Task.Run(async () =>
                {
                    await client.Videos.DownloadAsync(mixedStreams, new ConversionRequestBuilder($"video.{nejlepsiVideo.Container}").Build(),p);
                });
                t.Wait();


                Console.WriteLine("\nStazeno");

            }
        }
    }
}