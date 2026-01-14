using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class RelicDetailsResponse
	{
		public class RelicDataLevel
		{
			public string rarity;

			public Dictionary<int, RelicDataLevelSquad> bySquad = new Dictionary<int, RelicDataLevelSquad>();

			public float GetRefinementCostUpToThisRarity()
			{
				switch (rarity.ToLower())
				{
				case "intact":
					return 0f;
				case "exceptional":
					return 25f;
				case "flawless":
					return 50f;
				case "radiant":
					return 100f;
				default:
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Unknown relic type on GetRefinementCostUpToThisRarity!");
					return 0f;
				}
			}

			public RelicDataLevel(DataRelic referencedRelic, DataRelic.RelicRarities rarity)
			{
				this.rarity = rarity.ToString();
				bySquad.Add(1, new RelicDataLevelSquad(this, referencedRelic, rarity, 1));
				bySquad.Add(2, new RelicDataLevelSquad(this, referencedRelic, rarity, 2));
				bySquad.Add(3, new RelicDataLevelSquad(this, referencedRelic, rarity, 3));
				bySquad.Add(4, new RelicDataLevelSquad(this, referencedRelic, rarity, 4));
			}
		}

		public class RelicDataLevelSquad
		{
			public class RelicDataItem
			{
				[NonSerialized]
				public float platinumValue = -1f;

				[NonSerialized]
				public float dropPerOne;

				[NonSerialized]
				public float realDropPerOneWithSquad;

				public string plat = "-";

				public float ducats = -1f;

				public string image;

				public string name;

				public bool owned;

				public bool parentOwned;

				public string percentageDrop;
			}

			public List<RelicDataItem> bronzeItems = new List<RelicDataItem>();

			public List<RelicDataItem> silverItems = new List<RelicDataItem>();

			public List<RelicDataItem> goldItems = new List<RelicDataItem>();

			[NonSerialized]
			public RelicDataLevel relicRefinementTierReference;

			public string expectedPlat = "-";

			[NonSerialized]
			public float plat;

			public string expectedDucats = "-";

			[NonSerialized]
			public float ducats;

			public int squadSize = 1;

			public RelicDataLevelSquad(RelicDataLevel relicRefinementTierReference, DataRelic referencedRelic, DataRelic.RelicRarities rarity, int squadSize)
			{
				this.relicRefinementTierReference = relicRefinementTierReference;
				this.squadSize = squadSize;
				List<DataRelic.RelicDropData.RelicDropDataWithRarity> list = referencedRelic.relicRewards.GetOrDefault(rarity)?.chance;
				if (list == null || list.Count == 0)
				{
					return;
				}
				foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item2 in list)
				{
					RelicDataItem relicDataItem = new RelicDataItem
					{
						percentageDrop = Math.Round(item2.chance * 100f, 1) + "%"
					};
					FoundryItemComponent foundryItemComponent = new FoundryItemComponent(item2.item);
					relicDataItem.name = foundryItemComponent.name;
					relicDataItem.image = foundryItemComponent.picture;
					relicDataItem.owned = foundryItemComponent.recipeNeccessaryComponents;
					relicDataItem.parentOwned = foundryItemComponent.parentOwned;
					relicDataItem.ducats = item2.item.ducats;
					relicDataItem.dropPerOne = item2.chance;
					if (relicDataItem.name.Contains("Forma Blueprint"))
					{
						relicDataItem.image = Misc.GetFullImagePath("afRemoteImg://forma.png");
					}
					switch (item2.rarity)
					{
					case DataRelic.RelicDropData.ItemRarity.Common:
						bronzeItems.Add(relicDataItem);
						break;
					case DataRelic.RelicDropData.ItemRarity.Uncommon:
						silverItems.Add(relicDataItem);
						break;
					case DataRelic.RelicDropData.ItemRarity.Rare:
						goldItems.Add(relicDataItem);
						break;
					}
				}
				List<RelicDataItem> list2 = new List<RelicDataItem>();
				list2.AddRange(bronzeItems);
				list2.AddRange(silverItems);
				list2.AddRange(goldItems);
				List<List<RelicDataItem>> allRelicOutcomes = GetAllRelicOutcomes();
				foreach (RelicDataItem item in list2)
				{
					item.realDropPerOneWithSquad = allRelicOutcomes.Sum((List<RelicDataItem> p) => (float)(p.Any((RelicDataItem q) => q.name == item.name) ? 1 : 0) * p.Aggregate(1f, (float x, RelicDataItem y) => x * y.dropPerOne));
					item.percentageDrop = Math.Round(item.realDropPerOneWithSquad * 100f, 1) + "%";
				}
				expectedDucats = Math.Round(ducats = allRelicOutcomes.Sum((List<RelicDataItem> p) => p.Max((RelicDataItem q) => q.ducats) * p.Aggregate(1f, (float x, RelicDataItem y) => x * y.dropPerOne)), 1).ToString();
			}

			public List<List<RelicDataItem>> GetAllRelicOutcomes()
			{
				List<RelicDataItem> list = new List<RelicDataItem>();
				list.AddRange(bronzeItems);
				list.AddRange(silverItems);
				list.AddRange(goldItems);
				List<List<RelicDataItem>> list2 = new List<List<RelicDataItem>>();
				foreach (RelicDataItem item in list)
				{
					if (squadSize == 1)
					{
						list2.Add(new List<RelicDataItem> { item });
						continue;
					}
					foreach (RelicDataItem item2 in list)
					{
						if (squadSize == 2)
						{
							list2.Add(new List<RelicDataItem> { item, item2 });
							continue;
						}
						foreach (RelicDataItem item3 in list)
						{
							if (squadSize == 3)
							{
								list2.Add(new List<RelicDataItem> { item, item2, item3 });
								continue;
							}
							foreach (RelicDataItem item4 in list)
							{
								if (squadSize == 4)
								{
									list2.Add(new List<RelicDataItem> { item, item2, item3, item4 });
								}
							}
						}
					}
				}
				return list2;
			}
		}

		public bool initializationSuccessful;

		public List<RelicDataLevel> levels = new List<RelicDataLevel>();

		public string relicName;

		public string image;

		public bool vaulted;

		public List<FoundryDetailsComponentDrop> drops = new List<FoundryDetailsComponentDrop>();

		public bool isFav;

		private DataRelic referencedRelic;

		public RelicDetailsResponse(string uniqueID)
		{
			if (!StaticData.dataHandler.relics.ContainsKey(uniqueID))
			{
				return;
			}
			referencedRelic = StaticData.dataHandler.relics[uniqueID];
			relicName = referencedRelic.name;
			relicName = relicName.Replace("Exceptional", "").Replace("Intact", "").Replace("Flawless", "")
				.Replace("Radiant", "")
				.Trim();
			image = Misc.GetFullImagePath(referencedRelic.imageName);
			vaulted = referencedRelic.vaulted;
			levels.Add(new RelicDataLevel(referencedRelic, DataRelic.RelicRarities.Intact));
			levels.Add(new RelicDataLevel(referencedRelic, DataRelic.RelicRarities.Exceptional));
			levels.Add(new RelicDataLevel(referencedRelic, DataRelic.RelicRarities.Flawless));
			levels.Add(new RelicDataLevel(referencedRelic, DataRelic.RelicRarities.Radiant));
			if (referencedRelic.drops != null)
			{
				Drop[] array = referencedRelic.drops;
				foreach (Drop drop in array)
				{
					if (drop != null)
					{
						FoundryDetailsComponentDrop item = new FoundryDetailsComponentDrop
						{
							dropPlace = drop.location,
							dropType = FoundryDetailsComponentDrop.DropType.Normal,
							rawDropChance = drop.chance.GetValueOrDefault(),
							dropPercent = Math.Round(drop.chance.GetValueOrDefault() * 100f, 1) + "%",
							imageURL = "assets/img/world.png"
						};
						drops.Add(item);
					}
				}
				drops = drops.OrderByDescending((FoundryDetailsComponentDrop p) => p.rawDropChance).ToList();
			}
			initializationSuccessful = true;
			isFav = FavouriteHelper.IsFavourite(referencedRelic.uniqueName) || FavouriteHelper.IsFavourite(relicName);
		}

		public void FillPriceData()
		{
			List<RelicDataLevelSquad.RelicDataItem> list = new List<RelicDataLevelSquad.RelicDataItem>();
			list.AddRange(levels[0].bySquad[1].bronzeItems);
			list.AddRange(levels[0].bySquad[1].silverItems);
			list.AddRange(levels[0].bySquad[1].goldItems);
			string[] array = list.Select((RelicDataLevelSquad.RelicDataItem p) => p.name).ToArray();
			OverwolfWrapper.ItemPriceSmallResponse[] array2 = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(array.ToArray(), TimeSpan.FromSeconds(10.0));
			Dictionary<string, OverwolfWrapper.ItemPriceSmallResponse> dictionary = new Dictionary<string, OverwolfWrapper.ItemPriceSmallResponse>();
			for (int num = 0; num < array.Length; num++)
			{
				dictionary[array[num]] = array2[num];
			}
			foreach (RelicDataLevel level in levels)
			{
				foreach (RelicDataLevelSquad value in level.bySquad.Values)
				{
					List<RelicDataLevelSquad.RelicDataItem> list2 = new List<RelicDataLevelSquad.RelicDataItem>();
					list2.AddRange(value.bronzeItems);
					list2.AddRange(value.silverItems);
					list2.AddRange(value.goldItems);
					for (int num2 = 0; num2 < list2.Count; num2++)
					{
						list2[num2].platinumValue = (dictionary.GetOrDefault(list2[num2].name)?.post).GetValueOrDefault();
						list2[num2].plat = list2[num2].platinumValue.ToString();
					}
					value.expectedPlat = Math.Round(value.plat = value.GetAllRelicOutcomes().Sum((List<RelicDataLevelSquad.RelicDataItem> p) => p.Max((RelicDataLevelSquad.RelicDataItem q) => q.platinumValue) * p.Aggregate(1f, (float x, RelicDataLevelSquad.RelicDataItem y) => x * y.dropPerOne)), 1).ToString();
				}
			}
		}
	}
}
