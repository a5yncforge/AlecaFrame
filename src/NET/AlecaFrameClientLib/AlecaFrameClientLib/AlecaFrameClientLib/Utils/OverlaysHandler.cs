using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlecaFrameClientLib.Utils
{
	public class OverlaysHandler
	{
		public static string lastRelicType = "";

		public static void OpenRelicRecommendationHandler(bool debugMode = false, Bitmap debugBitmap = null, bool fromAfterRelic = false)
		{
			Bitmap bitmap = null;
			Task task = Task.Delay(750);
			Misc.WarframeLanguage warframeLanguage = Misc.GetWarframeLanguage(defaultToEnglish: true);
			if (warframeLanguage == Misc.WarframeLanguage.UNKNOWN || warframeLanguage == Misc.WarframeLanguage.Russian)
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Not opening relic recommendation because the game language is not supported.");
				return;
			}
			if (StaticData.dataHandler.warframeRootObject == null || StaticData.dataHandler.warframeRootObject.MiscItems == null || StaticData.dataHandler.warframeRootObject.MiscItems.Length == 0)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Now showing recommendated relics because there are no misc items in the inventory!");
				return;
			}
			if (StaticData.overwolfWrappwer.cachedRelicPlannerData.Count == 0)
			{
				Task.Run(delegate
				{
					StaticData.overwolfWrappwer.ReloadRelicCache(fromRelicRecommendations: true);
				});
			}
			StaticData.overwolfWrappwer.relicRecommendationReadyEvent.Reset();
			StaticData.overwolfWrappwer.OpenRelicRecommendations();
			int num = 1;
			string text2;
			while (true)
			{
				if (debugMode && debugBitmap != null)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Opening relic recommendation in debug mode");
					bitmap = debugBitmap;
				}
				else
				{
					task.Wait();
					if (!OCRHelper.TakeScreenshotOfWarframeInCSharp(null))
					{
						StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to take screenshot for relic recommendations!");
						return;
					}
					bitmap = new Bitmap((int)((float)StaticData.overwolfWrappwer.lastWarframeScreenshot.Width * 0.15f), (int)((float)StaticData.overwolfWrappwer.lastWarframeScreenshot.Height * 0.12f));
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.DrawImage(StaticData.overwolfWrappwer.lastWarframeScreenshot, new Point(-(int)((float)StaticData.overwolfWrappwer.lastWarframeScreenshot.Width * 0.025f), -(int)((float)StaticData.overwolfWrappwer.lastWarframeScreenshot.Height * 0.065f)));
					}
					try
					{
						StaticData.overwolfWrappwer.lastWarframeScreenshot.Dispose();
					}
					catch
					{
					}
				}
				string text = OCRHelper.SendRAWRivenOCR(bitmap).Replace(" ", "").ToLower();
				text2 = "none";
				if (text.ToLower().Contains("lith"))
				{
					text2 = "lith";
				}
				else if (text.ToLower().Contains("meso"))
				{
					text2 = "meso";
				}
				else if (text.ToLower().Contains("neo"))
				{
					text2 = "neo";
				}
				else if (text.ToLower().Contains("axi"))
				{
					text2 = "axi";
				}
				else if (text.ToLower().Contains("requiem"))
				{
					text2 = "requiem";
				}
				else if (text.ToLower().Contains("all"))
				{
					text2 = "all";
				}
				if (fromAfterRelic && text2 == "none" && !string.IsNullOrEmpty(lastRelicType))
				{
					text2 = lastRelicType;
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Detected a relic suggestion after relic opening. Resuggesting previous category");
				}
				if (!(text2 == "none"))
				{
					break;
				}
				if (num > 0)
				{
					num--;
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to detect category, retrying again later...");
					Thread.Sleep(1000);
					continue;
				}
				StaticData.overwolfWrappwer.SendRelicRecommendationError("Failed to detect relic type from: " + text);
				Thread.Sleep(500);
				CloseRelicRecommendationHandler();
				return;
			}
			lastRelicType = text2;
			string text3 = "";
			text3 = JsonConvert.SerializeObject(new Dictionary<string, string>
			{
				{ "type", text2 },
				{ "order", "plat" },
				{ "orderLargerToSmaller", "true" }
			});
			if (!string.IsNullOrWhiteSpace(StaticData.relicRecommendationOverlayFilters))
			{
				text3 = StaticData.relicRecommendationOverlayFilters;
			}
			try
			{
				JToken? jToken = JsonConvert.DeserializeObject<JToken>(text3);
				jToken["type"] = text2;
				text3 = JsonConvert.SerializeObject(jToken);
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to update the era in the relic overlay filters! " + ex.Message);
			}
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Opening relic recommendation overlay with filters: " + text3);
			if (debugMode || StaticData.overwolfWrappwer.relicRecommendationReadyEvent.WaitOne(TimeSpan.FromSeconds(5.0)))
			{
				if (text2 == "requiem")
				{
					StaticData.overwolfWrappwer.SendRelicRecommendationError("Requiem relics are not supported");
					CloseRelicRecommendationHandler();
					return;
				}
				AnalyticsHandler.AddMetric("Recommendation_Overlay_Success", Misc.GetPossibleOverlayIssuesCached().ToString().ToLower());
				StaticData.overwolfWrappwer.getFilteredRelicPlanner(text3, forcingReload: false, onlyOwned: true, showAll: false, compactView: false, delegate
				{
				}, fromRelicRecommendations: true);
			}
			else
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Recommendation window didn't open in time!");
			}
			try
			{
				bitmap.Dispose();
			}
			catch
			{
			}
		}

		public static void CloseRelicRecommendationHandler()
		{
			StaticData.overwolfWrappwer.CloseRelicRecommendationOverlay();
		}
	}
}
