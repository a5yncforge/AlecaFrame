using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Data.Types.WFM;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public class RivenExplorerHelper
	{
		public class RivenMarketRivenImport
		{
			public string weaponData { get; set; }

			public int Rank { get; set; }

			public int MR { get; set; }

			public int Rolls { get; set; }

			public string name { get; set; }

			public Dictionary<string, string> Positives { get; set; }

			public Dictionary<string, string> Negatives { get; set; }
		}

		public class RivenMarketStat
		{
			public string id { get; set; }

			public string Name { get; set; }

			public string Pre { get; set; }

			public string Unit { get; set; }

			public string Desc { get; set; }

			public string Prefix { get; set; }

			public string Suffix { get; set; }

			public bool MeleeOnly { get; set; }

			public string IconURL { get; set; }
		}

		private static Lazy<RivenAtttrResponse> LazyWfmRivenAttrData = new Lazy<RivenAtttrResponse>(() => JsonConvert.DeserializeObject<RivenAtttrResponse>(new MyWebClient
		{
			Proxy = null
		}.DownloadString("https://" + StaticData.CachedWFMAPIHostname + "/v2/riven/attributes")));

		private static Lazy<WFMRivenItemsResponse> LazyWfmRivenItemsData = new Lazy<WFMRivenItemsResponse>(() => JsonConvert.DeserializeObject<WFMRivenItemsResponse>(new MyWebClient
		{
			Proxy = null
		}.DownloadString("https://" + StaticData.CachedWFMAPIHostname + "/v2/riven/weapons")));

		private static Dictionary<string, RivenMarketStat> lastRivenMarketData = null;

		public static IEnumerable<VeiledRivenGroup> GetVeiledRivens(string typeToShow, Dictionary<string, string> yesNoFilters, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> list = new List<Miscitem>();
			if (StaticData.dataHandler.warframeRootObject?.Upgrades != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade p) => p.IsRivenMod() && ShouldShowRivenBasedOnTypeAndUID(p.ItemType, typeToShow)).Select((Func<Upgrade, Miscitem>)((Upgrade p) => p)));
			}
			if (StaticData.dataHandler.warframeRootObject?.RawUpgrades != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.RawUpgrades.Where((Miscitem p) => p.IsRivenMod() && ShouldShowRivenBasedOnTypeAndUID(p.ItemType, typeToShow)));
			}
			List<RivenSummaryData> source = (from p in list
				select new RivenSummaryData(p, isUnveiled: false) into p
				where !p.errorOccurred
				group p by p.randomID).Select(delegate(IGrouping<string, RivenSummaryData> p)
			{
				RivenSummaryData rivenSummaryData = p.First();
				rivenSummaryData.amountOwned = p.Sum((RivenSummaryData u) => u.amountOwned);
				return rivenSummaryData;
			}).ToList();
			string[] itemListNames = (from p in source
				group p by p.name into p
				select p.Key + " (Veiled)").ToArray();
			StaticData.overwolfWrappwer.SYNC_GetHugePriceList(itemListNames, TimeSpan.FromSeconds(7.0));
			return from p in source.Select(delegate(RivenSummaryData item)
				{
					item.tradePrice = (StaticData.overwolfWrappwer.GetSYNCSingleItemPrice(item.name + " (Veiled)")?.post).GetValueOrDefault();
					return item;
				})
				group p by p.challengeID into p
				select new VeiledRivenGroup
				{
					simpleChallengeName = p.First().simpleChallengeName,
					challengeDescription = p.First().challengeDescription,
					rivens = p.ToList()
				};
		}

		private static bool ShouldShowRivenBasedOnTypeAndUID(string itemType, string typeToShow)
		{
			if (typeToShow == "all")
			{
				return true;
			}
			if (typeToShow == "arch" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusArchgunRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawArchgunRandomMod"))
			{
				return true;
			}
			if (typeToShow == "zaw" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusModularMeleeRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawModularMeleeRandomMod"))
			{
				return true;
			}
			if (typeToShow == "kitgun" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusModularPistolRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawModularPistolRandomMod"))
			{
				return true;
			}
			if (typeToShow == "pistol" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusPistolRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawPistolRandomMod"))
			{
				return true;
			}
			if (typeToShow == "rifle" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusRifleRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawRifleRandomMod"))
			{
				return true;
			}
			if (typeToShow == "shotgun" && (itemType == "/Lotus/Upgrades/Mods/Randomized/LotusShotgunRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawShotgunRandomMod"))
			{
				return true;
			}
			if (typeToShow == "melee" && (itemType == "/Lotus/Upgrades/Mods/Randomized/PlayerMeleeWeaponRandomModRare" || itemType == "/Lotus/Upgrades/Mods/Randomized/RawMeleeRandomMod"))
			{
				return true;
			}
			return false;
		}

		public static IEnumerable<RivenSummaryData> GetUnveiledRivens(string typeToShow, Dictionary<string, string> yesNoFilters, string orderingType, bool orderedFromLargerToSmaller, string searchPrompt)
		{
			List<Miscitem> list = new List<Miscitem>();
			if (StaticData.dataHandler?.warframeRootObject?.Upgrades != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject?.Upgrades?.Where((Upgrade p) => p.IsRivenMod() && ShouldShowRivenBasedOnTypeAndUID(p.ItemType, typeToShow)).Select((Func<Upgrade, Miscitem>)((Upgrade p) => p)));
			}
			if (StaticData.dataHandler?.warframeRootObject?.RawUpgrades != null)
			{
				list.AddRange((from p in StaticData.dataHandler.warframeRootObject?.RawUpgrades?.Where((Miscitem p) => p.IsRivenMod() && ShouldShowRivenBasedOnTypeAndUID(p.ItemType, typeToShow))
					where p.UpgradeFingerprint != null
					select p));
			}
			IEnumerable<RivenSummaryData> enumerable = (from p in list
				select new RivenSummaryData(p, isUnveiled: true) into p
				where !p.errorOccurred
				select p).ToList();
			if (StaticData.overwolfWrappwer?.WFMarketContracts != null)
			{
				foreach (WFMRivenDataAuction item in StaticData.overwolfWrappwer.WFMarketContracts.payload.auctions.Where((WFMRivenDataAuction p) => p?.item?.type == "riven"))
				{
					if (item == null)
					{
						continue;
					}
					RivenSummaryData singleRivenDetailsFromWFM = GetSingleRivenDetailsFromWFM(item);
					if (singleRivenDetailsFromWFM.errorOccurred)
					{
						continue;
					}
					foreach (RivenSummaryData item2 in enumerable)
					{
						if (item2.IsRoughlyEqual(singleRivenDetailsFromWFM))
						{
							item2.listedInWFMarket = true;
							break;
						}
					}
				}
			}
			enumerable = enumerable.Where((RivenSummaryData p) => ShouldShowRivenBasedOnSearch(p, searchPrompt));
			if (orderedFromLargerToSmaller)
			{
				return from p in enumerable
					orderby OrderRivensBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in enumerable
				orderby OrderRivensBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		private static bool ShouldShowRivenBasedOnSearch(RivenSummaryData arg, string filter)
		{
			filter = filter.ToLower();
			if (string.IsNullOrWhiteSpace(filter))
			{
				return true;
			}
			if (arg.name.ToLower().Contains(filter))
			{
				return true;
			}
			if (arg.weaponName.ToLower().Contains(filter))
			{
				return true;
			}
			if (arg.statsPerWeapon[0].byLevel[0].positiveTraits.Any((RivenUnveiledSingleStat p) => p.noMarkupDescription.ToLower().Contains(filter)))
			{
				return true;
			}
			arg.statsPerWeapon[0].byLevel[0].positiveTraits.Any((RivenUnveiledSingleStat p) => p.noMarkupDescription.ToLower().Contains(filter));
			return false;
		}

		private static object OrderRivensBasedOnCriteria(RivenSummaryData p, string orderingType)
		{
			return orderingType switch
			{
				"disposition" => p.disposition, 
				"perfectness" => (p.statsPerWeapon[0].byLevel[0].positiveTraits.Sum((RivenUnveiledSingleStat u) => u.rawRandomValue) + p.statsPerWeapon[0].byLevel[0].negativeTraits.Sum((RivenUnveiledSingleStat u) => u.rawRandomValue)) / (double)(p.statsPerWeapon[0].byLevel[0].positiveTraits.Count() + p.statsPerWeapon[0].byLevel[0].negativeTraits.Count()), 
				"rerolls" => p.rerollCount, 
				"grade" => 10 - p.grade, 
				"requiredMR" => p.minimumMastery, 
				_ => p.weaponName, 
			};
		}

		public static RivenSummaryData GetSingleRivenDetails(string rivenRandomID)
		{
			return new RivenSummaryData(StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade p) => p.ItemId.oid == rivenRandomID).Select((Func<Upgrade, Miscitem>)((Upgrade p) => p)).FirstOrDefault() ?? throw new Exception("Riven not found"), isUnveiled: true);
		}

		public static string GetRivenMarketImportString(string rivenRandomID)
		{
			RivenSummaryData rivenSummaryData = null;
			Miscitem miscitem = StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade p) => p.ItemId.oid == rivenRandomID).Select((Func<Upgrade, Miscitem>)((Upgrade p) => p)).FirstOrDefault();
			if (miscitem != null)
			{
				rivenSummaryData = new RivenSummaryData(miscitem, isUnveiled: true);
			}
			if (rivenSummaryData == null)
			{
				try
				{
					rivenSummaryData = GetSingleRivenDetailsFromWFM(rivenRandomID);
				}
				catch
				{
				}
			}
			if (rivenSummaryData == null)
			{
				throw new Exception("Riven not found");
			}
			if (lastRivenMarketData == null)
			{
				string text = new MyWebClient
				{
					Proxy = null
				}.DownloadString("https://riven.market/_modules/riven/warframeData.js").Replace("var statsData = {", "").Replace("};", "");
				text = text.Substring(0, text.LastIndexOf("var weaponData = "));
				text = "{" + text + "}";
				lastRivenMarketData = JsonConvert.DeserializeObject<Dictionary<string, RivenMarketStat>>(text);
				if (lastRivenMarketData.ContainsKey("ComboGainLost"))
				{
					RivenMarketStat rivenMarketStat = lastRivenMarketData["ComboGainLost"];
					if (rivenMarketStat.Prefix.ToLower() == "laci" && rivenMarketStat.Suffix.ToLower() == "nus")
					{
						rivenMarketStat.Prefix = "";
						rivenMarketStat.Suffix = "";
					}
				}
			}
			RivenMarketRivenImport rivenMarketRivenImport = new RivenMarketRivenImport();
			rivenMarketRivenImport.MR = rivenSummaryData.minimumMastery;
			rivenMarketRivenImport.Rank = rivenSummaryData.currentImprovementLevel;
			rivenMarketRivenImport.weaponData = rivenSummaryData.weaponName.Replace(" ", "_");
			rivenMarketRivenImport.Rolls = rivenSummaryData.rerollCount;
			rivenMarketRivenImport.Positives = new Dictionary<string, string>();
			foreach (RivenUnveiledSingleStat stat in rivenSummaryData.statsPerWeapon[0].byLevel[rivenSummaryData.currentImprovementLevel].positiveTraits)
			{
				KeyValuePair<string, RivenMarketStat> keyValuePair = lastRivenMarketData.FirstOrDefault((KeyValuePair<string, RivenMarketStat> p) => (p.Value.Prefix ?? "").ToLower() + "|" + (p.Value.Suffix ?? "").ToLower() == stat.prefixSufixCombo.ToLower());
				if (keyValuePair.Value == null)
				{
					throw new Exception("Positive stat not found in riven.market data");
				}
				double value = (keyValuePair.Value.Name.Contains("Damage to") ? Math.Round(stat.currentValue + 1.0, 2) : Math.Round(stat.currentValue, 1));
				rivenMarketRivenImport.Positives.Add(keyValuePair.Key, Math.Abs(value).ToString());
			}
			rivenMarketRivenImport.Negatives = new Dictionary<string, string>();
			foreach (RivenUnveiledSingleStat stat2 in rivenSummaryData.statsPerWeapon[0].byLevel[rivenSummaryData.currentImprovementLevel].negativeTraits)
			{
				KeyValuePair<string, RivenMarketStat> keyValuePair2 = lastRivenMarketData.FirstOrDefault((KeyValuePair<string, RivenMarketStat> p) => (p.Value.Prefix ?? "").ToLower() + "|" + (p.Value.Suffix ?? "").ToLower() == stat2.prefixSufixCombo.ToLower());
				if (keyValuePair2.Value == null)
				{
					throw new Exception("Negative stat not found in riven.market data");
				}
				double value2 = (keyValuePair2.Value.Name.Contains("Damage to") ? Math.Round(stat2.currentValue + 1.0, 2) : Math.Round(stat2.currentValue, 1));
				rivenMarketRivenImport.Negatives.Add(keyValuePair2.Key, Math.Abs(value2).ToString());
			}
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rivenMarketRivenImport)));
		}

		public static GoodRollDataEvaluated GetGoodRollData(string weaponUniqueName, RivenUnveiledStats statsToMatch = null)
		{
			RivenWeaponData orDefault = StaticData.dataHandler.rivenData.weaponStats.GetOrDefault(weaponUniqueName);
			if (orDefault == null)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to fill (data not found) good rolls for: " + weaponUniqueName);
				return null;
			}
			GoodRollData goodRolls = orDefault.goodRolls;
			Dictionary<string, DataRivenStatsModifier> dictionary = StaticData.dataHandler.rivenData.dataByRivenInternalID.GetOrDefault(orDefault.rivenUID)?.rivenStats;
			GoodRollDataEvaluated goodRollDataEvaluated = new GoodRollDataEvaluated();
			foreach (string negAttr in goodRolls.acceptedBadAttrs)
			{
				bool valueOrDefault = statsToMatch?.negativeTraits?.Any((RivenUnveiledSingleStat statsPerWeapon) => statsPerWeapon.uniqueID == negAttr) == true;
				goodRollDataEvaluated.acceptedBadAttrs.Add(new GoodRollDataEvaluated.AttrEval
				{
					text = Misc.ReplaceStringWithNothing(dictionary[negAttr].shortString),
					matches = valueOrDefault
				});
			}
			goodRollDataEvaluated.goodAttrs = new List<GoodRollDataEvaluated.GoodRollEvaluated>();
			foreach (GoodRollData.GoodRoll goodAttr in goodRolls.goodAttrs)
			{
				GoodRollDataEvaluated.GoodRollEvaluated goodRollEvaluated = new GoodRollDataEvaluated.GoodRollEvaluated();
				goodRollEvaluated.mandatory = new List<GoodRollDataEvaluated.AttrEval>();
				goodRollEvaluated.optional = new List<GoodRollDataEvaluated.AttrEval>();
				foreach (string mandatoryAttr in goodAttr.mandatory)
				{
					bool valueOrDefault2 = statsToMatch?.positiveTraits?.Any((RivenUnveiledSingleStat statsPerWeapon) => statsPerWeapon.uniqueID == mandatoryAttr) == true;
					goodRollEvaluated.mandatory.Add(new GoodRollDataEvaluated.AttrEval
					{
						text = Misc.ReplaceStringWithNothing(dictionary[mandatoryAttr].shortString),
						matches = valueOrDefault2
					});
				}
				foreach (string optionalAttr in goodAttr.optional)
				{
					bool valueOrDefault3 = statsToMatch?.positiveTraits?.Any((RivenUnveiledSingleStat statsPerWeapon) => statsPerWeapon.uniqueID == optionalAttr) == true;
					goodRollEvaluated.optional.Add(new GoodRollDataEvaluated.AttrEval
					{
						text = Misc.ReplaceStringWithNothing(dictionary[optionalAttr].shortString),
						matches = valueOrDefault3
					});
				}
				goodRollDataEvaluated.goodAttrs.Add(goodRollEvaluated);
			}
			return goodRollDataEvaluated;
		}

		public static RivenSummaryData GetSingleRivenDetailsFromWFM(WFMRivenDataAuction auction)
		{
			RivenAtttrResponse value;
			WFMRivenItemsResponse value2;
			try
			{
				value = LazyWfmRivenAttrData.Value;
				value2 = LazyWfmRivenItemsData.Value;
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to get extra riven data! " + ex.Message);
				throw new Exception("Extra riven data not found");
			}
			return new RivenSummaryData(auction, value, value2);
		}

		public static RivenSummaryData GetSingleRivenDetailsFromWFM(string WFMUrlOrID)
		{
			WFMUrlOrID = WFMUrlOrID.Trim(' ', '\t', '\r', '\t');
			string text = "";
			if (WFMUrlOrID.Length == 24)
			{
				text = WFMUrlOrID;
			}
			else if (WFMUrlOrID.Contains("/auction/"))
			{
				text = WFMUrlOrID.Substring(WFMUrlOrID.IndexOf("/auction/") + 9).Trim();
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new Exception("Wrong URL format");
			}
			MyWebClient myWebClient = new MyWebClient();
			myWebClient.Proxy = null;
			WFMRivenData wFMRivenData;
			try
			{
				wFMRivenData = JsonConvert.DeserializeObject<WFMRivenData>(myWebClient.DownloadString("https://api.warframe.market/v1/auctions/entry/" + text));
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to get WFM riven data: " + text + " " + ex.Message);
				throw new Exception("Riven not found");
			}
			return GetSingleRivenDetailsFromWFM(wFMRivenData.payload.auction);
		}

		public static void ListRivenOnWFM(string rivenRandomID, string listType, string listVisibility, int wFMsellingPrice, int wFMstartingPrice, int wFMbuyoutPrice, int wFMminReputation, string wFMdescription, bool useLvl8Stats)
		{
			if (string.IsNullOrEmpty(StaticData.overwolfWrappwer.lastWFMarketToken))
			{
				throw new Exception("Please login in WFMarket first!");
			}
			if (listType == "direct")
			{
				if (wFMsellingPrice <= 0)
				{
					throw new Exception("Selling price must be greater than 0");
				}
			}
			else if (listType == "auction")
			{
				if (wFMstartingPrice <= 0)
				{
					throw new Exception("Starting price must be greater than 0");
				}
				if (wFMbuyoutPrice < 0)
				{
					throw new Exception("Buyout price must be greater or equal to 0");
				}
				if (wFMminReputation < 0)
				{
					throw new Exception("Minimum reputation must be greater or equal to 0");
				}
				if (wFMbuyoutPrice != 0 && wFMbuyoutPrice < wFMstartingPrice)
				{
					throw new Exception("Buyout price must be greater or equal to starting price");
				}
			}
			RivenSummaryData rivenSummaryData = new RivenSummaryData(StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade p) => p.ItemId.oid == rivenRandomID).Select((Func<Upgrade, Miscitem>)((Upgrade p) => p)).FirstOrDefault() ?? throw new Exception("Riven not found"), isUnveiled: true);
			MyWebClient myWebClient = new MyWebClient(37000);
			myWebClient.Proxy = null;
			WFMListRiven wFMListRiven = new WFMListRiven();
			wFMListRiven.buyout_price = ((listType == "direct") ? wFMsellingPrice : wFMbuyoutPrice);
			if (wFMListRiven.buyout_price == 0)
			{
				wFMListRiven.buyout_price = null;
			}
			wFMListRiven.starting_price = ((listType == "direct") ? wFMsellingPrice : wFMstartingPrice);
			wFMListRiven.minimal_reputation = ((!(listType == "direct")) ? wFMminReputation : 0);
			wFMListRiven.note = wFMdescription;
			wFMListRiven._private = listVisibility == "private";
			wFMListRiven.item = new WFMListRivenItem();
			wFMListRiven.item.name = rivenSummaryData.name;
			wFMListRiven.item.mastery_level = rivenSummaryData.minimumMastery;
			wFMListRiven.item.mod_rank = (useLvl8Stats ? 8 : rivenSummaryData.currentImprovementLevel);
			wFMListRiven.item.polarity = rivenSummaryData.polarity;
			wFMListRiven.item.re_rolls = rivenSummaryData.rerollCount;
			wFMListRiven.item.type = "riven";
			wFMListRiven.item.weapon_url_name = Misc.GetWarframeMarketURLName(rivenSummaryData.weaponName);
			RivenAtttrResponse rivenAtttrResponse = JsonConvert.DeserializeObject<RivenAtttrResponse>(myWebClient.DownloadString("https://api.warframe.market/v2/riven/attributes"));
			RivenUnveiledStats rivenUnveiledStats = rivenSummaryData.statsPerWeapon[0].byLevel[wFMListRiven.item.mod_rank];
			foreach (RivenUnveiledSingleStat stat in rivenUnveiledStats.positiveTraits)
			{
				wFMListRiven.item.attributes.Add(new WFMListRivenAttribute
				{
					positive = true,
					value = stat.currentValueBeingShownInUI,
					url_name = rivenAtttrResponse.data.First((RivenAtttrResponseAttribute p) => p?.prefix?.ToLower() + "|" + p?.suffix?.ToLower() == stat.prefixSufixCombo)?.slug
				});
			}
			foreach (RivenUnveiledSingleStat stat2 in rivenUnveiledStats.negativeTraits)
			{
				wFMListRiven.item.attributes.Add(new WFMListRivenAttribute
				{
					positive = false,
					value = stat2.currentValueBeingShownInUI,
					url_name = rivenAtttrResponse.data.First((RivenAtttrResponseAttribute p) => p?.prefix?.ToLower() + "|" + p?.suffix?.ToLower() == stat2.prefixSufixCombo)?.slug
				});
			}
			myWebClient.Headers.Add("Authorization", "JWT " + StaticData.overwolfWrappwer.lastWFMarketToken);
			myWebClient.Headers.Add("language", "en");
			myWebClient.Headers.Add("accept", "application/json");
			myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			myWebClient.Headers.Add("platform", "pc");
			myWebClient.Headers.Add("auth_type", "header");
			try
			{
				myWebClient.UploadString("https://api.warframe.market/v1/auctions/create", JsonConvert.SerializeObject(wFMListRiven));
			}
			catch (WebException ex)
			{
				string text = "???";
				using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
				{
					text = streamReader.ReadToEnd();
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to list riven on WFM. Error:" + ((HttpWebResponse)ex.Response).StatusCode.ToString() + ". Message: " + text);
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Request: " + JsonConvert.SerializeObject(wFMListRiven));
				}
				if (text.Contains("app.auctions.errors.auctions_limit_exceeded"))
				{
					throw new Exception("Maximum number of auctions listings reached. Check the WFMarket webpage for more details.");
				}
				throw new Exception(text);
			}
		}

		public static void EditRivenOnWFM(string wfmContractID, string listType, string listVisibility, int wFMsellingPrice, int wFMstartingPrice, int wFMbuyoutPrice, int wFMminReputation, string wFMdescription, bool useLvl8Stats)
		{
			if (string.IsNullOrEmpty(StaticData.overwolfWrappwer.lastWFMarketToken))
			{
				throw new Exception("Please login in WFMarket first!");
			}
			if (listType == "direct")
			{
				if (wFMsellingPrice <= 0)
				{
					throw new Exception("Selling price must be greater than 0");
				}
			}
			else if (listType == "auction")
			{
				if (wFMstartingPrice <= 0)
				{
					throw new Exception("Starting price must be greater than 0");
				}
				if (wFMbuyoutPrice < 0)
				{
					throw new Exception("Buyout price must be greater or equal to 0");
				}
				if (wFMminReputation < 0)
				{
					throw new Exception("Minimum reputation must be greater or equal to 0");
				}
				if (wFMbuyoutPrice != 0 && wFMbuyoutPrice < wFMstartingPrice)
				{
					throw new Exception("Buyout price must be greater or equal to starting price");
				}
			}
			WFMListRiven wFMListRiven = new WFMListRiven();
			wFMListRiven.buyout_price = ((listType == "direct") ? wFMsellingPrice : wFMbuyoutPrice);
			if (wFMListRiven.buyout_price == 0)
			{
				wFMListRiven.buyout_price = null;
			}
			wFMListRiven.starting_price = ((listType == "direct") ? wFMsellingPrice : wFMstartingPrice);
			wFMListRiven.minimal_reputation = ((!(listType == "direct")) ? wFMminReputation : 0);
			wFMListRiven.note = wFMdescription;
			wFMListRiven._private = listVisibility == "private";
			wFMListRiven.visible = !wFMListRiven._private;
			MyWebClient myWebClient = new MyWebClient(37000);
			myWebClient.Proxy = null;
			myWebClient.Headers.Add("Authorization", "JWT " + StaticData.overwolfWrappwer.lastWFMarketToken);
			myWebClient.Headers.Add("language", "en");
			myWebClient.Headers.Add("accept", "application/json");
			myWebClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			myWebClient.Headers.Add("platform", "pc");
			myWebClient.Headers.Add("auth_type", "header");
			try
			{
				myWebClient.UploadString("https://api.warframe.market/v1/auctions/entry/" + wfmContractID, "PUT", JsonConvert.SerializeObject(wFMListRiven));
			}
			catch (WebException ex)
			{
				using (StreamReader streamReader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
				{
					string text = streamReader.ReadToEnd();
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to edit riven on WFM. Error:" + ((HttpWebResponse)ex.Response).StatusCode.ToString() + ". Message: " + text);
				}
				throw;
			}
		}

		public static (double min, double max) GetMinMaxForAttribute(string attrID, string weaponUID, bool isPositive, int positiveAttrCount, int negativeAttrCount, int modLevel = 8)
		{
			RivenWeaponData rivenWeaponData = StaticData.dataHandler.rivenData.weaponStats[weaponUID];
			DataRivenStatsModifier dataRivenStatsModifier = StaticData.dataHandler.rivenData.dataByRivenInternalID[rivenWeaponData.rivenUID].rivenStats[attrID];
			MultipliersBasedOnGoodBadModifiers multipliersBasedOnGoodBadModifiers = StaticData.dataHandler.rivenData.modifiersBasedOnTraitCount.First((MultipliersBasedOnGoodBadModifiers p) => p.goodModifiersCount == positiveAttrCount && p.badModifiersCount == negativeAttrCount);
			double num = 10.0 * dataRivenStatsModifier.baseValue * rivenWeaponData.omegaAtt * (isPositive ? multipliersBasedOnGoodBadModifiers.goodModifierMultiplier : multipliersBasedOnGoodBadModifiers.badModifierMultiplier);
			num *= (double)(modLevel + 1);
			return (min: num * 0.9, max: num * 1.1);
		}

		public static List<RivenUISimilarRiven> GetSimilarRivens(RivenSimilaritySource source, BigItem baseWeaponReference, List<RivenSimilarityRequestAttribute> attributeData, RivenSimilarityRequestFilters filters, float minPeroneEqual = 0.34f)
		{
			using WebClient webClient = new MyWebClient(10000);
			webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			RivenSimilarityRequest rivenSimilarityRequest = new RivenSimilarityRequest();
			rivenSimilarityRequest.source = source;
			rivenSimilarityRequest.filters = filters;
			rivenSimilarityRequest.weaponURLName = Misc.GetWarframeMarketURLName(baseWeaponReference.name);
			rivenSimilarityRequest.attrs = attributeData.ToArray();
			RivenSimilarityResponse? rivenSimilarityResponse = JsonConvert.DeserializeObject<RivenSimilarityResponse>(webClient.UploadString(StaticData.RivenAPIHostname + "/similarRivens", JsonConvert.SerializeObject(rivenSimilarityRequest)));
			string rivenType = StaticData.dataHandler.rivenData.weaponStats.GetOrDefault(baseWeaponReference.uniqueName)?.rivenUID ?? "";
			return (from p in (from p in rivenSimilarityResponse.similarRivens
					where p.percentEqual >= (double)minPeroneEqual
					where p.price > 0
					orderby p.percentEqual descending
					select p).Take(25)
				select new RivenUISimilarRiven
				{
					source = p.source.ToString(),
					similarity = p.percentEqual,
					price = p.price,
					id = p.id,
					link = p.link,
					attrs = p.attrs.Select((RivenSimilarityResponseRivenAttribute k) => new RivenUISimilarRiven.Attr
					{
						text = Misc.ReplaceStringWithIcons(GetShortRivenAttrNameFromTag(k.name, k.positive, rivenType)),
						positive = k.positive,
						matches = k.matches
					}).ToList(),
					grade = GetRivenGradeFromRAWAttrList(baseWeaponReference.uniqueName, (from k in p.attrs
						where k.positive
						select k.name).ToList(), (from k in p.attrs
						where !k.positive
						select k.name).ToList(), out var _, out var _)
				}).ToList();
		}

		public static string GetShortRivenAttrNameFromTag(string name, bool positive, string rivenType = "")
		{
			DataRivenStatsModifier dataRivenStatsModifier = ((!string.IsNullOrEmpty(rivenType)) ? StaticData.dataHandler?.rivenData.dataByRivenInternalID[rivenType].rivenStats[name] : StaticData.dataHandler?.rivenData.dataByRivenInternalID?.Values?.FirstOrDefault((DataRivenStats p) => p.rivenStats?.ContainsKey(name) ?? false)?.rivenStats[name]);
			bool flag = dataRivenStatsModifier != null && dataRivenStatsModifier.baseValue > 0.0;
			if (!positive)
			{
				flag = !flag;
			}
			return (flag ? "+" : "-") + dataRivenStatsModifier?.shortString;
		}

		public static AlecaFrameRivenGrade GetRivenGradeFromRAWAttrList(string weaponUniqueID, List<string> goodAttrIDs, List<string> badAttrIDs, out List<AlecaFrameAttributeGrade> positiveGrades, out List<AlecaFrameAttributeGrade> negativeGrades)
		{
			positiveGrades = new List<AlecaFrameAttributeGrade>();
			for (int i = 0; i < goodAttrIDs.Count; i++)
			{
				positiveGrades.Add(AlecaFrameAttributeGrade.Unknown);
			}
			negativeGrades = new List<AlecaFrameAttributeGrade>();
			for (int j = 0; j < badAttrIDs.Count; j++)
			{
				negativeGrades.Add(AlecaFrameAttributeGrade.Unknown);
			}
			if (!StaticData.dataHandler.rivenData.weaponStats.TryGetValue(weaponUniqueID, out var value))
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find weapon data when grading for: " + weaponUniqueID);
				return AlecaFrameRivenGrade.Unknown;
			}
			if (value.goodRolls == null)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find good rolls (no data) for: " + weaponUniqueID);
				return AlecaFrameRivenGrade.Unknown;
			}
			List<GoodRollData.GoodRoll> goodAttrs = value.goodRolls.goodAttrs;
			IEnumerable<GoodRollData.GoodRoll> source = from p in goodAttrs
				where p.mandatory.All((string k) => goodAttrIDs.Contains(k))
				where p.mandatory.Count((string k) => goodAttrIDs.Contains(k)) + p.optional.Count((string k) => goodAttrIDs.Contains(k)) == goodAttrIDs.Count()
				select p;
			int i2;
			for (i2 = 0; i2 < badAttrIDs.Count; i2++)
			{
				if (value.goodRolls.acceptedBadAttrs.Contains(badAttrIDs[i2]))
				{
					negativeGrades[i2] = AlecaFrameAttributeGrade.Good;
				}
				else if (goodAttrs.Any((GoodRollData.GoodRoll k) => k.mandatory.Contains(badAttrIDs[i2]) || k.optional.Contains(badAttrIDs[i2])))
				{
					negativeGrades[i2] = AlecaFrameAttributeGrade.Bad;
				}
				else
				{
					negativeGrades[i2] = AlecaFrameAttributeGrade.NotHelping;
				}
			}
			int i3;
			for (i3 = 0; i3 < goodAttrIDs.Count; i3++)
			{
				if (goodAttrs.Any((GoodRollData.GoodRoll k) => k.mandatory.Contains(goodAttrIDs[i3])))
				{
					positiveGrades[i3] = AlecaFrameAttributeGrade.Decisive;
				}
				else if (goodAttrs.Any((GoodRollData.GoodRoll k) => k.optional.Contains(goodAttrIDs[i3])))
				{
					positiveGrades[i3] = AlecaFrameAttributeGrade.Good;
				}
				else
				{
					positiveGrades[i3] = AlecaFrameAttributeGrade.NotHelping;
				}
			}
			int num = positiveGrades.Count((AlecaFrameAttributeGrade p) => p == AlecaFrameAttributeGrade.Good || p == AlecaFrameAttributeGrade.Decisive);
			bool flag = source.Count() > 0;
			if (negativeGrades.Any((AlecaFrameAttributeGrade p) => p == AlecaFrameAttributeGrade.Bad))
			{
				if ((flag && num >= 2) || num >= 3)
				{
					return AlecaFrameRivenGrade.HasPotential;
				}
				return AlecaFrameRivenGrade.Bad;
			}
			if (negativeGrades.Any((AlecaFrameAttributeGrade p) => p == AlecaFrameAttributeGrade.NotHelping))
			{
				if (flag || num >= 2)
				{
					return AlecaFrameRivenGrade.Good;
				}
				if (num >= 1)
				{
					return AlecaFrameRivenGrade.HasPotential;
				}
				return AlecaFrameRivenGrade.Bad;
			}
			if (flag)
			{
				if (num >= 2 && negativeGrades.Any())
				{
					return AlecaFrameRivenGrade.Perfect;
				}
				return AlecaFrameRivenGrade.Good;
			}
			if (num >= 2)
			{
				return AlecaFrameRivenGrade.Good;
			}
			if (num >= 1)
			{
				return AlecaFrameRivenGrade.HasPotential;
			}
			return AlecaFrameRivenGrade.Bad;
		}
	}
}
