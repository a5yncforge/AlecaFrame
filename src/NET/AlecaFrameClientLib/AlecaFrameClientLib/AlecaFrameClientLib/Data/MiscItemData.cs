using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class MiscItemData : InventoryItemData
	{
		public MiscItemData(Miscitem miscitem)
		{
			SetBase(miscitem.ItemType);
			amountOwned = miscitem.ItemCount;
			string text = internalName;
			isFav = FavouriteHelper.IsFavourite(internalName);
			if (internalName.Contains("ModFuser"))
			{
				picture = "assets/img/fusionCore.png";
				tradeable = true;
				name = (internalName.Contains("LegendaryModFuser") ? "Legendary Fusion Core" : "Ancient Fusion Core");
			}
			else if (internalName.StartsWith("/Lotus/Types/Items/Emotes"))
			{
				DataSkin value = StaticData.dataHandler.skins.FirstOrDefault((KeyValuePair<string, DataSkin> p) => p.Value?.uniqueName == internalName).Value;
				if (value != null)
				{
					picture = Misc.GetFullImagePath(value.imageName);
					tradeable = true;
					name = value.name;
					amountOwned = ((miscitem.ItemCount <= 0) ? 1 : miscitem.ItemCount);
				}
				else
				{
					errorOccurred = true;
				}
			}
			else if (StaticData.dataHandler.skinParts.ContainsKey(text))
			{
				ItemComponent itemComponent = StaticData.dataHandler.skinParts[text];
				picture = Misc.GetFullImagePath(itemComponent.imageName);
				tradeable = itemComponent.tradable;
				name = itemComponent.name;
				if (itemComponent.isPartOf != null && name == "Blueprint")
				{
					name = itemComponent.isPartOf.name + " " + name.ToLower();
				}
			}
			else if (StaticData.dataHandler.arcanes.ContainsKey(text))
			{
				DataArcane dataArcane = StaticData.dataHandler.arcanes[text];
				picture = Misc.GetFullImagePath(dataArcane.imageName);
				tradeable = dataArcane.tradable;
				name = dataArcane.name;
			}
			else if (StaticData.dataHandler.misc.ContainsKey(text))
			{
				DataMisc dataMisc = StaticData.dataHandler.misc[text];
				picture = Misc.GetFullImagePath(dataMisc.imageName);
				tradeable = dataMisc.tradable || miscitem.IsScene();
				name = dataMisc.name;
			}
			else if (StaticData.dataHandler.resources.ContainsKey(text))
			{
				DataResource dataResource = StaticData.dataHandler.resources[text];
				picture = Misc.GetFullImagePath(dataResource.imageName);
				tradeable = dataResource.tradable;
				name = dataResource.name;
			}
			else if (StaticData.dataHandler.fish.ContainsKey(text))
			{
				DataFish dataFish = StaticData.dataHandler.fish[text];
				picture = Misc.GetFullImagePath(dataFish.imageName);
				tradeable = dataFish.tradable;
				name = dataFish.name;
			}
			else if (StaticData.dataHandler.skins.ContainsKey(text))
			{
				DataSkin dataSkin = StaticData.dataHandler.skins[text];
				picture = Misc.GetFullImagePath(dataSkin.imageName);
				tradeable = dataSkin.name.Contains("Arcane");
				name = dataSkin.name.Replace(" Helmet", "");
				string text2 = dataSkin.uniqueName.Replace("/Lotus/Upgrades/Skins/", "").Split('/')[0];
				if (text2 == "Asp")
				{
					text2 = "Saryn";
				}
				if (text2 == "Decree")
				{
					text2 = "Banshee";
				}
				string key = "/Lotus/Powersuits/" + text2 + "/" + text2;
				string text3 = text2;
				if (StaticData.dataHandler.warframes.TryGetValue(key, out var value2))
				{
					text3 = value2.name;
				}
				name = name + " " + text3 + " Helmet";
			}
			else if (miscitem.IsFactionOrBaro())
			{
				BigItem orDefault = StaticData.dataHandler.primaryWeapons.GetOrDefault(miscitem.ItemType);
				if (orDefault == null)
				{
					orDefault = StaticData.dataHandler.secondaryWeapons.GetOrDefault(miscitem.ItemType);
				}
				if (orDefault == null)
				{
					orDefault = StaticData.dataHandler.meleeWeapons.GetOrDefault(miscitem.ItemType);
				}
				if (orDefault == null)
				{
					orDefault = StaticData.dataHandler.archGuns.GetOrDefault(miscitem.ItemType);
				}
				if (orDefault == null)
				{
					orDefault = StaticData.dataHandler.archMelees.GetOrDefault(miscitem.ItemType);
				}
				if (orDefault == null)
				{
					orDefault = StaticData.dataHandler.sentinelWeapons.GetOrDefault(miscitem.ItemType);
				}
				if (orDefault == null)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find Baro weapon: " + miscitem.ItemType);
					errorOccurred = true;
				}
				else
				{
					picture = Misc.GetFullImagePath(orDefault.imageName);
					tradeable = true;
					name = orDefault.name;
				}
			}
			else if (miscitem.IsLandingCraftPart())
			{
				if (miscitem.ItemType.EndsWith("component"))
				{
					errorOccurred = true;
				}
				else
				{
					string nameToUse = miscitem.ItemType;
					nameToUse = nameToUse.Replace("Blueprint", "Component");
					ItemComponent itemComponent2 = StaticData.dataHandler.misc.FirstOrDefault((KeyValuePair<string, DataMisc> p) => p.Value?.components?.Any((ItemComponent u) => u.uniqueName == nameToUse) == true).Value?.components?.FirstOrDefault((ItemComponent p) => p.uniqueName == nameToUse);
					if (itemComponent2 == null)
					{
						errorOccurred = true;
					}
					else
					{
						picture = Misc.GetFullImagePath(itemComponent2.imageName);
						tradeable = true;
						name = itemComponent2.GetRealExternalName();
					}
				}
			}
			else if (text == "/Lotus/Types/Keys/Nightwave/GlassmakerBossFightKey")
			{
				name = "Nihil's Oubliette (Key)";
				tradeable = true;
				picture = Misc.GetFullImagePath("nihil's-oubliette.png");
			}
			else
			{
				StaticData.ModsNotFoundThatShouldNotErrorOut.All((string p) => !internalName.Contains(p));
				errorOccurred = true;
			}
			if (!errorOccurred)
			{
				ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
			}
			vualtedMakesSense = false;
		}

		public MiscItemData(KubrowPetPrint imprint)
		{
			SetBase(imprint.DominantTraits.Personality);
			amountOwned = 1;
			isFav = FavouriteHelper.IsFavourite(imprint.DominantTraits.Personality);
			if (StaticData.dataHandler.pets.ContainsKey(imprint.DominantTraits.Personality))
			{
				DataPet dataPet = StaticData.dataHandler.pets[componentSearchName];
				picture = Misc.GetFullImagePath(dataPet.imageName);
				tradeable = true;
				name = dataPet.name + " Imprint";
			}
			else
			{
				errorOccurred = true;
			}
			ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
			vualtedMakesSense = false;
		}
	}
}
