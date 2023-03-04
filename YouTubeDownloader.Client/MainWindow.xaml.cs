using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YoutubeExplode.Converter;
using Microsoft.Win32;
using System.Diagnostics;

namespace YouTubeDownloader.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool ValidniUrl;
        string NazevVidea = "video";

        private void SemnapisURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = SemnapisURL_TextBox.Text;
            try
            {
                var validUri = Uri.TryCreate(url, UriKind.Absolute, out var link2);
                if (validUri)
                {
                    this.ValidaceCervenePole.BorderBrush = Brushes.Green;
                    this.ValidniUrl = true;
                }
                else
                {
                    this.ValidaceCervenePole.BorderBrush = Brushes.Red;
                    this.ValidniUrl = false;
                }
            }
            catch (Exception)
            {
                this.ValidaceCervenePole.BorderBrush = Brushes.Red;
                this.ValidniUrl = false;
            }
        }

        private async void FindVideoFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (kvalita_videa.SelectedIndex == -1 && kvalita_zvuku.SelectedIndex == -1)
            {
                MessageBox.Show("Nemáš vybraný vůbec nic, co chceš jako stahovat?");
                return;
            }
            //string link2 = SemnapisURL_TextBox.Text;
            if (kvalita_zvuku.SelectedIndex == -1)
            {
                MessageBox.Show("Nemáš vybranej zvuk");
                return;
            }
            if (kvalita_videa.SelectedIndex == -1)
            {
                MessageBox.Show("Nemáš vybrané video");
                return;
            }
            if (string.IsNullOrEmpty(this.CestaKSouboru.Text))
            {
                MessageBox.Show("neznam cestu k souboru");
                return;
            }
                YoutubeClient client = new();
            var vybranyVideo = kvalita_videa.SelectedItem as VideoOnlyStreamInfo;
            var vybranyAudio = kvalita_zvuku.SelectedItem as AudioOnlyStreamInfo;
            //StreamManifest streamManifest = await client.Videos.Streams.GetManifestAsync(link2.ToString());
            //List<VideoOnlyStreamInfo> vsechnyVidea = streamManifest.GetVideoOnlyStreams().ToList();
            //List<AudioOnlyStreamInfo> vsechnyAudia = streamManifest.GetAudioOnlyStreams().ToList();
            //IVideoStreamInfo nejlepsiVideo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
            //IStreamInfo nejlepsiAudio = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            IStreamInfo[] mixedStreams = new IStreamInfo[] { vybranyVideo, vybranyAudio };
            
            var velikost = mixedStreams.Select(x => x.Size.KiloBytes).Sum();
            var p = new Progress<double>(x =>
            {
                //Debug.Write("\r" + (x * 100).ToString("N2") + " % z " + (velikost / 1024).ToString("N2") + " MB");
                this.DownloadProgress.Value = x*100;
            });
            await client.Videos.DownloadAsync(mixedStreams, new ConversionRequestBuilder(this.CestaKSouboru.Text).Build(),p);
        }

        private async void Najdi_varianty_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidniUrl) return;
            string link = SemnapisURL_TextBox.Text;
            YoutubeClient client = new YoutubeClient();
            var video = await client.Videos.GetAsync(link);
            this.NazevVidea = video.Title;
            var listStrymu = await client.Videos.Streams.GetManifestAsync(link);
            var listvidea = listStrymu.GetVideoOnlyStreams().ToList();
            var listaudio = listStrymu.GetAudioOnlyStreams().ToList();
            kvalita_videa.ItemsSource = listvidea;
            kvalita_zvuku.ItemsSource = listaudio;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new SaveFileDialog();
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = ".mp4";
            openFileDialog.Filter = "Video | *.mp4";
            openFileDialog.ShowDialog();
            this.CestaKSouboru.Text = openFileDialog.FileName;
        }
    }
}
