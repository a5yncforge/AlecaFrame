using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AlecaFrameClientLib.Utils
{
    public static class HTTPHandler
    {
        public static string MakeGETRequest(string url, int timeoutMSperRequest = 100000, int retries = 3)
        {
            MyWebClient myWebClient = new MyWebClient(timeoutMSperRequest);
            myWebClient.Proxy = null;
            myWebClient.Encoding = Encoding.UTF8;
            string result = "";
            int num = retries;
            do
            {
                try
                {
                    result = myWebClient.DownloadString(url);
                }
                catch (Exception ex)
                {
                    num--;
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to make GET request: " + url + ": " + ex.Message);
                    if (num <= 0)
                    {
                        throw new Exception("Failed to make GET request: " + url + ": " + ex.Message);
                    }
                    continue;
                }
                break;
            }
            while (num > 0);
            return result;
        }

        public static string MakePOSTRequest(string url, string dataToSend, int timeoutMSperRequest = 100000, int retries = 3)
        {
            MyWebClient myWebClient = new MyWebClient(timeoutMSperRequest);
            myWebClient.Proxy = null;
            string result = "";
            int num = retries;
            do
            {
                try
                {
                    myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    result = myWebClient.UploadString(url, dataToSend);
                }
                catch (Exception ex)
                {
                    num--;
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to make POST request: " + url);
                    if (num <= 0)
                    {
                        throw ex;
                    }
                    continue;
                }
                break;
            }
            while (num > 0);
            return result;
        }

        public static void MakeFileRequest(string url, string file, int timeoutMSperRequest = 100000, int retries = 3)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            int num = retries;
            do
            {
                try
                {
                    HttpClient val = new HttpClient((HttpMessageHandler)new HttpClientHandler
                    {
                        UseProxy = false,
                        Proxy = null
                    });
                    try
                    {
                        val.Timeout = TimeSpan.FromMilliseconds(timeoutMSperRequest);
                        using FileStream destination = File.OpenWrite(file);
                        val.GetStreamAsync(url).Result.CopyTo(destination);
                        break;
                    }
                    finally
                    {
                        ((IDisposable)val)?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    num--;
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to make FILE request: " + url + ": " + ex);
                    if (num <= 0)
                    {
                        throw new Exception("Failed to make FILE request: " + url + ": " + ex);
                    }
                }
            }
            while (num > 0);
        }
    }
}
