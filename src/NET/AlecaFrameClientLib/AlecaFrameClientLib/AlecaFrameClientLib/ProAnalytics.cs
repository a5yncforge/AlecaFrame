using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using AlecaFramePublicLib.DataTypes;
using Newtonsoft.Json;

namespace AlecaFrameClientLib
{
	public static class ProAnalytics
	{
		public class ProAnalyticsFilters
		{
			public string bestItemsOrder = "value";

			public string topSalesOrder = "value";

			public string topPurchasesOrder = "value";

			public string tradePartnersOrder = "salesValue";

			public string statementSummaryOrder = "profit";
		}

		public class ProAnalyticsData
		{
			public class ProAnalyticsWFMItem
			{
				public string name;

				public string url_name;

				public string image;

				public string value;

				public int unitPrice;

				public int volume;

				public int totalValue;

				public string user;

				public string itemType;
			}

			public class ProAnalyticsUserData
			{
				public string name;

				public int purchaseCount;

				public int purchaseValue;

				public int saleCount;

				public int saleValue;

				public int totalCount;

				public int totalValue;
			}

			public class ProAnalyticsStatement
			{
				public class ItemStatement
				{
					public int totalPlatReceived;

					public int totalPlatSpent;

					public int totalPlatinumProfits;

					public string type;

					public int percentReceived;

					public int percentSpent;

					public int graphValue;
				}

				public ItemStatement general;

				public int expectedtotalPlatReceivedAtCurrentPrices;

				public string deltaExpectedtotalPlatReceivedAtCurrentPrices;

				public List<ItemStatement> statementByType;

				public ProAnalyticsWFMItem largestSale;

				public ProAnalyticsWFMItem largestPurchase;
			}

			public class ProAnalyticsPieChart
			{
				public string name;

				public int value;
			}

			internal class TempTradeData
			{
				public string Name { get; }

				public string ItemType { get; }

				public string Pic { get; }

				public string Url_name { get; }

				public int Plat { get; }

				public string Person { get; }

				public TradeClassification Type { get; }

				public int AmountInTrade { get; }

				public TempTradeData(string itemName, string itemType, string itemPicture, string itemURLName, int platAmount, string person, TradeClassification type, int amountInTrade)
				{
					Name = itemName;
					ItemType = itemType;
					Pic = itemPicture;
					Url_name = itemURLName;
					Plat = platAmount;
					Person = person;
					Type = type;
					AmountInTrade = amountInTrade;
				}
			}

			public List<ProAnalyticsWFMItem> bestItemsForTrading;

			public List<ProAnalyticsPieChart> bestItemsForTradingCategories;

			public List<ProAnalyticsWFMItem> mostBoughtItems;

			public List<ProAnalyticsWFMItem> mostSoldItems;

			public List<ProAnalyticsUserData> mostCommonTradePartners;

			public ProAnalyticsStatement proAnalyticsStatement;
		}

		private static PlayerStatsData lastPlayerStatsData;

