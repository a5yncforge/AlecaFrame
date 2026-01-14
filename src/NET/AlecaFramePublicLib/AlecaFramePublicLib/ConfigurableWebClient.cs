using System;
using System.Net;
using System.Threading;

namespace AlecaFramePublicLib
{
	public class ConfigurableWebClient : WebClient
	{
		public int Timeout { get; set; }

		public string UserAgent { get; set; }

		public ConfigurableWebClient()
		{
			Timeout = 60000;
		}

		public ConfigurableWebClient(TimeSpan timeout, string userAgent = "")
		{
			Timeout = (int)timeout.TotalMilliseconds;
			UserAgent = userAgent;
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest webRequest = base.GetWebRequest(uri);
			(webRequest as HttpWebRequest).AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			webRequest.Timeout = Timeout;
			if (!string.IsNullOrEmpty(UserAgent))
			{
				webRequest.Headers.Add("user-agent", UserAgent);
			}
			return webRequest;
		}

		public string DownloadStringWithReattempts(string url, int maxReattempts = 3, int waitTimeMS = 0)
		{
			WebHeaderCollection headers = base.Headers;
			int num = 0;
			while (true)
			{
				try
				{
					base.Headers = headers;
					return DownloadString(url);
				}
				catch
				{
					num++;
					if (num > maxReattempts)
					{
						if (waitTimeMS > 0)
						{
							Thread.Sleep(waitTimeMS);
						}
						throw;
					}
				}
			}
		}
	}
}
