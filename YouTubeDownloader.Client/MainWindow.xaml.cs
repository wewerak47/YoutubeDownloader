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
            this.DataContext = this;
        }

        bool ValidniUrl;
        string NazevVidea = "video";
        private bool multiSelectEnabled = false;
        private SelectionMode multiSelectMode;

        public bool MultiSelectEnabled
        {
            get => multiSelectEnabled;
            set
            {
                multiSelectEnabled = value;
                this.MultiSelectMode = value ? SelectionMode.Multiple : SelectionMode.Single;
            }
        }
        public SelectionMode MultiSelectMode
        {
            get => multiSelectMode;
            set
            {
                multiSelectMode = value;
                if (value == SelectionMode.Single)
                {
                    this.kvalita_videa.SelectedItems.Clear();
                    this.kvalita_zvuku.SelectedItems.Clear();
                }
                this.kvalita_videa.SelectionMode = value;
                this.kvalita_zvuku.SelectionMode = value;

            }
        }

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
                    NajdiVarianty();
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
            IStreamInfo[] mixedStreams;
            if (this.MultiSelectMode == SelectionMode.Single)
            {
                var vybranyVideo = kvalita_videa.SelectedItem as VideoOnlyStreamInfo;
                var vybranyAudio = kvalita_zvuku.SelectedItem as AudioOnlyStreamInfo;
                mixedStreams = new IStreamInfo[] { vybranyVideo, vybranyAudio };
            }
            else
            {
                var vybranyVidea = kvalita_videa.SelectedItems.Cast<VideoOnlyStreamInfo>().ToList();
                var vybranyZvuky = kvalita_zvuku.SelectedItems.Cast<AudioOnlyStreamInfo>().ToList();
                int streamCount = vybranyVidea.Count + vybranyZvuky.Count;
                mixedStreams = new IStreamInfo[streamCount];

                for (int i = 0; i < vybranyVidea.Count; i++)
                {
                    mixedStreams[i] = vybranyVidea[i];
                    //mixedStreams.Append(vybranyVidea[i]);
                }
                for (int i = 0; i < vybranyZvuky.Count; i++)
                {
                    mixedStreams[vybranyVidea.Count + i] = vybranyZvuky[i];
                }
            }

            var velikost = mixedStreams.Select(x => x.Size.KiloBytes).Sum();
            var p = new Progress<double>(x =>
            {
                //Debug.Write("\r" + (x * 100).ToString("N2") + " % z " + (velikost / 1024).ToString("N2") + " MB");
                this.DownloadProgress.Value = x * 100;
            });
            try
            {
                await client.Videos.DownloadAsync(mixedStreams, new ConversionRequestBuilder(this.CestaKSouboru.Text).Build(), p);
            }
            catch(Exception ex)
            {

            }
        }

        private async void Najdi_varianty_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidniUrl) return;
            NajdiVarianty();
        }
        private async void NajdiVarianty()
        {
            string link = SemnapisURL_TextBox.Text;
            YoutubeClient client = new YoutubeClient();
            try
            {
                var video = await client.Videos.GetAsync(link);
                this.NazevVidea = video.Title;
            }
            catch (Exception) { return; }
            var listStrymu = await client.Videos.Streams.GetManifestAsync(link);
            var listvidea = listStrymu.GetVideoOnlyStreams().ToList();
            var listaudio = listStrymu.GetAudioOnlyStreams().ToList();
            kvalita_videa.ItemsSource = listvidea;
            kvalita_zvuku.ItemsSource = listaudio;
            kvalita_videa.SelectedItem = listvidea.GetWithHighestVideoQuality();
            kvalita_zvuku.SelectedItem = listaudio.TryGetWithHighestBitrate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new SaveFileDialog();
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = ".mp4";
            openFileDialog.Filter = "Video | *.mp4";
            openFileDialog.FileName = this.NazevVidea;
            openFileDialog.ShowDialog();
            this.CestaKSouboru.Text = openFileDialog.FileName;
        }
    }
}
