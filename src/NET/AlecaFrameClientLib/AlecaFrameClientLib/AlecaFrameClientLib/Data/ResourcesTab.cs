using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data
{
	public static class ResourcesTab
	{
		public static RecourcesTabOutputData GetData(bool onlyFavItems, string resourcesOrderingMode)
		{
			Dictionary<string, string> filters = new Dictionary<string, string> { { "type", "all" } };
			List<FoundryItemData> list = (from p in OverwolfWrapper.GetFoundryTabData(showAll: true, filters, new Dictionary<string, string>())
				where !p.owned && !p.mastered && (!onlyFavItems || p.isFav)
				select p).ToList();
			Dictionary<string, RecourcesTabOutputData.ResourcesTabOutputDataItem> dictionary = new Dictionary<string, RecourcesTabOutputData.ResourcesTabOutputDataItem>();
			foreach (FoundryItemData item in list)
			{
				CraftingTreeHelper.CraftingTreeData craftingTreeForItem = CraftingTreeHelper.GetCraftingTreeForItem(item.internalName, hideCompleted: true, requestPrices: false);
				if (craftingTreeForItem == null || craftingTreeForItem.treeData == null)
				{
					continue;
				}
				foreach (CraftingTreeHelper.CraftingTreeData.CraftingTreeDataTreeItem item2 in craftingTreeForItem.craftingTreeDataSummary.resourcesNeeded)
				{
					if (!dictionary.ContainsKey(item2.data.uniqueName))
					{
						dictionary[item2.data.uniqueName] = new RecourcesTabOutputData.ResourcesTabOutputDataItem
						{
							uniqueName = item2.data.uniqueName,
							totalNeeded = 0,
							owned = item2.quantityOwned
						};
					}
					dictionary[item2.data.uniqueName].usedInList.Add(new RecourcesTabOutputData.UsedInData
					{
						uniqueName = item.internalName,
						name = item.name,
						picture = item.picture
					});
					dictionary[item2.data.uniqueName].totalNeeded += item2.amountNeeded;
				}
			}
			RecourcesTabOutputData recourcesTabOutputData = new RecourcesTabOutputData();
			foreach (KeyValuePair<string, RecourcesTabOutputData.ResourcesTabOutputDataItem> item3 in dictionary)
			{
				item3.Value.percentOwned = (float)item3.Value.owned / (float)item3.Value.totalNeeded;
				item3.Value.hasEnough = item3.Value.owned >= item3.Value.totalNeeded;
				recourcesTabOutputData.resources.Add(item3.Value);
			}
			foreach (KeyValuePair<string, DataMisc> item4 in StaticData.dataHandler.misc.Where((KeyValuePair<string, DataMisc> p) => p.Key.StartsWith("/Lotus/Types/Gameplay/NarmerSorties/ArchonCrystal") && p.Key != "/Lotus/Types/Gameplay/NarmerSorties/ArchonCrystal" && !p.Key.EndsWith("Mythic")).ToList())
			{
				RecourcesTabOutputData.ResourceTabOutputArchonShard shardData = new RecourcesTabOutputData.ResourceTabOutputArchonShard
				{
					uniqueName = item4.Key,
					uniqueNameMythic = item4.Key + "Mythic",
					name = Misc.ReplaceStringWithNothing(item4.Value.name),
					picture = Misc.GetFullImagePath(item4.Value.imageName)
				};
				shardData.inventoryNormal = StaticData.dataHandler.warframeRootObject.MiscItemsLookup[shardData.uniqueName].FirstOrDefault()?.ItemCount ?? 0;
				shardData.inventoryMythic = StaticData.dataHandler.warframeRootObject.MiscItemsLookup[shardData.uniqueNameMythic].FirstOrDefault()?.ItemCount ?? 0;
				shardData.equippedMythic = 0;
				shardData.equippedNormal = 0;
				IEnumerable<Suit> enumerable = StaticData.dataHandler.warframeRootObject?.Suits;
				foreach (Suit item5 in enumerable ?? Enumerable.Empty<Suit>())
				{
					int num = item5.ArchonCrystalUpgrades?.Count((SuitArchonCrystalUpgrades p) => p.Color == StaticData.dataHandler.customShardData.shardUniqueIDToUpgrade[shardData.uniqueName]) ?? 0;
					int num2 = item5.ArchonCrystalUpgrades?.Count((SuitArchonCrystalUpgrades p) => p.Color == StaticData.dataHandler.customShardData.shardUniqueIDToUpgrade[shardData.uniqueNameMythic]) ?? 0;
					if ((num > 0 || num2 > 0) && StaticData.dataHandler.warframes.TryGetValue(item5.ItemType, out var value))
					{
						shardData.equippedUsedInList.Add(new RecourcesTabOutputData.ShardUsedInData
						{
							uniqueName = value.uniqueName,
							name = value.name,
							picture = Misc.GetFullImagePath(value.imageName),
							numNormal = num,
							numMythic = num2
						});
					}
					shardData.equippedNormal += num;
					shardData.equippedMythic += num2;
				}
				recourcesTabOutputData.shards.Add(shardData);
			}
			return recourcesTabOutputData;
		}
	}
}