		public static ProAnalyticsData GetProAnalytics(ProAnalyticsFilters filters)
		{
			ProAnalyticsData proAnalyticsData = new ProAnalyticsData();
			Dictionary<string, WFMItemListItem> wfmItems = StaticData.LazyWfmItemData.Value.data.ToDictionary((WFMItemListItem p) => p.slug);
			Dictionary<string, OverwolfWrapper.ItemPriceSmallResponse> priceDataByURLname = new Dictionary<string, OverwolfWrapper.ItemPriceSmallResponse>();
			foreach (WFMItemListItem value in wfmItems.Values)
			{
				priceDataByURLname.Add(value.slug, PriceHelper.GetLazyItemPrice(value.slug));
			}
			PriceHelper.Flush(TimeSpan.FromSeconds(7.0));
			proAnalyticsData.bestItemsForTrading = (from x in wfmItems.Values.OrderByDescending((WFMItemListItem x) => OrderBestItemsForTrading(x, priceDataByURLname, filters.bestItemsOrder)).Take(50)
				select new ProAnalyticsData.ProAnalyticsWFMItem
				{
					name = x.item_name,
					url_name = x.slug,
					image = "https://warframe.market/static/assets/" + x.i18n.en.thumb,
					value = (priceDataByURLname[x.slug].post * priceDataByURLname[x.slug].volume).ToString(),
					unitPrice = priceDataByURLname[x.slug].post.GetValueOrDefault(),
					totalValue = (priceDataByURLname[x.slug].post * priceDataByURLname[x.slug].volume).Value,
					volume = priceDataByURLname[x.slug].volume.Value,
					itemType = GetItemType(x.item_name, x.item_name)
				}).ToList();
			proAnalyticsData.bestItemsForTradingCategories = (from p in wfmItems.Values
				group p by GetItemType(p.item_name, p.item_name) into p
				select new ProAnalyticsData.ProAnalyticsPieChart
				{
					name = p.Key.FirstCharToUpper().Replace("Riven", "Riven (Veiled)").Replace("Prime", "Prime part"),
					value = p.Sum((WFMItemListItem x) => OrderBestItemsForTrading(x, priceDataByURLname, filters.bestItemsOrder))
				}).ToList();
			string playerUserHash = StatsHandler.GetPlayerUserHash();
			string tokenSecret = StatsHandler.GetTokenSecret();
			try
			{
				if (playerUserHash != null && tokenSecret != null && StaticData.statsTabEnabled)
				{
					PlayerStatsData playerStatsData = lastPlayerStatsData;
					if (playerStatsData == null)
					{
						playerStatsData = (lastPlayerStatsData = JsonConvert.DeserializeObject<PlayerStatsData>(HTTPHandler.MakeGETRequest(StaticData.StatsAPIHostname + "/api/stats/" + playerUserHash + "?secretToken=" + tokenSecret, 50000, 1)));
					}
					if (playerStatsData != null)
					{
						var source = (from p in lastPlayerStatsData.trades
							where p.type == TradeClassification.Purchase
							select new
							{
								type = TradeClassification.Purchase,
								platAmount = p.tx[0].cnt,
								item = p.rx,
								person = p.user,
								date = p.ts
							}).Union(from p in lastPlayerStatsData.trades
							where p.type == TradeClassification.Sale
							select new
							{
								type = TradeClassification.Sale,
								platAmount = p.rx[0].cnt,
								item = p.tx,
								person = p.user,
								date = p.ts
							}).ToList();
						List<WFMItemListItem> wfmItemsSets = wfmItems.Values.Where((WFMItemListItem p) => p.item_name.EndsWith("Set")).ToList();
						List<ProAnalyticsData.TempTradeData> source2 = (from p in source.Select(p =>
							{
								string name = p.item[0].name;
								string text = name;
								if (text.StartsWith("/AF_Special/Riven"))
								{
									text = "/AF_Special/Riven";
								}
								BasicRemoteDataItemData basicData = StaticData.dataHandler?.basicRemoteData?.items?.GetOrDefault(text);
								if (basicData == null)
								{
									return (ProAnalyticsData.TempTradeData)null;
								}
								string text2 = basicData.name;
								string text3 = "";
								text3 = ((!basicData.pic.StartsWith("http")) ? Misc.GetFullImagePath(basicData.pic) : basicData.pic);
								if (name.StartsWith("/AF_Special/Riven/"))
								{
									text2 = name.Replace("/AF_Special/Riven/", "").Replace("/", " ");
								}
								string text4 = Misc.GetWarframeMarketURLName(text2);
								if (p.item.Count > 1)
								{
									string text5 = wfmItemsSets.FirstOrDefault((WFMItemListItem x) => basicData.name.Contains(x.item_name.Replace(" Set", "")))?.slug;
									if (text5 != null)
									{
										text4 = text5;
									}
								}
								if (text4.EndsWith("_riven_mod"))
								{
									text4 += "_(veiled)";
								}
								WFMItemListItem orDefault = wfmItems.GetOrDefault(text4);
								if (orDefault != null)
								{
									text2 = orDefault.item_name;
									text3 = "https://warframe.market/static/assets/" + orDefault.i18n.en.thumb;
								}
								text2.Contains("Relic");
								return new ProAnalyticsData.TempTradeData(text2, GetItemType(text2, text), text3, text4, p.platAmount, p.person, p.type, p.item[0].cnt);
							})
							where p != null
							select p).ToList();
						proAnalyticsData.mostBoughtItems = (from p in source2
							where p.Type == TradeClassification.Purchase
							group p by p.Url_name into p
							select new ProAnalyticsData.ProAnalyticsWFMItem
							{
								name = p.First().Name,
								url_name = p.First().Url_name,
								image = p.First().Pic,
								value = p.Sum((ProAnalyticsData.TempTradeData x) => x.Plat).ToString(),
								unitPrice = (int)p.Average((ProAnalyticsData.TempTradeData x) => (double)x.Plat / (double)x.AmountInTrade),
								totalValue = p.Sum((ProAnalyticsData.TempTradeData x) => x.Plat),
								volume = p.Sum((ProAnalyticsData.TempTradeData x) => x.AmountInTrade)
							} into p
							orderby OrderMostBoughtOrSoldItems(p, filters.topPurchasesOrder) descending
							select p).Take(35).ToList();
						proAnalyticsData.mostSoldItems = (from p in source2
							where p.Type == TradeClassification.Sale
							group p by p.Url_name into p
							select new ProAnalyticsData.ProAnalyticsWFMItem
							{
								name = p.First().Name,
								url_name = p.First().Url_name,
								image = p.First().Pic,
								value = p.Sum((ProAnalyticsData.TempTradeData x) => x.Plat).ToString(),
								unitPrice = (int)p.Average((ProAnalyticsData.TempTradeData x) => (double)x.Plat / (double)x.AmountInTrade),
								totalValue = p.Sum((ProAnalyticsData.TempTradeData x) => x.Plat),
								volume = p.Sum((ProAnalyticsData.TempTradeData x) => x.AmountInTrade)
							} into p
							orderby OrderMostBoughtOrSoldItems(p, filters.topSalesOrder) descending
							select p).Take(35).ToList();
						List<ProAnalyticsData.TempTradeData> list = source2.Where((ProAnalyticsData.TempTradeData p) => p.Type == TradeClassification.Purchase).ToList();
						List<ProAnalyticsData.TempTradeData> list2 = source2.Where((ProAnalyticsData.TempTradeData p) => p.Type == TradeClassification.Sale).ToList();
						proAnalyticsData.mostCommonTradePartners = (from p in source2
							group p by p.Person.Substring(0, p.Person.Length - 1) into p
							select new ProAnalyticsData.ProAnalyticsUserData
							{
								name = p.First().Person.Substring(0, p.First().Person.Length - 1),
								purchaseCount = p.Where((ProAnalyticsData.TempTradeData x) => x.Type == TradeClassification.Purchase).Count(),
								purchaseValue = p.Where((ProAnalyticsData.TempTradeData x) => x.Type == TradeClassification.Purchase).Sum((ProAnalyticsData.TempTradeData x) => x.Plat),
								saleCount = p.Where((ProAnalyticsData.TempTradeData x) => x.Type == TradeClassification.Sale).Count(),
								saleValue = p.Where((ProAnalyticsData.TempTradeData x) => x.Type == TradeClassification.Sale).Sum((ProAnalyticsData.TempTradeData x) => x.Plat),
								totalCount = p.Count(),
								totalValue = p.Sum((ProAnalyticsData.TempTradeData x) => x.Plat)
							} into p
							orderby OrderMostCommonTraders(p, filters.tradePartnersOrder) descending
							select p).Take(50).ToList();
						proAnalyticsData.proAnalyticsStatement = new ProAnalyticsData.ProAnalyticsStatement();
						proAnalyticsData.proAnalyticsStatement.general = GenerateStatementDetails(list, list2);
						proAnalyticsData.proAnalyticsStatement.statementByType = new List<ProAnalyticsData.ProAnalyticsStatement.ItemStatement>
						{
							GenerateStatementDetails(list, list2, "riven", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "set", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "arcane", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "mod", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "prime", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "relic", proAnalyticsData.proAnalyticsStatement.general),
							GenerateStatementDetails(list, list2, "other", proAnalyticsData.proAnalyticsStatement.general)
						}.OrderByDescending((ProAnalyticsData.ProAnalyticsStatement.ItemStatement p) => OrderStatement(p, filters.statementSummaryOrder)).ToList();
						foreach (ProAnalyticsData.ProAnalyticsStatement.ItemStatement item in proAnalyticsData.proAnalyticsStatement.statementByType)
						{
							item.graphValue = OrderStatement(item, filters.statementSummaryOrder);
						}
						proAnalyticsData.proAnalyticsStatement.expectedtotalPlatReceivedAtCurrentPrices = list2.Sum((ProAnalyticsData.TempTradeData p) => (priceDataByURLname.GetOrDefault(p.Url_name)?.post * p.AmountInTrade) ?? p.Plat);
						proAnalyticsData.proAnalyticsStatement.deltaExpectedtotalPlatReceivedAtCurrentPrices = (proAnalyticsData.proAnalyticsStatement.expectedtotalPlatReceivedAtCurrentPrices - proAnalyticsData.proAnalyticsStatement.general.totalPlatReceived).ToString();
						proAnalyticsData.proAnalyticsStatement.largestSale = (from p in list2
							orderby p.Plat descending
							select new ProAnalyticsData.ProAnalyticsWFMItem
							{
								name = p.Name,
								url_name = p.Url_name,
								image = p.Pic,
								value = p.Plat.ToString(),
								unitPrice = p.Plat,
								totalValue = p.Plat,
								volume = p.AmountInTrade,
								user = p.Person
							}).FirstOrDefault();
						proAnalyticsData.proAnalyticsStatement.largestPurchase = (from p in list
							orderby p.Plat descending
							select new ProAnalyticsData.ProAnalyticsWFMItem
							{
								name = p.Name,
								url_name = p.Url_name,
								image = p.Pic,
								value = p.Plat.ToString(),
								unitPrice = p.Plat,
								totalValue = p.Plat,
								volume = p.AmountInTrade,
								user = p.Person
							}).FirstOrDefault();
					}
				}
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has occurred while getting pro analytics: " + ex.Message);
			}
			return proAnalyticsData;
		}

