using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsComponentsItem
	{
		public FoundryItemComponent baseData;

		public string description;

		public bool isSet;

		public List<FoundryDetailsComponentDrop> drops = new List<FoundryDetailsComponentDrop>();

		public FoundryDetailsComponentsItem(ItemComponent component, bool _isSet = false)
		{
			isSet = _isSet;
			baseData = new FoundryItemComponent(component);
			description = component.description?.Replace(".", ". ")?.Trim();
			if (component.name == "Blueprint" && component != null && component.isPartOf?.name != null)
			{
				description = "Blueprint of " + component?.isPartOf?.name;
			}
			List<Drop> list = component.drops?.ToList();
			if (list == null)
			{
				list = new List<Drop>();
			}
			if (list.Count == 0)
			{
				if (StaticData.dataHandler.misc.ContainsKey(baseData.uniqueName))
				{
					DataMisc dataMisc = StaticData.dataHandler.misc[baseData.uniqueName];
					if (dataMisc.drops != null)
					{
						list.AddRange(dataMisc.drops);
					}
				}
				else if (StaticData.dataHandler.resources.ContainsKey(baseData.uniqueName))
				{
					DataResource dataResource = StaticData.dataHandler.resources[baseData.uniqueName];
					if (dataResource.drops != null)
					{
						list.AddRange(dataResource.drops);
					}
				}
			}
			drops = CreateParsedDrops(list.ToArray());
		}

		public static List<FoundryDetailsComponentDrop> CreateParsedDrops(Drop[] source)
		{
			List<FoundryDetailsComponentDrop> list = new List<FoundryDetailsComponentDrop>();
			foreach (Drop drop in source)
			{
				if (drop == null)
				{
					continue;
				}
				if (drop.location.ToLower().Contains("relic"))
				{
					if (!drop.location.Contains("("))
					{
						drop.location += " (Intact)";
					}
					string relicName = drop.location.Split('(')[0].Trim();
					string text = drop.location.Split('(')[1].Replace(")", "").Trim();
					string key = relicName.Replace("Relic", "").Trim();
					if (!StaticData.dataHandler.relicsByShortName.ContainsKey(key))
					{
						continue;
					}
					DataRelic dataRelic = StaticData.dataHandler.relicsByShortName[key].FirstOrDefault((DataRelic p) => p.name == drop.location.Replace("Relic", "").Replace("  ", " ").Replace("(", "")
						.Replace(")", "")
						.Trim());
					if (dataRelic == null)
					{
						continue;
					}
					FoundryDetailsComponentDrop foundryDetailsComponentDrop = list.FirstOrDefault((FoundryDetailsComponentDrop p) => p.dropPlace == relicName);
					if (foundryDetailsComponentDrop == null)
					{
						foundryDetailsComponentDrop = new FoundryDetailsComponentDrop
						{
							dropPlace = relicName,
							dropType = FoundryDetailsComponentDrop.DropType.Relic,
							imageURL = Misc.GetFullImagePath(dataRelic.imageName)
						};
						foundryDetailsComponentDrop.rawDropChance = drop.chance.GetValueOrDefault();
						foundryDetailsComponentDrop.vaulted = dataRelic.vaulted;
						foundryDetailsComponentDrop.ownedAmount = 0;
						foreach (DataRelic relicWithType in StaticData.dataHandler.relicsByShortName[key])
						{
							foundryDetailsComponentDrop.ownedAmount += (StaticData.dataHandler.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == relicWithType.uniqueName)?.ItemCount).GetValueOrDefault();
						}
						list.Add(foundryDetailsComponentDrop);
					}
					string text2 = Math.Round(drop.chance.GetValueOrDefault() * 100f, 1) + "%";
					foundryDetailsComponentDrop.levels.Add(new FoundryDetailsComponentDrop.RelicLevelDropPercentages
					{
						type = text,
						chance = text2
					});
					if (text.ToLower() == "intact")
					{
						foundryDetailsComponentDrop.dropPercent = text2;
					}
					try
					{
						foundryDetailsComponentDrop.relicUID = StaticData.dataHandler.relicsByShortName[key].First((DataRelic p) => p.uniqueName.Contains("Platinum")).uniqueName;
					}
					catch
					{
					}
				}
				else
				{
					FoundryDetailsComponentDrop foundryDetailsComponentDrop2 = new FoundryDetailsComponentDrop();
					foundryDetailsComponentDrop2.dropPlace = drop.location;
					if (foundryDetailsComponentDrop2.dropPlace.Contains("market with credits"))
					{
						foundryDetailsComponentDrop2.dropType = FoundryDetailsComponentDrop.DropType.Market;
						foundryDetailsComponentDrop2.dropPercent = "";
						foundryDetailsComponentDrop2.imageURL = "assets/img/market.png";
					}
					else
					{
						foundryDetailsComponentDrop2.dropType = FoundryDetailsComponentDrop.DropType.Normal;
						foundryDetailsComponentDrop2.dropPercent = Math.Round(drop.chance.GetValueOrDefault() * 100f, 2) + "%";
						foundryDetailsComponentDrop2.imageURL = "assets/img/world.png";
					}
					foundryDetailsComponentDrop2.rawDropChance = drop.chance.GetValueOrDefault();
					list.Add(foundryDetailsComponentDrop2);
				}
			}
			return (from p in list
				orderby p.ownedAmount descending, p.rawDropChance descending
				select p).ToList();
		}
	}
}
