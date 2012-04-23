using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;

namespace YoutubePlaylistDownloader
{
    public class YoutubeWebHelper
    {
        public YoutubeWebHelper()
        {

        }

        public static bool IsValidId(string id)
        {
            return id.All(c => char.IsLetterOrDigit(c));
        }

        private static async Task<HtmlDocument> LoadHtmlDocumentFromString(string html)
        {
            return await TaskEx.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                return doc;
            });
        }

        public static async Task<IEnumerable<YoutubeVideoEntry>> DownloadPlaylistAsync(string id)
        {
            try
            {
                var url = String.Format("http://www.youtube.com/playlist?list={0}", id);

                var client = new WebClient();
                var htmlCode = await client.DownloadStringTaskAsync(url);

                if (String.IsNullOrEmpty(htmlCode))
                    return null;
                else
                {
                    var document = await LoadHtmlDocumentFromString(htmlCode);

                    var nodes = await TaskEx.Run(() =>
                    {
                        return document.DocumentNode.SelectNodes("//li[contains(@class, 'playlist-video-item')]");
                    });

                    return from node in nodes select new YoutubeVideoEntry(node);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
