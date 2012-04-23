using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YoutubePlaylistDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubePlaylistDownloaderApplication appCommand = new YoutubePlaylistDownloaderApplication();

        public YoutubePlaylistDownloaderApplication AppCommand
        {
            get { return appCommand; }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = AppCommand;
        }
    }
}
