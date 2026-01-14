using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AF_DamageCalculatorLib.Classes;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Data.Types.WFM;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using AlecaFramePublicLib.DataTypes;
using JsonDiffPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace AlecaFrameClientLib
{
	public class OverwolfWrapper
	{
		private class RivenFinderSelectedAttrs
		{
			public bool positive;

			public string selectedAttrUID;

			public bool required;
		}

		public enum WFMarketMyStatus
		{
			online,
			ingame,
			invisible
		}

		private class BannerStorageData
		{
			public bool enabled;

			public string message;

			public string maxVersion;
		}

		public class WFMarketLoginRequest
		{
			public string email;

			public string password;

			public string auth_type;
		}

		public class WFMarketPostListingRequest
		{
			public string type;

			public string itemId;

			public int platinum;

			public int quantity;

			public bool visible;

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public int? perTrade;

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public string subtype;

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public int? rank;

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public int? amberStars;

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public int? cyanStars;
		}

		public enum LogType
		{
			INFO,
			WARN,
			ERROR
		}

		public class ItemPriceSmallResponse
		{
			public int? post;

			public int? insta;

			public int? postMax;

			public int? minR0;

			public int? minRMax;

			public int? volume;
		}

		public string currentVersion = "0.0.0";

		public string initializationError = "";

		private DateTime lastTimeGoodDataReceived = DateTime.MinValue;

		public string lastWFMarketToken = "";

		public bool isWFLoggedIn;

		public string lastWFMOrderResponse = "";

		public string lastWFMContractResponse = "";

		public bool wfmarketInErrorState;

		public int lastWFMUnreadMessages = -1;

		public int wfmarketTimeouts;

		public WFMarketProfileOrderList WFMarketOrders;

		public WFMarketProfileContractsList WFMarketContracts;

		private WFMarketProfileData lastWFMarketProfileData;

		private AutoResetEvent checkWFMarketStatusAndOrders = new AutoResetEvent(initialState: true);

		private AutoResetEvent checkWFMarketContracts = new AutoResetEvent(initialState: true);

		public AutoResetEvent newRelicScreenshotReady = new AutoResetEvent(initialState: false);

		public Bitmap lastWarframeScreenshot;

		public AutoResetEvent newRelicDataOrErrorReady = new AutoResetEvent(initialState: false);

		public string newRelicDataError = "";

		public bool newRelicDataErrorOccurred;

		public RelicOutputDataClass newRelicData;

		public bool isNewRelicDataReady;

		public WebSocket newWFMarketWS;

		public WFMarketMyStatus wfMarketMyStatus = WFMarketMyStatus.invisible;

		public bool WFMAutoStatus;

		private DateTime lastMessageReceivedData = DateTime.MinValue;

		public Dictionary<string, CachedRelicPlannerData> cachedRelicPlannerData = new Dictionary<string, CachedRelicPlannerData>();

		private DateTime lastTimeRelicPlannerDataUpdated = DateTime.MinValue;

		public AutoResetEvent relicRecommendationReadyEvent = new AutoResetEvent(initialState: false);

		private bool earlyInitializationDone;

		public BuildHandler buildHandler;

		private DateTime wsErrorBackoffUntilTime = DateTime.MinValue;

		private int wfmSocketExpoentialBackoffCounter;

		private DateTime lastTimeTokenUpdated = DateTime.MinValue;

		private bool relicPlannerLoadedWithAllRelics;

		private MemoryCache cache = MemoryCache.Default;

		private string lastWarframeData = "";

		public DateTime lastRelicStartTime = DateTime.MinValue;

		private bool usingSquadTestAccount;

		public string TradeFinishedNotificationData { get; set; }

		public string lastDataUpdateTime { get; set; }

		public string newVersionAvailable { get; set; }

		public string lastSavedVersion { get; set; } = "N/A";

		public event Action<object, object> OnSimulationResultsUpdate;

		public event Action<object, object> OnBuildsModBrowserUpdate;

		public event Action<object, object> logInBackground;

		public event Action<object> onWFMarketOrdersUpdated;

		public event Action<object> onWFMarketContractsUpdated;

		public event Action<object> onWFMarketStatusChanged;

		public event Action<object> onWFMarketLoginStatusChanged;

		public event Action<object, object, object> onRelicPlannerUpdate;

		public event Action<object, object, object> onRelicRecommendationUpdate;

		public event Action<object> onOpenRelicRecommendation;

		public event Action<object> onCloseRelicRecommendation;

		public event Action<object> OnDeltaUpdate;

		public event Action<object> onRivenOverlayOpen;

		public event Action<object> onRivenOverlayChange;

		public event Action<object> onWarframeDataChanged;

		public event Action<object> onStatsUpdateNeeded;

		public event Action<object> OnFavouritesUpdate;

		public event Action<object, object, object> OnInGameNotification;

		public event Action<object> OnSubscriptionStatusChanged;

		public event Action<object> OnTradeFinishedNotification;

		public event Action<object> onRelicScreenshotRequest;

		public event Action<object> onRelicOpenRequest;

		public void UserIsActive()
		{
			AnalyticsHandler.lastUserInteraction = DateTime.UtcNow;
		}

		public void BuildInitialize(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (buildHandler == null)
					{
						buildHandler = new BuildHandler();
					}
					bool flag = buildHandler.InitializeFromData(StaticData.dataHandler.basicRemoteData, StaticData.dataHandler.buildSourceDataFile, new List<EnemySetup>
					{
						new EnemySetup
						{
							metadata = new EnemySetup.Metadata("Default")
						}
					});
					callback(flag, null);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, $"An error has occurred in BuildInitialize: {ex}");
					callback(false, ex.Message);
				}
			});
		}

		public void GetBuildStatus(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandlerStatus.Request, BuildHandlerStatus>(buildHandler.GetStatus, requestData, callback, "GetBuildStatus");
		}

		public void BuildsSwitchMods(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.SwitchModPositionRequest>(buildHandler.SwitchMods, requestData, callback, "BuildsSwitchMods");
		}

		public void BuildsRemoveModFromSlot(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.RemoveModPositionRequest>(buildHandler.RemoveModFromSlot, requestData, callback, "BuildsRemoveModFromSlot");
		}

		public void BuildsGetModBrowser(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.ModBrowserRequest>(buildHandler.GetModBrowser, requestData, callback, "BuildsGetModBrowser");
		}

		public void BuildsPlaceNewMod(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.PlaceNewModRequest>(buildHandler.PlaceNewMod, requestData, callback, "BuildsPlaceNewMod");
		}

		public void BuildsSetBuildSettings(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.SetBuildSettingsRequest>(buildHandler.SetBuildSettings, requestData, callback, "BuildsSetBuildSettings");
		}

		public void BuildsEnemySetupGetList(Action<object, object> callback)
		{
			RunFunctionWithSimpleCallbackNoRequest(buildHandler.GetEnemySetupList, callback, "BuildsEnemySetupGetList");
		}

		public void BuildsEnemySetupSelect(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.EnemySetupSelectRequest>(buildHandler.SelectEnemySetup, requestData, callback, "BuildsEnemySetupSelect");
		}

		public void BuildsEnemyCustomizationGetEnemies(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.EnemyCustomizationGetEnemiesRequest, BuildHandler.EnemyCustomizationGetEnemiesResponse>(buildHandler.GetEnemyCustomizationEnemies, requestData, callback, "BuildsEnemyCustomizationGetEnemies");
		}

		public void BuildsEnemyCustomizationGetCurrentEnemiesStatus(Action<object, object> callback)
		{
			RunFunctionWithSimpleCallbackNoRequest(buildHandler.GetCurrentEnemyCustomizationStatus, callback, "BuildsEnemyCustomizationGetCurrentEnemiesStatus");
		}

		public void BuildsEnemyCustomizationAddEnemy(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.EnemyCustomizationAddEnemyRequest>(buildHandler.AddEnemyToCustomization, requestData, callback, "BuildsEnemyCustomizationAddEnemy");
		}

		public void BuildsEnemyCustomizationChangeEnemySettings(string requestData, Action<object, object> callback)
		{
			RunFunctionWithSimpleCallback<BuildHandler.EnemyCustomizationEditEnemyRequest>(buildHandler.EditEnemyInCustomization, requestData, callback, "BuildsEnemyCustomizationChangeEnemySettings");
		}

		public BuildHandler BuildsGetInstance()
		{
			return buildHandler;
		}

		public void OnSimulationResultsUpdateInvoke(object sender, object data)
		{
			this.OnSimulationResultsUpdate?.Invoke(sender, data);
		}

		public void OnBuildsModBrowserUpdateInvoke(object sender, object data)
		{
			this.OnBuildsModBrowserUpdate?.Invoke(sender, data);
		}

		public void RunFunctionWithSimpleCallback<R, T>(Func<R, T> function, string arg1JSON, Action<object, object> callback, [CallerMemberName] string caller = null)
		{
			if (StaticData.runOverwolfFunctionsAsync)
			{
				Task.Run(delegate
				{
					try
					{
						T val2 = function(JsonConvert.DeserializeObject<R>(arg1JSON));
						callback(true, JsonConvert.SerializeObject(val2));
					}
					catch (Exception arg)
					{
						StaticData.Log(LogType.ERROR, $"An error has occurred in async execution function in {caller}: {arg}");
						callback(false, default(T));
					}
				});
				return;
			}
			try
			{
				T val = function(JsonConvert.DeserializeObject<R>(arg1JSON));
				callback(true, JsonConvert.SerializeObject(val));
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, $"An error has occurred in sync execution function in {caller}: {ex}");
				callback(false, ex.Message);
			}
		}

		public void RunFunctionWithSimpleCallbackReinvokable<R, T>(Action<R, bool, Action<T>, Action<string>> function, string arg1JSON, Action<object, object> callback, [CallerMemberName] string caller = null)
		{
			if (StaticData.runOverwolfFunctionsAsync)
			{
				Task.Run(delegate
				{
					try
					{
						function(JsonConvert.DeserializeObject<R>(arg1JSON), arg2: true, delegate(T returnVal)
						{
							callback(true, JsonConvert.SerializeObject(returnVal));
							callback(true, JsonConvert.SerializeObject(returnVal));
							callback(true, JsonConvert.SerializeObject(returnVal));
						}, delegate(string errorMessage)
						{
							StaticData.Log(LogType.ERROR, "An error has occurred in async execution function in " + caller + ": " + errorMessage);
							callback(false, errorMessage);
						});
					}
					catch (Exception arg)
					{
						StaticData.Log(LogType.ERROR, $"An error has occurred in async execution function in {caller}: {arg}");
						callback(false, default(T));
					}
				});
				return;
			}
			try
			{
				function(JsonConvert.DeserializeObject<R>(arg1JSON), arg2: false, delegate(T returnVal)
				{
					callback(true, JsonConvert.SerializeObject(returnVal));
				}, delegate(string errorMessage)
				{
					StaticData.Log(LogType.ERROR, "An error has occurred in sync execution function in " + caller + ": " + errorMessage);
					callback(false, errorMessage);
				});
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, $"An error has occurred in sync execution function in {caller}: {ex}");
				callback(false, ex.Message);
			}
		}

		public void RunFunctionWithSimpleCallbackNoRequest<T>(Func<T> function, Action<object, object> callback, [CallerMemberName] string caller = null)
		{
			if (StaticData.runOverwolfFunctionsAsync)
			{
				Task.Run(delegate
				{
					try
					{
						T val2 = function();
						callback(true, JsonConvert.SerializeObject(val2));
					}
					catch (Exception arg)
					{
						StaticData.Log(LogType.ERROR, $"An error has occurred in async execution function in {caller}: {arg}");
						callback(false, default(T));
					}
				});
				return;
			}
			try
			{
				T val = function();
				callback(true, JsonConvert.SerializeObject(val));
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, $"An error has occurred in sync execution function in {caller}: {ex}");
				callback(false, ex.Message);
			}
		}

		public void RunFunctionWithSimpleCallback<R>(Action<R> function, string arg1JSON, Action<object, object> callback, [CallerMemberName] string caller = null)
		{
			if (StaticData.runOverwolfFunctionsAsync)
			{
				Task.Run(delegate
				{
					try
					{
						function(JsonConvert.DeserializeObject<R>(arg1JSON));
						callback(true, "");
					}
					catch (Exception ex2)
					{
						StaticData.Log(LogType.ERROR, $"An error has occurred in async execution function in {caller}: {ex2}");
						callback(false, ex2.Message);
					}
				});
				return;
			}
			try
			{
				function(JsonConvert.DeserializeObject<R>(arg1JSON));
				callback(true, "");
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, $"An error has occurred in sync execution function in {caller}: {ex}");
				callback(false, ex.Message);
			}
		}

		public void GetCraftingTreeForItem(string uniqueID, bool hideCompleted, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(CraftingTreeHelper.GetCraftingTreeForItem(uniqueID, hideCompleted)));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Error while getting crafting tree for item: " + ex.Message);
					callback(false, ex.Message);
				}
			});
		}

		public void getMasteryTabData(string orderingMode, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(MasteryHelper.GetMasteryData(orderingMode)));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering foundry items: " + ex.Message);
					callback(false, "");
				}
			});
		}

		public void GetProAnalytics(string filters, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					ProAnalytics.ProAnalyticsData proAnalytics = ProAnalytics.GetProAnalytics(JsonConvert.DeserializeObject<ProAnalytics.ProAnalyticsFilters>(filters));
					callback(true, JsonConvert.SerializeObject(proAnalytics));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has occurred while getting pro analytics: " + ex.Message);
					callback(false, ex.Message);
				}
			});
		}

		public void GetRivenHistoryData(string uniqueID, string subscriptionStatus, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (!Enum.TryParse<AlecaFrameSubscriptionStatus>(subscriptionStatus, out var result))
					{
						result = AlecaFrameSubscriptionStatus.None;
						StaticData.Log(LogType.ERROR, "Failed to parse subscriptionStatus in GetRivenHistoryData: " + subscriptionStatus);
					}
					BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(uniqueID);
					if (bigItemRefernceOrNull == null)
					{
						callback(false, "Weapon not found");
						StaticData.Log(LogType.ERROR, "Weapon (original) not found in GetRivenHistoryData: " + uniqueID);
					}
					else
					{
						BigItem bigItemRefernceOrNull2 = Misc.GetBigItemRefernceOrNull(Misc.GetReducedRivenWeaponUID(uniqueID));
						if (bigItemRefernceOrNull2 != null)
						{
							DataRivenStats rivenDataForWeapon = StaticData.dataHandler.rivenData.dataByRivenInternalID[StaticData.dataHandler.rivenData.weaponStats[uniqueID].rivenUID];
							using MyWebClient myWebClient = new MyWebClient(10000);
							myWebClient.Proxy = null;
							Task<string> task = myWebClient.DownloadStringTaskAsync(StaticData.RivenAPIHostname + "/rivenMinimumPrice?weaponURLname=" + Misc.GetWarframeMarketURLName(bigItemRefernceOrNull2.name));
							RivenHistoryDataPoint[] data;
							using (MyWebClient myWebClient2 = new MyWebClient(10000))
							{
								myWebClient2.Proxy = null;
								data = JsonConvert.DeserializeObject<RivenHistoryDataPoint[]>(myWebClient2.DownloadString(StaticData.RivenAPIHostname + "/rivenWeaponHistory?weaponURLname=" + Misc.GetWarframeMarketURLName(bigItemRefernceOrNull2.name)));
							}
							task.GetAwaiter().GetResult();
							int lowestPrice = int.Parse(task.Result.Replace("\"", ""));
							callback(true, JsonConvert.SerializeObject(new
							{
								attrData = ChangeAttrPriceForIcon(result, data.LastOrDefault()?.attrPriceAvg.Select((KeyValuePair<string, double> p) => new AttrHistoryDataPoint
								{
									name = Misc.ReplaceStringWithIcons(rivenDataForWeapon.rivenStats.GetOrDefault(p.Key)?.shortString ?? "???"),
									price = p.Value.ToString(),
									usage = (data.LastOrDefault()?.attrPopulatity.GetOrDefault(p.Key) ?? 0.0)
								}).ToList(), (int)(data.LastOrDefault()?.basePrice ?? 0.0), (int)(data.LastOrDefault()?.price ?? 0.0)),
								priceData = data.Select((RivenHistoryDataPoint p) => new
								{
									price = p.price,
									basePrice = p.basePrice,
									time = p.endts
								}),
								goodRollData = RivenExplorerHelper.GetGoodRollData(uniqueID),
								weaponName = bigItemRefernceOrNull.name,
								lowestPrice = lowestPrice,
								weaponImage = Misc.GetFullImagePath(Misc.GetBigItemRefernceOrNull(uniqueID)?.imageName),
								attrs = rivenDataForWeapon.rivenStats.Select((KeyValuePair<string, DataRivenStatsModifier> p) => new RivenFinderPossibleAttr(Misc.ReplaceStringWithIcons(p.Value.shortString), p.Key, 0.0, 0.0, p.Value.localizationString.Contains("%"), p.Value.localizationString.Contains("Damage to"))).ToList()
							}));
							return;
						}
						callback(false, "Weapon not found");
						StaticData.Log(LogType.ERROR, "Weapon (reduced) not found in GetRivenHistoryData: " + uniqueID);
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Error in GetRivenHistoryData: " + ex.ToString());
					callback(false, ex.Message);
				}
			});
		}

		private List<AttrHistoryDataPoint> ChangeAttrPriceForIcon(AlecaFrameSubscriptionStatus subscriptionStatus, List<AttrHistoryDataPoint> list, int basePrice, int averagePrice)
		{
			foreach (AttrHistoryDataPoint item in list)
			{
				if (item.usage < 5.0)
				{
					item.price = "-";
				}
				else
				{
					if (!int.TryParse(item.price, out var result))
					{
						continue;
					}
					if (subscriptionStatus == AlecaFrameSubscriptionStatus.PatreonT2 || subscriptionStatus == AlecaFrameSubscriptionStatus.PatreonT3)
					{
						if (result > 0)
						{
							item.price = item.price.ToString();
						}
						else
						{
							item.price = "-";
						}
					}
					else if (result > averagePrice)
					{
						item.price = "+++";
					}
					else if (result > basePrice)
					{
						item.price = "++";
					}
					else if (result > 0)
					{
						item.price = "+";
					}
					else
					{
						item.price = "-";
					}
				}
			}
			return list;
		}

		public void FinderRivenAttrsJustChanged(string weaponUniqueID, string selectedAttrsJSON, string selectedFiltersJSON, Action<object, object> attrUpdateCallback, Action<object, object> similarRivenUpdateCallback)
		{
			Task.Run(delegate
			{
				try
				{
					if (Misc.GetBigItemRefernceOrNull(weaponUniqueID) == null)
					{
						attrUpdateCallback(false, "Weapon not found");
						StaticData.Log(LogType.ERROR, "Weapon (original) not found in FinderRivenAttrsJustChanged: " + weaponUniqueID);
					}
					else
					{
						BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(Misc.GetReducedRivenWeaponUID(weaponUniqueID));
						if (bigItemRefernceOrNull != null)
						{
							DataRivenStats dataRivenStats = StaticData.dataHandler.rivenData.dataByRivenInternalID[StaticData.dataHandler.rivenData.weaponStats[weaponUniqueID].rivenUID];
							RivenFinderSelectedAttrs[] selectedAttrs = JsonConvert.DeserializeObject<RivenFinderSelectedAttrs[]>(selectedAttrsJSON);
							int positiveAttrCount = selectedAttrs.Count((RivenFinderSelectedAttrs p) => p.positive && !string.IsNullOrWhiteSpace(p.selectedAttrUID));
							int negativeAttrCount = selectedAttrs.Count((RivenFinderSelectedAttrs p) => !p.positive && !string.IsNullOrWhiteSpace(p.selectedAttrUID));
							if (positiveAttrCount < 2)
							{
								positiveAttrCount = 2;
							}
							IEnumerable<RivenFinderPossibleAttr> value = dataRivenStats.rivenStats.Select(delegate(KeyValuePair<string, DataRivenStatsModifier> p)
							{
								bool flag = selectedAttrs.Any((RivenFinderSelectedAttrs q) => !q.positive && q.selectedAttrUID == p.Key);
								(double, double) minMaxForAttribute = RivenExplorerHelper.GetMinMaxForAttribute(p.Key, weaponUniqueID, !flag, positiveAttrCount, negativeAttrCount);
								if (p.Value.localizationString.Contains("Damage to"))
								{
									minMaxForAttribute.Item1 += 1.0;
									minMaxForAttribute.Item2 += 1.0;
									minMaxForAttribute.Item2 = Math.Round(minMaxForAttribute.Item2, 2);
									minMaxForAttribute.Item1 = Math.Round(minMaxForAttribute.Item1, 2);
								}
								else
								{
									if (p.Value.localizationString.Contains("%"))
									{
										minMaxForAttribute.Item1 *= 100.0;
										minMaxForAttribute.Item2 *= 100.0;
									}
									minMaxForAttribute.Item2 = Math.Round(minMaxForAttribute.Item2, 1);
									minMaxForAttribute.Item1 = Math.Round(minMaxForAttribute.Item1, 1);
								}
								return new RivenFinderPossibleAttr(Misc.ReplaceStringWithIcons(p.Value.shortString), p.Key, minMaxForAttribute.Item1, minMaxForAttribute.Item2, p.Value.localizationString.Contains("%"), p.Value.localizationString.Contains("Damage to"));
							});
							attrUpdateCallback(true, JsonConvert.SerializeObject(value));
							try
							{
								List<RivenSimilarityRequestAttribute> list = (from p in selectedAttrs
									where !string.IsNullOrWhiteSpace(p.selectedAttrUID)
									select new RivenSimilarityRequestAttribute
									{
										name = p.selectedAttrUID,
										positive = p.positive,
										required = p.required
									}).ToList();
								RivenSimilarityRequestFilters rivenSimilarityRequestFilters = JsonConvert.DeserializeObject<RivenSimilarityRequestFilters>(selectedFiltersJSON);
								List<RivenUISimilarRiven> value2 = new List<RivenUISimilarRiven>();
								if (list.Count > 0)
								{
									value2 = RivenExplorerHelper.GetSimilarRivens(RivenSimilaritySource.WFMarket | RivenSimilaritySource.RivenMarket, bigItemRefernceOrNull, list, rivenSimilarityRequestFilters, (float)rivenSimilarityRequestFilters.minSimilarity / 100f);
								}
								similarRivenUpdateCallback(true, JsonConvert.SerializeObject(value2));
								return;
							}
							catch (Exception ex)
							{
								StaticData.Log(LogType.ERROR, "Error in FinderRivenAttrsJustChanged (similarRivenUpdate): " + ex.ToString());
								similarRivenUpdateCallback(false, ex.Message);
								return;
							}
						}
						attrUpdateCallback(false, "Weapon not found");
						StaticData.Log(LogType.ERROR, "Weapon (reduced) not found in FinderRivenAttrsJustChanged: " + weaponUniqueID);
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Error in FinderRivenAttrsJustChanged (attrUpdate): " + ex2.ToString());
					attrUpdateCallback(false, ex2.Message);
				}
			});
		}

		public void GetRivenWeapons(string search, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(RivenFinderSniper.GetRivenWeapons(search)));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Error in GetRivenWeapons: " + ex.ToString());
					callback(false, ex.Message);
				}
			});
		}

		public void GetRivenSniperAccount(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RivenSniperStatus remoteAccountDataOrNull = RivenFinderSniper.GetRemoteAccountDataOrNull();
					if (remoteAccountDataOrNull == null)
					{
						callback(false, "not_logged_in");
					}
					else
					{
						callback(true, JsonConvert.SerializeObject(new
						{
							AlecaFrameSubscriptionStatus = Enum.GetName(typeof(AlecaFrameSubscriptionStatus), remoteAccountDataOrNull.AlecaFrameSubscriptionStatus),
							maxSubscriptions = remoteAccountDataOrNull.maxSubscriptions,
							notificationDiscordWebhook = remoteAccountDataOrNull.notificationDiscordWebhook,
							notifications = remoteAccountDataOrNull.notifications.Select(delegate(RivenNotificationEntry p)
							{
								BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(p.realWeaponUID);
								return new
								{
									creationDate = p.creationDate.ToShortDateString(),
									id = p.id,
									enabled = p.enabled,
									filters = p.data.filters,
									weaponName = bigItemRefernceOrNull?.name,
									name = p.name,
									weaponImage = Misc.GetFullImagePath(bigItemRefernceOrNull?.imageName),
									attrs = p.data.attrs.Select((RivenSimilarityRequestAttribute k) => new RivenUISimilarRiven.Attr
									{
										text = Misc.ReplaceStringWithIcons(RivenExplorerHelper.GetShortRivenAttrNameFromTag(k.name, k.positive)),
										positive = k.positive
									}).ToList()
								};
							})
						}));
						this.OnSubscriptionStatusChanged?.Invoke(remoteAccountDataOrNull.AlecaFrameSubscriptionStatus);
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get GetRivenSniperAccount: " + ex);
					callback(false, ex.Message);
				}
			});
		}

		public void AddRivenSniperConfig(string weaponUniqueID, string selectedAttrsJSON, string selectedFiltersJSON, string configName, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (StatsHandler.GetPlayerUserHash() == null)
					{
						callback(false, "Your Warframe data needs to be recongnized before you can use the Sniper");
					}
					else
					{
						BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(weaponUniqueID);
						if (bigItemRefernceOrNull == null)
						{
							callback(false, "Weapon not found");
							StaticData.Log(LogType.ERROR, "Weapon (original) not found in AddRivenSniperConfig: " + weaponUniqueID);
						}
						else
						{
							BigItem bigItemRefernceOrNull2 = Misc.GetBigItemRefernceOrNull(Misc.GetReducedRivenWeaponUID(weaponUniqueID));
							if (bigItemRefernceOrNull2 != null)
							{
								_ = StaticData.dataHandler.rivenData.dataByRivenInternalID[StaticData.dataHandler.rivenData.weaponStats[weaponUniqueID].rivenUID];
								List<RivenSimilarityRequestAttribute> list = (from p in JsonConvert.DeserializeObject<RivenFinderSelectedAttrs[]>(selectedAttrsJSON)
									where !string.IsNullOrWhiteSpace(p.selectedAttrUID)
									select new RivenSimilarityRequestAttribute
									{
										name = p.selectedAttrUID,
										positive = p.positive,
										required = p.required
									}).ToList();
								RivenSimilarityRequestFilters filters = JsonConvert.DeserializeObject<RivenSimilarityRequestFilters>(selectedFiltersJSON);
								using WebClient webClient = new MyWebClient(10000);
								webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
								RivenSimilarityRequest value = new RivenSimilarityRequest
								{
									source = RivenSimilaritySource.All,
									filters = filters,
									weaponURLName = Misc.GetWarframeMarketURLName(bigItemRefernceOrNull2.name),
									attrs = list.ToArray()
								};
								webClient.UploadString(StaticData.RivenAPIHostname + "/sniper/addConfig?token=" + StatsHandler.GetPlayerUserHash() + "&configName=" + WebUtility.UrlEncode(configName) + "&realWeaponUniqueID=" + WebUtility.UrlEncode(bigItemRefernceOrNull.uniqueName), JsonConvert.SerializeObject(value));
								callback(true, "");
								return;
							}
							callback(false, "Weapon not found");
							StaticData.Log(LogType.ERROR, "Weapon (reduced) not found in AddRivenSniperConfig: " + weaponUniqueID);
						}
					}
				}
				catch (WebException ex)
				{
					if (ex.Response != null)
					{
						using (Stream stream = ex.Response.GetResponseStream())
						{
							using StreamReader streamReader = new StreamReader(stream);
							string arg = streamReader.ReadToEnd();
							callback(false, arg);
							return;
						}
					}
					callback(false, ex.Message);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Error in AddRivenSniperConfig: " + ex2.ToString());
					callback(false, ex2.Message);
				}
			});
		}

		public void DeleteRivenSniperConfig(string notificationID, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (StatsHandler.GetPlayerUserHash() == null)
					{
						callback(false, "Your Warframe data needs to be recongnized before you can use the Sniper");
						return;
					}
					using WebClient webClient = new MyWebClient(10000);
					webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					webClient.UploadString(StaticData.RivenAPIHostname + "/sniper/removeConfig?token=" + StatsHandler.GetPlayerUserHash() + "&notificationID=" + WebUtility.UrlEncode(notificationID), "");
					callback(true, "");
				}
				catch (WebException ex)
				{
					if (ex.Response != null)
					{
						using (Stream stream = ex.Response.GetResponseStream())
						{
							using StreamReader streamReader = new StreamReader(stream);
							string arg = streamReader.ReadToEnd();
							callback(false, arg);
							return;
						}
					}
					callback(false, ex.Message);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Error in DeleteRivenSniperConfig: " + ex2.ToString());
					callback(false, ex2.Message);
				}
			});
		}

		public void RivenSniperChangeNotificationEnabled(string notificationID, bool newStatus, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (StatsHandler.GetPlayerUserHash() == null)
					{
						callback(false, "Your Warframe data needs to be recongnized before you can use the Sniper");
						return;
					}
					using WebClient webClient = new MyWebClient(10000);
					webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					webClient.UploadString($"{StaticData.RivenAPIHostname}/sniper/changeNotificationEnabled?token={StatsHandler.GetPlayerUserHash()}&notificationID={WebUtility.UrlEncode(notificationID)}&enabled={newStatus}", "");
					callback(true, "");
				}
				catch (WebException ex)
				{
					if (ex.Response != null)
					{
						using (Stream stream = ex.Response.GetResponseStream())
						{
							using StreamReader streamReader = new StreamReader(stream);
							string arg = streamReader.ReadToEnd();
							callback(false, arg);
							return;
						}
					}
					callback(false, ex.Message);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Error in RivenSniperChangeNotificationEnabled: " + ex2.ToString());
					callback(false, ex2.Message);
				}
			});
		}

		public void RivenSniperChangeSettings(string changesJSON, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (StatsHandler.GetPlayerUserHash() == null)
					{
						callback(false, "Your Warframe data needs to be recongnized before you can use the Sniper");
						return;
					}
					using WebClient webClient = new MyWebClient(10000);
					webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					webClient.UploadString(StaticData.RivenAPIHostname + "/sniper/changeGlobalSettings?token=" + StatsHandler.GetPlayerUserHash() + "&changes=" + changesJSON, changesJSON);
					callback(true, "");
				}
				catch (WebException ex)
				{
					if (ex.Response != null)
					{
						using (Stream stream = ex.Response.GetResponseStream())
						{
							using StreamReader streamReader = new StreamReader(stream);
							string arg = streamReader.ReadToEnd();
							callback(false, arg);
							return;
						}
					}
					callback(false, ex.Message);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Error in RivenSniperChangeSettings: " + ex2.ToString());
					callback(false, ex2.Message);
				}
			});
		}

		public string GetInitializationError()
		{
			return initializationError;
		}

		public void SendDeltaUpdate(int deltaCount)
		{
			this.OnDeltaUpdate?.Invoke(deltaCount);
		}

		public void DoEarlyInitialization(Action<object, object> callback)
		{
			try
			{
				DoEarlyInitializationSync();
				callback(true, "");
			}
			catch (Exception ex)
			{
				callback(false, ex.Message);
			}
		}

		public void DoEarlyInitializationSync()
		{
			StaticData.overwolfWrappwer = this;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.DefaultConnectionLimit = 9999;
			earlyInitializationDone = true;
		}

		public void Initialize(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					StaticData.Log(LogType.INFO, "Starting plugin initialization...");
					if (!earlyInitializationDone)
					{
						DoEarlyInitializationSync();
					}
					if (!Directory.Exists(StaticData.saveFolder))
					{
						StaticData.isFirstRunOnInstall = true;
					}
					Directory.CreateDirectory(StaticData.saveFolder);
					Directory.CreateDirectory(StaticData.saveFolder + "/cachedData/");
					Directory.CreateDirectory(StaticData.saveFolder + "/themePresets/");
					UpdateHandler.CheckLocalDataAndUpdateIfNeccessary();
					StaticData.Log(LogType.INFO, "Local data ready");
					try
					{
						StaticData.dataHandler = new DataHandler(this, StaticData.saveFolder + "/cachedData/");
					}
					catch (Exception ex)
					{
						StaticData.Log(LogType.WARN, "Failed to initialize dataHandler! (" + ex.Message + ") Removing cache folder and trying again...");
						Directory.Delete(StaticData.saveFolder + "/cachedData/", recursive: true);
						Directory.CreateDirectory(StaticData.saveFolder + "/cachedData/");
						UpdateHandler.CheckLocalDataAndUpdateIfNeccessary(reAttempt: true);
						StaticData.dataHandler = new DataHandler(this, StaticData.saveFolder + "/cachedData/");
					}
					FoundryHelper.Initialize();
					FavouriteHelper.Initialize();
					InventoryDeltaHelper.Initialize();
					AnalyticsHandler.Initialize();
					if (File.Exists(StaticData.saveFolder + "WFMarketToken.tk"))
					{
						lastWFMarketToken = File.ReadAllText(StaticData.saveFolder + "WFMarketToken.tk");
						lastTimeTokenUpdated = DateTime.MinValue;
						checkWFMarketStatusAndOrders.Set();
						checkWFMarketContracts.Set();
					}
					if (File.Exists(StaticData.saveFolder + "ignoreSSLerrors.dat"))
					{
						ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
					}
					OCRHelper.Initialize();
					UpdateHandler.DownloadGlobalSettingsNonBlocking();
					Task.Run(delegate
					{
						bool updateOrders = true;
						bool updateContracts = true;
						while (true)
						{
							try
							{
								UpdateWFInfoLoginStatus(updateOrders, updateContracts);
							}
							catch (Exception ex3)
							{
								StaticData.Log(LogType.ERROR, "An unhandled error in the WFMarket loop has occurred: " + ex3);
							}
							finally
							{
								updateOrders = false;
								updateContracts = false;
							}
							Thread.Sleep(500);
							switch (WaitHandle.WaitAny(new WaitHandle[2] { checkWFMarketStatusAndOrders, checkWFMarketContracts }, AnalyticsHandler.IsUserActiveInTheLastXMinutes() ? TimeSpan.FromSeconds(60.0) : TimeSpan.FromMinutes(5.0)))
							{
							case 258:
								updateOrders = true;
								updateContracts = true;
								break;
							case 0:
								updateOrders = true;
								break;
							case 1:
								updateContracts = true;
								break;
							}
						}
					});
					Task.Run(delegate
					{
						while (true)
						{
							try
							{
								DoWFMWebSocketWork();
							}
							catch (Exception ex3)
							{
								StaticData.Log(LogType.ERROR, "Failed to do periodic WFMarket WS work: " + ex3);
							}
							Thread.Sleep(5000);
						}
					});
					StaticData.Log(LogType.INFO, "Initialization finished!");
					callback(true, null);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to initialize plugin: " + ex2.Message);
					initializationError = ex2.Message;
					callback(false, ex2.Message);
				}
			});
		}

		public void onRelicScreenshotRequestCaller()
		{
			if (this.onRelicScreenshotRequest != null)
			{
				this.onRelicScreenshotRequest("hi!");
			}
			else
			{
				StaticData.Log(LogType.ERROR, "onRelicScreenshotRequest is null!");
			}
		}

		public void onTradeFinishedNotificationCaller()
		{
			if (this.OnTradeFinishedNotification != null)
			{
				this.OnTradeFinishedNotification("hi!");
			}
			else
			{
				StaticData.Log(LogType.ERROR, "OnTradeFinishedNotification is null!");
			}
		}

		public void TestTradeFinishedNotification()
		{
			Task.Run(delegate
			{
				try
				{
					TradeFinishedNotificationData = JsonConvert.SerializeObject(new WFMarketHelper.ActiveTradeFinishedNotificationData
					{
						image = "http://cdn.alecaframe.com/warframeData/custom/imgRemote/platinum.png",
						isContract = false,
						itemAmount = 3,
						itemName = "Item name",
						listingOrContractID = "1231232132132",
						remoteUsername = "XxBlueDawnxX"
					});
					onTradeFinishedNotificationCaller();
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Error in TestTradeFinishedNotification: " + ex.ToString());
				}
			});
		}

		public void SetAnalyticsData(string OWuserID, string OWpromo, Action<object> callback)
		{
			StaticData.overwolfID = OWuserID;
			StaticData.overwolfPromo = OWpromo;
			if (string.IsNullOrWhiteSpace(OWpromo))
			{
				OWpromo = "none";
			}
			StaticData.analyticsDataReady = true;
			StaticData.analyticsDataReadyEvent.Set();
			callback("Hi!");
		}

		public void onRelicOpenRequestCaller()
		{
			if (this.onRelicOpenRequest != null)
			{
				this.onRelicOpenRequest("hi!");
			}
			else
			{
				StaticData.Log(LogType.ERROR, "onRelicOpenRequest is null!");
			}
		}

		public void onWFMStatusChangedCaller()
		{
			if (this.onWFMarketStatusChanged != null)
			{
				this.onWFMarketStatusChanged("");
			}
			else
			{
				StaticData.Log(LogType.ERROR, "onWFMarketStatusChanged is null!");
			}
		}

		public void CloseRelicRecommendationOverlay()
		{
			if (this.onCloseRelicRecommendation != null)
			{
				this.onCloseRelicRecommendation("");
			}
		}

		public void relicRecommendationReady()
		{
			relicRecommendationReadyEvent.Set();
		}

		public void OpenRelicRecommendations()
		{
			if (this.onOpenRelicRecommendation != null)
			{
				this.onOpenRelicRecommendation("");
			}
		}

		public void SendRelicRecommendationError(string error)
		{
			StaticData.Log(LogType.WARN, "Relic recommendation error: " + error);
			if (this.onRelicRecommendationUpdate != null)
			{
				this.onRelicRecommendationUpdate("error", error, "");
			}
		}

		public void DoRelicRecommendationTest()
		{
			Task.Run(delegate
			{
				OverlaysHandler.OpenRelicRecommendationHandler(debugMode: true, new Bitmap("C:\\Users\\aleca\\Downloads\\relicRecommendation.jpg"));
			});
		}

		private void DoWFMWebSocketWork()
		{
			if (DateTime.UtcNow < wsErrorBackoffUntilTime)
			{
				return;
			}
			if (newWFMarketWS == null || newWFMarketWS.State != WebSocketState.Open || DateTime.UtcNow - lastMessageReceivedData > TimeSpan.FromMinutes(3.0))
			{
				if (string.IsNullOrEmpty(lastWFMarketToken))
				{
					return;
				}
				if (newWFMarketWS != null)
				{
					try
					{
						StaticData.Log(LogType.WARN, "Closing WS manually! State=" + newWFMarketWS.State.ToString() + " lastReceivedTime=" + (DateTime.UtcNow - lastMessageReceivedData).ToTimeString());
						wfmSocketExpoentialBackoffCounter++;
						wsErrorBackoffUntilTime = DateTime.UtcNow.Add(GetWFMExponentialBackoffTimeout());
						wfmarketInErrorState = true;
						newWFMarketWS.Close();
						onWFMStatusChangedCaller();
					}
					catch
					{
					}
				}
				newWFMarketWS = new WebSocket("wss://warframe.market/socket-v2", "", null, new Dictionary<string, string> { { "Sec-WebSocket-Protocol", "wfm" } }.ToList(), "AlecaFrameWS");
				newWFMarketWS.Security.EnabledSslProtocols = SslProtocols.Tls12;
				newWFMarketWS.MessageReceived += delegate(object sender, MessageReceivedEventArgs e)
				{
					try
					{
						if (e.Message != null)
						{
							lastMessageReceivedData = DateTime.UtcNow;
							if (e.Message.Contains("@wfm|cmd/auth/signIn:ok"))
							{
								StaticData.Log(LogType.INFO, "Successfully authenticated with the WFM socket!");
								wfmarketInErrorState = false;
								wfmarketTimeouts = 0;
								SendWFMMarketStatus(wfMarketMyStatus);
								onWFMStatusChangedCaller();
								wfmSocketExpoentialBackoffCounter = 0;
							}
							else if (e.Message.Contains("@wfm|cmd/auth/signIn:error"))
							{
								wfmarketInErrorState = true;
								StaticData.Log(LogType.ERROR, "Failed to authenticate with the WFM socket! Token might be invalid.");
							}
							else if (e.Message.Contains("@wfm|event/status/set") || e.Message.Contains("@wfm|cmd/status/set:ok"))
							{
								wfmarketInErrorState = false;
								if (e.Message.Contains("\"invisible\""))
								{
									wfMarketMyStatus = WFMarketMyStatus.invisible;
								}
								else if (e.Message.Contains("\"offline\""))
								{
									wfMarketMyStatus = WFMarketMyStatus.invisible;
								}
								else if (e.Message.Contains("\"ingame\""))
								{
									wfMarketMyStatus = WFMarketMyStatus.ingame;
								}
								else if (e.Message.Contains("\"online\""))
								{
									wfMarketMyStatus = WFMarketMyStatus.online;
								}
								StaticData.Log(LogType.INFO, "Received new status update from WFM: " + wfMarketMyStatus);
								onWFMStatusChangedCaller();
							}
						}
					}
					catch (Exception ex2)
					{
						StaticData.Log(LogType.ERROR, "Failed to process WFM socket message: " + e.Message + " Error: " + ex2);
					}
				};
				newWFMarketWS.Opened += delegate
				{
					wfmarketInErrorState = false;
					lastMessageReceivedData = DateTime.UtcNow;
					SendWFMSocketLogin();
				};
				try
				{
					StaticData.Log(LogType.INFO, "Connecting to the WFM socket...");
					newWFMarketWS.Open();
					lastMessageReceivedData = DateTime.UtcNow;
					return;
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to connect to the WFM socket: " + ex);
					wfmSocketExpoentialBackoffCounter++;
					wsErrorBackoffUntilTime = DateTime.UtcNow.Add(GetWFMExponentialBackoffTimeout());
					Thread.Sleep(30000);
					return;
				}
			}
			if (WFMAutoStatus)
			{
				WFMarketMyStatus wFMarketMyStatus = (OCRHelper.CheckIsWarframeIsOpen() ? WFMarketMyStatus.ingame : WFMarketMyStatus.invisible);
				if (wFMarketMyStatus != wfMarketMyStatus)
				{
					StaticData.Log(LogType.INFO, "Auto updated WFM status to: " + wFMarketMyStatus);
					SendWFMMarketStatus(wFMarketMyStatus);
				}
			}
		}

		private TimeSpan GetWFMExponentialBackoffTimeout()
		{
			return TimeSpan.FromSeconds(Math.Min(600.0, 10.0 * Math.Pow(2.0, wfmSocketExpoentialBackoffCounter)));
		}

		private void SendWFMSocketLogin()
		{
			if (newWFMarketWS != null)
			{
				try
				{
					newWFMarketWS.Send("{\"route\":\"@wfm|cmd/auth/signIn\",\"payload\":{\"token\":\"" + lastWFMarketToken + "\"}}");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMMarketStatus (" + wfMarketMyStatus.ToString() + "). Error: " + ex);
				}
			}
		}

		private void SendWFMMarketStatus(WFMarketMyStatus wfMarketMyStatus)
		{
			if (newWFMarketWS != null)
			{
				try
				{
					newWFMarketWS.Send("{\"route\":\"@wfm|cmd/status/set\",\"payload\":{\"status\":\"" + wfMarketMyStatus.ToString() + "\"}}" + wfMarketMyStatus.ToString() + "\"}");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMMarketStatus (" + wfMarketMyStatus.ToString() + "). Error: " + ex);
				}
			}
		}

		public void SetWFMMarketStatus(string newStatus, bool autoMode, Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					WFMAutoStatus = autoMode;
					wfMarketMyStatus = (WFMarketMyStatus)Enum.Parse(typeof(WFMarketMyStatus), newStatus);
					SendWFMMarketStatus(wfMarketMyStatus);
					StaticData.Log(LogType.INFO, $"New WFM status set to: auto={WFMAutoStatus}, status={wfMarketMyStatus}");
					callback(true);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to call SetWFMMarketStatus: " + ex.Message);
					callback(false);
				}
			});
		}

		public void GetAvailableThemePresets(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					List<string> list = new List<string>();
					new FileInfo(Assembly.GetExecutingAssembly().Location);
					if (Directory.Exists(StaticData.saveFolder + "/cachedData/custom/themePresets/"))
					{
						list.AddRange(Directory.GetFiles(StaticData.saveFolder + "/cachedData/custom/themePresets/", "*.alecatheme"));
					}
					if (Directory.Exists(StaticData.saveFolder + "/themePresets/"))
					{
						list.AddRange(Directory.GetFiles(StaticData.saveFolder + "/themePresets/", "*.alecatheme"));
					}
					List<ThemePreset> themeDataFromFiles = Misc.GetThemeDataFromFiles(list);
					callback(true, JsonConvert.SerializeObject(themeDataFromFiles));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get available themes! " + ex);
					callback(false, "");
				}
			});
		}

		public void CreateNewThemePreset(string colorData, string iconPath, string name, string author, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					ThemePreset themePreset = new ThemePreset();
					if (string.IsNullOrWhiteSpace(name))
					{
						throw new Exception("Name cannot be empty!");
					}
					if (string.IsNullOrWhiteSpace(author))
					{
						throw new Exception("Author cannot be empty!");
					}
					if (!File.Exists(iconPath))
					{
						throw new Exception("Icon file not found!");
					}
					FileInfo fileInfo = new FileInfo(iconPath);
					if (fileInfo.Length > 15000)
					{
						throw new Exception("Icon file is too big! Max 15kb");
					}
					if (fileInfo.Extension != ".png")
					{
						throw new Exception("Icon file must be a png!");
					}
					themePreset.name = name;
					themePreset.author = author;
					themePreset.iconBase64 = Convert.ToBase64String(File.ReadAllBytes(iconPath));
					themePreset.colorData = colorData;
					themePreset.creationDate = DateTime.UtcNow;
					string contents = JsonConvert.SerializeObject(themePreset, Formatting.Indented);
					string text = StaticData.saveFolder.Replace("/", "\\") + "\\themePresets\\" + Misc.ReplaceInvalidChars(name).ToLower() + "_" + Misc.ReplaceInvalidChars(author).ToLower() + ".alecatheme";
					File.WriteAllText(text, contents);
					string arguments = "/select,\"" + text.Replace("\\\\", "\\") + "\"";
					Process.Start("explorer.exe", arguments);
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to create theme preset! " + ex);
					callback(false, "Failed to create theme: " + ex.Message);
				}
			});
		}

		public void OpenThemesFolder()
		{
			string arguments = "/e.," + (StaticData.saveFolder.Replace("/", "\\") + "\\themePresets\\").Replace("\\\\", "\\");
			Process.Start("explorer.exe", arguments);
		}

		public void DoUninstallWork(Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (Directory.Exists(StaticData.saveFolder))
					{
						Directory.Delete(StaticData.saveFolder, recursive: true);
					}
					AnalyticsHandler.SendUninstall();
				}
				catch
				{
				}
				callback(":(");
			});
		}

		public void IsFTUEDone(Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(IsFTUEDoneSync());
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.WARN, "Failed to IsFTUEDone: " + ex.Message);
					callback(false);
				}
			});
		}

		public bool IsFTUEDoneSync()
		{
			return File.Exists(StaticData.saveFolder + "FTUEDone.tmp");
		}

		public void MarkFTUEAsDone(Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					File.Create(StaticData.saveFolder + "FTUEDone.tmp");
					callback(true);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.WARN, "Failed to MarkFTUEAsDone: " + ex.Message);
					callback(false);
				}
			});
		}

		public void UpdateWFInfoLoginStatus(bool updateOrders, bool updateContracts)
		{
			if (string.IsNullOrEmpty(lastWFMarketToken))
			{
				if (isWFLoggedIn)
				{
					isWFLoggedIn = false;
					this.onWFMarketOrdersUpdated("Hi!");
				}
				else
				{
					isWFLoggedIn = false;
				}
				return;
			}
			int num = 0;
			while (true)
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient(21000);
					myWebClient.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + lastWFMarketToken);
					myWebClient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en");
					myWebClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					if (DateTime.UtcNow - lastTimeTokenUpdated > TimeSpan.FromHours(5.0))
					{
						try
						{
							lastWFMarketProfileData = JsonConvert.DeserializeObject<WFMarketProfileData>(myWebClient.DownloadString("https://api.warframe.market/v2/me"));
							string text = myWebClient.ResponseHeaders["Authorization"]?.ToString()?.Replace("JWT ", "")?.Trim() ?? "";
							if (!string.IsNullOrWhiteSpace(text) && text != lastWFMarketToken)
							{
								StaticData.Log(LogType.INFO, "WFMarket token updated!");
								lastWFMarketToken = text;
								File.WriteAllText(StaticData.saveFolder + "WFMarketToken.tk", lastWFMarketToken);
							}
							else
							{
								StaticData.Log(LogType.WARN, "Failed to update WFMarket token!!");
							}
							lastTimeTokenUpdated = DateTime.UtcNow;
						}
						catch (Exception ex)
						{
							StaticData.Log(LogType.WARN, "Failed to get profile data: " + ex);
							lastWFMarketProfileData = null;
						}
					}
					bool flag = lastWFMarketProfileData != null;
					if (flag != isWFLoggedIn)
					{
						this.onWFMarketLoginStatusChanged?.Invoke("Hi!");
						isWFLoggedIn = flag;
						if (!isWFLoggedIn)
						{
							WFMarketOrders = new WFMarketProfileOrderList();
							WFMarketContracts = new WFMarketProfileContractsList();
							lastWFMContractResponse = "";
							lastWFMOrderResponse = "";
							this.onWFMarketOrdersUpdated?.Invoke("Hi!");
							this.onWFMarketContractsUpdated?.Invoke("Hi!");
						}
					}
					else if (lastWFMarketProfileData?.data?.unread_messages != lastWFMUnreadMessages)
					{
						lastWFMUnreadMessages = (lastWFMarketProfileData?.data?.unread_messages).GetValueOrDefault();
						onWFMStatusChangedCaller();
					}
					if (updateOrders)
					{
						myWebClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
						string text2 = myWebClient.DownloadString("https://api.warframe.market/v2/orders/my");
						bool flag2 = false;
						flag2 = string.IsNullOrEmpty(lastWFMOrderResponse) || new JsonDiffer().Diff(JToken.Parse(text2), JToken.Parse(lastWFMOrderResponse), useIdPropertyToDetermineEquality: false).Operations.Count > 0;
						WFMarketOrders = JsonConvert.DeserializeObject<WFMarketProfileOrderList>(text2);
						if (flag2)
						{
							checkWFMarketStatusAndOrders.Reset();
							this.onWFMarketOrdersUpdated?.Invoke("Hi!");
							lastWFMOrderResponse = text2;
						}
					}
					if (updateContracts)
					{
						myWebClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
						string slug = lastWFMarketProfileData.data.slug;
						myWebClient.Headers.Remove(HttpRequestHeader.Authorization);
						myWebClient.Headers.Add(HttpRequestHeader.Authorization, "JWT " + lastWFMarketToken);
						string text3 = myWebClient.DownloadString("https://api.warframe.market/v1/profile/" + slug + "/auctions");
						bool flag3 = false;
						flag3 = string.IsNullOrEmpty(lastWFMContractResponse) || new JsonDiffer().Diff(JToken.Parse(text3), JToken.Parse(lastWFMContractResponse), useIdPropertyToDetermineEquality: false).Operations.Count > 0;
						WFMarketContracts = JsonConvert.DeserializeObject<WFMarketProfileContractsList>(text3);
						if (flag3)
						{
							checkWFMarketContracts.Reset();
							this.onWFMarketContractsUpdated?.Invoke("Hi!");
							lastWFMContractResponse = text3;
						}
					}
					if (wfmarketTimeouts != 0)
					{
						this.onWFMarketLoginStatusChanged?.Invoke("hi");
					}
					wfmarketTimeouts = 0;
					break;
				}
				catch (Exception ex2)
				{
					num++;
					StaticData.Log(LogType.ERROR, $"Failed to refresh WFM data (Attempt {num}/3) {ex2}");
					if (ex2 is WebException ex3)
					{
						if (ex3.Response is HttpWebResponse httpWebResponse)
						{
							using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
							StaticData.Log(LogType.ERROR, "Response: " + streamReader.ReadToEnd());
						}
						if (ex3.Status == WebExceptionStatus.Timeout)
						{
							if (wfmarketTimeouts == 0)
							{
								this.onWFMarketLoginStatusChanged?.Invoke("hi");
							}
							wfmarketTimeouts++;
						}
					}
					if (num < 3)
					{
						Thread.Sleep((wfmarketTimeouts > 0) ? 3000 : 30000);
						continue;
					}
					isWFLoggedIn = false;
					break;
				}
			}
		}

		public void GetSingleRivenDetails(string rivenRandomID, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RivenSummaryData singleRivenDetails = RivenExplorerHelper.GetSingleRivenDetails(rivenRandomID);
					try
					{
						singleRivenDetails.FillSimilarRivens();
					}
					catch (Exception ex)
					{
						StaticData.Log(LogType.ERROR, "Failed to fill similar rivens: " + ex.Message);
					}
					singleRivenDetails.FillGoodRolls();
					callback(true, JsonConvert.SerializeObject(singleRivenDetails));
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "An error has occured while getting single riven details: " + ex2.Message);
					callback(false, "An error has occured while getting single riven details: " + ex2.Message);
				}
			});
		}

		public void SetFavouriteStatus(string uniqueID, bool value)
		{
			try
			{
				if (value)
				{
					FavouriteHelper.AddFavourite(uniqueID);
				}
				else
				{
					FavouriteHelper.RemoveFavourite(uniqueID);
				}
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, "An error has occured while setting favourite status: " + ex.Message);
			}
		}

		public void GetSingleRivenDetailsFromWFM(string wfmURL, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RivenSummaryData singleRivenDetailsFromWFM = RivenExplorerHelper.GetSingleRivenDetailsFromWFM(wfmURL);
					try
					{
						singleRivenDetailsFromWFM.FillSimilarRivens();
					}
					catch (Exception ex)
					{
						StaticData.Log(LogType.ERROR, "Failed to fill similar rivens: " + ex.Message);
					}
					singleRivenDetailsFromWFM.FillGoodRolls();
					callback(true, JsonConvert.SerializeObject(singleRivenDetailsFromWFM));
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "An error has occured while getting single riven details from WFM: " + ex2.Message);
					callback(false, "An error has occured while getting single riven details from WFM: " + ex2.Message);
				}
			});
		}

		public void ListRivenOnWFM(string rivenRandomID, string listType, string listVisibility, int WFMsellingPrice, int WFMstartingPrice, int WFMbuyoutPrice, int WFMminReputation, string WFMdescription, bool useLvl8Stats, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RivenExplorerHelper.ListRivenOnWFM(rivenRandomID, listType, listVisibility, WFMsellingPrice, WFMstartingPrice, WFMbuyoutPrice, WFMminReputation, WFMdescription, useLvl8Stats);
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has occured while listing riven on WFM: " + ex.Message);
					callback(false, "An error has occured while listing riven on WFM: " + ex.Message);
				}
			});
		}

		public void EditRivenOnWFM(string wfmContractID, string listType, string listVisibility, int WFMsellingPrice, int WFMstartingPrice, int WFMbuyoutPrice, int WFMminReputation, string WFMdescription, bool useLvl8Stats, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RivenExplorerHelper.EditRivenOnWFM(wfmContractID, listType, listVisibility, WFMsellingPrice, WFMstartingPrice, WFMbuyoutPrice, WFMminReputation, WFMdescription, useLvl8Stats);
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has occured while editing riven on WFM: " + ex.Message);
					callback(false, "An error has occured while editing riven on WFM: " + ex.Message);
				}
			});
		}

		public void CustomScalingChanged(int newScaling, Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					StaticData.customScale = (float)newScaling / 100f;
				}
				catch
				{
				}
			});
		}

		public void getFilteredFoundry(string filtersJSON, bool showAll, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary["yesnoFilters"]);
					IEnumerable<FoundryItemData> foundryTabData = GetFoundryTabData(showAll, dictionary, yesNoFilters);
					callback(true, JsonConvert.SerializeObject(foundryTabData));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering foundry items: " + ex.Message);
					callback(false, "");
				}
			});
		}

		public static IEnumerable<FoundryItemData> GetFoundryTabData(bool showAll, Dictionary<string, string> filters, Dictionary<string, string> yesNoFilters)
		{
			string searchString = (filters.ContainsKey("search") ? filters["search"] : "");
			string orDefault = yesNoFilters.GetOrDefault("primeType");
			bool seePrimes = orDefault == "prime" || orDefault == null;
			bool seeNonPrimes = orDefault == "normal" || orDefault == null;
			WorldStatePrimeResurgenceData primeResurgenceData = null;
			if (yesNoFilters.ContainsKey("primeResurgence"))
			{
				primeResurgenceData = WorldStateHelper.GetPrimeResurgenceData();
			}
			List<FoundryItemData> list = new List<FoundryItemData>();
			if (filters["type"] == "warframe" || filters["type"] == "all")
			{
				IEnumerable<DataWarframe> source = StaticData.dataHandler.warframes.Values.Where((DataWarframe p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				list.AddRange(source.Select((DataWarframe p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "primary" || filters["type"] == "all")
			{
				IEnumerable<DataPrimaryWeapon> source2 = StaticData.dataHandler.primaryWeapons.Values.Where((DataPrimaryWeapon p) => ((seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime())) && p.ShouldBeShown());
				list.AddRange(source2.Select((DataPrimaryWeapon p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "secondary" || filters["type"] == "all")
			{
				IEnumerable<DataSecondaryWeapon> source3 = StaticData.dataHandler.secondaryWeapons.Values.Where((DataSecondaryWeapon p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				list.AddRange(source3.Select((DataSecondaryWeapon p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "melee" || filters["type"] == "all")
			{
				IEnumerable<DataMeleeWeapon> source4 = StaticData.dataHandler.meleeWeapons.Values.Where((DataMeleeWeapon p) => !p.IsModularOrModularVariant() && ((seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime())));
				list.AddRange(source4.Select((DataMeleeWeapon p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "modular" || filters["type"] == "all")
			{
				IEnumerable<DataMisc> source5 = StaticData.dataHandler.misc.Values.Where((DataMisc p) => (seeNonPrimes || filters["type"] == "modular") && !p.uniqueName.Contains("Blueprint") && p.IsModular() && (p.uniqueName.Contains("/Barrel/") || p.uniqueName.Contains("/Barrels/")));
				list.AddRange(source5.Select((DataMisc p) => new FoundryItemData(p)).ToList());
				IEnumerable<DataMeleeWeapon> source6 = StaticData.dataHandler.meleeWeapons.Values.Where((DataMeleeWeapon p) => (seeNonPrimes || filters["type"] == "modular") && !p.uniqueName.Contains("Blueprint") && p.IsModular() && (p.uniqueName.Contains("/Tip/") || p.uniqueName.Contains("/Tips/")));
				list.AddRange(source6.Select((DataMeleeWeapon p) => new FoundryItemData(p)).ToList());
				IEnumerable<DataMisc> source7 = StaticData.dataHandler.kdrives.Values.Where((DataMisc p) => seeNonPrimes || filters["type"] == "modular");
				list.AddRange(source7.Select((DataMisc p) => new FoundryItemData(p)).ToList());
				IEnumerable<DataMisc> source8 = StaticData.dataHandler.amps.Values.Where((DataMisc p) => seeNonPrimes || filters["type"] == "modular");
				list.AddRange(source8.Select((DataMisc p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "arch" || filters["type"] == "all")
			{
				IEnumerable<DataArchwing> source9 = StaticData.dataHandler.archWings.Values?.Where((DataArchwing p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				IEnumerable<DataArchGun> source10 = StaticData.dataHandler.archGuns.Values?.Where((DataArchGun p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				IEnumerable<DataArchMelee> source11 = StaticData.dataHandler.archMelees.Values?.Where((DataArchMelee p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				list.AddRange(source9.Select((DataArchwing p) => new FoundryItemData(p)).ToList());
				list.AddRange(source10.Select((DataArchGun p) => new FoundryItemData(p)).ToList());
				list.AddRange(source11.Select((DataArchMelee p) => new FoundryItemData(p)).ToList());
			}
			if (filters["type"] == "companion" || filters["type"] == "all")
			{
				IEnumerable<DataPet> source12 = StaticData.dataHandler.pets.Values.Where((DataPet p) => p.IsPet() && ((seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime())));
				IEnumerable<DataSentinel> source13 = StaticData.dataHandler.sentinels.Values.Where((DataSentinel p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				IEnumerable<DataSentinelWeapons> source14 = StaticData.dataHandler.sentinelWeapons.Values.Where((DataSentinelWeapons p) => (seePrimes && p.IsPrime()) || (seeNonPrimes && !p.IsPrime()));
				IEnumerable<KeyValuePair<string, DataSkin>> source15 = StaticData.dataHandler.skins.Where((KeyValuePair<string, DataSkin> p) => seePrimes && p.Key == "/Lotus/Upgrades/Skins/Kubrows/Collars/PrimeKubrowCollarA");
				list.AddRange(source12.Select((DataPet p) => new FoundryItemData(p)).ToList());
				list.AddRange(source13.Select((DataSentinel p) => new FoundryItemData(p)).ToList());
				list.AddRange(source14.Select((DataSentinelWeapons p) => new FoundryItemData(p)).ToList());
				list.AddRange(source15.Select((KeyValuePair<string, DataSkin> p) => new FoundryItemData(p.Value)).ToList());
			}
			return (from p in list
				where !StaticData.HideFoundersPackItemsEnabled || !p.IsPartOfFoundersPack()
				where ShouldShowFoundryItemBasedOnFilters(p, yesNoFilters, searchString, primeResurgenceData)
				orderby p.name
				select p).Take(showAll ? 10000000 : 100);
		}

		public void getAnalyticsName(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (FoundryHelper.lastFetchedUsername != "" && FoundryHelper.lastFetchedUsername != null && FoundryHelper.lastFetchedUsername != "AlecaFrame")
					{
						callback(true, FoundryHelper.lastFetchedUsername);
					}
					else
					{
						callback(false, "NOT READY");
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on getAnalyticsName: " + ex.Message);
					callback(false, "ERROR");
				}
			});
		}

		public void GetWFMarketStatus(Action<object, object, object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(isWFLoggedIn, wfmarketInErrorState, wfMarketMyStatus, (lastWFMarketProfileData?.data?.unread_messages).GetValueOrDefault());
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on getWFMarketStatus: " + ex.Message);
					callback(false, true, wfMarketMyStatus, 0);
				}
			});
		}

		public bool IsWarframeMarketSlow()
		{
			return wfmarketTimeouts > 2;
		}

		public void statsExport(string fileExtension, string jsonData, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (fileExtension == "json")
					{
						File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\alecaframeStatsExport.json", jsonData);
					}
					else if (fileExtension == "csv")
					{
						PlayerStatsData playerStatsData = JsonConvert.DeserializeObject<PlayerStatsData>(jsonData);
						using (StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\alecaframeStatsExport_DATA.csv"))
						{
							streamWriter.WriteLine("Timestamp,Platinum,Ducats,MR,Credits,Endo,Relics opened,% prime completion");
							foreach (PlayerStatsDataPoint generalDataPoint in playerStatsData.generalDataPoints)
							{
								streamWriter.WriteLine($"{generalDataPoint.ts},{generalDataPoint.plat},{generalDataPoint.ducats},{generalDataPoint.mr},{generalDataPoint.credits},{generalDataPoint.endo},{generalDataPoint.relicOpened},{generalDataPoint.percentageCompletion}");
							}
						}
						using StreamWriter streamWriter2 = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\alecaframeStatsExport_TRADES.csv");
						streamWriter2.WriteLine("Timestamp,User,Type,TotalPlat,TX(Item1;Item2;... {Item=(internalName|name|amount)}),RX(Item1;Item2;... {Item=(internalName|name|amount)})");
						foreach (PlayerStatsTrade trade in playerStatsData.trades)
						{
							streamWriter2.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", trade.ts, trade.user, trade.type, trade.totalPlat, string.Join(";", trade.tx.Select((PlayerStatsTradeTradedObjectInfo p) => p.name + "|" + p.displayName + "|" + p.cnt)), string.Join(";", trade.rx.Select((PlayerStatsTradeTradedObjectInfo p) => p.name + "|" + p.displayName + "|" + p.cnt))));
						}
					}
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on statsExport: " + ex.Message);
					callback(false, ex.Message);
				}
			});
		}

		public void SetNewRelicScreenshot(string path, bool debugMode, Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					try
					{
						if (lastWarframeScreenshot != null)
						{
							lastWarframeScreenshot.Dispose();
							lastWarframeScreenshot = null;
						}
					}
					catch
					{
					}
					if (debugMode)
					{
						lastWarframeScreenshot = new Bitmap("E:\\NAS\\Programacion\\AlecaFrame\\OverwolfApp\\Dist\\web\\assets\\reliq\\test3.png");
					}
					else
					{
						lastWarframeScreenshot = new Bitmap(path);
					}
					newRelicScreenshotReady.Set();
					callback(true);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on SetNewRelicScreenshot: " + ex.Message);
					callback(false);
				}
				finally
				{
					try
					{
						string[] files = Directory.GetFiles(new FileInfo(path).Directory.FullName);
						foreach (string path2 in files)
						{
							try
							{
								File.Delete(path2);
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
			});
		}

		public void GetRelicWindowData(bool getOldValues, Action<object, object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (getOldValues || newRelicDataOrErrorReady.WaitOne(TimeSpan.FromSeconds(8.0)) || isNewRelicDataReady)
					{
						isNewRelicDataReady = false;
						string arg = "15";
						if (lastRelicStartTime != DateTime.MinValue && DateTime.UtcNow > lastRelicStartTime)
						{
							arg = (14.5 - (double)(int)(DateTime.UtcNow - lastRelicStartTime).TotalSeconds).ToString();
						}
						else
						{
							Console.WriteLine("Weird relic start time. Not doing anything");
						}
						if (!newRelicDataErrorOccurred)
						{
							callback(true, JsonConvert.SerializeObject(newRelicData), arg);
						}
						else
						{
							callback(false, newRelicDataError, arg);
						}
					}
					else
					{
						callback(false, "Couldn't get relic data in time", "10");
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on GetRelicWindowData: " + ex.Message);
					callback(false, "An unknown error has happened", "10");
				}
			});
		}

		public void DoWFMarketLogout(Action<object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					lastWFMarketToken = "";
					File.WriteAllText(StaticData.saveFolder + "WFMarketToken.tk", "");
					try
					{
						newWFMarketWS?.Close();
					}
					catch
					{
					}
					checkWFMarketStatusAndOrders.Set();
					callback(true);
					lastTimeTokenUpdated = DateTime.MinValue;
					this.onWFMarketLoginStatusChanged("Hi!");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on DoWFMarketLogout: " + ex.Message);
					callback(false);
				}
			});
		}

		public void SetSettingsGeneric(bool takeModRankIntoAccountWFM, bool statsTabEnabled, bool notificationSoundsEnabled, bool includeFormaLevelMasteryHelper, bool enableRivenOverlay)
		{
			StaticData.WFMTakeModRankIntoAccount = takeModRankIntoAccountWFM;
			StaticData.statsTabEnabled = statsTabEnabled;
			StaticData.notificationSoundsEnabled = notificationSoundsEnabled;
			StaticData.includeFormaLevelMasteryHelper = includeFormaLevelMasteryHelper;
			StaticData.enableRivenOverlay = enableRivenOverlay;
		}

		public bool IsTradeHistoryEnabled()
		{
			return Misc.GetWarframeLanguage(defaultToEnglish: false) == Misc.WarframeLanguage.English;
		}

		public void getFoundryPlayerStats(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					FoundryPlayerStatsResponse playerStats = FoundryHelper.GetPlayerStats();
					if (playerStats != null)
					{
						callback(true, JsonConvert.SerializeObject(playerStats));
					}
					else
					{
						callback(false, "No inventory data available to load player stats!");
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when getting foundry player stats items: " + ex);
					callback(false, "");
				}
			});
		}

		public void getFoundryWorldStats(bool includeDetailed, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					FoundryWorldStatusResponse worldStats = FoundryHelper.GetWorldStats(includeDetailed);
					callback(true, JsonConvert.SerializeObject(worldStats));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when getting foundry world stats items: " + ex);
					callback(false, "");
				}
			});
		}

		public bool IsOrderPlaced(string realWorldItemName)
		{
			if (WFMarketOrders == null)
			{
				return false;
			}
			try
			{
				string wfmName = Misc.GetWarframeMarketURLName(realWorldItemName.Replace(" Blueprint", ""));
				return WFMarketOrders.data.Any((WFMarketProfileOrder p) => p.GetItemDataObject()?.slug.Replace("_blueprint", "") == wfmName);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void getFilteredInventory(string filtersJSON, bool showAll, Action<object, object, object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary.ContainsKey("yesnoFilters") ? dictionary["yesnoFilters"] : "{}");
					string arg = "[]";
					string partNameFilter = (dictionary.ContainsKey("search") ? dictionary["search"] : "");
					int totalDucats = 0;
					int totalPlat = 0;
					string text = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
					bool flag = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
					if (text == "name")
					{
						flag = !flag;
					}
					if (StaticData.dataHandler.warframeRootObject == null)
					{
						arg = "[]";
					}
					else
					{
						switch (dictionary["type"])
						{
						case "allParts":
						{
							IEnumerable<InventoryItemData> enumerable3 = InventoryHelpers.GetInventoryAllParts(yesNoFilters, partNameFilter, out totalDucats, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable3 = enumerable3.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable3);
							break;
						}
						case "warframeParts":
						{
							IEnumerable<WarframePartsItemData> enumerable5 = InventoryHelpers.GetInventoryWarframeParts(yesNoFilters, partNameFilter, out totalDucats, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable5 = enumerable5.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable5);
							break;
						}
						case "weaponParts":
						{
							IEnumerable<WeaponPartsItemData> enumerable8 = InventoryHelpers.GetInventoryWeaponParts(yesNoFilters, partNameFilter, out totalDucats, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable8 = enumerable8.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable8);
							break;
						}
						case "relics":
						{
							IEnumerable<RelicsItemData> enumerable6 = InventoryHelpers.GetInventoryRelics(yesNoFilters, partNameFilter, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable6 = enumerable6.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable6);
							break;
						}
						case "mods":
						{
							bool showOnlyOwned = !dictionary.ContainsKey("onlyOwned") || bool.Parse(dictionary["onlyOwned"]);
							IEnumerable<ModsItemData> enumerable2 = InventoryHelpers.GetInventoryMods(yesNoFilters, partNameFilter, out totalPlat, text, flag, showOnlyOwned);
							if (!showAll)
							{
								enumerable2 = enumerable2.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable2);
							break;
						}
						case "arcanes":
						{
							IEnumerable<ModsItemData> enumerable7 = InventoryHelpers.GetInventoryArcanes(yesNoFilters, partNameFilter, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable7 = enumerable7.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable7);
							break;
						}
						case "misc":
						{
							IEnumerable<MiscItemData> enumerable4 = InventoryHelpers.GetInventoryMisc(partNameFilter, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable4 = enumerable4.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable4);
							break;
						}
						case "sets":
						{
							IEnumerable<SetItemData> enumerable = InventoryHelpers.GetInventorySets(yesNoFilters, partNameFilter, out totalDucats, out totalPlat, text, flag);
							if (!showAll)
							{
								enumerable = enumerable.Take(100);
							}
							arg = JsonConvert.SerializeObject(enumerable);
							break;
						}
						}
					}
					callback(true, arg, totalDucats.ToString(), totalPlat.ToString());
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering inventory items: " + ex.Message);
					callback(false, "", "-", "0");
				}
			});
		}

		public void getFoundryComponentTooltip(string componentUniqueID, string componentName, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(FoundryHelper.GetPlayerRelicsToGetItem(componentUniqueID, componentName, showAll: false)));
				}
				catch (Exception ex)
				{
					callback(false, ex.Message);
				}
			});
		}

		public void getFilteredRelicPlanner(string filtersJSON, bool forcingReload, bool onlyOwned, bool showAll, bool compactView, Action<object, object, object> callback, bool fromRelicRecommendations = false)
		{
			Task.Run(delegate
			{
				Action<object, object, object> action = (fromRelicRecommendations ? this.onRelicRecommendationUpdate : this.onRelicPlannerUpdate);
				try
				{
					bool flag = false;
					if (StaticData.dataHandler.warframeRootObject == null || StaticData.dataHandler.misc == null || StaticData.dataHandler.misc.Count == 0)
					{
						action?.Invoke("noItems", "", "");
					}
					else
					{
						lock (this.cachedRelicPlannerData)
						{
							if (!forcingReload && this.cachedRelicPlannerData.Count == 0)
							{
								if (this.cachedRelicPlannerData.Count == 0)
								{
									action?.Invoke("updateNeeded", "", "");
									return;
								}
								flag = true;
							}
						}
						if (!onlyOwned && !relicPlannerLoadedWithAllRelics)
						{
							forcingReload = true;
							relicPlannerLoadedWithAllRelics = true;
						}
						Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
						Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>((dictionary != null && dictionary.ContainsKey("yesnoFilters")) ? dictionary["yesnoFilters"] : "{}");
						string searchFilter = (dictionary.ContainsKey("search") ? dictionary["search"] : "");
						searchFilter = searchFilter.ToLower();
						string orderingType = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
						bool orderedFromLargerToSmaller = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
						int result = 4;
						if (dictionary.ContainsKey("squadSize"))
						{
							int.TryParse(dictionary["squadSize"], out result);
						}
						if (orderingType == "name")
						{
							orderedFromLargerToSmaller = !orderedFromLargerToSmaller;
						}
						if (forcingReload)
						{
							ReloadRelicCache(fromRelicRecommendations, !onlyOwned);
						}
						IEnumerable<RelicsItemData> enumerable = new List<RelicsItemData>();
						Dictionary<string, RelicsItemData> dictionary2 = (from p in StaticData.dataHandler.warframeRootObject.MiscItems
							where p.IsRelic()
							select new RelicsItemData(p) into p
							where p.tradeable && !p.errorOccurred && !p.name.Contains("Requiem")
							select p).ToDictionary((RelicsItemData p) => p.internalName);
						if (!onlyOwned)
						{
							foreach (KeyValuePair<string, DataRelic> relic in StaticData.dataHandler.relics)
							{
								if (!dictionary2.ContainsKey(relic.Key))
								{
									RelicsItemData relicsItemData = new RelicsItemData(new Miscitem
									{
										ItemType = relic.Key,
										ItemCount = 0
									});
									if (relicsItemData.tradeable && !relicsItemData.errorOccurred && !relicsItemData.name.Contains("Requiem"))
									{
										dictionary2.Add(relic.Key, relicsItemData);
									}
								}
							}
						}
						enumerable = dictionary2.Values.ToList();
						switch (dictionary["type"])
						{
						case "lith":
							enumerable = enumerable.Where((RelicsItemData p) => p.name.ToLower().Contains("lith"));
							break;
						case "meso":
							enumerable = enumerable.Where((RelicsItemData p) => p.name.ToLower().Contains("meso"));
							break;
						case "neo":
							enumerable = enumerable.Where((RelicsItemData p) => p.name.ToLower().Contains("neo"));
							break;
						case "axi":
							enumerable = enumerable.Where((RelicsItemData p) => p.name.ToLower().Contains("axi"));
							break;
						}
						List<RelicPlannerSingleResponse> list = new List<RelicPlannerSingleResponse>();
						foreach (RelicsItemData item in enumerable)
						{
							RelicPlannerSingleResponse relicPlannerSingleResponse = new RelicPlannerSingleResponse();
							if (!this.cachedRelicPlannerData.ContainsKey(item.internalName))
							{
								CachedRelicPlannerData cachedRelicPlannerData = new CachedRelicPlannerData();
								cachedRelicPlannerData.data = new RelicDetailsResponse(item.internalName);
								cachedRelicPlannerData.data.FillPriceData();
								if (!this.cachedRelicPlannerData.ContainsKey(item.internalName))
								{
									this.cachedRelicPlannerData.Add(item.internalName, cachedRelicPlannerData);
								}
							}
							RelicDetailsResponse data = this.cachedRelicPlannerData[item.internalName].data;
							relicPlannerSingleResponse.normalData = item;
							relicPlannerSingleResponse.custom = new RelicPlannerCustomDetails();
							DataRelic.RelicRarities thisRelicLevel = (DataRelic.RelicRarities)Enum.Parse(typeof(DataRelic.RelicRarities), relicPlannerSingleResponse.normalData.name.Split(' ').Last().Trim());
							if (yesNoFilters.ContainsKey("asRadiant") && yesNoFilters["asRadiant"] == "yes")
							{
								thisRelicLevel = DataRelic.RelicRarities.Radiant;
								relicPlannerSingleResponse.normalData.name = relicPlannerSingleResponse.normalData.name.Replace("Intact", "Radiant");
								relicPlannerSingleResponse.normalData.name = relicPlannerSingleResponse.normalData.name.Replace("Exceptional", "Radiant");
								relicPlannerSingleResponse.normalData.name = relicPlannerSingleResponse.normalData.name.Replace("Flawless", "Radiant");
							}
							relicPlannerSingleResponse.custom.plat = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == thisRelicLevel.ToString()).bySquad[result].plat;
							relicPlannerSingleResponse.custom.expectedPlat = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == thisRelicLevel.ToString()).bySquad[result].expectedPlat;
							relicPlannerSingleResponse.custom.ducats = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == thisRelicLevel.ToString()).bySquad[result].ducats;
							relicPlannerSingleResponse.custom.expectedDucats = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == thisRelicLevel.ToString()).bySquad[result].expectedDucats;
							relicPlannerSingleResponse.custom.rewards = new List<FoundryItemComponent>();
							relicPlannerSingleResponse.custom.tier = thisRelicLevel.ToString();
							relicPlannerSingleResponse.custom.bestForPlat = data.levels.OrderByDescending((RelicDetailsResponse.RelicDataLevel p) => p.bySquad[4].plat).FirstOrDefault()?.rarity;
							relicPlannerSingleResponse.custom.bestForDucats = data.levels.OrderByDescending((RelicDetailsResponse.RelicDataLevel p) => p.bySquad[4].plat).FirstOrDefault()?.rarity;
							RelicDetailsResponse.RelicDataLevelSquad relicDataLevelSquad = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == thisRelicLevel.ToString()).bySquad[result];
							RelicDetailsResponse.RelicDataLevelSquad relicDataLevelSquad2 = data.levels.First((RelicDetailsResponse.RelicDataLevel p) => p.rarity == "Radiant").bySquad[result];
							float num = relicDataLevelSquad2.relicRefinementTierReference.GetRefinementCostUpToThisRarity() - relicDataLevelSquad.relicRefinementTierReference.GetRefinementCostUpToThisRarity();
							if (num <= 0f)
							{
								num = 1E+09f;
							}
							relicPlannerSingleResponse.custom.intact2radDucatDiff = (relicDataLevelSquad2.ducats - relicDataLevelSquad.ducats) / num;
							relicPlannerSingleResponse.custom.intact2radPlatDiff = (relicDataLevelSquad2.plat - relicDataLevelSquad.plat) / num;
							IEnumerable<DataRelic.RelicDropData.RelicDropDataWithRarity> enumerable2 = StaticData.dataHandler.relics[item.internalName].relicRewards.GetOrDefault(thisRelicLevel)?.chance?.OrderBy((DataRelic.RelicDropData.RelicDropDataWithRarity p) => p.rarity);
							foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item2 in enumerable2 ?? Enumerable.Empty<DataRelic.RelicDropData.RelicDropDataWithRarity>())
							{
								relicPlannerSingleResponse.custom.rewards.Add(new FoundryItemComponent(item2.item));
							}
							relicPlannerSingleResponse.custom.isFav = FavouriteHelper.IsFavourite(relicPlannerSingleResponse.normalData.internalName) || FavouriteHelper.IsFavourite(relicPlannerSingleResponse.normalData.name.Substring(0, relicPlannerSingleResponse.normalData.name.LastIndexOf(" "))) || relicPlannerSingleResponse.custom.rewards.Any((FoundryItemComponent p) => p.isFav);
							list.Add(relicPlannerSingleResponse);
						}
						if (compactView)
						{
							IEnumerable<IGrouping<string, RelicPlannerSingleResponse>> enumerable3 = from p in list
								group p by p.normalData.name.Substring(0, p.normalData.name.LastIndexOf(" "));
							List<RelicPlannerSingleResponse> list2 = new List<RelicPlannerSingleResponse>();
							foreach (IGrouping<string, RelicPlannerSingleResponse> item3 in enumerable3)
							{
								RelicPlannerSingleResponse relicPlannerSingleResponse2 = item3.First();
								relicPlannerSingleResponse2.custom.groupedDetails = new List<GroupedDetails>();
								foreach (RelicPlannerSingleResponse item4 in item3)
								{
									string text = item4.normalData.name.Split(' ').Last();
									if (text == "Exceptional")
									{
										text = "Excep.";
									}
									relicPlannerSingleResponse2.custom.groupedDetails.Add(new GroupedDetails
									{
										expectedDucats = item4.custom.expectedDucats,
										expectedPlat = item4.custom.expectedPlat,
										tier = text,
										num = item4.normalData.amountOwned
									});
								}
								relicPlannerSingleResponse2.custom.groupedDetails = relicPlannerSingleResponse2.custom.groupedDetails.OrderBy((GroupedDetails p) => (!(p.tier == "Intact")) ? p.tier : "aaIntact").ToList();
								relicPlannerSingleResponse2.custom.expectedDucats = null;
								relicPlannerSingleResponse2.custom.expectedPlat = null;
								relicPlannerSingleResponse2.custom.isFav = item3.Any((RelicPlannerSingleResponse p) => p.custom.isFav);
								relicPlannerSingleResponse2.normalData = relicPlannerSingleResponse2.normalData.ShallowClone();
								relicPlannerSingleResponse2.normalData.name = item3.Key;
								relicPlannerSingleResponse2.normalData.amountOwned = item3.Sum((RelicPlannerSingleResponse p) => p.normalData.amountOwned);
								list2.Add(relicPlannerSingleResponse2);
								list = list2;
							}
						}
						IEnumerable<RelicPlannerSingleResponse> source = list.Where((RelicPlannerSingleResponse p) => ShouldShowRelicPlannerItemBasedOnFilters(p, yesNoFilters, searchFilter));
						bool num2 = yesNoFilters.ContainsKey("fav") && yesNoFilters["fav"] == "order";
						source = ((!orderedFromLargerToSmaller) ? (from p in source
							orderby OrderRelicPlannerBasedOnCriteria(p, orderingType, compactView, orderedFromLargerToSmaller), p.normalData.name
							select p) : (from p in source
							orderby OrderRelicPlannerBasedOnCriteria(p, orderingType, compactView, orderedFromLargerToSmaller) descending, p.normalData.name
							select p));
						if (num2)
						{
							source = source.OrderByDescending((RelicPlannerSingleResponse p) => p.custom.rewards.Count((FoundryItemComponent u) => u.isFav));
						}
						if (!showAll)
						{
							source = source.Take(50);
						}
						action?.Invoke("data", JsonConvert.SerializeObject(source), flag);
						int valueOrDefault = (StaticData.dataHandler?.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == "/Lotus/Types/Items/MiscItems/VoidTearDrop")?.ItemCount).GetValueOrDefault();
						action?.Invoke("traces", valueOrDefault.ToString(), flag);
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering the relic planner: " + ex.Message);
					action?.Invoke("error", ex.Message, "");
				}
			});
		}

		public void SetRelicRecommendationFilters(string filters)
		{
			StaticData.relicRecommendationOverlayFilters = filters;
			StaticData.Log(LogType.INFO, "Filters set: " + filters);
		}

		public void DoNotificationTest()
		{
			Task.Run(delegate
			{
				OCRHelper.SendNewInGameConversationNotification("TestPlayerXXX");
			});
		}

		public void ReloadRelicCache(bool fromRelicRecommendations, bool loadNotOwnedToo = false)
		{
			Action<object, object, object> action = (fromRelicRecommendations ? this.onRelicRecommendationUpdate : this.onRelicPlannerUpdate);
			if (!Monitor.TryEnter(this.cachedRelicPlannerData, 45000))
			{
				return;
			}
			try
			{
				this.cachedRelicPlannerData.Clear();
				List<RelicsItemData> list = new List<RelicsItemData>();
				list = ((!loadNotOwnedToo) ? (from p in StaticData.dataHandler.warframeRootObject.MiscItems
					where p.IsRelic()
					select new RelicsItemData(p) into p
					where p.tradeable && !p.errorOccurred && !p.name.Contains("Requiem")
					select p).ToList() : (from p in StaticData.dataHandler.relics
					select new RelicsItemData(new Miscitem
					{
						ItemType = p.Key
					}) into p
					where p.tradeable && !p.errorOccurred && !p.name.Contains("Requiem")
					select p).ToList());
				HashSet<string> hashSet = new HashSet<string>();
				for (int num = 0; num < list.Count; num++)
				{
					IEnumerable<DataRelic.RelicDropData.RelicDropDataWithRarity> enumerable = StaticData.dataHandler.relics[list[num].internalName].relicRewards.GetOrDefault(DataRelic.RelicRarities.Intact)?.chance;
					foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item in enumerable ?? Enumerable.Empty<DataRelic.RelicDropData.RelicDropDataWithRarity>())
					{
						hashSet.Add(item.item.GetRealExternalName());
					}
				}
				action?.Invoke("progress", 0, "Downloading item prices (1/4)...");
				List<string> list2 = hashSet.ToList();
				for (int num2 = 0; num2 < 4; num2++)
				{
					if (num2 < 3)
					{
						IEnumerable<string> source = list2.Take(list2.Count / 4);
						list2 = list2.Skip(list2.Count / 4).ToList();
						SYNC_GetHugePriceList(source.ToArray(), TimeSpan.FromSeconds(25.0));
					}
					else
					{
						SYNC_GetHugePriceList(list2.ToArray(), TimeSpan.FromSeconds(25.0));
					}
					action?.Invoke("progress", (num2 + 2) * 3, "Downloading item prices (" + (num2 + 1) + "/4)...");
				}
				action?.Invoke("progress", 10, "Loading relics...");
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					CachedRelicPlannerData cachedRelicPlannerData = new CachedRelicPlannerData();
					cachedRelicPlannerData.data = new RelicDetailsResponse(list[num3].internalName);
					cachedRelicPlannerData.data.FillPriceData();
					if (!this.cachedRelicPlannerData.ContainsKey(list[num3].internalName))
					{
						this.cachedRelicPlannerData.Add(list[num3].internalName, cachedRelicPlannerData);
					}
					action?.Invoke("progress", 10.0 + Math.Round(90f * ((float)num3 / (float)list.Count)), "Loading relic " + num3 + "/" + list.Count + "...");
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				if (Monitor.IsEntered(this.cachedRelicPlannerData))
				{
					Monitor.Exit(this.cachedRelicPlannerData);
				}
			}
		}

		public void ConversationNotificationsChanged(bool windowsNotificationEnabled, bool discordNotificationEnabled, string discordNotificationWebhook, bool notificationsOnlyBackground, string discordNotificationTemplate, string minutesAhead, string wfmarketDiscordTemplate)
		{
			StaticData.windowsNotificationsEnabled = windowsNotificationEnabled;
			StaticData.discordNotificationsEnabled = discordNotificationEnabled;
			StaticData.discordNotificationWebhook = discordNotificationWebhook;
			StaticData.notificationOnlyBackground = notificationsOnlyBackground;
			StaticData.discordNotificationTemplate = discordNotificationTemplate;
			StaticData.discordWarframeMarketNotificationTemplate = wfmarketDiscordTemplate;
			int result = 3;
			int.TryParse(minutesAhead, out result);
			StaticData.timeAheadTimerNotifications = TimeSpan.FromMinutes(result);
		}

		public void GetRivenMarketImportString(string rivenUID, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string rivenMarketImportString = RivenExplorerHelper.GetRivenMarketImportString(rivenUID);
					callback(true, rivenMarketImportString);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to create Riven.Market import string: " + ex);
					callback(false, "Failed to create import string: " + ex.Message);
				}
			});
		}

		private static object OrderRelicPlannerBasedOnCriteria(RelicPlannerSingleResponse arg, string orderingMode, bool compactView, bool orderedFromLargerToSmaller)
		{
			switch (orderingMode)
			{
			case "plat":
				if (compactView)
				{
					if (orderedFromLargerToSmaller)
					{
						return arg.custom?.groupedDetails?.Max((GroupedDetails p) => p?.expectedPlat.ParseOrDefault());
					}
					return arg.custom?.groupedDetails?.Min((GroupedDetails p) => p?.expectedPlat.ParseOrDefault());
				}
				return arg.custom.plat;
			case "amount":
				if (compactView)
				{
					return (arg.custom?.groupedDetails?.Sum((GroupedDetails p) => p.num)).GetValueOrDefault();
				}
				return arg.normalData.amountOwned;
			case "ducats":
				if (compactView)
				{
					if (orderedFromLargerToSmaller)
					{
						return arg.custom?.groupedDetails?.Max((GroupedDetails p) => p?.expectedDucats.ParseOrDefault());
					}
					return arg.custom?.groupedDetails?.Min((GroupedDetails p) => p?.expectedDucats.ParseOrDefault());
				}
				return arg.custom.ducats;
			case "partsOwned":
				return arg.custom.rewards.Count((FoundryItemComponent p) => p.recipeNeccessaryComponents && !p.name.Contains("Forma"));
			case "masteredOwned":
				return arg.custom.rewards.Count((FoundryItemComponent p) => p.parentOwned && !p.name.Contains("Forma"));
			case "missingItems":
				return arg.custom.rewards.Count((FoundryItemComponent p) => !p.recipeNeccessaryComponents && !p.parentOwned && !p.name.Contains("Forma"));
			case "improvementDucat":
				return arg.custom.intact2radDucatDiff;
			case "improvementPlat":
				return arg.custom.intact2radPlatDiff;
			default:
				return arg.normalData.name;
			}
		}

		private bool ShouldShowRelicPlannerItemBasedOnFilters(RelicPlannerSingleResponse relic, Dictionary<string, string> yesNoFilters, string searchText)
		{
			if (yesNoFilters.ContainsKey("vaulted"))
			{
				if (yesNoFilters["vaulted"] == "yes" && !relic.normalData.vaulted)
				{
					return false;
				}
				if (yesNoFilters["vaulted"] == "no" && relic.normalData.vaulted)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("allRewardsOwned"))
			{
				if (yesNoFilters["allRewardsOwned"] == "yes" && !relic.custom.rewards.All((FoundryItemComponent p) => p.recipeNeccessaryComponents || p.name.Contains("Forma")))
				{
					return false;
				}
				if (yesNoFilters["allRewardsOwned"] == "no" && relic.custom.rewards.All((FoundryItemComponent p) => p.recipeNeccessaryComponents || p.name.Contains("Forma")))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("moreThan10copies"))
			{
				if (yesNoFilters["moreThan10copies"] == "yes" && relic.normalData.amountOwned < 10)
				{
					return false;
				}
				if (yesNoFilters["moreThan10copies"] == "no" && relic.normalData.amountOwned >= 10)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("allItemsOwned"))
			{
				if (yesNoFilters["allItemsOwned"] == "yes" && !relic.custom.rewards.All((FoundryItemComponent p) => p.parentOwned || p.name.Contains("Forma")))
				{
					return false;
				}
				if (yesNoFilters["allItemsOwned"] == "no" && relic.custom.rewards.All((FoundryItemComponent p) => p.parentOwned || p.name.Contains("Forma")))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("relicTier") && !string.IsNullOrEmpty(yesNoFilters["relicTier"]))
			{
				if (relic.custom.groupedDetails == null)
				{
					if (relic.custom.tier.ToLower() != yesNoFilters["relicTier"].ToLower())
					{
						return false;
					}
				}
				else if (!relic.custom.groupedDetails.Any((GroupedDetails p) => p.tier.ToLower() == yesNoFilters["relicTier"].ToLower()))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("fav"))
			{
				if (yesNoFilters["fav"] == "yes" && !relic.custom.rewards.Any((FoundryItemComponent p) => p.isFav) && !relic.normalData.isFav && !relic.custom.isFav)
				{
					return false;
				}
				if (yesNoFilters["fav"] == "no" && (relic.custom.rewards.Any((FoundryItemComponent p) => p.isFav) || relic.normalData.isFav || relic.custom.isFav))
				{
					return false;
				}
			}
			if (string.IsNullOrEmpty(searchText))
			{
				return true;
			}
			bool flag = false;
			foreach (FoundryItemComponent reward in relic.custom.rewards)
			{
				if (reward.name.ToLower().Contains(searchText.ToLower()))
				{
					flag = true;
					reward.recipeHighlightedComponent = true;
				}
			}
			bool flag2 = relic.normalData.name.ToLower().Contains(searchText);
			if (!flag && !flag2)
			{
				return false;
			}
			return true;
		}

		private static bool ShouldShowFoundryItemBasedOnFilters(FoundryItemData item, Dictionary<string, string> yesNoFilters, string partNameFilter, WorldStatePrimeResurgenceData primeResurgenceData)
		{
			if (yesNoFilters.ContainsKey("mastered"))
			{
				if (item.name.ToLower().Contains("kavasa prime"))
				{
					return false;
				}
				if (yesNoFilters["mastered"] == "yes" && !item.mastered)
				{
					return false;
				}
				if (yesNoFilters["mastered"] == "no" && item.mastered)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("vaulted"))
			{
				if (yesNoFilters["vaulted"] == "yes" && !item.vaulted)
				{
					return false;
				}
				if (yesNoFilters["vaulted"] == "no")
				{
					if (!item.vaulted)
					{
						List<FoundryItemComponent> components = item.components;
						if (components != null && components.Count != 0)
						{
							goto IL_00fd;
						}
					}
					return false;
				}
			}
			goto IL_00fd;
			IL_00fd:
			if (yesNoFilters.ContainsKey("owned"))
			{
				if (yesNoFilters["owned"] == "yes" && !item.owned)
				{
					return false;
				}
				if (yesNoFilters["owned"] == "no" && item.owned)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("ready2Build"))
			{
				if (yesNoFilters["ready2Build"] == "yes")
				{
					List<FoundryItemComponent> components2 = item.components;
					if (components2 == null || !components2.All((FoundryItemComponent p) => p.recipeNeccessaryComponents))
					{
						return false;
					}
				}
				if (yesNoFilters["ready2Build"] == "no")
				{
					List<FoundryItemComponent> components3 = item.components;
					if (components3 != null && components3.All((FoundryItemComponent p) => p.recipeNeccessaryComponents))
					{
						return false;
					}
				}
			}
			if (yesNoFilters.ContainsKey("enoughMastery"))
			{
				if (yesNoFilters["enoughMastery"] == "yes" && FoundryHelper.lastMasteryLevel < item.minimumMastery)
				{
					return false;
				}
				if (yesNoFilters["enoughMastery"] == "no" && FoundryHelper.lastMasteryLevel >= item.minimumMastery)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("primeResurgence"))
			{
				if (yesNoFilters["primeResurgence"] == "yes" && !primeResurgenceData.items.Any((FoundryItemData p) => p.internalName == item.internalName))
				{
					return false;
				}
				if (yesNoFilters["primeResurgence"] == "no" && primeResurgenceData.items.Any((FoundryItemData p) => p.internalName == item.internalName))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("favorite"))
			{
				if (yesNoFilters["favorite"] == "yes" && !item.isFav)
				{
					List<FoundryItemComponent> components4 = item.components;
					if (components4 == null || !components4.Any((FoundryItemComponent p) => p.isFav))
					{
						return false;
					}
				}
				if (yesNoFilters["favorite"] == "no")
				{
					if (!item.isFav)
					{
						List<FoundryItemComponent> components5 = item.components;
						if (components5 == null || !components5.Any((FoundryItemComponent p) => p.isFav))
						{
							goto IL_03a8;
						}
					}
					return false;
				}
			}
			goto IL_03a8;
			IL_03a8:
			if (yesNoFilters.ContainsKey("usedCrafting"))
			{
				if (yesNoFilters["usedCrafting"] == "yes" && !item.neededForOtherStuff)
				{
					return false;
				}
				if (yesNoFilters["usedCrafting"] == "no" && item.neededForOtherStuff)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("helminth"))
			{
				if (!item.supportsHelminth)
				{
					return false;
				}
				if (yesNoFilters["helminth"] == "yes" && !item.helminthDone)
				{
					return false;
				}
				if (yesNoFilters["helminth"] == "no" && item.helminthDone)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("archonShards"))
			{
				if (yesNoFilters["archonShards"] == "yes" && (item.archonShards?.Count ?? 0) <= 0)
				{
					return false;
				}
				if (yesNoFilters["archonShards"] == "no" && (item.archonShards?.Count ?? 0) > 0)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("incarnon"))
			{
				if (yesNoFilters["incarnon"] == "yes" && !item.incarnion)
				{
					return false;
				}
				if (yesNoFilters["incarnon"] == "no" && item.incarnion)
				{
					return false;
				}
			}
			if (partNameFilter != "")
			{
				if (item?.itemReference.type?.ToLower() == partNameFilter.ToLower())
				{
					return true;
				}
				if (!item.name.ToLower().Contains(partNameFilter.ToLower()))
				{
					List<FoundryItemComponent> components6 = item.components;
					if (components6 == null || !components6.Any((FoundryItemComponent p) => p.name.ToLower().Contains(partNameFilter.ToLower())))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void DOFrelicOverlayBypassChanged(bool bypassEnabled, Action<object> callback)
		{
			Task.Run(delegate
			{
				StaticData.DOFbypassEnabled = bypassEnabled;
				callback("Hi!");
			});
		}

		public void HideFoundersProgramChanged(bool hideFoundersEnabled, Action<object> callback)
		{
			Task.Run(delegate
			{
				StaticData.HideFoundersPackItemsEnabled = hideFoundersEnabled;
				FoundryHelper.lastTimeUnlockPercentUpdate = DateTime.MinValue;
				callback("Hi!");
			});
		}

		public void SendRelicRewardMetrics(string status, double time)
		{
			Task.Run(delegate
			{
				try
				{
					AnalyticsHandler.SendRelicReward(status, time, StaticData.lastRelicLogWorstDelta);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred in SendRelicRewardMetrics: " + ex.Message);
				}
			});
		}

		public void SendRelicRecommendationMetrics(string status, double time)
		{
			Task.Run(delegate
			{
				try
				{
					AnalyticsHandler.SendRelicRecommendation(status, time);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred in SendRelicRecommendationMetrics: " + ex.Message);
				}
			});
		}

		public void SelectedScalingChanged(string scalingMode, Action<object> callback)
		{
			Task.Run(delegate
			{
				if (scalingMode == "custom")
				{
					StaticData.WarframeScalingMode = ScalingMode.Custom;
				}
				else if (scalingMode == "legacy")
				{
					StaticData.WarframeScalingMode = ScalingMode.Legacy;
				}
				else
				{
					StaticData.WarframeScalingMode = ScalingMode.Full;
				}
				callback("Hi!");
			});
		}

		public void AutodetectWarframeScaling(bool automaticOnFirstInstall, Action<object, object, object> callback)
		{
			Task.Run(delegate
			{
				if (!automaticOnFirstInstall && Misc.IsWarframeRunning())
				{
					callback(false, "Please close Warframe before trying to autodetect scaling mode.", "");
				}
				else if (automaticOnFirstInstall && !StaticData.isFirstRunOnInstall)
				{
					callback(false, "AutodetectWarframeScaling called in an automatic way but this is not the first run after install!", "");
				}
				else
				{
					ScalingMode scalingMode = ScalingMode.Full;
					float scalePerOne = 1f;
					bool flag = Misc.TryGetScalingModeFromGameSettings(ref scalingMode, ref scalePerOne);
					Console.WriteLine($"AutodetectWarframeScaling went ok: {flag}. ScalingMode: {scalingMode}, scale: {scalePerOne}");
					if (flag)
					{
						StaticData.WarframeScalingMode = scalingMode;
						StaticData.customScale = scalePerOne;
						string arg = "full";
						if (StaticData.WarframeScalingMode == ScalingMode.Custom)
						{
							arg = "custom";
						}
						if (StaticData.WarframeScalingMode == ScalingMode.Legacy)
						{
							arg = "legacy";
						}
						callback(true, arg, Math.Round(scalePerOne * 100f));
					}
					else
					{
						callback(false, "", 1);
					}
				}
			});
		}

		public void ShowRelicOverlayChanged(bool overlayEnabled, Action<object> callback)
		{
			Task.Run(delegate
			{
				StaticData.RelicOverlayEnabled = overlayEnabled;
				callback("Hi!");
			});
		}

		public void ShowRelicRecommendationChanged(bool bypassEnabled, Action<object> callback)
		{
			Task.Run(delegate
			{
				StaticData.RelicRecommendationEnabled = bypassEnabled;
				callback("Hi!");
			});
		}

		public void getHugePriceList(string itemNameList, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string[] itemListNames = JsonConvert.DeserializeObject<string[]>(itemNameList.ToLower());
					ItemPriceSmallResponse[] value = SYNC_GetHugePriceList(itemListNames, TimeSpan.FromSeconds(25.0));
					callback(true, JsonConvert.SerializeObject(value));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when getting huge price list: " + ex.Message);
					callback(false, "");
				}
			});
		}

		public ItemPriceSmallResponse GetSYNCSingleItemPrice(string itemName)
		{
			return SYNC_GetHugePriceList(new string[1] { itemName }, TimeSpan.FromSeconds(20.0))[0];
		}

		public ItemPriceSmallResponse[] SYNC_GetHugePriceList(string[] itemListNames, TimeSpan timeout)
		{
			ItemPriceSmallResponse[] array = new ItemPriceSmallResponse[itemListNames.Length];
			for (int i = 0; i < itemListNames.Length; i++)
			{
				array[i] = PriceHelper.GetLazyItemPrice(itemListNames[i]);
			}
			PriceHelper.Flush(timeout);
			return array;
		}

		public void GetBuySellWindowData(string itemName, Action<object, object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient obj = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								HttpRequestHeader.Accept,
								"application/json"
							},
							{ "platform", "pc" },
							{ "crossplay", "true" }
						}
					};
					string warframeMarketURLName = Misc.GetWarframeMarketURLName(itemName);
					WFMarketOrdersResponse wFMarketOrdersResponse = JsonConvert.DeserializeObject<WFMarketOrdersResponse>(obj.DownloadString("https://api.warframe.market/v2/orders/item/" + warframeMarketURLName));
					IEnumerable<Order> source = wFMarketOrdersResponse.data.Where((Order p) => p.user.status == "ingame");
					IOrderedEnumerable<Order> source2 = from p in source
						where p.type == "sell" && p.user.locale == "en"
						orderby p.platinum
						select p;
					IOrderedEnumerable<Order> source3 = from p in source
						where p.type == "buy" && p.user.locale == "en"
						orderby p.platinum descending
						select p;
					WFMItemListItem wFMItemListItem = StaticData.LazyWfmItemData.Value.AsDictionaryBySlug[warframeMarketURLName];
					BuySellPanelResponse buySellPanelResponse = new BuySellPanelResponse
					{
						buyListings = source3.Select((Order p) => new BuySellPanelResponseItem(p)).ToList(),
						sellListings = source2.Select((Order p) => new BuySellPanelResponseItem(p)).ToList()
					};
					if (Misc.GetWarframeMarketURLName(itemName).ToLower().Contains("relic"))
					{
						buySellPanelResponse.postingSettings.isRelic = true;
						buySellPanelResponse.buyListings.ForEach(delegate(BuySellPanelResponseItem p)
						{
							p.specialValue = p.order.subtype.ToUpper().ToCharArray()[0].ToString();
						});
						buySellPanelResponse.sellListings.ForEach(delegate(BuySellPanelResponseItem p)
						{
							p.specialValue = p.order.subtype.ToUpper().ToCharArray()[0].ToString();
						});
					}
					else if (wFMarketOrdersResponse.data.Any((Order p) => p.rank != -69))
					{
						buySellPanelResponse.postingSettings.isMod = true;
						buySellPanelResponse.buyListings.ForEach(delegate(BuySellPanelResponseItem p)
						{
							p.specialValue = p.order.rank.ToString();
						});
						buySellPanelResponse.sellListings.ForEach(delegate(BuySellPanelResponseItem p)
						{
							p.specialValue = p.order.rank.ToString();
						});
					}
					else
					{
						string[] tags = wFMItemListItem.tags;
						if (tags != null && tags.Contains("fish"))
						{
							string[] subtypes = wFMItemListItem.subtypes;
							if (subtypes != null && subtypes.Contains("small"))
							{
								buySellPanelResponse.postingSettings.isFish = true;
							}
							else
							{
								buySellPanelResponse.postingSettings.isFish2 = true;
							}
							buySellPanelResponse.buyListings.ForEach(delegate(BuySellPanelResponseItem p)
							{
								p.specialValue = p.order.subtype.FirstCharToUpper();
							});
							buySellPanelResponse.sellListings.ForEach(delegate(BuySellPanelResponseItem p)
							{
								p.specialValue = p.order.subtype.FirstCharToUpper();
							});
						}
						else if (itemName.Contains("(veiled)"))
						{
							buySellPanelResponse.postingSettings.isVeiledRiven = true;
							buySellPanelResponse.buyListings.ForEach(delegate(BuySellPanelResponseItem p)
							{
								p.specialValue = p.order.subtype.FirstCharToUpper();
							});
							buySellPanelResponse.sellListings.ForEach(delegate(BuySellPanelResponseItem p)
							{
								p.specialValue = p.order.subtype.FirstCharToUpper();
							});
						}
						else
						{
							string[] tags2 = wFMItemListItem.tags;
							if (tags2 != null && tags2.Contains("ayatan_sculpture"))
							{
								buySellPanelResponse.postingSettings.isAyatan = true;
								buySellPanelResponse.buyListings.ForEach(delegate(BuySellPanelResponseItem p)
								{
									p.specialValue = $"{p.order.cyanStars} cyan, {p.order.amberStars} amber";
								});
								buySellPanelResponse.sellListings.ForEach(delegate(BuySellPanelResponseItem p)
								{
									p.specialValue = $"{p.order.cyanStars} cyan, {p.order.amberStars} amber";
								});
							}
						}
					}
					buySellPanelResponse.postingSettings.wfmItemName = wFMItemListItem.id;
					buySellPanelResponse.postingSettings.readableName = itemName;
					callback(true, JsonConvert.SerializeObject(buySellPanelResponse), JsonConvert.SerializeObject(wFMItemListItem?.subtypes ?? new string[0]));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when getting huge price list: " + ex.Message);
					callback(false, "", "");
				}
			});
		}

		public void DoDeltasTest()
		{
			Task.Run(delegate
			{
				InventoryDeltaHelper.RemoveDeltas();
			});
		}

		public void GetDetailedDeltas(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(InventoryDeltaHelper.GetDeltas(withPrices: true)));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get detailed deltas: " + ex);
					callback(false, "An unknown error has occurred");
				}
			});
		}

		public void GetWFMOrders(string filtersJSON, Action<object, object, object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					int num = 0;
					int num2 = 0;
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary.ContainsKey("yesnoFilters") ? dictionary["yesnoFilters"] : "{}");
					string text = "[]";
					string searchPrompt = (dictionary.ContainsKey("search") ? dictionary["search"] : "");
					string text2 = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
					bool flag = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
					if (text2 == "name")
					{
						flag = !flag;
					}
					List<WFMarketOrderData> currentOrders = WFMarketHelper.GetCurrentOrders(dictionary["type"], yesNoFilters, text2, flag, searchPrompt);
					num2 = (currentOrders?.Where((WFMarketOrderData p) => !p.isSellOrder)?.Sum((WFMarketOrderData p) => p.platinumPerItem * p.amountOnSale)).GetValueOrDefault();
					num = (currentOrders?.Where((WFMarketOrderData p) => p.isSellOrder)?.Sum((WFMarketOrderData p) => p.platinumPerItem * p.amountOnSale)).GetValueOrDefault();
					text = JsonConvert.SerializeObject(currentOrders);
					callback(true, text, num, num2);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on GetWFMOrders: " + ex);
					callback(false, "", 0, 0);
				}
			});
		}

		public void GetWFMContracts(string filtersJSON, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary.ContainsKey("yesnoFilters") ? dictionary["yesnoFilters"] : "{}");
					string text = "[]";
					string searchPrompt = (dictionary.ContainsKey("search") ? dictionary["search"] : "");
					string text2 = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
					bool flag = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
					if (text2 == "name")
					{
						flag = !flag;
					}
					text = JsonConvert.SerializeObject(WFMarketHelper.GetCurrentContracts(dictionary["type"], yesNoFilters, text2, flag, searchPrompt));
					callback(true, text);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred on GetWFMContracts: " + ex);
					callback(false, "");
				}
			});
		}

		public void GenerateDeltaEvent()
		{
			Task.Run(delegate
			{
				SendDeltaUpdate(InventoryDeltaHelper.GetDeltas(withPrices: false).items.Sum((DeltaResponseItem p) => p.baseData.amountOwned));
			});
		}

		public void OnFavouritesUpdateCaller()
		{
			this.OnFavouritesUpdate?.Invoke("Hi!");
		}

		public void ClearDeltas()
		{
			Task.Run(delegate
			{
				try
				{
					InventoryDeltaHelper.RemoveDeltas();
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to remove deltas: " + ex);
				}
			});
		}

		public void StatsUpdateNeededCaller()
		{
			this.onStatsUpdateNeeded?.Invoke("Hi!");
		}

		public void SetWarframeData(string warframeData, Action<object, object, object> callback)
		{
			Task.Run(delegate
			{
				if (warframeData == lastWarframeData)
				{
					return;
				}
				lastWarframeData = warframeData;
				try
				{
					if (warframeData == "|NOT_AVAILABLE|")
					{
						if (File.Exists(StaticData.saveFolder + "\\lastData.dat"))
						{
							if (StaticData.dataHandler.LoadWarframeData(Misc.ReadLastDataFile()))
							{
								onWarframeDataChangedCaller();
							}
							callback(true, true, 4);
						}
						else
						{
							callback(false, false, 3);
						}
						return;
					}
					try
					{
						if (!warframeData.Contains("LastInventorySync") || warframeData.Last() != '}')
						{
							throw new Exception("No/wrong inventory data detected!");
						}
						StaticData.dataHandler.LoadWarframeData(warframeData);
						Misc.WriteLastDataFile(warframeData);
						StaticData.Log(LogType.INFO, "[GOOD] Cached warframe data for future use");
						lastTimeGoodDataReceived = DateTime.UtcNow;
						callback(true, false, 1);
						lastDataUpdateTime = "Last update: " + DateTime.Now.ToShortTimeString();
						StatsHandler.NewDataJustReceived_BG();
						onWarframeDataChangedCaller();
						AnalyticsHandler.AddMetric("WFDataOK", "");
					}
					catch (Exception ex)
					{
						AnalyticsHandler.AddMetric("WFDataError", "");
						Misc.WriteAllTextEncrypted(StaticData.saveFolder + "\\lastDataERROR.dat", warframeData);
						StaticData.Log(LogType.INFO, "[BAD] Saved BAD warframe data for future use: " + ex);
						if (DateTime.UtcNow - lastTimeGoodDataReceived < TimeSpan.FromMinutes(45.0))
						{
							StaticData.Log(LogType.INFO, "OK data received recently, just ignoring this error");
							callback(true, false, 1);
						}
						else if (File.Exists(StaticData.saveFolder + "\\lastData.dat"))
						{
							if (StaticData.dataHandler.LoadWarframeData(Misc.ReadLastDataFile()))
							{
								onWarframeDataChangedCaller();
							}
							callback(true, true, 5);
						}
						else
						{
							callback(false, false, 6);
						}
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to parse warframe inventory data! " + ex2);
					callback(false, false, 0);
				}
			});
		}

		public void getResourcesTabData(bool resourcesFavOnly, string resourcesOrderingMode, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RecourcesTabOutputData data = ResourcesTab.GetData(resourcesFavOnly, resourcesOrderingMode);
					callback(true, JsonConvert.SerializeObject(data));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get resources tab data: " + ex);
					callback(false, "An unknown error has occurred: " + ex.Message);
				}
			});
		}

		public void DoWFMarketLogin(string email, string password, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{ "Authorization", "JWT" },
							{ "language", "en" },
							{ "accept", "application/json" },
							{
								HttpRequestHeader.ContentType,
								"application/json"
							},
							{ "platform", "pc" },
							{ "auth_type", "header" }
						},
						Encoding = Encoding.UTF8
					};
					string text = myWebClient.UploadString("https://api.warframe.market/v1/auth/signin", JsonConvert.SerializeObject(new WFMarketLoginRequest
					{
						auth_type = "header",
						email = email,
						password = password
					}));
					StaticData.Log(LogType.ERROR, "req0 " + text);
					bool flag = false;
					string[] allKeys = myWebClient.ResponseHeaders.AllKeys;
					foreach (string text2 in allKeys)
					{
						if (text2.ToLower().Contains("authorization"))
						{
							flag = true;
							string text3 = myWebClient.ResponseHeaders[text2].Substring(4);
							lastTimeTokenUpdated = DateTime.MinValue;
							File.WriteAllText(StaticData.saveFolder + "WFMarketToken.tk", text3);
							try
							{
								myWebClient.Headers.Clear();
								myWebClient.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + text3);
								myWebClient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en");
								myWebClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
								myWebClient.Headers.Add("platform", "pc");
								myWebClient.Headers.Add("auth_type", "header");
								StaticData.Log(LogType.ERROR, "req1");
								lastWFMarketProfileData = JsonConvert.DeserializeObject<WFMarketProfileData>(myWebClient.DownloadString("https://api.warframe.market/v2/me"));
								StaticData.Log(LogType.ERROR, "req2 " + JsonConvert.SerializeObject(lastWFMarketProfileData));
								if (lastWFMarketProfileData.data.verification)
								{
									lastWFMarketToken = text3;
									lastTimeTokenUpdated = DateTime.MinValue;
									wfmSocketExpoentialBackoffCounter = 0;
									wsErrorBackoffUntilTime = DateTime.MinValue;
									newWFMarketWS.Close();
									callback(true, "");
								}
								else
								{
									callback(false, "Please verify your WFM account first!");
								}
							}
							catch (Exception ex)
							{
								StaticData.Log(LogType.ERROR, "Failed to fetch initial WFM profile after login: " + ex);
								callback(false, "Unknown error during initial WFM profile fetch: " + ex.Message);
							}
						}
					}
					if (!flag)
					{
						callback(false, "An unknown error was detected (NWD)");
					}
					checkWFMarketStatusAndOrders.Set();
				}
				catch (WebException ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to login into WFMarket! " + ex2);
					string text4 = "";
					using (StreamReader streamReader = new StreamReader((ex2.Response as HttpWebResponse).GetResponseStream()))
					{
						text4 = streamReader.ReadToEnd();
					}
					StaticData.Log(LogType.ERROR, text4);
					if (!string.IsNullOrEmpty(text4))
					{
						if (text4.Contains("app.form.invalid") || text4.Contains("app.account.password_invalid") || text4.Contains("app.account.email_not_exist"))
						{
							lastWFMarketToken = "";
							lastTimeTokenUpdated = DateTime.MinValue;
							callback(false, "Invalid email/password");
						}
						else
						{
							lastWFMarketToken = "";
							lastTimeTokenUpdated = DateTime.MinValue;
							callback(false, "Unknown login error");
						}
					}
					lastWFMarketToken = "";
					lastTimeTokenUpdated = DateTime.MinValue;
					callback(false, text4);
				}
				catch (Exception ex3)
				{
					StaticData.Log(LogType.ERROR, "Failed to login into WFMarket (Unknown error)! " + ex3);
					callback(false, "Unknown error");
				}
			});
		}

		public string GetUserIdentifier()
		{
			return StatsHandler.GetPlayerUserHash();
		}

		public string GetUserIdentifierSecret()
		{
			return StatsHandler.GetTokenSecret();
		}

		public void GenerateStatsPublicToken(string forceUIDorEmpty, bool[] partsEnabled, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string text = GetUserIdentifier();
					if (!string.IsNullOrEmpty(forceUIDorEmpty))
					{
						text = forceUIDorEmpty;
					}
					if (string.IsNullOrEmpty(text))
					{
						callback(false, "Stat data not ready");
					}
					else
					{
						string text2 = FoundryHelper.lastFetchedUsername;
						if (string.IsNullOrEmpty(text2))
						{
							text2 = "Unknown";
						}
						MyWebClient obj = new MyWebClient
						{
							Proxy = null
						};
						PublicLinkParts publicLinkParts = PublicLinkParts.None;
						if (partsEnabled[0])
						{
							publicLinkParts |= PublicLinkParts.AccountData;
						}
						if (partsEnabled[1])
						{
							publicLinkParts |= PublicLinkParts.Trades;
						}
						if (partsEnabled[2])
						{
							publicLinkParts |= PublicLinkParts.Platinum;
						}
						if (partsEnabled[3])
						{
							publicLinkParts |= PublicLinkParts.Ducats;
						}
						if (partsEnabled[4])
						{
							publicLinkParts |= PublicLinkParts.Endo;
						}
						if (partsEnabled[5])
						{
							publicLinkParts |= PublicLinkParts.Credits;
						}
						if (partsEnabled[6])
						{
							publicLinkParts |= PublicLinkParts.Aya;
						}
						if (partsEnabled[7])
						{
							publicLinkParts |= PublicLinkParts.Relics;
						}
						string text3 = obj.UploadString($"{StaticData.StatsAPIHostname}/api/stats/public/create?token={text}&expirationDate={DateTime.UtcNow.AddYears(1)}&publicParts={publicLinkParts}&username={text2}&secretToken={StatsHandler.GetTokenSecret()}", "");
						text3 = text3.Substring(1, text3.Length - 2);
						callback(true, text3);
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to GenerateStatsShareLink: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void ItemJustHighlighted(string highlightedInfoJSON)
		{
			Task.Run(delegate
			{
			});
		}

		public void WFMRemoveAll(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					List<WFMarketProfileOrder> list = WFMarketOrders.data.ToList();
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"Bearer " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					foreach (WFMarketProfileOrder item in list)
					{
						try
						{
							myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
							myWebClient.Headers.Add("accept", "application/json");
							myWebClient.UploadString("https://api.warframe.market/v2/order/" + item.id, "DELETE", "");
						}
						catch
						{
						}
						Thread.Sleep(333);
					}
					checkWFMarketStatusAndOrders.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMRemoveAll: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void FixWFMOrders(string idToFixOrEmpty, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					List<WFMarketOrderData> source = WFMarketHelper.GetCurrentOrders("all", new Dictionary<string, string>(), "name", orderedFromLargerToSmaller: true, "");
					if (idToFixOrEmpty != "all")
					{
						source = source.Where((WFMarketOrderData p) => p.randomID == idToFixOrEmpty).ToList();
					}
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"Bearer " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					foreach (WFMarketOrderData item in source.Where((WFMarketOrderData p) => p.showWarning))
					{
						try
						{
							myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
							myWebClient.Headers.Add("accept", "application/json");
							if (item.amountOwned > 0)
							{
								myWebClient.UploadString("https://api.warframe.market/v2/order/" + item.randomID, "PATCH", "{\"quantity\":" + item.amountOwned + "}");
							}
							else
							{
								myWebClient.UploadString("https://api.warframe.market/v2/order/" + item.randomID, "DELETE", "");
							}
						}
						catch
						{
						}
						Thread.Sleep(333);
					}
					checkWFMarketStatusAndOrders.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to FixWFMOrders: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void FixWFMContracts(string idToFixOrEmpty, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					List<WFMarketContractData> source = WFMarketHelper.GetCurrentContracts("all", new Dictionary<string, string>(), "name", orderedFromLargerToSmaller: true, "");
					if (idToFixOrEmpty != "all" && idToFixOrEmpty != "")
					{
						source = source.Where((WFMarketContractData p) => p.randomID == idToFixOrEmpty).ToList();
					}
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"JWT " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					foreach (WFMarketContractData item in source.Where((WFMarketContractData p) => p.showWarning))
					{
						try
						{
							myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
							myWebClient.Headers.Add("accept", "application/json");
							myWebClient.UploadString("https://api.warframe.market/v1/auctions/entry/" + item.randomID + "/close", "PUT", "");
						}
						catch
						{
						}
						Thread.Sleep(333);
					}
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to FixWFMContracts: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void WFMAllInvisible(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"Bearer " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					try
					{
						myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
						myWebClient.Headers.Add("accept", "application/json");
						myWebClient.UploadString("https://api.warframe.market/v2/orders/group/all", "PATCH", "{\"visible\":false}");
					}
					catch
					{
					}
					checkWFMarketStatusAndOrders.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMAllInvisible: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void WFMAllVisible(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"Bearer " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					try
					{
						myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
						myWebClient.Headers.Add("accept", "application/json");
						myWebClient.UploadString("https://api.warframe.market/v2/orders/group/all", "PATCH", "{\"visible\":true}");
					}
					catch
					{
					}
					checkWFMarketStatusAndOrders.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMAllVisible: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void WFMContractsSetVisibility(string id, bool newVisibility, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "JWT " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.UploadString("https://api.warframe.market/v1/auctions/entry/" + id, "PUT", "{\"visible\":\"" + newVisibility.ToString().ToLower() + "\"}");
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMContractsSetVisibility: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void DoOnClosingWork()
		{
			Task.Run(delegate
			{
				StaticData.Log(LogType.INFO, "Closing work started");
				WFMAutoStatus = false;
				wfMarketMyStatus = WFMarketMyStatus.invisible;
				SendWFMMarketStatus(WFMarketMyStatus.invisible);
			});
		}

		public void WFMContractsRemove(string id, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "JWT " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.UploadString("https://api.warframe.market/v1/auctions/entry/" + id + "/close", "PUT", "");
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMContractsRemove: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void WFMContractsAllVisibility(bool newVisibility, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "JWT " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.UploadString("https://api.warframe.market/v1/profile/auctions/visibility", "PUT", "{\"visibility\":" + newVisibility.ToString().ToLower() + "}");
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMContractsAllVisibility: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void WFMContractsRemoveAll(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					List<WFMRivenDataAuction> list = WFMarketContracts.payload.auctions.ToList();
					MyWebClient myWebClient = new MyWebClient
					{
						Proxy = null,
						Headers = 
						{
							{
								"Authorization",
								"JWT " + lastWFMarketToken
							},
							{ "language", "en" },
							{ "platform", "pc" },
							{ "auth_type", "header" }
						}
					};
					foreach (WFMRivenDataAuction item in list)
					{
						try
						{
							myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
							myWebClient.Headers.Add("accept", "application/json");
							myWebClient.UploadString("https://api.warframe.market/v1/auctions/entry/" + item.id + "/close", "PUT", "");
						}
						catch
						{
						}
						Thread.Sleep(333);
					}
					checkWFMarketContracts.Set();
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to WFMContractsRemoveAll: " + ex);
					callback(false, "Unknown error");
				}
			});
		}

		public void DoManualRelic(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				OCRHelper.DoScreenshotRequestWork(Misc.IsWarframeUIScaleLegacy());
			});
		}

		public void DoManualRelicTest(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				OCRHelper.DoScreenshotRequestWork(Misc.IsWarframeUIScaleLegacy(), debugMode: true, new Bitmap("C:\\GIT\\AlecaFrame\\ML\\testPictures\\test3.png"));
			});
		}

		public void GetBannerData(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (!File.Exists(StaticData.saveFolder + "\\cachedData\\custom\\banner.txt"))
					{
						callback(false, "No banner data available (file not found)");
					}
					else
					{
						BannerStorageData bannerStorageData = JsonConvert.DeserializeObject<BannerStorageData[]>(File.ReadAllText(StaticData.saveFolder + "\\cachedData\\custom\\banner.txt")).FirstOrDefault((BannerStorageData p) => p?.enabled ?? false);
						if (bannerStorageData == null)
						{
							callback(false, "No enabled banner found");
						}
						else if (File.Exists(StaticData.saveFolder + "\\lastBannerClosed.tmp") && File.ReadAllText(StaticData.saveFolder + "\\lastBannerClosed.tmp") == bannerStorageData.message && (DateTime.UtcNow - File.GetLastWriteTimeUtc(StaticData.saveFolder + "\\lastBannerClosed.tmp")).TotalDays < 3.0)
						{
							callback(false, "Same banner was shown and closed recently");
						}
						else if (!string.IsNullOrEmpty(bannerStorageData.maxVersion) && new Version(currentVersion) > new Version(bannerStorageData.maxVersion))
						{
							callback(false, "Banner is disabled for this version");
						}
						else
						{
							callback(true, bannerStorageData.message);
						}
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get banner data: " + ex);
					callback(false, "An unknown error has occurred");
				}
			});
		}

		public void CloseCurrentBanner()
		{
			Task.Run(delegate
			{
				try
				{
					if (!File.Exists(StaticData.saveFolder + "\\cachedData\\custom\\banner.txt"))
					{
						StaticData.Log(LogType.ERROR, "Failed to close banner: banner file not found");
					}
					else
					{
						BannerStorageData bannerStorageData = JsonConvert.DeserializeObject<BannerStorageData[]>(File.ReadAllText(StaticData.saveFolder + "\\cachedData\\custom\\banner.txt")).FirstOrDefault((BannerStorageData p) => p?.enabled ?? false);
						if (bannerStorageData == null)
						{
							StaticData.Log(LogType.ERROR, "Failed to close banner: no enabled banner found");
						}
						else
						{
							File.WriteAllText(StaticData.saveFolder + "\\lastBannerClosed.tmp", bannerStorageData.message);
						}
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to close banner: " + ex);
				}
			});
		}

		public void DoWFMarketPostListing(string type, string itemName, int platinum, int quantity, string subtype, string mod_rank_str, string cyan_stars_str, string amber_stars_str, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					int? rank = null;
					if (!string.IsNullOrEmpty(mod_rank_str) && int.TryParse(mod_rank_str, out var result))
					{
						rank = result;
					}
					int? cyanStars = null;
					if (!string.IsNullOrEmpty(cyan_stars_str) && int.TryParse(cyan_stars_str, out var result2))
					{
						cyanStars = result2;
					}
					int? amberStars = null;
					if (!string.IsNullOrEmpty(amber_stars_str) && int.TryParse(amber_stars_str, out var result3))
					{
						amberStars = result3;
					}
					if (itemName == "ayatan_ayr_sculpture")
					{
						amberStars = null;
					}
					if (string.IsNullOrEmpty(subtype))
					{
						subtype = null;
					}
					WFMItemListItem orDefault = StaticData.LazyWfmItemData.Value.AsDictionaryByID.GetOrDefault(itemName);
					if (StaticData.LazyWfmItemData.Value.AsDictionaryByID.GetOrDefault(itemName)?.i18n?.en?.name?.ToLower() == null)
					{
						_ = itemName;
					}
					int? perTrade = null;
					if (orDefault.bulkTradable)
					{
						perTrade = 1;
					}
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "Bearer " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.UploadString("https://api.warframe.market/v2/order", JsonConvert.SerializeObject(new WFMarketPostListingRequest
					{
						itemId = itemName,
						type = type,
						platinum = platinum,
						quantity = quantity,
						subtype = subtype,
						rank = rank,
						cyanStars = cyanStars,
						amberStars = amberStars,
						visible = true,
						perTrade = perTrade
					}));
					callback(true, "");
					checkWFMarketStatusAndOrders.Set();
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to post listing to WFMarket! " + ex);
					string text = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						text = streamReader.ReadToEnd();
					}
					if (!string.IsNullOrEmpty(text))
					{
						if (text.Contains("app.order.error.exceededOrderLimitSameItem") || text.Contains("app.order.error.exceededOrderLimitSamePrice"))
						{
							callback(false, "Order already exists");
						}
						else if (text.Contains("app.form.field_required"))
						{
							callback(false, "Incomplete information");
						}
						else if (text.Contains("app.form.invalid"))
						{
							callback(false, "Invalid value");
						}
						else if (text.Contains("app.errors.banned"))
						{
							callback(false, "WFMarket account banned");
						}
						else if (text.Contains("app.order.error.exceededOrderLimit"))
						{
							callback(false, "Maximum number of orders exceeded. Check WFMarket for more info");
						}
						else
						{
							StaticData.Log(LogType.ERROR, "Post listing error: " + text);
							callback(false, "Unknown error");
						}
					}
					callback(false, text);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to post listing to WFMarket (Unknown error)! " + ex2);
					callback(false, "Unknown error");
				}
			});
		}

		public void DoWFMarketRemoveListing(string listingID, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "Bearer " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.UploadString("https://api.warframe.market/v2/order/" + listingID, "DELETE", "");
					callback(true, "");
					checkWFMarketStatusAndOrders.Set();
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to remove listing from WFMarket! " + ex);
					string arg = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						arg = streamReader.ReadToEnd();
					}
					callback(false, arg);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to remove listing from WFMarket (Unknown error)! " + ex2);
					callback(false, "Unknown error");
				}
			});
		}

		public void GetFilteredVeiledRivensInfo(string filtersJSON, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary.ContainsKey("yesnoFilters") ? dictionary["yesnoFilters"] : "{}");
					string text = "[]";
					string text2 = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
					bool flag = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
					if (text2 == "name")
					{
						flag = !flag;
					}
					text = JsonConvert.SerializeObject(RivenExplorerHelper.GetVeiledRivens(dictionary["type"], yesNoFilters, text2, flag));
					callback(true, text);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering veiled rivens: " + ex);
					callback(false, "");
				}
			});
		}

		public void GetFilteredUnveiledRivensInfo(string filtersJSON, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filtersJSON);
					Dictionary<string, string> yesNoFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(dictionary.ContainsKey("yesnoFilters") ? dictionary["yesnoFilters"] : "{}");
					string text = "[]";
					string searchPrompt = (dictionary.ContainsKey("search") ? dictionary["search"] : "");
					string text2 = (dictionary.ContainsKey("order") ? dictionary["order"] : "name");
					bool flag = bool.Parse(dictionary.ContainsKey("orderLargerToSmaller") ? dictionary["orderLargerToSmaller"] : "false");
					if (text2 == "name")
					{
						flag = !flag;
					}
					text = JsonConvert.SerializeObject(RivenExplorerHelper.GetUnveiledRivens(dictionary["type"], yesNoFilters, text2, flag, searchPrompt));
					callback(true, text);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "An error has ocurred when filtering unveiled rivens: " + ex);
					callback(false, "");
				}
			});
		}

		public void AddMetric(string metricKey, string metricsValue)
		{
			Task.Run(delegate
			{
				AnalyticsHandler.AddMetric(metricKey, metricsValue);
			});
		}

		public void ExportDataToDesktop(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export");
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\foundry.json", JsonConvert.SerializeObject(GetFoundryTabData(showAll: true, new Dictionary<string, string> { { "type", "all" } }, new Dictionary<string, string>())));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventoryParts.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventoryAllParts(new Dictionary<string, string>(), "", out var totalDucats, out var totalPlat, "name", orderedFromLargerToSmaller: false)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventoryRelics.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventoryRelics(new Dictionary<string, string>(), "", out totalPlat, "name", orderedFromLargerToSmaller: false)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventoryMods.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventoryMods(new Dictionary<string, string>(), "", out totalPlat, "name", orderedFromLargerToSmaller: false, showOnlyOwned: true)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventoryArcanes.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventoryArcanes(new Dictionary<string, string> { { "type", "all" } }, "", out totalPlat, "name", orderedFromLargerToSmaller: false)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventoryMisc.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventoryMisc("", out totalPlat, "name", orderedFromLargerToSmaller: false)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\inventorySets.json", JsonConvert.SerializeObject(InventoryHelpers.GetInventorySets(new Dictionary<string, string>(), "", out totalPlat, out totalDucats, "name", orderedFromLargerToSmaller: false)));
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AlecaFrame_Export\\rivens.json", JsonConvert.SerializeObject(RivenExplorerHelper.GetUnveiledRivens("all", new Dictionary<string, string>(), "name", orderedFromLargerToSmaller: false, "")));
					callback(true, "");
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to export data: " + ex);
					callback(false, "Failed to export data: " + ex.Message);
				}
			});
		}

		public void SetTimerNotificationSettings(bool notiEarthDay, bool notiEarthNight, bool notiCetusDay, bool notiCetusNight, bool notiVallisCold, bool notiVallisWarm, bool notiCambionVome, bool notiCambionFass)
		{
			WorldStateHelper.notiEarthDay = notiEarthDay;
			WorldStateHelper.notiEarthNight = notiEarthNight;
			WorldStateHelper.notiCetusDay = notiCetusDay;
			WorldStateHelper.notiCetusNight = notiCetusNight;
			WorldStateHelper.notiVallisCold = notiVallisCold;
			WorldStateHelper.notiVallisWarm = notiVallisWarm;
			WorldStateHelper.notiCambionFass = notiCambionFass;
			WorldStateHelper.notiCambionVome = notiCambionVome;
		}

		public void SetFissureNotificationSettings(bool fissureNotificationsEnabled, string fissureNotificationSettingsJSON)
		{
			try
			{
				WorldStateHelper.fissureNotificationsEnabled = fissureNotificationsEnabled;
				WorldStateHelper.fissureNotificationSettings = JsonConvert.DeserializeObject<List<FissureNotificationSetting>>(fissureNotificationSettingsJSON);
			}
			catch (Exception ex)
			{
				StaticData.Log(LogType.ERROR, "Failed to SetFissureNotificationSettings: " + ex);
			}
		}

		public void GetHelpChecklist(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string[] array = new string[6] { "na", "na", "na", "na", "na", "na" };
					Process process = Process.GetProcessesByName("Warframe.x64").FirstOrDefault();
					if (process != null && Misc.IsAccountLogedIn() && (DateTime.Now - process.StartTime).TotalSeconds > 7.0)
					{
						array[0] = "ok";
					}
					else
					{
						array[0] = "error";
					}
					if (process != null)
					{
						Process process2 = Process.GetProcessesByName("Overwolf").FirstOrDefault();
						if (process2 != null)
						{
							if (process.StartTime - process2.StartTime > TimeSpan.FromSeconds(15.0))
							{
								array[1] = "ok";
							}
							else
							{
								array[1] = "error";
							}
							int num = (Misc.IsProcessRunAsAdmin(process2) ? 1 : 0);
							if ((Misc.IsProcessRunAsAdmin(process) ? 1 : 0) > num)
							{
								array[2] = "error";
							}
							else
							{
								array[2] = "ok";
							}
						}
					}
					try
					{
						string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Overwolf\\Log\\Apps\\Overwolf General GameEvents Provider\\index.html.log");
						if (File.Exists(path))
						{
							if (File.ReadAllText(path).Contains("Failed to load Warframe  plugin: plugin is not signed"))
							{
								array[4] = "error";
							}
							else
							{
								array[4] = "ok";
							}
						}
					}
					catch (Exception ex)
					{
						StaticData.Log(LogType.ERROR, "Failed to GetHelpChecklist in Overwolf install step: " + ex);
					}
					callback(true, JsonConvert.SerializeObject(array));
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to GetHelpChecklist: " + ex2);
					callback(false, "Unknown error: " + ex2.Message);
				}
			});
		}

		public void DoWFMarketMarkAsDoneListing(string listingID, int amount, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "Bearer " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.UploadString("https://api.warframe.market/v2/order/" + listingID + "/close", "{\"quantity\": " + amount + "}");
					callback(true, "");
					checkWFMarketStatusAndOrders.Set();
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to mark as done listing from WFMarket! " + ex);
					string arg = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						arg = streamReader.ReadToEnd();
					}
					callback(false, arg);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to mark as done listing from WFMarket (Unknown error)! " + ex2);
					callback(false, "Unknown error");
				}
			});
		}

		public void SendWFMReputation(string username, string userInput, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					StaticData.Log(LogType.INFO, "Sending WFMarket reputation to user " + username + " with user input " + userInput);
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "JWT " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.UploadString("https://api.warframe.market/v1/profile/" + username + "/review", "POST", JsonConvert.SerializeObject(new
					{
						review_type = 1,
						text = userInput
					}));
					callback(true, "");
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMReputation in WFMarket! " + ex);
					string text = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						text = streamReader.ReadToEnd();
					}
					StaticData.Log(LogType.ERROR, "Extra details: " + text);
					if (text.Contains("app.review.already_exist"))
					{
						callback(false, "Review already exists");
					}
					else
					{
						HttpWebResponse obj = ex.Response as HttpWebResponse;
						if (obj != null && obj.StatusCode == HttpStatusCode.NotFound)
						{
							callback(false, "User not found in WFMarket");
						}
						else
						{
							callback(false, "Unknown error");
						}
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMReputation in WFMarket (Unknown error)! " + ex2);
					callback(false, "Unknown error");
				}
			});
		}

		public void GetResourcesTabData(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string arg = JsonConvert.SerializeObject(ResourcesTab.GetData(onlyFavItems: false, "normal"));
					callback(true, arg);
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get resources tab data: " + ex);
					callback(false, "");
				}
			});
		}

		public void SendWFMReport(string username, string userInput, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (string.IsNullOrWhiteSpace(userInput))
					{
						callback(false, "The report should include an explanation");
					}
					else
					{
						StaticData.Log(LogType.INFO, "Sending WFMarket report to user " + username + " with user input " + userInput);
						MyWebClient myWebClient = new MyWebClient();
						myWebClient.Proxy = null;
						myWebClient.Headers.Add("Authorization", "JWT " + lastWFMarketToken);
						myWebClient.Headers.Add("language", "en");
						myWebClient.Headers.Add("accept", "application/json");
						myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
						myWebClient.Headers.Add("platform", "pc");
						myWebClient.Headers.Add("auth_type", "header");
						myWebClient.UploadString("https://api.warframe.market/v1/profile/" + username + "/review", "POST", JsonConvert.SerializeObject(new
						{
							review_type = -1,
							text = userInput + " (via AlecaFrame)"
						}));
						callback(true, "");
					}
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMReport in WFMarket! " + ex);
					string text = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						text = streamReader.ReadToEnd();
					}
					StaticData.Log(LogType.ERROR, "Extra details: " + text);
					if (text.Contains("app.review.already_exist"))
					{
						callback(false, "Review/report already exists.\nPlease send a report manually");
					}
					else
					{
						callback(false, "Unknown error");
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to SendWFMReport in WFMarket (Unknown error)! " + ex2);
					callback(false, "Unknown error");
				}
			});
		}

		public void GetWFMarketSearchSuggestions(string currentInput, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					currentInput = currentInput.ToLower();
					List<WFMSearchSuggestion> value = (from p in StaticData.LazyWfmItemData.Value.data
						where p.item_name.ToLower().Contains(currentInput)
						select new WFMSearchSuggestion
						{
							name = p.item_name,
							picture = p.i18n.en.thumb,
							urlName = p.slug
						} into p
						orderby p.name.ToLower() == currentInput.ToLower() descending, p.name
						select p).Take(20).ToList();
					callback(true, JsonConvert.SerializeObject(value));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get WFMarket search suggestions: " + ex);
					callback(false, "");
				}
			});
		}

		public void DoWFMarketUpdateListing(string listingID, int newAmonut, int newPrice, bool isVisible, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					if (newAmonut > 9998)
					{
						throw new Exception("Amount needs to be less than 9999");
					}
					if (newPrice > 800000)
					{
						throw new Exception("Price needs to be less than 800000");
					}
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.Headers.Add("Authorization", "Bearer " + lastWFMarketToken);
					myWebClient.Headers.Add("language", "en");
					myWebClient.Headers.Add("accept", "application/json");
					myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					myWebClient.Headers.Add("platform", "pc");
					myWebClient.Headers.Add("auth_type", "header");
					myWebClient.UploadString("https://api.warframe.market/v2/order/" + listingID, "PATCH", "{\"platinum\":" + newPrice.ToString().ToLower() + ",\"quantity\":" + newAmonut.ToString().ToLower() + ",\"visible\":" + isVisible.ToString().ToLower() + "}");
					callback(true, "");
					checkWFMarketStatusAndOrders.Set();
				}
				catch (WebException ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to change listing from WFMarket! " + ex);
					string arg = "";
					using (StreamReader streamReader = new StreamReader((ex.Response as HttpWebResponse).GetResponseStream()))
					{
						arg = streamReader.ReadToEnd();
					}
					callback(false, arg);
				}
				catch (Exception ex2)
				{
					StaticData.Log(LogType.ERROR, "Failed to change listing from WFMarket (Unknown error)! " + ex2);
					callback(false, ex2.Message);
				}
			});
		}

		public void showInGameNotification(string message, string source, bool makeSound)
		{
			Task.Run(delegate
			{
				try
				{
					StaticData.Log(LogType.INFO, "Sending in-game notification with message: " + message);
					if (source == "TennoFinder")
					{
						this.OnInGameNotification?.Invoke(message, "assets/img/tennofinder.png", source);
					}
					else
					{
						this.OnInGameNotification?.Invoke(message, "assets/img/alecaframe.png", source);
					}
					if (StaticData.notificationSoundsEnabled && makeSound)
					{
						new SoundPlayer(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "notification.wav")).Play();
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to send in-game notification: " + ex);
					throw;
				}
			});
		}

		public void GetFoundryDetails(string uniqueID, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					FoundryDetailsResponse foundryDetailsResponse = new FoundryDetailsResponse(uniqueID);
					if (foundryDetailsResponse.initializationSuccessful)
					{
						callback(true, JsonConvert.SerializeObject(foundryDetailsResponse));
					}
					else
					{
						callback(false, "Item not found");
					}
				}
				catch (Exception ex)
				{
					callback(false, "An unknown error has occurred in GetFoundryDetails: " + ex.Message);
				}
			});
		}

		public void GetRelicDetails(string uniqueID, bool includePriceData, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					RelicDetailsResponse relicDetailsResponse = new RelicDetailsResponse(uniqueID);
					if (includePriceData)
					{
						relicDetailsResponse.FillPriceData();
					}
					if (relicDetailsResponse.initializationSuccessful)
					{
						callback(true, JsonConvert.SerializeObject(relicDetailsResponse));
					}
					else
					{
						callback(false, "Data initialization error");
					}
				}
				catch (Exception ex)
				{
					callback(false, "An unknown error has occurred in GetRelicDetails: " + ex.Message);
				}
			});
		}

		public void GetPatreonLinkingURL(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					string playerUserHash = StatsHandler.GetPlayerUserHash();
					if (string.IsNullOrEmpty(playerUserHash))
					{
						callback(false, "Your Warframe data needs to be recognized before you can link your Patreon account");
					}
					else
					{
						string arg = "https://www.patreon.com/oauth2/authorize?response_type=code&client_id=JI1y6gcqgCuygEn7S9Syig765wz340HvYJCZg-szcfqlGmP_Httmo65DJt97QRj6&scope=identity+identity%5Bemail%5D&redirect_uri=https://api.alecaframe.com/rivens/linkPatreon&state=" + playerUserHash;
						callback(true, arg);
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get GetPatreonLinkingURL: " + ex);
					callback(ex, "An error has occurred: " + ex.Message);
				}
			});
		}

		public void setLocalVersion(string _version)
		{
			lock (currentVersion)
			{
				try
				{
					currentVersion = _version;
					StaticData.Log(LogType.INFO, "Setting local version to: " + _version);
					lastSavedVersion = "N/A";
					if (!Directory.Exists(StaticData.saveFolder))
					{
						StaticData.isFirstRunOnInstall = true;
						Directory.CreateDirectory(StaticData.saveFolder);
					}
					if (File.Exists(StaticData.saveFolder + "version.txt"))
					{
						lastSavedVersion = File.ReadAllText(StaticData.saveFolder + "version.txt");
					}
					if (lastSavedVersion == "N/A" || lastSavedVersion != currentVersion)
					{
						File.WriteAllText(StaticData.saveFolder + "version.txt", currentVersion);
						if (IsFTUEDoneSync())
						{
							newVersionAvailable = currentVersion;
						}
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to set local version: " + ex);
				}
			}
		}

		public void logInBackgroundCaller_PRIVATE(LogType logType, string data)
		{
			if (this.logInBackground == null)
			{
				Console.WriteLine("[" + logType.ToString() + "] " + data);
				return;
			}
			try
			{
				this.logInBackground(logType.ToString(), data);
			}
			catch
			{
			}
			Console.WriteLine("[" + logType.ToString() + "] " + data);
		}

		public void ParseFakeEELogs()
		{
			using StreamReader streamReader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Warframe/EE - Copy.log");
			while (!streamReader.EndOfStream)
			{
				EELogProcessor.ProcessLine(streamReader.ReadLine());
			}
		}

		public void GetSquadRequirementsOptions(string type, Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(SquadFinderHelper.GetAvailableSquadRequirementOptions(type)));
				}
				catch (Exception ex)
				{
					callback(false, "An unknown error has occurred in GetSquadRequirementsOptions: " + ex.Message);
				}
			});
		}

		public void GetSquadLoginData(Action<object, object, object> callback)
		{
			if (usingSquadTestAccount)
			{
				callback(true, "test-skuid4ever", "skuid4ever");
			}
			Task.Run(delegate
			{
				try
				{
					string text = "";
					string text2 = "";
					text2 = FoundryHelper.lastFetchedUsername;
					if (string.IsNullOrWhiteSpace(text2) || text2 == "AlecaFrame")
					{
						callback(false, "", "");
					}
					else
					{
						text = StaticData.dataHandler.warframeRootObject?.DataKnives?.FirstOrDefault((Dataknife1 p) => p.ItemType == "/Lotus/Weapons/Tenno/HackingDevices/TnHackingDevice/TnHackingDeviceWeapon")?.ItemId?.oid;
						if (string.IsNullOrEmpty(text))
						{
							callback(false, "", "");
						}
						callback(true, text, text2);
					}
				}
				catch (Exception)
				{
					callback(false, "", "");
				}
			});
		}

		public void SetSquadMakingTestAccount()
		{
			usingSquadTestAccount = true;
		}

		public void onWarframeDataChangedCaller()
		{
			this.onWarframeDataChanged?.Invoke("Hi!");
		}

		public void onRivenOverlayOpenCaller()
		{
			this.onRivenOverlayOpen?.Invoke("Hi!");
		}

		public void onRivenOverlayChangeCaller()
		{
			this.onRivenOverlayChange?.Invoke("Hi!");
		}

		public void GetRivenOverlayData(Action<object, object> callback)
		{
			Task.Run(delegate
			{
				try
				{
					callback(true, JsonConvert.SerializeObject(RivenOverlays.workingData));
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get GetRivenOverlayData: " + ex);
					callback(false, ex.Message);
				}
			});
		}

		public void RetryLastRivenOverlayOCR()
		{
			Task.Run(delegate
			{
				try
				{
					RivenOverlays.RetryLastOCR();
				}
				catch (Exception ex)
				{
					StaticData.Log(LogType.ERROR, "Failed to get RetryLastRivenOverlayOCR: " + ex);
				}
			});
		}

		public void TestTest()
		{
			WorldStateHelper.fissuresAlreadySeen.Clear();
		}
	}
}
