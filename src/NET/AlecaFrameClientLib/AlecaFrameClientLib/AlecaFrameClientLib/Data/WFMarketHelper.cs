using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.WFM;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using AlecaFramePublicLib.DataTypes;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public static class WFMarketHelper
	{
		public class ActiveTradeFinishedNotificationData
		{
			public string listingOrContractID = "";

			public bool isContract;

			public string remoteUsername = "";

			public string itemName = "";

			public string image = "";

			public int itemAmount = 1;
		}

		public static List<WFMarketOrderData> GetCurrentOrders(string typeToShow, Dictionary<string, string> yesNoFilters, string orderingType, bool orderedFromLargerToSmaller, string searchPrompt)
		{
			WFMarketProfileOrderList wFMarketOrders = StaticData.overwolfWrappwer.WFMarketOrders;
			if (wFMarketOrders?.data == null)
			{
				return new List<WFMarketOrderData>();
			}
			List<WFMarketProfileOrder> source = wFMarketOrders.data.Where((WFMarketProfileOrder p) => ShouldShowWFMOrderBasedOnTypeAndUID(p, typeToShow)).ToList();
			List<SetItemData> sets = new List<SetItemData>();
			if (typeToShow == "all" || typeToShow == "sets")
			{
				sets = InventoryHelpers.GetInventorySets(new Dictionary<string, string>(), "", out var _, out var _, "name", orderedFromLargerToSmaller: true, getPrices: false).ToList();
			}
			IEnumerable<WFMarketOrderData> source2 = source.Select((WFMarketProfileOrder p) => new WFMarketOrderData(p, GetItemGroup(p), sets)).ToList();
			source2 = GetPrices(source2.ToList()).ToList();
			source2 = source2.Where((WFMarketOrderData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters));
			source2 = source2.Where((WFMarketOrderData p) => ShouldShowOrderBasedOnSearch(p, searchPrompt));
			source2 = ((!orderedFromLargerToSmaller) ? (from p in source2
				orderby OrderOrderBasedOnCriteria(p, orderingType), p.name
				select p) : (from p in source2
				orderby OrderOrderBasedOnCriteria(p, orderingType) descending, p.name
				select p));
			return source2.ToList();
		}

		private static bool ShouldShowInventoryItemBasedOnFilters(WFMarketOrderData p, Dictionary<string, string> yesNoFilters)
		{
			if (yesNoFilters.ContainsKey("type"))
			{
				if (yesNoFilters["type"] == "sell" && !p.isSellOrder)
				{
					return false;
				}
				if (yesNoFilters["type"] == "buy" && p.isSellOrder)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("missing"))
			{
				if (yesNoFilters["missing"] == "yes" && !p.showWarning)
				{
					return false;
				}
				if (yesNoFilters["missing"] == "no" && p.showWarning)
				{
					return false;
				}
			}
			return true;
		}

		private static object OrderOrderBasedOnCriteria(WFMarketOrderData p, string orderingType)
		{
			return orderingType switch
			{
				"platinum" => p.platinumPerItem, 
				"amount" => p.amountOnSale, 
				_ => p.name, 
			};
		}

		private static bool ShouldShowOrderBasedOnSearch(WFMarketOrderData p, string searchPrompt)
		{
			return p.name.ToLower().Contains(searchPrompt.ToLower());
		}

		private static string GetItemGroup(WFMarketProfileOrder wfmItem)
		{
			WFMItemListItem itemData = StaticData.LazyWfmItemData.Value.AsDictionaryByID.GetOrDefault(wfmItem.itemId);
			if (itemData.tags.Any((string p) => p == "component" || p == "blueprint" || itemData.slug.Contains("kavasa")))
			{
				return "parts";
			}
			if (itemData.tags.Any((string p) => p == "relic"))
			{
				return "relics";
			}
			if (itemData.tags.Any((string p) => p == "mod"))
			{
				return "mods";
			}
			if (itemData.tags.Any((string p) => p == "arcane" || p == "arcane_enhancement"))
			{
				return "arcanes";
			}
			if (itemData.tags.Any((string p) => p == "set"))
			{
				return "sets";
			}
			return "misc";
		}

		private static bool ShouldShowWFMOrderBasedOnTypeAndUID(WFMarketProfileOrder wfmItem, string typeToShow)
		{
			if (typeToShow == "all")
			{
				return true;
			}
			return GetItemGroup(wfmItem) == typeToShow;
		}

		public static IEnumerable<WFMarketOrderData> GetPrices(List<WFMarketOrderData> input)
		{
			string[] itemListNames = input.Select((WFMarketOrderData p) => p.itemData.slug).ToArray();
			OverwolfWrapper.ItemPriceSmallResponse[] prices = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(itemListNames, TimeSpan.FromSeconds(7.0));
			return input.Select(delegate(WFMarketOrderData p, int index)
			{
				p.lowestSalePrice = (prices[index]?.minR0).GetValueOrDefault();
				if (p.itemType == "mods" || p.itemType == "arcanes")
				{
					if (p.modLevel > 0)
					{
						p.shouldDisplayPlusInPrice = true;
						if (int.TryParse(p.itemData.maxRank, out var result) && p.modLevel == result && prices[index].minRMax > 0)
						{
							p.lowestSalePrice = prices[index].minRMax.GetValueOrDefault();
							p.shouldDisplayPlusInPrice = false;
						}
					}
					if (p.lowestSalePrice <= 0)
					{
						p.shouldDisplayPlusInPrice = false;
					}
				}
				return p;
			});
		}

		public static List<WFMarketContractData> GetCurrentContracts(string typeToShow, Dictionary<string, string> yesNoFilters, string orderingType, bool orderedFromLargerToSmaller, string searchPrompt)
		{
			WFMRivenDataAuction[] source = (StaticData.overwolfWrappwer.WFMarketContracts?.payload?.auctions ?? new WFMRivenDataAuction[0]).Where((WFMRivenDataAuction p) => p.item?.type == "riven").ToArray();
			IEnumerable<RivenSummaryData> unveiledRivens = RivenExplorerHelper.GetUnveiledRivens("all", new Dictionary<string, string>(), "name", orderedFromLargerToSmaller: true, "");
			IEnumerable<WFMarketContractData> source2 = source.Select((WFMRivenDataAuction p) => new WFMarketContractData(p, unveiledRivens)).ToList();
			source2 = source2.Where((WFMarketContractData p) => ShouldShowContractBasedOnFilters(p, typeToShow, yesNoFilters));
			source2 = source2.Where((WFMarketContractData p) => ShouldShowContractBasedOnSearch(p, searchPrompt));
			source2 = ((!orderedFromLargerToSmaller) ? (from p in source2
				orderby OrderContractsBasedOnCriteria(p, orderingType), p.name
				select p) : (from p in source2
				orderby OrderContractsBasedOnCriteria(p, orderingType) descending, p.name
				select p));
			return source2.ToList();
		}

		private static bool ShouldShowContractBasedOnFilters(WFMarketContractData p, string typeToShow, Dictionary<string, string> yesNoFilters)
		{
			if (typeToShow != "all" && p.weaponType.ToLower() != typeToShow)
			{
				return false;
			}
			if (yesNoFilters.ContainsKey("missing"))
			{
				if (yesNoFilters["missing"] == "yes" && !p.showWarning)
				{
					return false;
				}
				if (yesNoFilters["missing"] == "no" && p.showWarning)
				{
					return false;
				}
			}
			return true;
		}

		private static object OrderContractsBasedOnCriteria(WFMarketContractData p, string orderingType)
		{
			return orderingType switch
			{
				"platinum" => p.orderingPrice, 
				"weapon" => p.weaponName, 
				_ => p.name, 
			};
		}

		private static bool ShouldShowContractBasedOnSearch(WFMarketContractData p, string searchPrompt)
		{
			if (string.IsNullOrWhiteSpace(searchPrompt))
			{
				return true;
			}
			string value = searchPrompt.ToLower();
			if (!p.name.ToLower().Contains(value) && !p.weaponName.ToLower().Contains(value) && !p.stats.positiveTraits.Any((RivenUnveiledSingleStat u) => u.noMarkupDescription.ToLower().Contains(searchPrompt)))
			{
				return p.stats.negativeTraits.Any((RivenUnveiledSingleStat u) => u.noMarkupDescription.ToLower().Contains(searchPrompt));
			}
			return true;
		}

		public static void ItemsWereJustTraded(List<PlayerStatsTradeTradedObjectInfo> itemsUserSideList, string remoteUsername, bool selling, int platinumInTrade, int modArcaneRankGuess = -1)
		{
			string finalItemName = "";
			string finalItemInternalName = "";
			int num = 1;
			if (itemsUserSideList.Count > 1)
			{
				List<(ItemComponent, int)> list = new List<(ItemComponent, int)>();
				foreach (PlayerStatsTradeTradedObjectInfo itemsUserSide in itemsUserSideList)
				{
					ItemComponent itemComponent = null;
					if (itemComponent == null)
					{
						itemComponent = StaticData.dataHandler.warframeParts.GetOrDefault(itemsUserSide.name);
					}
					if (itemComponent == null)
					{
						itemComponent = StaticData.dataHandler.weaponParts.GetOrDefault(itemsUserSide.name);
					}
					list.Add((itemComponent, itemsUserSide.cnt));
				}
				BigItem setItem = list.FirstOrDefault().Item1?.isPartOf;
				if (setItem != null && list.All<(ItemComponent, int)>(((ItemComponent item, int count) p) => p.item != null && p.item.isPartOf?.uniqueName == setItem.uniqueName))
				{
					int num2 = list.Min<(ItemComponent, int)>(((ItemComponent item, int count) p) => p.count);
					if (setItem.name.ToLower().Contains("dual decurion"))
					{
						num2 /= 2;
					}
					finalItemName = Misc.GetSetName(setItem);
					finalItemInternalName = setItem.uniqueName;
					num = num2;
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Sold items conversion to set successful");
				}
				else
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Sold items conversion to set failed");
				}
			}
			else
			{
				finalItemName = itemsUserSideList.FirstOrDefault()?.displayName;
				num = itemsUserSideList.FirstOrDefault()?.cnt ?? 0;
				finalItemInternalName = itemsUserSideList.FirstOrDefault()?.name;
			}
			if (!string.IsNullOrWhiteSpace(finalItemName) && !string.IsNullOrWhiteSpace(finalItemInternalName) && num > 0)
			{
				if (num > 6)
				{
					num = 6;
				}
				if (StaticData.overwolfWrappwer.WFMarketOrders == null || StaticData.overwolfWrappwer.WFMarketContracts == null)
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Trade finisher won't do anything because the user is not logged in WFMarket");
					return;
				}
				ActiveTradeFinishedNotificationData activeTradeFinishedNotificationData = new ActiveTradeFinishedNotificationData();
				activeTradeFinishedNotificationData.remoteUsername = remoteUsername.Substring(0, remoteUsername.Length - 1);
				if (finalItemInternalName.Contains("/AF_Special/Riven/"))
				{
					if (!selling)
					{
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Purchase of Rivens are not supported (don't make sense) in WFMarket trade completed handler.");
						return;
					}
					WFMRivenDataAuction wFMRivenDataAuction = StaticData.overwolfWrappwer.WFMarketContracts?.payload?.auctions?.FirstOrDefault((WFMRivenDataAuction p) => p.item.weapon_url_name + "_" + Misc.GetWarframeMarketURLName(p.item.name.ToLower()) == Misc.GetWarframeMarketURLName(finalItemInternalName.Replace("/AF_Special/Riven/", "").Replace("/", " ").ToLower()));
					if (wFMRivenDataAuction == null)
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find contract for trade item: " + finalItemInternalName);
						return;
					}
					activeTradeFinishedNotificationData.isContract = true;
					activeTradeFinishedNotificationData.listingOrContractID = wFMRivenDataAuction.id;
					activeTradeFinishedNotificationData.image = "https://cdn.alecaframe.com/warframeData/custom/imgRemote/riven.png";
					activeTradeFinishedNotificationData.itemName = finalItemInternalName.Replace("/AF_Special/Riven/", "").Replace("/", " ");
					activeTradeFinishedNotificationData.itemAmount = num;
				}
				else
				{
					WFMarketProfileOrder wFMarketProfileOrder = null;
					wFMarketProfileOrder = ((!selling) ? (from p in StaticData.overwolfWrappwer.WFMarketOrders?.data?.Where((WFMarketProfileOrder p) => p.type == "buy" && p.GetItemDataObject()?.item_name.Replace(" Blueprint", "").ToLower() == finalItemName.Replace(" Blueprint", "").ToLower())
						orderby Math.Abs(p.platinum - platinumInTrade), (modArcaneRankGuess != -1 && int.TryParse(p.rank, out var result)) ? Math.Abs(result - modArcaneRankGuess) : 0, p.quantity
						select p).FirstOrDefault() : (from p in StaticData.overwolfWrappwer.WFMarketOrders?.data?.Where((WFMarketProfileOrder p) => p.type == "sell" && p.GetItemDataObject()?.item_name.Replace(" Blueprint", "").ToLower() == finalItemName.Replace(" Blueprint", "").ToLower())
						orderby Math.Abs(p.platinum - platinumInTrade), (modArcaneRankGuess != -1 && int.TryParse(p.rank, out var result)) ? Math.Abs(result - modArcaneRankGuess) : 0, p.quantity
						select p).FirstOrDefault());
					if (wFMarketProfileOrder == null)
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find listing for trade item: " + finalItemInternalName);
						return;
					}
					WFMItemListItem itemDataObject = wFMarketProfileOrder.GetItemDataObject();
					activeTradeFinishedNotificationData.isContract = false;
					activeTradeFinishedNotificationData.listingOrContractID = wFMarketProfileOrder.id;
					if (!string.IsNullOrEmpty(itemDataObject.i18n.en.thumb))
					{
						activeTradeFinishedNotificationData.image = "https://warframe.market/static/assets/" + itemDataObject.i18n.en.thumb;
					}
					else
					{
						activeTradeFinishedNotificationData.image = "https://warframe.market/static/assets/" + itemDataObject.i18n.en.icon;
					}
					activeTradeFinishedNotificationData.itemName = itemDataObject.item_name;
					activeTradeFinishedNotificationData.itemAmount = num;
				}
				StaticData.overwolfWrappwer.TradeFinishedNotificationData = JsonConvert.SerializeObject(activeTradeFinishedNotificationData);
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Showing trade finished notification data: " + JsonConvert.SerializeObject(activeTradeFinishedNotificationData));
				StaticData.overwolfWrappwer.onTradeFinishedNotificationCaller();
			}
			else
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to identify items for WFMarket trade finisher!");
			}
		}
	}
}
