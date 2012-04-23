using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace YoutubePlaylistDownloader
{
    public class YoutubePlaylistDownloaderApplication : INotifyPropertyChanged
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

        private bool verified = false;

        public bool Verified
        {
            get { return verified; }
            private set
            {
                verified = value;
                RaisePropertyChanged("Verified");
            }
        }
        
        public YoutubePlaylistDownloaderApplication()
        {
            executeUrlCommand = new GenericActionCommand(ExecuteUrlFunction);
            downloadCommand = new GenericActionCommand(DownloadFunction);
        }

        private IEnumerable<YoutubeVideoEntry> videoEntries;

        public IEnumerable<YoutubeVideoEntry> VideoEntries
        {
            get { return videoEntries; }
            private set
            {
                videoEntries = value;
                RaisePropertyChanged("VideoEntries");
            }
        }


        public async void ExecuteUrlFunction(object parameter)
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

                    VideoEntries = nodes;
                }
            }
        }

        public void DownloadFunction(object parameter)
        {
            if (!Verified)
                MessageBox.Show("Vérifier d'abord l'ID", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {

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
    }
}
