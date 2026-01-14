using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public static class MasteryHelper
	{
		public static MasteryResponse GetMasteryData(string orderingMode)
		{
			MasteryResponse masteryResponse = new MasteryResponse();
			masteryResponse.iconurl = Misc.GetMasteryLevelIcon(StaticData.dataHandler.warframeRootObject.PlayerLevel);
			masteryResponse.lvl = StaticData.dataHandler.warframeRootObject.PlayerLevel;
			List<FoundryItemData> source = (from p in OverwolfWrapper.GetFoundryTabData(showAll: true, new Dictionary<string, string> { { "type", "all" } }, new Dictionary<string, string>())
				where !p.name.StartsWith("Kavasa Prime")
				select p).ToList();
			Dictionary<string, int> dictionary = (from p in source
				group p by p.type into k
				select (Key: k.Key, k.Sum((FoundryItemData p) => p.itemReference.GetAccountMasteryGivenPerLevel() * p.itemReference.GetMasteryLevel(p.XP)))).ToDictionary(((string Key, int) p) => p.Key, ((string Key, int) p) => p.Item2);
			HashSet<string> hashSet = (from p in source
				where p.XP > 0
				select p.internalName).ToHashSet();
			HashSet<string> hashSet2 = (from p in StaticData.dataHandler.warframeRootObject.XPInfo
				where p.XP > 0
				select p.ItemType).ToHashSet();
			if (!dictionary.ContainsKey("companion"))
			{
				dictionary.Add("companions", 0);
			}
			IEnumerable<InGamePlexus> enumerable = StaticData.dataHandler?.warframeRootObject?.CrewShipHarnesses;
			foreach (InGamePlexus item2 in enumerable ?? Enumerable.Empty<InGamePlexus>())
			{
				dictionary["companion"] += Misc.GetMasteryLevelFromXP(item2.XP, isWarframeOrSentinel: true, 900000.0) * 200;
				hashSet.Add(item2.ItemType);
			}
			dictionary["companion"] += Misc.GetMasteryLevelFromXP((StaticData.dataHandler?.warframeRootObject?.XPInfo?.FirstOrDefault((Xpinfo p) => p.ItemType == "/Lotus/Powersuits/Khora/Kavat/KhoraKavatPowerSuit")?.XP).GetValueOrDefault(), isWarframeOrSentinel: true, 900000.0) * 200;
			hashSet.Add("/Lotus/Powersuits/Khora/Kavat/KhoraKavatPowerSuit");
			dictionary["companion"] += Misc.GetMasteryLevelFromXP((StaticData.dataHandler?.warframeRootObject?.XPInfo?.FirstOrDefault((Xpinfo p) => p.ItemType == "/Lotus/Powersuits/Khora/Kavat/KhoraPrimeKavatPowerSuit")?.XP).GetValueOrDefault(), isWarframeOrSentinel: true, 900000.0) * 200;
			hashSet.Add("/Lotus/Powersuits/Khora/Kavat/KhoraPrimeKavatPowerSuit");
			List<string> list = hashSet2.Except(hashSet).ToList();
			hashSet.Except(hashSet2).ToList();
			foreach (string uid in list)
			{
				int num = 0;
				if (uid.Contains("/Powersuits/") || uid.Contains("/Sentinel/"))
				{
					num = 200 * Misc.GetMasteryLevelFromXP(StaticData.dataHandler.warframeRootObject.XPInfo.FirstOrDefault((Xpinfo p) => p.ItemType == uid)?.XP ?? 0, isWarframeOrSentinel: true, 900000.0);
				}
				else
				{
					int value = 30;
					DataHandler dataHandler = StaticData.dataHandler;
					bool? obj;
					if (dataHandler == null)
					{
						obj = null;
					}
					else
					{
						BasicRemoteData basicRemoteData = dataHandler.basicRemoteData;
						if (basicRemoteData == null)
						{
							obj = null;
						}
						else
						{
							Dictionary<string, int> maxLevelOverrides = basicRemoteData.maxLevelOverrides;
							obj = ((maxLevelOverrides != null) ? new bool?(!maxLevelOverrides.TryGetValue(uid, out value)) : ((bool?)null));
						}
					}
					bool? flag = obj;
					if (flag == true)
					{
						value = 30;
					}
					num = 100 * Misc.GetMasteryLevelFromXP(StaticData.dataHandler.warframeRootObject.XPInfo.FirstOrDefault((Xpinfo p) => p.ItemType == uid)?.XP ?? 0, isWarframeOrSentinel: false, Misc.GetMasteryXPFromItemLevel(value, isWarframeOrSentinel: false));
				}
				dictionary.Add(uid, num);
			}
			IEnumerable<WarframeWorldStateDateMission> source2 = from p in StaticData.dataHandler.warframeRootObject.Missions
				where !p.Tag.EndsWith("Junction")
				where p != null && p.GetCompletionXP() > 0
				select p;
			IEnumerable<WarframeWorldStateDateMission> source3 = from p in StaticData.dataHandler.warframeRootObject.Missions
				where !p.Tag.EndsWith("Junction")
				where p.Tier > 0
				where p != null && p.GetCompletionXP() > 0
				select p;
			IEnumerable<DataNode> source4 = StaticData.dataHandler.nodes.Values.Where((DataNode p) => p.GetCompletionXP() > 0);
			IEnumerable<WarframeWorldStateDateMission> source5 = StaticData.dataHandler.warframeRootObject.Missions.Where((WarframeWorldStateDateMission p) => p.Tag.EndsWith("Junction") && p.Tag.Contains("To") && p.Completes > 0);
			int num2 = source2.Sum((WarframeWorldStateDateMission p) => p.GetCompletionXP());
			int num3 = 1000 * source5.Count((WarframeWorldStateDateMission p) => p.Completes > 0);
			int num4 = source3.Sum((WarframeWorldStateDateMission p) => p.GetCompletionXP());
			int num5 = source5.Count((WarframeWorldStateDateMission p) => p.Completes == 2 || (p.Completes >= 1 && p.Tier >= 1));
			int num6 = 1000 * num5;
			int num7 = num2 + num4 + num3 + num6;
			int num8 = (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_TACTICAL ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_PILOTING ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_GUNNERY ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_ENGINEERING ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_COMMAND ?? 0);
			int num9 = (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_DRIFT_COMBAT ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_DRIFT_OPPORTUNITY ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_DRIFT_RIDING ?? 0) + (StaticData.dataHandler.warframeRootObject.PlayerSkills?.LPS_DRIFT_ENDURANCE ?? 0);
			int num10 = num8 * 1500 + num9 * 1500;
			int num11 = dictionary.Sum((KeyValuePair<string, int> p) => p.Value) + num7 + num10;
			int masteryLevelTotalXPRequired = Misc.GetMasteryLevelTotalXPRequired(StaticData.dataHandler.warframeRootObject.PlayerLevel);
			int masteryLevelTotalXPRequired2 = Misc.GetMasteryLevelTotalXPRequired(StaticData.dataHandler.warframeRootObject.PlayerLevel + 1);
			int num12 = masteryLevelTotalXPRequired2 - masteryLevelTotalXPRequired;
			int percentLeftNextLevel = (num11 - masteryLevelTotalXPRequired) * 100 / num12;
			percentLeftNextLevel = Misc.Clamp(percentLeftNextLevel, 0, 100);
			masteryResponse.currentLevelXp = Math.Max(num11 - masteryLevelTotalXPRequired, 0);
			masteryResponse.nextLevelXp = masteryLevelTotalXPRequired2 - masteryLevelTotalXPRequired;
			masteryResponse.percent = Math.Max(percentLeftNextLevel, 0);
			masteryResponse.summary = new MasteryResponse.MasteryResponseSummary();
			masteryResponse.summary.intrinsic_duviri = new MasteryResponse.MasteryResponseCategory(num9, 40);
			masteryResponse.summary.intrinsic_rail = new MasteryResponse.MasteryResponseCategory(num8, 50);
			masteryResponse.summary.star_normal = new MasteryResponse.MasteryResponseCategory(source2.Count(), source4.Count());
			masteryResponse.summary.star_steel = new MasteryResponse.MasteryResponseCategory(source3.Count(), source4.Count());
			masteryResponse.summary.star_junctions = new MasteryResponse.MasteryResponseCategory(source5.Count(), 13);
			masteryResponse.summary.star_steel_junctions = new MasteryResponse.MasteryResponseCategory(num5, 13);
			List<FoundryItemData> source6 = source.Where((FoundryItemData p) => p.mastered).ToList();
			List<FoundryItemData> list2 = source.Where((FoundryItemData p) => !p.mastered).ToList();
			masteryResponse.summary.cont_warframes = new MasteryResponse.MasteryResponseCategory(source6.Count((FoundryItemData p) => p.itemReference is DataWarframe || p.itemReference is DataArchwing), source.Count((FoundryItemData p) => p.itemReference is DataWarframe || p.itemReference is DataArchwing));
			masteryResponse.summary.cont_weapons = new MasteryResponse.MasteryResponseCategory(source6.Count((FoundryItemData p) => p.itemReference is DataPrimaryWeapon || p.itemReference is DataSecondaryWeapon || p.itemReference is DataMeleeWeapon || p.itemReference is DataArchGun || p.itemReference is DataArchMelee), source.Count((FoundryItemData p) => p.itemReference is DataPrimaryWeapon || p.itemReference is DataSecondaryWeapon || p.itemReference is DataMeleeWeapon || p.itemReference is DataArchGun || p.itemReference is DataArchMelee));
			masteryResponse.summary.cont_companions = new MasteryResponse.MasteryResponseCategory(source6.Count((FoundryItemData p) => p.itemReference is DataPet || p.itemReference is DataSentinel || p.itemReference is DataSentinelWeapons), source.Count((FoundryItemData p) => p.itemReference is DataPet || p.itemReference is DataSentinel || p.itemReference is DataSentinelWeapons));
			masteryResponse.summary.contentPercent = (float)Math.Round((float)(100 * (masteryResponse.summary.cont_warframes.current + masteryResponse.summary.cont_weapons.current + masteryResponse.summary.cont_companions.current)) / (float)(masteryResponse.summary.cont_warframes.max + masteryResponse.summary.cont_weapons.max + masteryResponse.summary.cont_companions.max));
			masteryResponse.summary.starPercent = (float)Math.Round((float)(100 * (masteryResponse.summary.star_normal.current + masteryResponse.summary.star_steel.current + masteryResponse.summary.star_junctions.current + masteryResponse.summary.star_steel_junctions.current)) / (float)(masteryResponse.summary.star_normal.max + masteryResponse.summary.star_steel.max + masteryResponse.summary.star_junctions.max + masteryResponse.summary.star_steel_junctions.max));
			masteryResponse.summary.intrinsicPercent = (float)Math.Round((float)(100 * (masteryResponse.summary.intrinsic_duviri.current + masteryResponse.summary.intrinsic_rail.current)) / (float)(masteryResponse.summary.intrinsic_duviri.max + masteryResponse.summary.intrinsic_rail.max));
			foreach (FoundryItemData item3 in list2)
			{
				item3.masteryViewData = new MasteryResponse.MasteryResponseContentRemainingData();
				foreach (FoundryItemComponent item4 in item3.components?.Where((FoundryItemComponent p) => !p.recipeNeccessaryComponents) ?? Enumerable.Empty<FoundryItemComponent>())
				{
					int item = Math.Max(item4.neccessaryAmount - item4.quantity, 0);
					item3.masteryViewData.neccessaryComponentPrices.Add((PriceHelper.GetLazyItemPrice(item4.name), item));
				}
			}
			PriceHelper.Flush(TimeSpan.FromSeconds(10.0));
			foreach (FoundryItemData item5 in list2)
			{
				item5.masteryViewData.platinumNeededToBuyParts = item5.masteryViewData.neccessaryComponentPrices.Sum(((OverwolfWrapper.ItemPriceSmallResponse price, int amountNeeded) p) => p.price.post.GetValueOrDefault() * p.amountNeeded);
				bool flag2 = true;
				List<FoundryItemComponent> list3 = item5.components?.Where((FoundryItemComponent p) => !p.recipeNeccessaryComponents).ToList() ?? new List<FoundryItemComponent>();
				if (list3.Count != item5.masteryViewData.neccessaryComponentPrices.Count)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, $"Foundry item {item5.name} has {list3.Count} unowned components, but {item5.masteryViewData.neccessaryComponentPrices.Count} prices");
				}
				else
				{
					for (int num13 = 0; num13 < list3.Count; num13++)
					{
						if (!list3[num13].recipeNeccessaryComponents && list3[num13].name.Contains("Blueprint") && item5.masteryViewData.neccessaryComponentPrices[num13].price.post.GetValueOrDefault() <= 0)
						{
							flag2 = false;
							break;
						}
					}
				}
				item5.masteryViewData.canBeBoughtWithPlatinum = flag2 && item5.masteryViewData.neccessaryComponentPrices.Sum(((OverwolfWrapper.ItemPriceSmallResponse price, int amountNeeded) p) => p.price.post.GetValueOrDefault()) > 0;
				item5.masteryViewData.currentLevel = item5.itemReference.GetMasteryLevel(item5.XP);
				item5.masteryViewData.maxLevel = item5.itemReference.GetMaxMasteryLevel();
				item5.masteryViewData.potentialXPToGet = (item5.masteryViewData.maxLevel - item5.masteryViewData.currentLevel) * item5.itemReference.GetAccountMasteryGivenPerLevel();
				item5.masteryViewData.remainingPartsCount = item5.components?.Count((FoundryItemComponent p) => !p.recipeNeccessaryComponents) ?? 0;
				item5.masteryViewData.relicProbability = CalculateItemCompletitionProbabilityFromRelics(item5);
			}
			List<FoundryItemData> list4 = new List<FoundryItemData>();
			if (orderingMode == "normal")
			{
				if (masteryResponse.summary.intrinsic_duviri.current > 0 && masteryResponse.summary.intrinsic_duviri.current < masteryResponse.summary.intrinsic_duviri.max)
				{
					list4.Add(new FoundryItemData
					{
						name = "Duviri Intrinsic",
						picture = "assets/img/IntrinsicPoint.webp",
						owned = true,
						type = "intrinsicDuviri",
						masteryViewData = new MasteryResponse.MasteryResponseContentRemainingData
						{
							potentialXPToGet = 1500,
							remainingPartsCount = 10
						}
					});
				}
				if (masteryResponse.summary.intrinsic_rail.current > 0 && masteryResponse.summary.intrinsic_rail.current < masteryResponse.summary.intrinsic_rail.max)
				{
					list4.Add(new FoundryItemData
					{
						name = "Railjack Intrinsic",
						picture = "assets/img/IntrinsicPoint.webp",
						owned = true,
						type = "intrinsicRailjack",
						masteryViewData = new MasteryResponse.MasteryResponseContentRemainingData
						{
							potentialXPToGet = 1500,
							remainingPartsCount = 10
						}
					});
				}
			}
			masteryResponse.topItems = list2.Union(list4).ApplyMasteryTabFilteringAndOrdering(orderingMode).ToList();
			masteryResponse.summaryPlatinum = masteryResponse.topItems.Sum((FoundryItemData p) => p.masteryViewData.platinumNeededToBuyParts);
			return masteryResponse;
		}

		private static float CalculateItemCompletitionProbabilityFromRelics(FoundryItemData item)
		{
			if (!item.name.Contains("Prime"))
			{
				return 0f;
			}
			float num = 1f;
			_ = StaticData.dataHandler.relics.Values;
			foreach (FoundryItemComponent item2 in item.components?.Where((FoundryItemComponent p) => !p.recipeNeccessaryComponents) ?? Enumerable.Empty<FoundryItemComponent>())
			{
				double num2 = 1.0;
				foreach (Drop item3 in item2.componentReference.drops?.Where((Drop d) => d.IsRelic()) ?? Enumerable.Empty<Drop>())
				{
					string text = item3.relic.name.Substring(0, item3.relic.name.LastIndexOf(' '));
					string text2 = item3.location.Replace("(", "").Replace(")", "").Split(' ')
						.Last()
						.Trim();
					string lookupName = text + " " + text2;
					DataRelic dataRelic = StaticData.dataHandler.relicsByShortName.GetOrDefault(text)?.FirstOrDefault((DataRelic p) => p.name == lookupName);
					if (dataRelic == null)
					{
						continue;
					}
					foreach (Miscitem item4 in StaticData.dataHandler?.warframeRootObject?.MiscItemsLookup[dataRelic.uniqueName])
					{
						num2 *= Math.Pow(1f - item3.chance.GetValueOrDefault(), item4.ItemCount);
					}
				}
				num *= (float)(1.0 - num2);
			}
			return num;
		}

		private static IEnumerable<FoundryItemData> ApplyMasteryTabFilteringAndOrdering(this IEnumerable<FoundryItemData> source, string orderingMode)
		{
			if (!(orderingMode == "platinum"))
			{
				if (orderingMode == "relics")
				{
					return from p in source.Where((FoundryItemData p) => !p.owned).Where(delegate(FoundryItemData p)
						{
							if (p.masteryViewData.relicProbability > 0f)
							{
								List<FoundryItemComponent> components = p.components;
								if (components == null)
								{
									return false;
								}
								return components.Count > 0;
							}
							return false;
						})
						orderby p.owned descending, p.masteryViewData.relicProbability descending, p.masteryViewData.remainingPartsCount
						select p;
				}
				return from p in source
					where StaticData.includeFormaLevelMasteryHelper || p.masteryViewData.currentLevel < 30
					where p.owned || (p.components?.All((FoundryItemComponent q) => q.recipeNeccessaryComponents) ?? false)
					orderby p.owned descending, p.owned ? 1 : p.masteryViewData.remainingPartsCount, p.masteryViewData.potentialXPToGet descending
					select p;
			}
			return from p in source
				where !p.owned
				where p.masteryViewData.canBeBoughtWithPlatinum
				orderby p.owned descending, (float)p.masteryViewData.potentialXPToGet / (float)p.masteryViewData.platinumNeededToBuyParts descending, p.components?.Count((FoundryItemComponent k) => k.recipeNeccessaryComponents)
				select p;
		}
	}
}
