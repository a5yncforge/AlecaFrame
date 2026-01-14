using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data
{
	public static class InventoryHelpers
	{
		public static IEnumerable<SetItemData> GetInventorySets(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalDucats, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller, bool getPrices = true)
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				totalPlat = 0;
				totalDucats = 0;
				return new List<SetItemData>();
			}
			totalDucats = 0;
			int totalDucats2;
			int totalPlat2;
			IEnumerable<WarframePartsItemData> inventoryWarframeParts = GetInventoryWarframeParts(new Dictionary<string, string>(), "", out totalDucats2, out totalPlat2, orderingType, orderedFromLargerToSmaller);
			IEnumerable<WeaponPartsItemData> inventoryWeaponParts = GetInventoryWeaponParts(new Dictionary<string, string>(), "", out totalPlat2, out totalDucats2, orderingType, orderedFromLargerToSmaller);
			GetInventoryMisc("", out totalDucats2, orderingType, orderedFromLargerToSmaller);
			Dictionary<string, SetItemData> dictionary = new Dictionary<string, SetItemData>();
			foreach (WarframePartsItemData item2 in inventoryWarframeParts)
			{
				ItemComponent item;
				if (StaticData.dataHandler.warframeParts.ContainsKey(item2.internalName))
				{
					item = StaticData.dataHandler.warframeParts[item2.internalName];
				}
				else
				{
					if (!StaticData.dataHandler.warframeParts.ContainsKey(item2.internalName.Replace("Blueprint", "Component")))
					{
						continue;
					}
					item = StaticData.dataHandler.warframeParts[item2.internalName.Replace("Blueprint", "Component")];
				}
				DoAddToSetWork(item2, item, dictionary);
			}
			foreach (WeaponPartsItemData item3 in inventoryWeaponParts)
			{
				_ = item3.componentSearchName;
				if (StaticData.dataHandler.tradeableCraftingPartsByUID.ContainsKey(item3.internalName))
				{
					ItemComponent itemComponentComponentProblemAware = StaticData.dataHandler.tradeableCraftingPartsByUID[item3.internalName].First().GetItemComponentComponentProblemAware();
					DoAddToSetWork(item3, itemComponentComponentProblemAware, dictionary);
				}
			}
			if (getPrices)
			{
				GetPricesDictionary(dictionary);
			}
			foreach (string item4 in dictionary.Keys.ToList())
			{
				if (!ShouldShowSetItemBasedOnFilters(dictionary[item4], yesNoFilters, partNameFilter))
				{
					dictionary.Remove(item4);
				}
			}
			totalPlat = dictionary.Values.Sum((SetItemData p) => p.amountOwned * p.sellPrice);
			if (orderedFromLargerToSmaller)
			{
				return from p in dictionary.Values
					orderby OrderInventorySETSBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in dictionary.Values
				orderby OrderInventorySETSBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		private static bool ShouldShowSetItemBasedOnFilters(SetItemData item, Dictionary<string, string> yesNoFilters, string partNameFilter)
		{
			if (yesNoFilters.ContainsKey("completeSet"))
			{
				if (yesNoFilters["completeSet"] == "yes" && !item.components.All((FoundryItemComponent q) => q.recipeNeccessaryComponents))
				{
					return false;
				}
				if (yesNoFilters["completeSet"] == "no" && item.components.All((FoundryItemComponent q) => q.recipeNeccessaryComponents))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("itemOwned"))
			{
				if (yesNoFilters["itemOwned"] == "yes" && !item.goalItemOwned)
				{
					return false;
				}
				if (yesNoFilters["itemOwned"] == "no" && item.goalItemOwned)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("moreThanOneCopies"))
			{
				if (yesNoFilters["moreThanOneCopies"] == "yes" && item.amountOwned <= 1 && item.totalAmountIncludingRanks <= 1)
				{
					return false;
				}
				if (yesNoFilters["moreThanOneCopies"] == "no" && (item.amountOwned > 1 || item.totalAmountIncludingRanks > 1))
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
				if (yesNoFilters["vaulted"] == "no" && item.vaulted)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("orderPlaced"))
			{
				if (yesNoFilters["orderPlaced"] == "yes" && !item.ordersPlaced)
				{
					return false;
				}
				if (yesNoFilters["orderPlaced"] == "no" && item.ordersPlaced)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("partType"))
			{
				if (yesNoFilters["partType"] == "prime" && !item.name.Contains("Prime"))
				{
					return false;
				}
				if (yesNoFilters["partType"] == "normal" && item.name.Contains("Prime"))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("equipped"))
			{
				if (yesNoFilters["equipped"] == "yes" && string.IsNullOrWhiteSpace(item.modUsedBy))
				{
					return false;
				}
				if (yesNoFilters["equipped"] == "no" && !string.IsNullOrWhiteSpace(item.modUsedBy))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("leveledUp"))
			{
				if (yesNoFilters["leveledUp"] == "yes" && item.currentModRank == 0)
				{
					return false;
				}
				if (yesNoFilters["leveledUp"] == "no" && item.currentModRank > 0)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("favorite"))
			{
				if (yesNoFilters["favorite"] == "yes" && !item.isFav)
				{
					return false;
				}
				if (yesNoFilters["favorite"] == "no" && item.isFav)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("minPlat") && int.TryParse(yesNoFilters["minPlat"], out var result) && result > item.sellPrice)
			{
				return false;
			}
			if (partNameFilter != null && !item.name.ToLower().Contains(partNameFilter.ToLower()))
			{
				return false;
			}
			return true;
		}

		private static void DoAddToSetWork(InventoryItemData part, ItemComponent item, Dictionary<string, SetItemData> inventoryItems)
		{
			if (item.isPartOf == null)
			{
				return;
			}
			string setName = Misc.GetSetName(item.isPartOf);
			if (!inventoryItems.ContainsKey(setName))
			{
				SetItemData setItemData = new SetItemData();
				setItemData.name = setName;
				setItemData.picture = Misc.GetFullImagePath(item.isPartOf.imageName);
				setItemData.ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(setItemData.name);
				setItemData.InitializeSetComponents(item.isPartOf);
				setItemData.goalItemOwned = part.goalItemOwned;
				setItemData.isReadyToSell = setItemData.components.All((FoundryItemComponent p) => p.recipeNeccessaryComponents);
				if (setItemData.components.Count <= 1)
				{
					return;
				}
				inventoryItems.Add(setName, setItemData);
			}
			inventoryItems[setName].vaulted = part.vaulted;
			inventoryItems[setName].vualtedMakesSense = part.vualtedMakesSense;
			inventoryItems[setName].AddSetComponent(item, part.amountOwned);
			inventoryItems[setName].amountOwned = inventoryItems[setName].CountHowManyAreReady();
			inventoryItems[setName].isReadyToSell = inventoryItems[setName].amountOwned > 0;
			inventoryItems[setName].UpdateDucats();
		}

		public static IEnumerable<InventoryItemData> GetInventoryAllParts(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalDucats, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> list = new List<Miscitem>();
			if (StaticData.dataHandler.warframeRootObject?.MiscItems != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.MiscItems.Where((Miscitem p) => p.IsWarframePart() || p.IsArchFramePart() || p.IsLandingCraftPart()));
			}
			if (StaticData.dataHandler.warframeRootObject?.Recipes != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.Recipes.Where((Miscitem p) => p.IsWarframePart() || p.IsArchFramePart() || p.IsLandingCraftPart()));
			}
			list = list.Where((Miscitem p) => p.ItemType.ToLower().Contains("blueprint")).ToList();
			List<Miscitem> list2 = new List<Miscitem>();
			if (StaticData.dataHandler.warframeRootObject?.MiscItems != null)
			{
				list2.AddRange(StaticData.dataHandler.warframeRootObject.MiscItems.Where((Miscitem p) => p.IsWeaponPart() || p.IsArchWeaponPart()));
			}
			if (StaticData.dataHandler.warframeRootObject?.Recipes != null)
			{
				list2.AddRange(StaticData.dataHandler.warframeRootObject.Recipes.Where((Miscitem p) => p.IsWeaponPart() || p.IsArchWeaponPart()));
			}
			IEnumerable<InventoryItemData> source = (from p in list
				select new WarframePartsItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p).Cast<InventoryItemData>().Concat(from p in list2
				select new WeaponPartsItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p);
			source = GetPrices(source.ToList());
			source = RemoveInProgressBlueprints(source.ToList());
			List<SetItemData> sets = null;
			if (yesNoFilters.ContainsKey("completeSet"))
			{
				sets = GetInventorySets(new Dictionary<string, string> { { "completeSet", "yes" } }, partNameFilter, out var _, out var _, orderingType, orderedFromLargerToSmaller).ToList();
			}
			source = source.Where((InventoryItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, partNameFilter, sets));
			source = ((!orderedFromLargerToSmaller) ? (from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p) : (from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
				select p));
			totalDucats = source.Sum((InventoryItemData p) => p.ducats * p.amountOwned);
			totalPlat = source.Sum((InventoryItemData p) => p.sellPrice * p.amountOwned);
			return source;
		}

		public static IEnumerable<WarframePartsItemData> GetInventoryWarframeParts(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalDucats, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> list = new List<Miscitem>();
			if (StaticData.dataHandler.warframeRootObject?.MiscItems != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.MiscItems.Where((Miscitem p) => p.IsWarframePart() || p.IsArchFramePart() || p.IsLandingCraftPart()));
			}
			if (StaticData.dataHandler.warframeRootObject?.Recipes != null)
			{
				list.AddRange(StaticData.dataHandler.warframeRootObject.Recipes.Where((Miscitem p) => p.IsWarframePart() || p.IsArchFramePart() || p.IsLandingCraftPart()));
			}
			IEnumerable<WarframePartsItemData> source = from p in list
				select new WarframePartsItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p;
			source = GetPrices(source.ToList());
			source = RemoveInProgressBlueprints(source.ToList());
			source = source.Where((WarframePartsItemData p) => p.internalName.ToLower().Contains("blueprint"));
			source = source.Where((WarframePartsItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, partNameFilter));
			totalDucats = 0;
			totalDucats += source.Sum((WarframePartsItemData p) => p.ducats * p.amountOwned);
			source = ((!orderedFromLargerToSmaller) ? (from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p) : (from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
				select p));
			totalPlat = source.Sum((WarframePartsItemData p) => p.sellPrice * p.amountOwned);
			return source;
		}

		public static IEnumerable<T> GetPrices<T>(List<T> input) where T : InventoryItemData
		{
			IEnumerable<string> source = input.Select((T p) => Misc.GetWarframeMarketURLName(p.name));
			OverwolfWrapper.ItemPriceSmallResponse[] prices = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(source.ToArray(), TimeSpan.FromSeconds(20.0));
			return input.Select(delegate(T p, int index)
			{
				p.sellPrice = (prices[index]?.post).GetValueOrDefault();
				p.buyPrice = (prices[index]?.insta).GetValueOrDefault();
				if (p.type == "mod" || p.type == "arcane")
				{
					if (p.currentModRank > 0)
					{
						p.shouldDisplayPlusInPrice = true;
						if (p.currentModRank == p.modRankMax && prices[index].postMax > 0)
						{
							p.sellPrice = prices[index].postMax.GetValueOrDefault();
							p.shouldDisplayPlusInPrice = false;
						}
					}
					if (p.sellPrice <= 0)
					{
						p.shouldDisplayPlusInPrice = false;
					}
				}
				return p;
			});
		}

		public static List<T> RemoveInProgressBlueprints<T>(List<T> input) where T : InventoryItemData
		{
			IEnumerable<string> inProgressBlueprints = StaticData.dataHandler?.warframeRootObject?.PendingRecipes?.Select((Pendingrecipe p) => p.ItemType);
			if (inProgressBlueprints == null)
			{
				return input;
			}
			return (from p in input.Select(delegate(T p, int index)
				{
					if (inProgressBlueprints.Contains(p.internalName))
					{
						p.amountOwned--;
					}
					return p;
				})
				where p.amountOwned > 0
				select p).ToList();
		}

		public static void GetPricesDictionary<T>(Dictionary<string, T> input) where T : InventoryItemData
		{
			Dictionary<string, T>.ValueCollection values = input.Values;
			IEnumerable<string> source = values.Select((T p) => Misc.GetWarframeMarketURLName(p.name));
			OverwolfWrapper.ItemPriceSmallResponse[] array = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(source.ToArray(), TimeSpan.FromSeconds(7.0));
			for (int num = 0; num < values.Count; num++)
			{
				values.ElementAt(num).sellPrice = (array[num]?.post).GetValueOrDefault();
				values.ElementAt(num).buyPrice = (array[num]?.insta).GetValueOrDefault();
			}
		}

		public static IEnumerable<WeaponPartsItemData> GetInventoryWeaponParts(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalDucats, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> list = StaticData.dataHandler.warframeRootObject.MiscItems.Where((Miscitem p) => p.IsWeaponPart() || p.IsArchWeaponPart()).ToList();
			list.AddRange(StaticData.dataHandler.warframeRootObject.Recipes.Where((Miscitem p) => p.IsWeaponPart() || p.IsArchWeaponPart()));
			IEnumerable<WeaponPartsItemData> source = from p in list
				select new WeaponPartsItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p;
			source = GetPrices(source.ToList());
			source = RemoveInProgressBlueprints(source.ToList());
			source = source.Where((WeaponPartsItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, partNameFilter));
			totalPlat = source.Sum((WeaponPartsItemData p) => p.sellPrice * p.amountOwned);
			totalDucats = 0;
			totalDucats += source.Sum((WeaponPartsItemData p) => p.ducats * p.amountOwned);
			if (orderedFromLargerToSmaller)
			{
				return from p in source
					orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		public static IEnumerable<MiscItemData> GetInventoryMisc(string partNameFilter, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> obj = StaticData.dataHandler.warframeRootObject.MiscItems?.Where((Miscitem p) => p.IsMisc()).ToList();
			obj.AddRange(StaticData.dataHandler.warframeRootObject.FusionTreasures?.Where((Miscitem p) => p.IsMisc()) ?? Enumerable.Empty<Miscitem>());
			obj.AddRange(StaticData.dataHandler.warframeRootObject.RawUpgrades?.Where((Miscitem p) => p.IsMisc()) ?? Enumerable.Empty<Miscitem>());
			IEnumerable<Miscitem> enumerable = StaticData.dataHandler.warframeRootObject.WeaponSkins?.Where((Weaponskin p) => p.IsMisc());
			obj.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			obj.AddRange(StaticData.dataHandler.warframeRootObject.LevelKeys?.Where((Miscitem p) => p.IsMisc()) ?? Enumerable.Empty<Miscitem>());
			List<Miscitem> list = new List<Miscitem>();
			enumerable = StaticData.dataHandler.warframeRootObject.LongGuns?.Where((Longgun p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange((enumerable ?? Enumerable.Empty<Miscitem>()).ToList());
			enumerable = StaticData.dataHandler.warframeRootObject.Pistols?.Where((Pistol p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			enumerable = StaticData.dataHandler.warframeRootObject.Melee?.Where((Melee p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			enumerable = StaticData.dataHandler.warframeRootObject.SpaceGuns?.Where((Spacegun p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			enumerable = StaticData.dataHandler.warframeRootObject.SpaceMelee?.Where((Spacemelee p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			enumerable = StaticData.dataHandler.warframeRootObject.SpaceSuits?.Where((Spacesuit p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			enumerable = StaticData.dataHandler.warframeRootObject.SentinelWeapons?.Where((Sentinelweapon p) => p.IsFactionOrBaro() && p.XP == 0);
			list.AddRange(enumerable ?? Enumerable.Empty<Miscitem>());
			obj.AddRange(from p in list
				group p by p.ItemType into p
				select new Miscitem
				{
					ItemType = p.Key,
					ItemCount = p.Count()
				});
			IEnumerable<MiscItemData> first = from p in obj
				select new MiscItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p;
			first = first.Union((from p in StaticData.dataHandler.warframeRootObject.KubrowPetPrints?.Select((KubrowPetPrint p) => new MiscItemData(p))
				where p.tradeable && !p.errorOccurred
				group p by p.internalName).Select(delegate(IGrouping<string, MiscItemData> p)
			{
				MiscItemData miscItemData = p.First();
				miscItemData.amountOwned = p.Sum((MiscItemData u) => u.amountOwned);
				return miscItemData;
			}) ?? Enumerable.Empty<MiscItemData>());
			first = first.Union((from p in StaticData.dataHandler.warframeRootObject.FlavourItems?.Select((Miscitem p) => new MiscItemData(p))
				where p.tradeable && !p.errorOccurred
				group p by p.internalName).Select(delegate(IGrouping<string, MiscItemData> p)
			{
				MiscItemData miscItemData = p.First();
				miscItemData.amountOwned = p.Sum((MiscItemData u) => u.amountOwned);
				return miscItemData;
			}) ?? Enumerable.Empty<MiscItemData>());
			first = GetPrices(first.ToList());
			first = first.Where((MiscItemData p) => p.name.ToLower().Contains(partNameFilter.ToLower()));
			totalPlat = first.Sum((MiscItemData p) => p.sellPrice * p.amountOwned);
			if (orderedFromLargerToSmaller)
			{
				return from p in first
					orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in first
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		public static IEnumerable<ModsItemData> GetInventoryMods(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller, bool showOnlyOwned)
		{
			List<Miscitem> list = StaticData.dataHandler.warframeRootObject.RawUpgrades.Where((Miscitem p) => p.IsMod()).ToList();
			list.AddRange(from p in StaticData.dataHandler.warframeRootObject.Upgrades
				where p.IsMod()
				group p by p.UpgradeFingerprint + p.ItemType into p
				select new Miscitem
				{
					ItemType = p.First().ItemType,
					ItemCount = p.Count(),
					UpgradeFingerprint = p.First().UpgradeFingerprint,
					ItemId = p.First().ItemId
				});
			if (!showOnlyOwned)
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (Miscitem item in list)
				{
					hashSet.Add(item.ItemType);
				}
				foreach (KeyValuePair<string, DataMod> item2 in StaticData.dataHandler.mods.Where((KeyValuePair<string, DataMod> p) => !string.IsNullOrEmpty(p.Value.name) && !string.IsNullOrEmpty(p.Value.imageName) && !p.Value.name.Contains("setmod") && p.Value.name != "Unfused Artifact" && p.Value.type != "Focus Way"))
				{
					if (!hashSet.Contains(item2.Key))
					{
						list.Add(new Miscitem
						{
							ItemType = item2.Key,
							ItemCount = 0
						});
					}
				}
			}
			IEnumerable<ModsItemData> source = from p in list
				select new ModsItemData(p, isArcane: false) into p
				where !string.IsNullOrWhiteSpace(p.name) && !p.errorOccurred
				select p;
			source = GetPrices(source.ToList());
			source = CalculateTotalCountsForRankedMods(source);
			List<IGrouping<string, ModsItemData>> list2 = (from p in source
				where p.modType == "riven"
				group p by p.internalName).ToList();
			source = source.Where((ModsItemData p) => p.modType != "riven");
			foreach (IGrouping<string, ModsItemData> item3 in list2)
			{
				ModsItemData modsItemData = item3.First();
				modsItemData.amountOwned = item3.Count();
				source = source.Append(modsItemData);
			}
			source = source.Where((ModsItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, partNameFilter));
			totalPlat = source.Sum((ModsItemData p) => p.sellPrice * p.amountOwned);
			if (orderedFromLargerToSmaller)
			{
				return from p in source
					orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		private static IEnumerable<ModsItemData> CalculateTotalCountsForRankedMods(IEnumerable<ModsItemData> inventoryItems)
		{
			return inventoryItems.Select(delegate(ModsItemData p, int index)
			{
				p.totalAmountIncludingRanks = inventoryItems.Where((ModsItemData u) => u.internalName == p.internalName).Count((ModsItemData u) => u.internalName == p.internalName);
				return p;
			});
		}

		public static IEnumerable<ModsItemData> GetInventoryArcanes(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			List<Miscitem> list = StaticData.dataHandler.warframeRootObject.RawUpgrades.Where((Miscitem p) => p.IsArcane()).ToList();
			list.AddRange(from p in StaticData.dataHandler.warframeRootObject.Upgrades
				where p.IsArcane()
				group p by p.UpgradeFingerprint + p.ItemType into p
				select new Miscitem
				{
					ItemType = p.First().ItemType,
					ItemCount = p.Count(),
					UpgradeFingerprint = p.First().UpgradeFingerprint
				});
			IEnumerable<ModsItemData> source = from p in list
				select new ModsItemData(p, isArcane: true) into p
				where !string.IsNullOrWhiteSpace(p.name) && !p.errorOccurred
				select p;
			source = GetPrices(source.ToList());
			source = CalculateTotalCountsForRankedMods(source);
			source = source.Where((ModsItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, partNameFilter));
			totalPlat = source.Sum((ModsItemData p) => p.sellPrice * p.amountOwned);
			if (orderedFromLargerToSmaller)
			{
				return from p in source
					orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		public static IEnumerable<RelicsItemData> GetInventoryRelics(Dictionary<string, string> yesNoFilters, string partNameFilter, out int totalPlat, string orderingType, bool orderedFromLargerToSmaller)
		{
			IEnumerable<RelicsItemData> source = from p in StaticData.dataHandler.warframeRootObject.MiscItems
				where p.IsRelic()
				select new RelicsItemData(p) into p
				where p.tradeable && !p.errorOccurred
				select p;
			source = source.Where((RelicsItemData p) => p.name.ToLower().Contains(partNameFilter.ToLower()) || StaticData.dataHandler.relics[p.internalName].relicRewards.Any((KeyValuePair<DataRelic.RelicRarities, DataRelic.RelicDropData> u) => u.Value.chance.Any((DataRelic.RelicDropData.RelicDropDataWithRarity k) => k.item.GetRealExternalName().ToLower().Contains(partNameFilter.ToLower()))));
			source = GetPrices(source.ToList());
			totalPlat = source.Sum((RelicsItemData p) => p.sellPrice * p.amountOwned);
			source = source.Where((RelicsItemData p) => ShouldShowInventoryItemBasedOnFilters(p, yesNoFilters, ""));
			if (orderedFromLargerToSmaller)
			{
				return from p in source
					orderby OrderInventoryBasedOnCriteria(p, orderingType) descending, p.name
					select p;
			}
			return from p in source
				orderby OrderInventoryBasedOnCriteria(p, orderingType), p.name
				select p;
		}

		private static object OrderInventoryBasedOnCriteria(InventoryItemData arg, string orderingMode)
		{
			return orderingMode switch
			{
				"platPrice" => arg.sellPrice, 
				"ducats" => arg.ducats, 
				"amount" => arg.amountOwned, 
				"ducanator" => (double)(float)arg.ducats / Math.Max(arg.sellPrice, 0.0001), 
				_ => arg.name, 
			};
		}

		private static object OrderInventorySETSBasedOnCriteria(SetItemData arg, string orderingMode)
		{
			return orderingMode switch
			{
				"platPrice" => arg.sellPrice, 
				"ducats" => arg.ducats, 
				"amount" => arg.CountHowManyAreReady(), 
				"ducanator" => (float)arg.sellPrice / (float)arg.ducats, 
				"complete" => (float)arg.components.Count((FoundryItemComponent p) => p.recipeNeccessaryComponents) / (float)arg.components.Count, 
				_ => arg.name, 
			};
		}

		private static bool ShouldShowInventoryItemBasedOnFilters(InventoryItemData item, Dictionary<string, string> yesNoFilters, string partNameFilter, List<SetItemData> completeSets = null)
		{
			if (yesNoFilters.ContainsKey("itemOwned"))
			{
				if (yesNoFilters["itemOwned"] == "yes" && !item.goalItemOwned)
				{
					return false;
				}
				if (yesNoFilters["itemOwned"] == "no" && item.goalItemOwned)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("moreThanOneCopies"))
			{
				if (yesNoFilters["moreThanOneCopies"] == "yes" && item.amountOwned <= 1 && item.totalAmountIncludingRanks <= 1)
				{
					return false;
				}
				if (yesNoFilters["moreThanOneCopies"] == "no" && (item.amountOwned > 1 || item.totalAmountIncludingRanks > 1))
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
				if (yesNoFilters["vaulted"] == "no" && item.vaulted)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("orderPlaced"))
			{
				if (yesNoFilters["orderPlaced"] == "yes" && !item.ordersPlaced)
				{
					return false;
				}
				if (yesNoFilters["orderPlaced"] == "no" && item.ordersPlaced)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("partType"))
			{
				if (yesNoFilters["partType"] == "prime" && !item.name.Contains("Prime"))
				{
					return false;
				}
				if (yesNoFilters["partType"] == "normal" && item.name.Contains("Prime"))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("equipped"))
			{
				if (yesNoFilters["equipped"] == "yes" && string.IsNullOrWhiteSpace(item.modUsedBy))
				{
					return false;
				}
				if (yesNoFilters["equipped"] == "no" && !string.IsNullOrWhiteSpace(item.modUsedBy))
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("leveledUp"))
			{
				if (yesNoFilters["leveledUp"] == "yes" && item.currentModRank == 0)
				{
					return false;
				}
				if (yesNoFilters["leveledUp"] == "no" && item.currentModRank > 0)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("favorite"))
			{
				if (yesNoFilters["favorite"] == "yes" && !item.isFav)
				{
					return false;
				}
				if (yesNoFilters["favorite"] == "no" && item.isFav)
				{
					return false;
				}
			}
			if (yesNoFilters.ContainsKey("minPlat") && int.TryParse(yesNoFilters["minPlat"], out var result) && result > item.sellPrice)
			{
				return false;
			}
			if (partNameFilter != null && !item.name.ToLower().Contains(partNameFilter.ToLower()))
			{
				return false;
			}
			if (yesNoFilters.ContainsKey("completeSet") && completeSets != null)
			{
				string itemUIDWithComponentInsteadOfBlueprint = item.internalName.Replace("Blueprint", "Component");
				bool flag = completeSets.Any((SetItemData p) => p.components.Any((FoundryItemComponent u) => u.uniqueName == item.internalName || u.uniqueName == item.componentSearchName || u.uniqueName == itemUIDWithComponentInsteadOfBlueprint));
				if (yesNoFilters["completeSet"] == "yes" && !flag)
				{
					return false;
				}
				if (yesNoFilters["completeSet"] == "no" && flag)
				{
					return false;
				}
			}
			return true;
		}
	}
}
