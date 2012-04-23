using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace YoutubePlaylistDownloader
{
    public class YoutubeWebHelper
    {
        public YoutubeWebHelper()
        {

        }

        public bool IsValidId(string id)
        {
            return id.All(c => char.IsLetterOrDigit(c));
        }

        public IEnumerable<YoutubeVideoEntry> DownloadPlaylist(string id)
        {
            try
            {
                var url = String.Format("http://www.youtube.com/playlist?list={0}", id);

                var request = (HttpWebRequest)WebRequest.Create(url);

                var response = request.GetResponse();
            }
            catch
            {
                yield break;
            }
        }
    }
}
