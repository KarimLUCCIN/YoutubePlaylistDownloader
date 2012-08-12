using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;

namespace YoutubePlaylistDownloader
{
    public class YoutubePlaylistDownloaderApplication : INotifyPropertyChanged, IProgress<DownloadProgressChangedEventArgs>
    {
        private string currentUrl;

        public string CurrentUrl
        {
            get { return currentUrl; }
            set
            {
                currentUrl = value;
                RaisePropertyChanged("CurrentUrl");

                Verified = false;
            }
        }

        private GenericActionCommand executeUrlCommand;
        public GenericActionCommand ExecuteUrlCommand
        {
            get { return executeUrlCommand; }
        }

        private GenericActionCommand downloadCommand;
        public GenericActionCommand DownloadCommand
        {
            get { return downloadCommand; }
        }

        private GenericActionCommand cancelCommand;
        public GenericActionCommand CancelCommand
        {
            get { return cancelCommand; }
        }

        private GenericActionCommand invertSelectionCommand;
        public GenericActionCommand InvertSelectionCommand
        {
            get { return invertSelectionCommand; }
        }

        private bool verified = false;

        public bool Verified
        {
            get { return verified; }
            private set
            {
                verified = value;
                RaisePropertyChanged("Verified");
                RaisePropertyChanged("CanDownload");
            }
        }
        
        public YoutubePlaylistDownloaderApplication()
        {
            executeUrlCommand = new GenericActionCommand(ExecuteUrlFunction);
            downloadCommand = new GenericActionCommand(DownloadFunction);
            cancelCommand = new GenericActionCommand(CancelFunction);
            invertSelectionCommand = new GenericActionCommand(InvertSelectionFunction);
        }

        private IList<YoutubeVideoEntry> videoEntries;

        public IList<YoutubeVideoEntry> VideoEntries
        {
            get { return videoEntries; }
            private set
            {
                videoEntries = value;
                RaisePropertyChanged("VideoEntries");
            }
        }

        private bool isReady = true;

        public bool IsReady
        {
            get { return isReady && !downloading; }
            set
            {
                isReady = value;
                RaisePropertyChanged("IsReady");
                RaisePropertyChanged("CanDownload");
            }
        }

        public void InvertSelectionFunction(object parameter)
        {
            foreach (var item in videoEntries)
            {
                item.Selected = !item.Selected;
            }
        }
        
