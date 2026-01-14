using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AlecaFrameClientLib.Utils;
using Newtonsoft.Json;

namespace AlecaFrameClientLib
{
	public static class AnalyticsHandler
	{
		private static List<KeyValuePair<string, string>> metricAndLabelPairsScheduledToSend = new List<KeyValuePair<string, string>>();

		private static string currentMenuTab = "tabFoundry";

		public static DateTime lastUserInteraction = DateTime.MinValue;

		public static bool IsUserActiveInTheLastXMinutes(int minutes = 5)
		{
			return DateTime.UtcNow - lastUserInteraction < TimeSpan.FromMinutes(minutes);
		}

		public static void Initialize()
		{
			Task.Run(delegate
			{
				try
				{
					if (StaticData.isFirstRunOnInstall)
					{
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Detected fresh install");
						TrySendAnalytics("/analytics/install");
					}
					DateTime dateTime = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					while (true)
					{
						try
						{
							if (DateTime.UtcNow - dateTime > TimeSpan.FromHours(10.0))
							{
								dateTime = DateTime.UtcNow;
								TrySendAnalytics("/analytics/periodic");
							}
							if ((DateTime.UtcNow - dateTime2 > TimeSpan.FromSeconds(15.0) && IsUserActive()) || (DateTime.UtcNow - dateTime2 > TimeSpan.FromSeconds(60.0) && !IsUserActive()))
							{
								dateTime2 = DateTime.UtcNow;
								string jsonBody = "";
								lock (metricAndLabelPairsScheduledToSend)
								{
									jsonBody = JsonConvert.SerializeObject(metricAndLabelPairsScheduledToSend);
									metricAndLabelPairsScheduledToSend.Clear();
								}
								TrySendAnalytics("/metrics/periodic", 2, jsonBody);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine("Failed to send analytics!");
							StaticData.Log(OverwolfWrapper.LogType.INFO, "Failed to send metrics: " + ex.Message);
						}
						finally
						{
							Thread.Sleep(TimeSpan.FromSeconds(5.0));
						}
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to to Analytics work: " + ex2.Message);
				}
			});
		}

		public static void AddMetric(string metricsKey, string metricsValue)
		{
			lock (metricAndLabelPairsScheduledToSend)
			{
				if (metricsKey == "Menu_NavigateTab")
				{
					currentMenuTab = metricsValue;
					lastUserInteraction = DateTime.UtcNow;
				}
				metricAndLabelPairsScheduledToSend.Add(new KeyValuePair<string, string>(metricsKey, metricsValue));
			}
		}

		private static void TrySendAnalytics(string path, int retryCount = 2, string jsonBody = "", string extraQuery = "")
		{
			while (true)
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					string text = "userID=" + StaticData.overwolfID + "&promo=" + StaticData.overwolfPromo + "&version=" + (StaticData.overwolfWrappwer?.currentVersion ?? "0.0.0");
					if (!string.IsNullOrEmpty(extraQuery))
					{
						text = text + "&" + extraQuery;
					}
					if (IsUserActive())
					{
						text = text + "&activeTab=" + currentMenuTab;
					}
					myWebClient.UploadString("https://" + StaticData.LogAPIHostname + path + "?" + text, jsonBody);
					break;
				}
				catch
				{
					if (retryCount > 0)
					{
						retryCount--;
						continue;
					}
					throw;
				}
			}
		}

		private static bool IsUserActive()
		{
			return DateTime.UtcNow - lastUserInteraction < TimeSpan.FromSeconds(62.0);
		}

		public static void SendRelicReward(string status, double timeMS, double worstDeltaError)
		{
			TrySendAnalytics("/metrics/relicReward", 0, "", $"status={status}&timeTakenMS={timeMS}&worstDeltaError={worstDeltaError}");
		}

		public static void SendRelicRecommendation(string status, double timeMS)
		{
			TrySendAnalytics("/metrics/relicRecommendation", 0, "", $"status={status}&timeTakenMS={timeMS}");
		}

		public static void SendUninstall()
		{
			TrySendAnalytics("/analytics/uninstall", 0);
		}
	}
}
