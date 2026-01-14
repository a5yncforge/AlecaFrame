using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFramePublicLib;
using AlecaFramePublicLib.DataTypes;

namespace AlecaFrameClientLib.Data
{
	public class WFMarketOrderData
	{
		public int amountOnSale;

		public int platinumPerItem;

		public bool shouldDisplayPlusInPrice;

		public string picture;

		public string name;

		public int lowestSalePrice;

		public int amountOwned;

		public string extraItemDataString;

		public bool isSellOrder;

		public bool orderVisible = true;

		public bool showWarning;

		public string itemType;

		public string randomID;

		public string urlName;

		public bool dataInvalid;

		[NonSerialized]
		public WFMarketProfileOrder wfmarketOrder;

		[NonSerialized]
		public WFMItemListItem itemData;

		public int modLevel;

		public WFMarketOrderData(WFMarketProfileOrder p, string itemType, IEnumerable<SetItemData> cachedOwnedSetData = null)
		{
			itemData = StaticData.LazyWfmItemData.Value.AsDictionaryByID.GetOrDefault(p.itemId);
			this.itemType = itemType;
			wfmarketOrder = p;
			orderVisible = p.visible;
			isSellOrder = p.type == "sell";
			amountOnSale = p.quantity;
			platinumPerItem = p.platinum;
			picture = "https://warframe.market/static/assets/" + (string.IsNullOrEmpty(itemData.i18n.en.icon) ? itemData.i18n.en.thumb : itemData.i18n.en.icon);
			name = itemData.i18n.en.name;
			urlName = itemData.slug;
			randomID = p.id;
			if (name.ToLower().Contains("kavasa"))
			{
				this.itemType = "parts";
			}
			if (!string.IsNullOrWhiteSpace(p.rank) && int.TryParse(p.rank, out var result))
			{
				extraItemDataString = "Rank: " + result;
				modLevel = result;
			}
			else if (!string.IsNullOrWhiteSpace(p.subtype))
			{
				extraItemDataString = p.subtype;
			}
			else
			{
				extraItemDataString = "-";
			}
			amountOwned = 0;
			string uniqueID = "";
			if (StaticData.dataHandler.warframeRootObject != null)
			{
				switch (itemType)
				{
				case "parts":
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.warframeParts.Values.FirstOrDefault((ItemComponent u) => u.GetRealExternalName().Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", ""))?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.weaponParts.Values.FirstOrDefault((ItemComponent u) => u.GetRealExternalName().Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", ""))?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.resources.Values.FirstOrDefault((DataResource u) => u.name.Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", ""))?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc u) => u.name.Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", ""))?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc u) => u.components?.Any((ItemComponent k) => k.GetRealExternalName().Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", "")) ?? false)?.components?.FirstOrDefault((ItemComponent k) => k.GetRealExternalName().Replace(" Blueprint", "") == itemData.item_name.Replace(" Blueprint", ""))?.uniqueName;
					}
					if (!string.IsNullOrEmpty(uniqueID))
					{
						if (uniqueID.Contains("CrpArSniper") && !uniqueID.Contains("Blueprint"))
						{
							uniqueID = uniqueID.Replace("CrpArSniper", "Ambassador") + "Blueprint";
						}
						if (!itemData.slug.Contains("kavasa"))
						{
							uniqueID = uniqueID.Replace("Component", "Blueprint");
						}
						amountOwned += StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.Recipes.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID + "Blueprint")?.ItemCount ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.Recipes.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID + "Blueprint")?.ItemCount ?? 0;
					}
					break;
				case "relics":
				{
					string wfmRelicFullName = (itemData.item_name.Replace(" Relic", "") + " " + wfmarketOrder.subtype).ToLower();
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.relics.Values.FirstOrDefault((DataRelic u) => u.name.ToLower() == wfmRelicFullName)?.uniqueName;
					}
					if (!string.IsNullOrEmpty(uniqueID))
					{
						amountOwned += StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
					}
					break;
				}
				case "mods":
					foreach (DataMod item in StaticData.dataHandler.mods.Values.Where((DataMod u) => !u.uniqueName.Contains("/Beginner/") && u.name.ToLower() == itemData.item_name.ToLower().Replace(" (veiled)", "")))
					{
						uniqueID = item.uniqueName;
						amountOwned += StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade u) => u.ItemType == uniqueID && (u.TryGetModRank() == modLevel || !StaticData.WFMTakeModRankIntoAccount)).Count();
						if (!string.IsNullOrEmpty(uniqueID) && (modLevel == 0 || !StaticData.WFMTakeModRankIntoAccount))
						{
							amountOwned += StaticData.dataHandler.warframeRootObject.RawUpgrades.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
						}
					}
					break;
				case "arcanes":
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.arcanes.Values.FirstOrDefault((DataArcane u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						break;
					}
					amountOwned += StaticData.dataHandler.warframeRootObject.Upgrades.Where((Upgrade u) => u.ItemType == uniqueID && u.TryGetModRank() == modLevel).Count();
					if (modLevel == 0)
					{
						amountOwned += StaticData.dataHandler.warframeRootObject.RawUpgrades.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
					}
					break;
				case "sets":
					if (cachedOwnedSetData != null)
					{
						amountOwned += cachedOwnedSetData.FirstOrDefault((SetItemData u) => u.name.Replace(" Set", "") == itemData.item_name.Replace(" Set", ""))?.amountOwned ?? 0;
						if (itemData.item_name.Contains("Bonewidow") || itemData.item_name.Contains("Voidrig") || itemData.item_name.Contains("Bonewidow") || itemData.item_name.Contains("Damaged Necramech"))
						{
							dataInvalid = true;
						}
					}
					break;
				case "misc":
					if (itemData.item_name == "Nihil's Oubliette (Key)")
					{
						uniqueID = "/Lotus/Types/Keys/Nightwave/GlassmakerBossFightKey";
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.meleeWeapons.Values.FirstOrDefault((DataMeleeWeapon u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.primaryWeapons.Values.FirstOrDefault((DataPrimaryWeapon u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.secondaryWeapons.Values.FirstOrDefault((DataSecondaryWeapon u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.archMelees.Values.FirstOrDefault((DataArchMelee u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.archGuns.Values.FirstOrDefault((DataArchGun u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.sentinelWeapons.Values.FirstOrDefault((DataSentinelWeapons u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc u) => u.name.ToLower().Replace(" ", "") == itemData.item_name.Replace("Fusion Core", "Mod Fuser").ToLower().Replace(" ", ""))?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.fish.Values.Where((DataFish u) => u.name.ToLower() == itemData.item_name.ToLower())?.OrderBy((DataFish u) => u.uniqueName.Length)?.FirstOrDefault()?.uniqueName;
					}
					if (string.IsNullOrEmpty(uniqueID))
					{
						uniqueID = StaticData.dataHandler.skins.Values.FirstOrDefault((DataSkin u) => u.name == itemData.item_name)?.uniqueName;
					}
					if (!string.IsNullOrEmpty(uniqueID))
					{
						if (uniqueID.Contains("/Items/Fish/"))
						{
							string fishUIDPartToUse = uniqueID;
							if (fishUIDPartToUse.EndsWith("Item"))
							{
								fishUIDPartToUse = fishUIDPartToUse.Substring(0, fishUIDPartToUse.Length - 4);
							}
							amountOwned += StaticData.dataHandler.warframeRootObject.MiscItems.Where((Miscitem u) => u.ItemType.Contains(fishUIDPartToUse))?.Sum((Miscitem u) => u.ItemCount) ?? 0;
							break;
						}
						amountOwned += StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.LongGuns?.Count((Longgun u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.Pistols?.Count((Pistol u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.Melee?.Count((Melee u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.SpaceGuns?.Count((Spacegun u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.SpaceMelee?.Count((Spacemelee u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.SpaceSuits?.Count((Spacesuit u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += (StaticData.dataHandler.warframeRootObject.LevelKeys?.FirstOrDefault((Miscitem u) => u.ItemType == uniqueID)?.ItemCount).GetValueOrDefault();
						amountOwned += StaticData.dataHandler.warframeRootObject.SentinelWeapons?.Count((Sentinelweapon u) => u.ItemType == uniqueID && u.XP == 0) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.WeaponSkins?.Count((Weaponskin u) => u.ItemType == uniqueID) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.FlavourItems?.Count((Miscitem u) => u.ItemType == uniqueID) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.FusionTreasures?.Where((Miscitem u) => u.ItemType == uniqueID).Sum((Miscitem u) => u.ItemCount) ?? 0;
						amountOwned += StaticData.dataHandler.warframeRootObject.RawUpgrades?.Count((Miscitem u) => u.ItemType == uniqueID) ?? 0;
						if (uniqueID == "/Lotus/StoreItems/Upgrades/Mods/Fusers/LegendaryModFuser")
						{
							amountOwned += StaticData.dataHandler.warframeRootObject.RawUpgrades?.Count((Miscitem u) => u.ItemType == "/Lotus/Upgrades/Mods/Fusers/LegendaryModFuser") ?? 0;
						}
					}
					else
					{
						if (!itemData.item_name.ToLower().Contains("imprint"))
						{
							break;
						}
						string petTypeUID = StaticData.dataHandler.pets.Values.FirstOrDefault((DataPet u) => u.name.ToLower() == itemData.item_name.ToLower().Replace("imprint", "").Trim())?.uniqueName;
						if (!string.IsNullOrEmpty(petTypeUID))
						{
							amountOwned += (StaticData.dataHandler?.warframeRootObject?.KubrowPetPrints?.Count((KubrowPetPrint u) => u?.DominantTraits?.Personality == petTypeUID)).GetValueOrDefault();
						}
					}
					break;
				}
			}
			showWarning = amountOwned < amountOnSale && p.type == "sell" && !dataInvalid;
		}
	}
}
