﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using HtmlAgilityPack;

namespace YoutubePlaylistDownloader
{
    public class YoutubeVideoEntry : INotifyPropertyChanged
    {
        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }

        private string url;

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }

        private string imageUrl;

        public string ImageUrl
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                RaisePropertyChanged("ImageUrl");
            }
        }

        private bool selected = true;

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                RaisePropertyChanged("Selected");
            }
        }

        public YoutubeVideoEntry()
        {

        }
        
        public YoutubeVideoEntry(HtmlNode node)
        {
            var url_node = node.SelectNodes(".//a[@href]");

            if (url_node != null)
            {
                var url_value = url_node.FirstOrDefault().Attributes["href"].Value;
                var splitIndex = url_value.IndexOf("&");
             
                if (splitIndex > 0)
                {
                    url = "http://www.youtube.com" + url_value.Substring(0, splitIndex);
                }
            }

            var title_node = node.SelectNodes(".//span[contains(@class, 'video-title')]");

            if (title_node != null)
                title = title_node.FirstOrDefault().InnerText;

            if (!String.IsNullOrEmpty(title))
                title = title.Trim();

            var img_node = node.SelectNodes(".//img[@src]");

            if (img_node != null)
                imageUrl = "http:" + img_node.FirstOrDefault().Attributes["src"].Value;
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
