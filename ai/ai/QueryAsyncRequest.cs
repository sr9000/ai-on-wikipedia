using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace ai
{
    class QueryAsyncRequest
    {
        protected static string BeginUrl = "https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srinfo=&srprop=snippet&srsearch=";

        protected List<SGF> Result;
        protected Thread RequestProcessor;

        protected QueryAsyncRequest InitHttpWebRequest(ref HttpWebRequest request)
        {
            request.AllowAutoRedirect = false;
            request.Host = "en.wikipedia.org";
            request.Timeout = 7000;
            request.Headers.Add("Cache-Control", "no-cache");
            request.Referer = null;
            request.KeepAlive = false;
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            return this;
        }

        public QueryAsyncRequest(SGF query)
        {
            string searchQuery = query.Decompose().Select(x => x.GetContent()).Aggregate((a, b) => a + " " + b);
            string fullUrl = BeginUrl + WebUtility.UrlEncode(searchQuery);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
            InitHttpWebRequest(ref request);

            RequestProcessor = new Thread(() =>
            {
                DoRequest(request);
            });

            RequestProcessor.Start();
        }

        protected void DoRequest(HttpWebRequest request)
        {
            WebResponse resp;
            while (true)
            {
                try
                {
                    resp = request.GetResponse();
                    break;
                }
                catch (Exception ex)
                {
                    request = (HttpWebRequest)WebRequest.Create(request.RequestUri);
                    InitHttpWebRequest(ref request);
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    Thread.Sleep(0);
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch(Exception){}
            }

            try
            {
                Thread.Sleep(0);
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            catch (Exception){}

            Stream stream = resp.GetResponseStream();
            if (stream == null)
            {
                return;
            }
            StreamReader readStream = new StreamReader(stream, Encoding.UTF8);
            string response = readStream.ReadToEnd();
            resp.Close();

            var dirtyList = GetDirtyData(response);
            Regex removexml = new Regex("<.*?>");
            Regex removebad = new Regex("[^a-z ']+");
            Regex removespace = new Regex(" +");
            var cleanList = dirtyList
                .Select(WebUtility.UrlDecode)
                .Select(str => removexml.Replace(str.ToLower(), ""))
                .Select(str => removebad.Replace(str, " "))
                .Select(str => removespace.Replace(str.Trim(), ","));

            Result = new List<SGF>();
            foreach (var item in cleanList)
            {
                var msgfList = item.Split(',').Select(str => new MSGF().SetContent(str)).ToList();
                if (msgfList.Any())
                {
                    Result.Add(new SGF().AggregateFrom(msgfList));
                }
            }
        }

        protected List<string> GetDirtyData(string json)
        {
            List<string> res = new List<string>();
            Regex selector = new Regex("\"snippet\":\"(.*?)[^\\\\]\"");
            Match match = selector.Match(json);
            while (match.Success)
            {
                res.Add(match.Groups[1].Value);
                match = match.NextMatch();
            }
            return res;
        }

        public void Wait()
        {
            RequestProcessor.Join();
        }

        public void Stop()
        {
            RequestProcessor.Interrupt();
            RequestProcessor.Join();
            RequestProcessor = null;
        }

        public List<SGF> GetResult()
        {
            return Result;
        }
    }
}