		public static void NewTradeMade()
		{
			lastPlayerStatsData = null;
		}

		private static int OrderStatement(ProAnalyticsData.ProAnalyticsStatement.ItemStatement p, string statementSummaryOrder)
		{
			return statementSummaryOrder switch
			{
				"revenue" => p.totalPlatReceived, 
				"expenses" => p.totalPlatSpent, 
				_ => p.totalPlatinumProfits, 
			};
		}

		private static int OrderMostBoughtOrSoldItems(ProAnalyticsData.ProAnalyticsWFMItem p, string topSalesOrder)
		{
			return topSalesOrder switch
			{
				"volume" => p.volume, 
				"unitPrice" => p.unitPrice, 
				_ => p.totalValue, 
			};
		}

		private static int OrderMostCommonTraders(ProAnalyticsData.ProAnalyticsUserData p, string tradePartnersOrder)
		{
			return tradePartnersOrder switch
			{
				"totalValue" => p.totalValue, 
				"totalCount" => p.totalCount, 
				"saleValue" => p.saleValue, 
				"saleCount" => p.saleCount, 
				"purchaseValue" => p.purchaseValue, 
				"purchaseCount" => p.purchaseCount, 
				_ => p.totalValue, 
			};
		}

		private static int OrderBestItemsForTrading(WFMItemListItem x, Dictionary<string, OverwolfWrapper.ItemPriceSmallResponse> priceDataByURLname, string bestItemsOrder)
		{
			return bestItemsOrder switch
			{
				"volume" => priceDataByURLname[x.slug].volume.GetValueOrDefault(), 
				"unitPrice" => priceDataByURLname[x.slug].post.GetValueOrDefault(), 
				_ => priceDataByURLname[x.slug].post.GetValueOrDefault() * priceDataByURLname[x.slug].volume.GetValueOrDefault(), 
			};
		}

