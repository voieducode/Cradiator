using System;
using System.Net;

namespace Cradiator.Services
{
    public class HttpWebClient : IWebClient
    {
        readonly WebClient _webClient;

        public HttpWebClient()
        {
            _webClient = new WebClient();
        }

        public HttpWebClient(Uri url)
        {
            _webClient = new WebClient();
            if (!String.IsNullOrEmpty(url.UserInfo))
            {
                BasicAuthentication(url, _webClient);
            }
        }

        public string DownloadString(string url)
        {
            return _webClient.DownloadString(new Uri(url));
        }

        protected void BasicAuthentication(Uri uri, WebClient wc)
        {
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];

            var myCred = new NetworkCredential(username, password);
            var myCache = new CredentialCache { { uri, "Basic", myCred } };

            wc.Credentials = myCache;
        }
    }
}