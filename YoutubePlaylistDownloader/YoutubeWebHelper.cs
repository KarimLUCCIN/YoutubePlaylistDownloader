using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace YoutubePlaylistDownloader
{
    public class YoutubeWebHelper
    {
        public YoutubeWebHelper()
        {

        }

        public static bool IsValidId(string id)
        {
            return id.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
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

        static Regex titleParser = new Regex(@"\<title\>[^\<]*?\</title\>", RegexOptions.Multiline);

        public static async Task<IEnumerable<YoutubeVideoEntry>> DownloadPlaylistAsync(string id)
        {
            try
            {
                var url = String.Format("http://www.youtube.com/playlist?list={0}", id);

                var client = new WebClient();
                string htmlCode = null;

                bool tryDirectVideo = false;

                try
                {
                    htmlCode = await client.DownloadStringTaskAsync(url);
                }
                catch
                {
                    tryDirectVideo = true;
                }

                if (tryDirectVideo)
                {
                    return await TryDirectVideo(id, url, client, htmlCode);
                }
                else
                {
                    if (String.IsNullOrEmpty(htmlCode))
                        return null;
                    else
                    {
                        try
                        {
                            var document = await LoadHtmlDocumentFromString(htmlCode);

                            var nodes = await TaskEx.Run(() =>
                            {
                                return document.DocumentNode.SelectNodes("//li[contains(@class, 'playlist-video-item')]");
                            });

                            if (nodes == null)
                                tryDirectVideo = true;
                            else
                                return from node in nodes select new YoutubeVideoEntry(node);
                        }
                        catch
                        {
                            tryDirectVideo = true;
                        }

                        if (tryDirectVideo)
                            return await TryDirectVideo(id, url, client, htmlCode);
                        else
                            return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static async Task<IEnumerable<YoutubeVideoEntry>> TryDirectVideo(string id, string url, WebClient client, string htmlCode)
        {
            /* c'est peut être une vidéo */
            url = String.Format("http://www.youtube.com/watch?v={0}", id);
            htmlCode = await client.DownloadStringTaskAsync(url);

            if (String.IsNullOrEmpty(htmlCode))
                return null;
            else
            {
                var titleMatch = titleParser.Match(htmlCode);
                var title = (titleMatch != null && titleMatch.Value != null) ? titleMatch.Value.Substring("<title>".Length, titleMatch.Value.Length - "<title></title>".Length) : "Unknown";

                return new YoutubeVideoEntry[]{new YoutubeVideoEntry(){
                            Title = title,
                            Url = url,
                            ImageUrl = String.Empty}
                        };
            }
        }
    }
}