		private static ProAnalyticsData.ProAnalyticsStatement.ItemStatement GenerateStatementDetails(IEnumerable<ProAnalyticsData.TempTradeData> purchases, IEnumerable<ProAnalyticsData.TempTradeData> sales, string filter = "all", ProAnalyticsData.ProAnalyticsStatement.ItemStatement generalData = null)
		{
			List<ProAnalyticsData.TempTradeData> source = purchases.Where((ProAnalyticsData.TempTradeData p) => filter == "all" || p.ItemType == filter).ToList();
			List<ProAnalyticsData.TempTradeData> source2 = sales.Where((ProAnalyticsData.TempTradeData p) => filter == "all" || p.ItemType == filter).ToList();
			ProAnalyticsData.ProAnalyticsStatement.ItemStatement itemStatement = new ProAnalyticsData.ProAnalyticsStatement.ItemStatement
			{
				totalPlatReceived = source2.Sum((ProAnalyticsData.TempTradeData p) => p.Plat),
				totalPlatSpent = source.Sum((ProAnalyticsData.TempTradeData p) => p.Plat)
			};
			if (generalData != null)
			{
				itemStatement.percentReceived = (int)((double)(100 * source2.Sum((ProAnalyticsData.TempTradeData p) => p.Plat)) / (double)generalData.totalPlatReceived);
				itemStatement.percentSpent = (int)((double)(100 * source.Sum((ProAnalyticsData.TempTradeData p) => p.Plat)) / (double)generalData.totalPlatSpent);
			}
			itemStatement.totalPlatinumProfits = itemStatement.totalPlatReceived - itemStatement.totalPlatSpent;
			itemStatement.type = filter.FirstCharToUpper();
			if (filter == "all")
			{
				itemStatement.type = "general";
			}
			return itemStatement;
		}

		private static string GetItemType(string itemName, string correctedItemUID)
		{
			if (correctedItemUID.StartsWith("/AF_Special/Riven"))
			{
				return "riven";
			}
			if (itemName.EndsWith(" Set"))
			{
				return "set";
			}
			if (itemName.Contains("Prime"))
			{
				return "prime";
			}
			if (itemName.Contains("Riven"))
			{
				return "riven";
			}
			if (itemName.Contains("Arcane"))
			{
				return "arcane";
			}
			if (itemName.Contains("Relic"))
			{
				return "relic";
			}
			DataHandler dataHandler = StaticData.dataHandler;
			if (dataHandler != null && dataHandler.mods.Any((KeyValuePair<string, DataMod> p) => p.Value.name == itemName))
			{
				return "mod";
			}
			return "other";
		}
	}
}
