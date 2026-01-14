using System;
using System.Net;
using System.Text;

namespace AlecaFrameClientLib.Utils
{
	public class MyWebClient : WebClient
	{
		public int TimeoutMS { get; }

		public MyWebClient(int timeoutMS = 100000)
		{
			TimeoutMS = timeoutMS;
			base.Encoding = Encoding.UTF8;
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			HttpWebRequest httpWebRequest = base.GetWebRequest(uri) as HttpWebRequest;
			if (uri.AbsoluteUri.Contains("warframe.market"))
			{
				httpWebRequest.UserAgent = "AlecaFrame_Client";
			}
			httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			httpWebRequest.Timeout = TimeoutMS;
			return httpWebRequest;
		}
	}
}