        public async void ExecuteUrlFunction(object parameter)
        {
            if (isReady)
            {
                IsReady = false;
                Verified = false;

                try
                {
                    try
                    {
                        if (String.IsNullOrEmpty(currentUrl))
                            MessageBox.Show("ID invalide", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            if (!YoutubeWebHelper.IsValidId(currentUrl))
                                MessageBox.Show("ID invalide", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            else
                            {
                                var nodes = await YoutubeWebHelper.DownloadPlaylistAsync(currentUrl);

                                VideoEntries = nodes.ToList();

                                Verified = VideoEntries.Count((p) => true) > 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("ID invalide\n{0}", ex), "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    IsReady = true;
                }
            }
        }

        private bool downloading = false;

        public bool Downloading
        {
            get { return downloading; }
            private set
            {
                downloading = value;
                RaisePropertyChanged("Downloading");
                RaisePropertyChanged("IsReady");
                RaisePropertyChanged("CanDownload");
            }
        }

        public bool CanDownload
        {
            get { return Verified && !Downloading; }
        }

        private int maxProgress = 1;

        public int MaxProgress
        {
            get { return maxProgress; }
            set
            {
                maxProgress = value;
                RaisePropertyChanged("MaxProgress");
            }
        }

        private int currentProgress = 0;

        public int CurrentProgress
        {
            get { return currentProgress; }
            set
            {
                currentProgress = value;
                RaisePropertyChanged("CurrentProgress");
            }
        }

        private string currentSong = String.Empty;

        public string CurrentSong
        {
            get { return currentSong; }
            set
            {
                currentSong = value;
                RaisePropertyChanged("CurrentSong");
            }
        }
                        
        public async void DownloadFunction(object parameter)
        {
            if (!Verified)
                MessageBox.Show("Vérifier d'abord l'ID", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var dirDlg = new System.Windows.Forms.FolderBrowserDialog();
                if (dirDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var downloadDirectory = dirDlg.SelectedPath;

                    downloadCancel = new CancellationTokenSource();
                    Downloading = true;
                    try
                    {
                        try
                        {
                            MaxProgress = videoEntries.Count;

                            for (int currentVideoIndex = 0; currentVideoIndex < videoEntries.Count; currentVideoIndex++)
                            {
                                var video = videoEntries[currentVideoIndex];

                                if (video.Selected)
                                {
                                    CurrentSong = video.Title;
                                    CurrentProgress = currentVideoIndex;

                                    var finalName = Path.Combine(downloadDirectory, ConformTitle(video.Title));

                                    await DownloadYoutubeVideoTo(video, finalName);

                                    if (downloadCancel.IsCancellationRequested)
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    finally
                    {
                        Downloading = false;
                        CurrentSong = String.Empty;
                        
                        CurrentProgress = 0;
                        MaxProgress = 1;
                    }
                }
            }
        }

        private string ConformTitle(string path)
        {
            foreach (var c in Path.GetInvalidPathChars())
                path = path.Replace(c, '_');

            return path;
        }

        private static string CharMatch(Match match)
        {
            var code = match.Groups["code"].Value;
            int value = Convert.ToInt32(code, 16);
            return ((char)value).ToString();
        }

        Regex playerConfigRegex = new Regex(@"yt.playerConfig\ \=\ (.*);");
        Regex urlPartsRegex = new Regex(@"http://o[^;" + "\"" + @"]*(type=[a-z]*/x-[a-z]*)?(\&itag\=\d*)?");
        Regex videoTypeRegex = new Regex(@"&type=[^(;&)]*");
        Regex videoQualityRegex = new Regex(@"&quality=[^(;&)]*");

        private class YoutubeStreamEntry
        {
            public string StreamType { get; set; }
            public string StreamUrl { get; set; }
            public string StreamQuality { get; set; }

            public override string ToString()
            {
                return String.Format("Type:{0}, Quality:{1}", StreamType, StreamQuality);
            }
        }

        private bool enableHD = false;

        public bool EnableHD
        {
            get { return enableHD; }
            set
            {
                enableHD = value;
                RaisePropertyChanged("EnableHD");
            }
        }

        private int downloadMaxProgress = 1;

        public int DownloadMaxProgress
        {
            get { return downloadMaxProgress; }
            set
            {
                downloadMaxProgress = value;
                RaisePropertyChanged("DownloadMaxProgress");
            }
        }

        private int downloadCurrentProgress = 0;

        public int DownloadCurrentProgress
        {
            get { return downloadCurrentProgress; }
            set
            {
                downloadCurrentProgress = value;
                RaisePropertyChanged("DownloadCurrentProgress");
            }
        }

        private async Task DownloadYoutubeVideoTo(YoutubeVideoEntry video, string outputPath)
        {
            try
            {
                try
                {
                    var client = new WebClient();
                    var htmlCode = String.Empty;

                    try
                    {
                        htmlCode = await client.DownloadStringTaskAsync(new Uri(video.Url), downloadCancel.Token);
                    }
                    catch (Exception ex)
                    {
                        htmlCode = null;
                        OutputErrorLog(outputPath, "[not resolved] " + video.Url, ex);
                    }

                    if (!String.IsNullOrEmpty(htmlCode))
                    {
                        var matches = playerConfigRegex.Match(htmlCode);

                        var tx = matches.Groups[0].Value;

                        var json = tx.Substring(tx.IndexOf("=") + 1);
                        json = Regex.Replace(json, @"\\u(?<code>\d{4})", CharMatch);
                        json = HttpUtility.UrlDecode(json);
                        json = json.Replace(@"\/", "\\");
                        json = HttpUtility.UrlDecode(json);

                        var splitSequences = json.Split(new string[] { "url=" }, StringSplitOptions.RemoveEmptyEntries);

                        var streams = new List<YoutubeStreamEntry>();

                        foreach (var splitSequence in splitSequences)
                        {
                            if (splitSequence.StartsWith("http"))
                            {
                                var urlVideos = from Match part in urlPartsRegex.Matches(splitSequence) select part.Groups[0].Value;

                                foreach (var urlVideo in urlVideos)
                                {
                                    if (!String.IsNullOrEmpty(urlVideo) && urlVideo.Contains("http"))
                                    {
                                        var streamEntry = new YoutubeStreamEntry()
                                        {
                                            StreamType = videoTypeRegex.Match(urlVideo).Groups[0].Value.Substring("&type=".Length),
                                            StreamUrl = urlVideo
                                        };

                                        var qualityMatch = videoQualityRegex.Match(urlVideo).Groups;

                                        if (qualityMatch.Count > 0 
                                            && !String.IsNullOrEmpty(qualityMatch[0].Value)
                                            && qualityMatch[0].Value.Length > "quality=".Length)
                                            streamEntry.StreamQuality = qualityMatch[0].Value.Substring("quality=".Length);
                                        else
                                            streamEntry.StreamQuality = String.Empty;

                                        streams.Add(streamEntry);
                                    }
                                }
                            }
                        }

                        if (streams.Count > 0)
                        {
                            /* on évite la HD ... */


                            /* on cherche mp4 d'abord */
                            bool isMP4 = true;
                            var targetStream =
                                enableHD
                                ? (from entry in streams where entry.StreamType.Contains("mp4") select entry.StreamUrl).FirstOrDefault()
                                : (from entry in streams where (entry.StreamType.Contains("mp4") && !entry.StreamQuality.Contains("hd")) select entry.StreamUrl).FirstOrDefault();

                            if (String.IsNullOrEmpty(targetStream))
                            {
                                /* on cherche flv */
                                isMP4 = false;
                                targetStream =
                                    enableHD
                                    ? (from entry in streams where entry.StreamType.Contains("flv") select entry.StreamUrl).FirstOrDefault()
                                    : (from entry in streams where (entry.StreamType.Contains("flv") && !entry.StreamQuality.Contains("hd")) select entry.StreamUrl).FirstOrDefault();
                            }

                            if (String.IsNullOrEmpty(targetStream))
                            {
                                Debug.WriteLine(String.Format("Stream not found for {0}", video.Title));
                            }
                            else
                            {
                                outputPath += isMP4 ? ".mp4" : ".flv";

                                Debug.WriteLine(String.Format("Downloading {0}\nUrl : {1}\nTarget : {2}", video.Title, targetStream, outputPath));

                                if (!File.Exists(outputPath))
                                {
                                    const int maxRetry = 10;

                                    int retryCount = 0;

                                    bool success = false;

                                    Exception lastException = null;

                                    do
                                    {
                                        try
                                        {
                                            await client.DownloadFileTaskAsync(new Uri(targetStream), outputPath, downloadCancel.Token, this);

                                            success = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(lastException = ex);
                                        }

                                        retryCount++;
                                    } while (!success && retryCount <= maxRetry);

                                    if (!success)
                                    {
                                        OutputErrorLog(outputPath, targetStream, lastException);
                                    }

                                    Debug.WriteLine(success ? "Completed\n\n" : "Failed\n\n");
                                }
                                else
                                    Debug.WriteLine("Skipped");
                            }
                        }
                    }
                }
                finally
                {
                    DownloadCurrentProgress = 0;
                    DownloadMaxProgress = 1;
                }
            }
            catch (Exception ex)
            {
                OutputErrorLog(outputPath, "[general error] " + video.Url, ex);
            }
        }

        private static void OutputErrorLog(string outputPath, string targetStream, Exception lastException)
        {
            try
            {
                using (var strm = new FileStream(outputPath + ".error.log", FileMode.Create))
                {
                    using (var wr = new StreamWriter(strm))
                    {
                        wr.WriteLine("From : " + targetStream);
                        wr.WriteLine();
                        wr.WriteLine();
                        wr.WriteLine(lastException.ToString());
                    }
                }
            }
            catch
            {

            }
        }

        private CancellationTokenSource downloadCancel;

        public void CancelFunction(object parameter)
        {
            if (Downloading)
            {
                if (MessageBox.Show("Voulez-vous vraiment annuler le téléchargement ?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    var token = downloadCancel;
                    if (Downloading && token != null)
                    {
                        token.Cancel();
                    }
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region IProgress<DownloadProgressChangedEventArgs> Members

        public void Report(DownloadProgressChangedEventArgs value)
        {
            DownloadMaxProgress = 100;
            DownloadCurrentProgress = value.ProgressPercentage;
        }

        #endregion
    }
}
