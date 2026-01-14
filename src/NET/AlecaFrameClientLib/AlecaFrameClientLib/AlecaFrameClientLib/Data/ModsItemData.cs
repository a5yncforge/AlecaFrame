using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data
{
	public class ModsItemData : InventoryItemData
	{
		public ModsItemData(Miscitem miscitem, bool isArcane)
		{
			isMod = true;
			SetBase(miscitem.ItemType);
			amountOwned = miscitem.ItemCount;
			type = (isArcane ? "arcane" : "mod");
			string componentSearchName = internalName;
			isFav = FavouriteHelper.IsFavourite(internalName);
			DataMod dataMod = null;
			if (dataMod == null && StaticData.dataHandler.mods.ContainsKey(componentSearchName))
			{
				dataMod = StaticData.dataHandler.mods[componentSearchName];
			}
			if (dataMod == null && StaticData.dataHandler.arcanes.ContainsKey(componentSearchName))
			{
				dataMod = StaticData.dataHandler.arcanes[componentSearchName];
			}
			if (dataMod != null)
			{
				modRankMax = dataMod.GetMaxModLevel();
				currentModRank = 0;
				picture = Misc.GetFullImagePath(dataMod.imageName);
				if (!string.IsNullOrEmpty(dataMod.wikiaThumbnail))
				{
					picture = Misc.GetFullImagePath(dataMod.wikiaThumbnail);
				}
				tradeable = dataMod.tradable;
				name = dataMod.name;
				ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
				string text = dataMod.type;
				if (text != null && text.Contains("Riven"))
				{
					modType = "riven";
				}
				else
				{
					string text2 = dataMod.type;
					if (text2 != null && text2.Contains("Parazon"))
					{
						modType = "requiem";
					}
					else if (dataMod.rarity == "Uncommon")
					{
						modType = "silver";
					}
					else if (dataMod.rarity == "Common")
					{
						modType = "bronze";
					}
					else if (dataMod.rarity == "Rare")
					{
						modType = "gold";
					}
					else if (dataMod.rarity == "Legendary")
					{
						modType = "primed";
					}
					else
					{
						modType = "other";
					}
				}
				try
				{
					if (!string.IsNullOrEmpty(miscitem.UpgradeFingerprint))
					{
						ExtraModData extraModData = ExtraModData.DeserializeFromString(miscitem.UpgradeFingerprint);
						currentModRank = extraModData.lvl;
						if (modType == "riven")
						{
							if (extraModData.IsRivenUnveiled())
							{
								errorOccurred = true;
								return;
							}
							name += " (Veiled)";
							currentModRank = 0;
						}
					}
				}
				catch
				{
				}
				if (!string.IsNullOrWhiteSpace(miscitem.ItemId?.oid) && StaticData.dataHandler.modUsedInItems.ContainsKey(miscitem.ItemId.oid))
				{
					List<(ModeableItem, BigItem)> list = StaticData.dataHandler.modUsedInItems[miscitem.ItemId.oid];
					if (list.Count > 0)
					{
						List<string> list2 = (from p in list
							where p.Item2 != null && !string.IsNullOrWhiteSpace(p.Item2.name)
							select p.Item2.name into p
							group p by p into p
							select p.First() into p
							orderby p
							select p).ToList();
						modUsedBy = "Equipped in " + string.Join(", ", list2.Take(5));
						int num = list.Count(((ModeableItem, BigItem) p) => p.Item2 == null || string.IsNullOrWhiteSpace(p.Item2.name)) + Math.Max(list2.Count - 5, 0);
						if (num > 0)
						{
							if (list2.Count > 0)
							{
								modUsedBy += " and ";
							}
							modUsedBy += $"{num} other item(s)";
						}
					}
					else
					{
						modUsedBy = "";
					}
				}
			}
			else
			{
				StaticData.ModsNotFoundThatShouldNotErrorOut.All((string p) => !componentSearchName.Contains(p));
				errorOccurred = true;
			}
			vualtedMakesSense = false;
		}
	}
}
