using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AF_DamageCalculatorLib.Classes;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.Data;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AlecaFrameClientLib.Data
{
	public class DataHandler
	{
		public Dictionary<string, List<SinglePatch>> dataPatches;

		public Dictionary<string, DataWarframe> warframes;

		public Dictionary<string, DataPrimaryWeapon> primaryWeapons;

		public Dictionary<string, DataSecondaryWeapon> secondaryWeapons;

		public Dictionary<string, DataMeleeWeapon> meleeWeapons;

		public Dictionary<string, DataArchGun> archGuns;

		public Dictionary<string, DataArchMelee> archMelees;

		public Dictionary<string, DataArchwing> archWings;

		public Dictionary<string, DataPet> pets;

		public Dictionary<string, DataSentinel> sentinels;

		public Dictionary<string, DataSentinelWeapons> sentinelWeapons;

		public Dictionary<string, ItemComponent> warframeParts = new Dictionary<string, ItemComponent>();

		public Dictionary<string, ItemComponent> weaponParts = new Dictionary<string, ItemComponent>();

		public Dictionary<string, ItemComponent> skinParts = new Dictionary<string, ItemComponent>();

		public Dictionary<string, DataRelic> relics = new Dictionary<string, DataRelic>();

		public Dictionary<string, List<DataRelic>> relicsByShortName = new Dictionary<string, List<DataRelic>>();

		public Dictionary<string, DataFish> fish = new Dictionary<string, DataFish>();

		public Dictionary<string, DataMod> mods = new Dictionary<string, DataMod>();

		public Dictionary<string, DataArcane> arcanes = new Dictionary<string, DataArcane>();

		public Dictionary<string, DataMisc> misc = new Dictionary<string, DataMisc>();

		public Dictionary<string, DataSkin> skins = new Dictionary<string, DataSkin>();

		public Dictionary<string, DataResource> resources = new Dictionary<string, DataResource>();

		public Dictionary<string, DataMisc> kdrives = new Dictionary<string, DataMisc>();

		public Dictionary<string, DataMisc> amps = new Dictionary<string, DataMisc>();

		public Dictionary<string, DataQuest> quests = new Dictionary<string, DataQuest>();

		public Dictionary<string, ItemComponent> questParts = new Dictionary<string, ItemComponent>();

		public Dictionary<string, DataNode> nodes = new Dictionary<string, DataNode>();

		private HashSet<string> tradeableWeaponParts = new HashSet<string>();

		public Dictionary<Misc.WarframeLanguage, Dictionary<string, ItemComponent>> relicDropsRealNames = new Dictionary<Misc.WarframeLanguage, Dictionary<string, ItemComponent>>();

		public RivenRemoteData rivenData = new RivenRemoteData();

		public ExtendedCraftingRemoteData craftingData = new ExtendedCraftingRemoteData();

		public Dictionary<string, List<ExtendedCraftingRemoteDataItemComponent>> tradeableCraftingPartsByUID = new Dictionary<string, List<ExtendedCraftingRemoteDataItemComponent>>();

		public Dictionary<string, List<ExtendedCraftingRemoteDataItemComponent>> nonTradeableCraftingPartsByUID = new Dictionary<string, List<ExtendedCraftingRemoteDataItemComponent>>();

		public BasicRemoteData basicRemoteData = new BasicRemoteData();

		public BuildSourceDataFile buildSourceDataFile = new BuildSourceDataFile();

		public CustomShardData customShardData = new CustomShardData();

		public WarframeRootObject warframeRootObject;

		public bool isInitialized;

		private JsonSerializerSettings skipErrorsJSONSettings = new JsonSerializerSettings
		{
			Error = delegate(object se, Newtonsoft.Json.Serialization.ErrorEventArgs ev)
			{
				ev.ErrorContext.Handled = true;
			}
		};

		public Dictionary<string, List<(ModeableItem, BigItem)>> modUsedInItems = new Dictionary<string, List<(ModeableItem, BigItem)>>();

		public OverwolfWrapper OverwolfWrapper { get; }

		public string FolderPath { get; }

		public DataHandler(OverwolfWrapper overwolfWrapper, string folderPath)
		{
			OverwolfWrapper = overwolfWrapper;
			FolderPath = folderPath;
			dataPatches = JsonConvert.DeserializeObject<Dictionary<string, List<SinglePatch>>>(File.ReadAllText(folderPath + "/custom/dataFilePatches.json"), skipErrorsJSONSettings);
			warframes = ParseDataFileAsDictionaryAndPatch<DataWarframe>("Warframes");
			primaryWeapons = ParseDataFileAsDictionaryAndPatch<DataPrimaryWeapon>("Primary");
			secondaryWeapons = ParseDataFileAsDictionaryAndPatch<DataSecondaryWeapon>("Secondary");
			meleeWeapons = ParseDataFileAsDictionaryAndPatch<DataMeleeWeapon>("Melee");
			archGuns = ParseDataFileAsDictionaryAndPatch<DataArchGun>("Arch-Gun");
			archMelees = ParseDataFileAsDictionaryAndPatch<DataArchMelee>("Arch-Melee");
			archWings = ParseDataFileAsDictionaryAndPatch<DataArchwing>("Archwing");
			pets = ParseDataFileAsDictionaryAndPatch<DataPet>("Pets");
			fish = ParseDataFileAsDictionaryAndPatch<DataFish>("Fish");
			sentinels = ParseDataFileAsDictionaryAndPatch<DataSentinel>("Sentinels");
			sentinelWeapons = ParseDataFileAsDictionaryAndPatch<DataSentinelWeapons>("SentinelWeapons");
			relics = ParseDataFileAsDictionaryAndPatch<DataRelic>("Relics");
			mods = ParseDataFileAsDictionaryAndPatch<DataMod>("Mods");
			arcanes = ParseDataFileAsDictionaryAndPatch<DataArcane>("Arcanes");
			misc = ParseDataFileAsDictionaryAndPatch<DataMisc>("Misc");
			resources = ParseDataFileAsDictionaryAndPatch<DataResource>("Resources");
			skins = ParseDataFileAsDictionaryAndPatch<DataSkin>("Skins");
			quests = ParseDataFileAsDictionaryAndPatch<DataQuest>("Quests");
			nodes = ParseDataFileAsDictionaryAndPatch<DataNode>("Node");
			kdrives = misc.Values.Where((DataMisc p) => p.IsHoverboardComponent() && p.uniqueName.EndsWith("Deck")).ToDictionary((DataMisc dataPoint) => dataPoint.uniqueName);
			amps = misc.Values.Where((DataMisc p) => p.IsAmp() && p.uniqueName.Contains("/Barrel/")).ToDictionary((DataMisc dataPoint) => dataPoint.uniqueName);
			customShardData = JsonConvert.DeserializeObject<CustomShardData>(File.ReadAllText(folderPath + "/custom/shards.json"));
			try
			{
				if (File.Exists(folderPath + "/custom/rivens.json"))
				{
					rivenData = JsonConvert.DeserializeObject<RivenRemoteData>(File.ReadAllText(folderPath + "/custom/rivens.json"));
				}
				if (File.Exists(folderPath + "/custom/crafting.json"))
				{
					craftingData = JsonConvert.DeserializeObject<ExtendedCraftingRemoteData>(File.ReadAllText(folderPath + "/custom/crafting.json"));
				}
				if (File.Exists(folderPath + "/custom/basic.json"))
				{
					basicRemoteData = JsonConvert.DeserializeObject<BasicRemoteData>(File.ReadAllText(folderPath + "/custom/basic.json"));
				}
				if (File.Exists(folderPath + "/custom/builds.json"))
				{
					buildSourceDataFile = JsonConvert.DeserializeObject<BuildSourceDataFile>(File.ReadAllText(folderPath + "/custom/builds.json"));
				}
				if (File.Exists("C:\\GIT\\AlecaFrame\\DataCreator\\bin\\Debug\\net6.0\\AlecaFrame---CustomCDNData\\data\\builds.json"))
				{
					buildSourceDataFile = JsonConvert.DeserializeObject<BuildSourceDataFile>(File.ReadAllText("C:\\GIT\\AlecaFrame\\DataCreator\\bin\\Debug\\net6.0\\AlecaFrame---CustomCDNData\\data\\builds.json"));
				}
				tradeableWeaponParts = ParseListFromFile(folderPath + "/custom/tradeableWeaponParts.txt");
				warframeParts.Clear();
				ParseComponentsFromItem(((IEnumerable<DataWarframe>)warframes.Values).Select((Func<DataWarframe, BigItem>)((DataWarframe p) => p)).ToList(), warframeParts);
				ParseComponentsFromItem(((IEnumerable<DataArchwing>)archWings.Values).Select((Func<DataArchwing, BigItem>)((DataArchwing p) => p)).ToList(), warframeParts);
				AddLandingCraftParts(misc, warframeParts);
				weaponParts.Clear();
				ParseComponentsFromItem(((IEnumerable<DataPrimaryWeapon>)primaryWeapons.Values).Select((Func<DataPrimaryWeapon, BigItem>)((DataPrimaryWeapon p) => p)).ToList(), weaponParts);
				if (File.Exists(folderPath + "/custom/shotgunOverrides.txt"))
				{
					ApplyShotgunOverrides(File.ReadAllLines(folderPath + "/custom/shotgunOverrides.txt"));
				}
				ParseComponentsFromItem(((IEnumerable<DataSecondaryWeapon>)secondaryWeapons.Values).Select((Func<DataSecondaryWeapon, BigItem>)((DataSecondaryWeapon p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(((IEnumerable<DataMeleeWeapon>)meleeWeapons.Values).Select((Func<DataMeleeWeapon, BigItem>)((DataMeleeWeapon p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(((IEnumerable<DataSentinel>)sentinels.Values).Select((Func<DataSentinel, BigItem>)((DataSentinel p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(((IEnumerable<DataArchGun>)archGuns.Values).Select((Func<DataArchGun, BigItem>)((DataArchGun p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(((IEnumerable<DataArchMelee>)archMelees.Values).Select((Func<DataArchMelee, BigItem>)((DataArchMelee p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(skins.Values.Where((DataSkin p) => p.uniqueName == "/Lotus/Upgrades/Skins/Kubrows/Collars/PrimeKubrowCollarA").Select((Func<DataSkin, BigItem>)((DataSkin p) => p)).ToList(), weaponParts);
				ParseComponentsFromItem(((IEnumerable<DataQuest>)quests.Values).Select((Func<DataQuest, BigItem>)((DataQuest p) => p)).ToList(), questParts);
				ParseComponentsFromItem(((IEnumerable<DataPet>)pets.Values).Select((Func<DataPet, BigItem>)((DataPet p) => p)).ToList(), null);
				ParseComponentsFromItem(((IEnumerable<DataSentinelWeapons>)sentinelWeapons.Values).Select((Func<DataSentinelWeapons, BigItem>)((DataSentinelWeapons p) => p)).ToList(), null);
				ParseComponentsFromItem(((IEnumerable<DataMisc>)misc.Values).Select((Func<DataMisc, BigItem>)((DataMisc p) => p)).ToList(), null);
				skinParts.Clear();
				ParseComponentsFromItem(skins.Where((KeyValuePair<string, DataSkin> p) => p.Key == "/Lotus/Upgrades/Skins/Kubrows/Collars/PrimeKubrowCollarA").Select((Func<KeyValuePair<string, DataSkin>, BigItem>)((KeyValuePair<string, DataSkin> p) => p.Value)).ToList(), skinParts);
				CreateRelicDropQuickLookupTableAndRewards(relics);
				FixRelicTableDropRarity();
				CreateTranslationAlternativeLookupTables(FolderPath);
				CreateReverseComponentTables();
				CreateProcessedTradeableCraftingTable();
				UpdateVaultedStatus();
				if (!Debugger.IsAttached)
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Forcing GC...");
					GC.Collect(2, GCCollectionMode.Default, blocking: false, compacting: true);
				}
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to load warframe data: " + ex.Message);
			}
			isInitialized = true;
		}

		public Dictionary<string, T> ParseDataFileAsDictionaryAndPatch<T>(string itemTypeName) where T : BigItem
		{
			Dictionary<string, T> dictionary = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(FolderPath + "/json/" + itemTypeName + ".json"), skipErrorsJSONSettings).ToDictionary((T dataPoint) => dataPoint.uniqueName);
			IEnumerable<SinglePatch> orDefault = dataPatches.GetOrDefault(itemTypeName);
			foreach (SinglePatch item in orDefault ?? Enumerable.Empty<SinglePatch>())
			{
				if (item.type == SinglePatch.PathType.Add)
				{
					if (!dictionary.ContainsKey(item.uniqueName))
					{
						dictionary.Add(item.uniqueName, item.data.ToObject<T>());
						if (string.IsNullOrEmpty(dictionary[item.uniqueName].uniqueName))
						{
							dictionary[item.uniqueName].uniqueName = item.uniqueName;
						}
					}
					else
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Patch for dataset " + itemTypeName + " already contains an item with uid of " + item.uniqueName + ". Ignoring it...");
					}
				}
				else if (item.type == SinglePatch.PathType.Remove)
				{
					if (!dictionary.Remove(item.uniqueName))
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Patch for dataset " + itemTypeName + " tried to remove an item with uid of " + item.uniqueName + " which was not found. Ignoring it...");
					}
				}
				else if (item.type == SinglePatch.PathType.Replace)
				{
					dictionary[item.uniqueName] = item.data.ToObject<T>();
				}
				else if (item.type == SinglePatch.PathType.Patch)
				{
					T orDefault2 = dictionary.GetOrDefault(item.uniqueName);
					if (orDefault2 != null)
					{
						JObject jObject = JObject.FromObject(orDefault2);
						jObject.Merge(item.data, new JsonMergeSettings
						{
							MergeNullValueHandling = MergeNullValueHandling.Merge,
							MergeArrayHandling = MergeArrayHandling.Concat
						});
						dictionary[item.uniqueName] = jObject.ToObject<T>();
					}
					else
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Patch for dataset " + itemTypeName + " tried to patch an item with uid of " + item.uniqueName + " which was not found. Ignoring it...");
					}
				}
			}
			return dictionary;
		}

		private void ApplyShotgunOverrides(string[] shotgunOverrides)
		{
			foreach (KeyValuePair<string, DataPrimaryWeapon> primaryWeapon in primaryWeapons)
			{
				if (shotgunOverrides.Contains(primaryWeapon.Value.name))
				{
					primaryWeapon.Value.type = "Shotgun";
				}
			}
		}

		private void CreateProcessedTradeableCraftingTable()
		{
			tradeableCraftingPartsByUID.Clear();
			nonTradeableCraftingPartsByUID.Clear();
			foreach (KeyValuePair<string, ExtendedCraftingRemoteDataItem> item in craftingData.craftsByUUID)
			{
				BigItem orDefault = warframes.GetOrDefault(item.Key);
				if (orDefault == null)
				{
					orDefault = primaryWeapons.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = secondaryWeapons.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = meleeWeapons.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = archWings.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = archMelees.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = archGuns.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = sentinelWeapons.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = sentinels.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = pets.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = misc.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = kdrives.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = amps.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = skins.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = fish.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = resources.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = mods.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = arcanes.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					orDefault = quests.GetOrDefault(item.Key);
				}
				if (orDefault == null)
				{
					continue;
				}
				item.Value.bigItem = orDefault;
				IEnumerable<ExtendedCraftingRemoteDataItemComponent> components = item.Value.components;
				foreach (ExtendedCraftingRemoteDataItemComponent item2 in components ?? Enumerable.Empty<ExtendedCraftingRemoteDataItemComponent>())
				{
					item2.parentItem = item.Value;
					FillProcessedTradeableCraftingComponentsTableRecursive(orDefault, item2);
				}
			}
		}

		private void FillProcessedTradeableCraftingComponentsTableRecursive(BigItem itemReference, ExtendedCraftingRemoteDataItemComponent comp)
		{
			comp.itemComponentReference = itemReference?.components?.FirstOrDefault((ItemComponent p) => p.uniqueName == comp.uniqueName);
			if (comp.itemComponentReference != null)
			{
				comp.itemComponentReference.tradable = comp.tradeable || comp.components.Any((ExtendedCraftingRemoteDataItemComponent p) => p.tradeable);
			}
			if (comp.tradeable)
			{
				if (!tradeableCraftingPartsByUID.ContainsKey(comp.uniqueName))
				{
					tradeableCraftingPartsByUID.Add(comp.uniqueName, new List<ExtendedCraftingRemoteDataItemComponent>());
				}
				tradeableCraftingPartsByUID[comp.uniqueName].Add(comp);
			}
			else if (comp.components.Any((ExtendedCraftingRemoteDataItemComponent p) => p.componentType == ComponentType.SubBlueprint))
			{
				if (!nonTradeableCraftingPartsByUID.ContainsKey(comp.uniqueName))
				{
					nonTradeableCraftingPartsByUID.Add(comp.uniqueName, new List<ExtendedCraftingRemoteDataItemComponent>());
				}
				nonTradeableCraftingPartsByUID[comp.uniqueName].Add(comp);
			}
			comp.basicInfo = basicRemoteData.items?.GetOrDefault(comp.uniqueName);
			IEnumerable<ExtendedCraftingRemoteDataItemComponent> components = comp.components;
			foreach (ExtendedCraftingRemoteDataItemComponent item in components ?? Enumerable.Empty<ExtendedCraftingRemoteDataItemComponent>())
			{
				item.parentComponent = comp;
				FillProcessedTradeableCraftingComponentsTableRecursive(null, item);
			}
		}

		public void FillProcessedCraftingTableSubComponentsBasicInfo(ExtendedCraftingRemoteDataItemComponent comp)
		{
			comp.basicInfo = basicRemoteData.items?.GetOrDefault(comp.uniqueName);
			IEnumerable<ExtendedCraftingRemoteDataItemComponent> components = comp.components;
			foreach (ExtendedCraftingRemoteDataItemComponent item in components ?? Enumerable.Empty<ExtendedCraftingRemoteDataItemComponent>())
			{
				FillProcessedCraftingTableSubComponentsBasicInfo(item);
			}
		}

		private void AddLandingCraftParts(Dictionary<string, DataMisc> misc, Dictionary<string, ItemComponent> warframePrimeParts)
		{
			foreach (DataMisc value in misc.Values)
			{
				ItemComponent[] array = value.components ?? new ItemComponent[0];
				foreach (ItemComponent itemComponent in array)
				{
					if (itemComponent.IsLandingCraftPart())
					{
						itemComponent.tradable = true;
						if (!warframePrimeParts.ContainsKey(itemComponent.uniqueName))
						{
							warframePrimeParts.Add(itemComponent.uniqueName, itemComponent);
						}
					}
				}
			}
		}

		private void CreateReverseComponentTables()
		{
			CreateReverseComponentTablesForCollection(primaryWeapons.Values);
			CreateReverseComponentTablesForCollection(secondaryWeapons.Values);
			CreateReverseComponentTablesForCollection(meleeWeapons.Values);
			CreateReverseComponentTablesForCollection(archGuns.Values);
			CreateReverseComponentTablesForCollection(archMelees.Values);
			CreateReverseComponentTablesForCollection(archWings.Values);
		}

		private void CreateReverseComponentTablesForCollection(IEnumerable<BigItem> bigItems)
		{
			ItemComponent.ItemComponentComparer comparer = new ItemComponent.ItemComponentComparer();
			foreach (BigItem bigItem in bigItems)
			{
				if (bigItem.components == null)
				{
					continue;
				}
				foreach (ItemComponent item in bigItem.components.ToHashSet(comparer))
				{
					primaryWeapons.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
					secondaryWeapons.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
					meleeWeapons.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
					archGuns.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
					archMelees.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
					archWings.GetOrDefault(item.uniqueName)?.isPartOf.Add(bigItem);
				}
			}
		}

		private void UpdateVaultedStatus()
		{
			foreach (DataRelic value in relics.Values)
			{
				value.vaulted = value.drops == null || value.drops.Length == 0;
			}
			foreach (DataWarframe value2 in warframes.Values)
			{
				if (value2.components == null)
				{
					continue;
				}
				bool flag = value2.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value2.vaulted = !flag;
			}
			foreach (DataPrimaryWeapon value3 in primaryWeapons.Values)
			{
				if (value3.components == null)
				{
					continue;
				}
				bool flag2 = value3.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value3.vaulted = !flag2;
			}
			foreach (DataSecondaryWeapon value4 in secondaryWeapons.Values)
			{
				if (value4.components == null)
				{
					continue;
				}
				bool flag3 = value4.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value4.vaulted = !flag3;
			}
			foreach (DataMeleeWeapon value5 in meleeWeapons.Values)
			{
				if (value5.components == null)
				{
					continue;
				}
				bool flag4 = value5.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value5.vaulted = !flag4;
			}
			foreach (DataArchGun value6 in archGuns.Values)
			{
				if (value6.components == null)
				{
					continue;
				}
				bool flag5 = value6.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value6.vaulted = !flag5;
			}
			foreach (DataArchMelee value7 in archMelees.Values)
			{
				if (value7.components == null)
				{
					continue;
				}
				bool flag6 = value7.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value7.vaulted = !flag6;
			}
			foreach (DataArchwing value8 in archWings.Values)
			{
				if (value8.components == null)
				{
					continue;
				}
				bool flag7 = value8.components.Any((ItemComponent p) => p.drops != null && p.drops.Any((Drop u) => u.relic != null && !u.relic.vaulted));
				value8.vaulted = !flag7;
			}
		}

		private void FixRelicTableDropRarity()
		{
			foreach (DataRelic value in relics.Values)
			{
				foreach (DataRelic.RelicDropData value2 in value.relicRewards.Values)
				{
					List<IGrouping<int, DataRelic.RelicDropData.RelicDropDataWithRarity>> list = (from p in value2.chance
						group p by (int)(Math.Round(p.chance, 2) * 100.0) into p
						orderby p.Count() descending
						select p).ToList();
					if (list.Count() != 3)
					{
						continue;
					}
					foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item in list[0])
					{
						item.rarity = DataRelic.RelicDropData.ItemRarity.Common;
					}
					foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item2 in list[1])
					{
						item2.rarity = DataRelic.RelicDropData.ItemRarity.Uncommon;
					}
					foreach (DataRelic.RelicDropData.RelicDropDataWithRarity item3 in list[2])
					{
						item3.rarity = DataRelic.RelicDropData.ItemRarity.Rare;
					}
				}
			}
		}

		private void CreateTranslationAlternativeLookupTables(string folderPath)
		{
			Dictionary<string, Dictionary<string, DataTranslation>> translations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, DataTranslation>>>(File.ReadAllText(folderPath + "/json/lang.json"));
			if (!relicDropsRealNames.ContainsKey(Misc.WarframeLanguage.French))
			{
				relicDropsRealNames.Add(Misc.WarframeLanguage.French, new Dictionary<string, ItemComponent>());
			}
			if (!relicDropsRealNames.ContainsKey(Misc.WarframeLanguage.Spanish))
			{
				relicDropsRealNames.Add(Misc.WarframeLanguage.Spanish, new Dictionary<string, ItemComponent>());
			}
			if (!relicDropsRealNames.ContainsKey(Misc.WarframeLanguage.German))
			{
				relicDropsRealNames.Add(Misc.WarframeLanguage.German, new Dictionary<string, ItemComponent>());
			}
			if (!relicDropsRealNames.ContainsKey(Misc.WarframeLanguage.Russian))
			{
				relicDropsRealNames.Add(Misc.WarframeLanguage.Russian, new Dictionary<string, ItemComponent>());
			}
			foreach (KeyValuePair<string, ItemComponent> item in relicDropsRealNames[Misc.WarframeLanguage.English])
			{
				try
				{
					string key = item.Key;
					ItemComponent value = item.Value;
					relicDropsRealNames[Misc.WarframeLanguage.French].Add(Misc.RemoveDiacritics(TranslationHelper.GetFrenchName(key, value)).ToLower().Replace("dual", "doubles"), value);
					relicDropsRealNames[Misc.WarframeLanguage.Spanish].Add(Misc.RemoveDiacritics(TranslationHelper.GetESPANAName(key, value)).ToLower().Replace("dual kamas", "kamas dobles")
						.Replace("dual keres", "keres dobles"), value);
					relicDropsRealNames[Misc.WarframeLanguage.German].Add(Misc.RemoveDiacritics(TranslationHelper.GetGermanName(key, value).ToLower().Replace("aegis", "agis")), value);
					relicDropsRealNames[Misc.WarframeLanguage.Russian].Add(TranslationHelper.GetRussianName(key, value, translations).ToLower().Replace("й", "и")
						.Replace("ё", "е"), value);
				}
				catch
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to create translation for part: " + item.Value.uniqueName);
				}
			}
		}

		private void CreateRelicDropQuickLookupTableAndRewards(Dictionary<string, DataRelic> relics)
		{
			relicDropsRealNames.Clear();
			relicsByShortName.Clear();
			relicDropsRealNames.Add(Misc.WarframeLanguage.English, new Dictionary<string, ItemComponent>());
			foreach (KeyValuePair<string, DataRelic> relic in relics)
			{
				string key = relic.Value.name.Substring(0, relic.Value.name.LastIndexOf(' '));
				if (!relicsByShortName.ContainsKey(key))
				{
					relicsByShortName.Add(key, new List<DataRelic>());
				}
				relicsByShortName[key].Add(relic.Value);
			}
			foreach (KeyValuePair<string, ItemComponent> warframePart in warframeParts)
			{
				string text = warframePart.Value.GetRealExternalName();
				if ((!text.ToLower().Contains("prime") && text != "Forma Blueprint") || text.Contains("Nitain Extract") || text.Contains("Orokin Cell"))
				{
					continue;
				}
				if ((text.Contains("Chassis") || text.Contains("Neuroptics") || text.Contains("Systems")) && !text.Contains("Blueprint"))
				{
					text += " Blueprint";
				}
				if (text.Contains("Odonata Prime") && (text.Contains("Harness") || text.Contains("Systems") || text.Contains("Wings")) && !text.Contains("Blueprint"))
				{
					text += " Blueprint";
				}
				if (!relicDropsRealNames[Misc.WarframeLanguage.English].ContainsKey(text.ToLower()))
				{
					relicDropsRealNames[Misc.WarframeLanguage.English].Add(text.ToLower(), warframePart.Value);
				}
				IEnumerable<Drop> drops = warframePart.Value.drops;
				foreach (Drop item in drops ?? Enumerable.Empty<Drop>())
				{
					if (item.location.Contains("Relic"))
					{
						AddDropToRelicTable(item, warframePart.Value);
					}
				}
			}
			foreach (KeyValuePair<string, ItemComponent> weaponPart in weaponParts)
			{
				string text2 = weaponPart.Value.GetRealExternalName();
				if ((!text2.ToLower().Contains("prime") && text2 != "Forma Blueprint") || text2.Contains("Nitain Extract") || text2.Contains("Orokin Cell"))
				{
					continue;
				}
				if ((text2.Contains("Chassis") || text2.Contains("Neuroptics") || text2.Contains("Systems")) && !text2.Contains("Blueprint") && !(weaponPart.Value.isPartOf is DataSentinel))
				{
					text2 += " Blueprint";
				}
				if (!relicDropsRealNames[Misc.WarframeLanguage.English].ContainsKey(text2.ToLower()))
				{
					relicDropsRealNames[Misc.WarframeLanguage.English].Add(text2.ToLower(), weaponPart.Value);
				}
				IEnumerable<Drop> drops = weaponPart.Value.drops;
				foreach (Drop item2 in drops ?? Enumerable.Empty<Drop>())
				{
					if (item2.location.Contains("Relic"))
					{
						AddDropToRelicTable(item2, weaponPart.Value);
					}
				}
			}
			Drop[] drops2;
			foreach (KeyValuePair<string, ItemComponent> skinPart in skinParts)
			{
				string realExternalName = skinPart.Value.GetRealExternalName();
				if (!realExternalName.ToLower().Contains("kavasa prime"))
				{
					continue;
				}
				if (!relicDropsRealNames[Misc.WarframeLanguage.English].ContainsKey(realExternalName.ToLower()))
				{
					relicDropsRealNames[Misc.WarframeLanguage.English].Add(realExternalName.ToLower(), skinPart.Value);
				}
				drops2 = skinPart.Value.drops;
				foreach (Drop drop in drops2)
				{
					if (drop.location.Contains("Relic"))
					{
						AddDropToRelicTable(drop, skinPart.Value);
					}
				}
			}
			DataMisc dataMisc = misc["/Lotus/StoreItems/Types/Items/MiscItems/Forma"];
			ItemComponent itemComponent = new ItemComponent
			{
				name = dataMisc.name,
				imageName = dataMisc.imageName,
				drops = dataMisc.drops,
				uniqueName = dataMisc.uniqueName
			};
			drops2 = itemComponent.drops;
			foreach (Drop drop2 in drops2)
			{
				if (drop2.location.Contains("Relic"))
				{
					AddDropToRelicTable(drop2, itemComponent);
				}
			}
			ItemComponent twoformaItemComponent = new ItemComponent
			{
				name = "2x Forma Blueprint",
				imageName = dataMisc.imageName,
				drops = dataMisc.drops,
				uniqueName = dataMisc.uniqueName
			};
			foreach (DataRelic value in relics.Values)
			{
				if (value.relicRewards.GetOrDefault(DataRelic.RelicRarities.Intact) == null || value.relicRewards[DataRelic.RelicRarities.Intact].chance.Count == value.rewards.Count)
				{
					continue;
				}
				foreach (KeyValuePair<DataRelic.RelicRarities, DataRelic.RelicDropData> relicReward in value.relicRewards)
				{
					List<DataRelic.RelicDropData.RelicDropDataWithRarity> chance = relicReward.Value.chance;
					List<DataRelic.DataRelicReward> rewards = value.rewards;
					if (!chance.Any((DataRelic.RelicDropData.RelicDropDataWithRarity p) => p.item.name == twoformaItemComponent.name) && rewards.Any((DataRelic.DataRelicReward p) => p.item.name.Contains("2X Forma Blueprint")))
					{
						DataRelic.DataRelicReward dataRelicReward = rewards.First((DataRelic.DataRelicReward p) => p.item.name.Contains("Forma Blueprint"));
						float num = dataRelicReward.chance;
						if (num != 11f && num != 13f && num != 17f && num != 20f)
						{
							StaticData.Log(OverwolfWrapper.LogType.WARN, "Chance for 2x forma is not on the expected list for relic: " + value.name);
						}
						switch (relicReward.Key)
						{
						case DataRelic.RelicRarities.Intact:
							num = 0.11f;
							break;
						case DataRelic.RelicRarities.Exceptional:
							num = 0.13f;
							break;
						case DataRelic.RelicRarities.Flawless:
							num = 0.17f;
							break;
						case DataRelic.RelicRarities.Radiant:
							num = 0.2f;
							break;
						}
						chance.Add(new DataRelic.RelicDropData.RelicDropDataWithRarity
						{
							chance = num,
							item = twoformaItemComponent,
							rarity = (DataRelic.RelicDropData.ItemRarity)Enum.Parse(typeof(DataRelic.RelicDropData.ItemRarity), dataRelicReward.rarity)
						});
					}
				}
			}
		}

		public void AddDropToRelicTable(Drop drop, ItemComponent itemComponent)
		{
			if (!drop.location.Contains("("))
			{
				drop.location += " (Intact)";
			}
			string text = drop.location.Split('(')[0].Trim();
			DataRelic.RelicRarities key = (DataRelic.RelicRarities)Enum.Parse(typeof(DataRelic.RelicRarities), drop.location.Split('(')[1].Replace(")", "").Trim());
			string key2 = text.Replace("Relic", "").Trim();
			if (!relicsByShortName.ContainsKey(key2))
			{
				return;
			}
			drop.relic = relicsByShortName[key2][0];
			foreach (DataRelic item in relicsByShortName[key2])
			{
				if (!item.relicRewards.ContainsKey(key))
				{
					item.relicRewards.Add(key, new DataRelic.RelicDropData());
				}
				DataRelic.RelicDropData relicDropData = item.relicRewards[key];
				if (relicDropData.chance.Count((DataRelic.RelicDropData.RelicDropDataWithRarity p) => p.item.uniqueName == itemComponent.uniqueName) == 0)
				{
					relicDropData.chance.Add(new DataRelic.RelicDropData.RelicDropDataWithRarity
					{
						chance = drop.chance.GetValueOrDefault(),
						item = itemComponent,
						rarity = (DataRelic.RelicDropData.ItemRarity)Enum.Parse(typeof(DataRelic.RelicDropData.ItemRarity), drop.rarity)
					});
				}
			}
		}

		private void ParseComponentsFromItem(List<BigItem> itemList, Dictionary<string, ItemComponent> componentList)
		{
			foreach (BigItem item in itemList)
			{
				ItemComponent[] array = item.components ?? new ItemComponent[0];
				foreach (ItemComponent itemComponent in array)
				{
					itemComponent.isPartOf = item;
					if (componentList != null && !componentList.ContainsKey(itemComponent.uniqueName) && itemComponent.productCategory != "Pistols")
					{
						if (tradeableWeaponParts.Contains(itemComponent.uniqueName))
						{
							itemComponent.tradable = true;
						}
						else if ((item is DataArchMelee || item is DataArchGun || item is DataArchwing) && !item.name.Contains("Prime"))
						{
							itemComponent.tradable = false;
						}
						componentList.Add(itemComponent.uniqueName, itemComponent);
					}
				}
			}
		}

		public DataHandler()
		{
		}

		public bool LoadWarframeData(string str)
		{
			WarframeRootObject warframeRootObject = JsonConvert.DeserializeObject<WarframeRootObject>(str, new JsonSerializerSettings
			{
				Error = HandleDeserializationError
			});
			if (!string.IsNullOrEmpty(warframeRootObject.InventoryJSON))
			{
				warframeRootObject = JsonConvert.DeserializeObject<WarframeRootObject>(warframeRootObject.InventoryJSON, new JsonSerializerSettings
				{
					Error = HandleDeserializationError
				});
			}
			if (warframeRootObject == null || warframeRootObject.Suits == null || warframeRootObject.Suits.Length == 0 || warframeRootObject.LongGuns == null || warframeRootObject.LongGuns.Length == 0)
			{
				throw new Exception("Manually found non valid warframe data!");
			}
			bool flag = this.warframeRootObject == null && warframeRootObject != null;
			flag |= this.warframeRootObject?.LastInventorySync?.oid != warframeRootObject?.LastInventorySync?.oid;
			this.warframeRootObject = warframeRootObject;
			InitializeWarframeDataLookupTables();
			try
			{
				PrecalculateModUsedByTables();
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to Precaulculate mod tables: " + ex);
			}
			try
			{
				InventoryDeltaHelper.ApplyNewData(warframeRootObject);
			}
			catch (Exception ex2)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to update deltas on new data: " + ex2);
			}
			return flag;
		}

		private void InitializeWarframeDataLookupTables()
		{
			warframeRootObject.MiscItemsLookup = warframeRootObject.MiscItems.ToLookup((Miscitem p) => p.ItemType);
		}

		private void PrecalculateModUsedByTables()
		{
			modUsedInItems.Clear();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			ParseModeableItemList<DataWarframe>(warframeRootObject.Suits, warframes);
			ParseModeableItemList<DataPrimaryWeapon>(warframeRootObject.LongGuns, primaryWeapons);
			ParseModeableItemList<DataMeleeWeapon>(warframeRootObject.Melee, meleeWeapons);
			ParseModeableItemList<DataSecondaryWeapon>(warframeRootObject.Pistols, secondaryWeapons);
			ParseModeableItemList<DataArchwing>(warframeRootObject.SpaceSuits, archWings);
			ParseModeableItemList<DataArchGun>(warframeRootObject.SpaceGuns, archGuns);
			ParseModeableItemList<DataArchMelee>(warframeRootObject.SpaceMelee, archMelees);
			ParseModeableItemList<DataPet>(warframeRootObject.KubrowPets, pets);
			ParseModeableItemList<DataSentinel>(warframeRootObject.Sentinels, sentinels);
			ParseModeableItemList<DataSentinelWeapons>(warframeRootObject.SentinelWeapons, sentinelWeapons);
			ParseModeableItemList<DataPet>(warframeRootObject.MoaPets, pets);
			ParseModeableItemList<DataWarframe>(warframeRootObject.MechSuits, warframes);
			ParseModeableItemList<BigItem>(warframeRootObject.Scoops, null);
			ParseModeableItemList<BigItem>(warframeRootObject.CrewShips, null);
			ParseModeableItemList<BigItem>(warframeRootObject.CrewShipSalvagedWeapons, null);
			ParseModeableItemList<BigItem>(warframeRootObject.CrewShipWeapons, null);
			ParseModeableItemList<BigItem>(warframeRootObject.DataKnives, null);
			ParseModeableItemList<BigItem>(warframeRootObject.Hoverboards, null);
			ParseModeableItemList<BigItem>(warframeRootObject.OperatorAmps, null);
			ParseModeableItemList<BigItem>(warframeRootObject.SpecialItems, null);
			stopwatch.Stop();
			Console.WriteLine("Precalculating mod used by tables took: " + stopwatch.ElapsedMilliseconds + "ms");
			void ParseModeableItemList<T>(IEnumerable<ModeableItem> input, Dictionary<string, T> itemDataLocation) where T : BigItem
			{
				foreach (ModeableItem item in input ?? Enumerable.Empty<ModeableItem>())
				{
					if (item.Configs != null)
					{
						IEnumerable<Config> configs = item.Configs;
						foreach (Config item2 in configs ?? Enumerable.Empty<Config>())
						{
							if (item2.Upgrades != null)
							{
								IEnumerable<string> upgrades = item2.Upgrades;
								foreach (string item3 in upgrades ?? Enumerable.Empty<string>())
								{
									if (!string.IsNullOrEmpty(item3))
									{
										if (!modUsedInItems.ContainsKey(item3))
										{
											modUsedInItems.Add(item3, new List<(ModeableItem, BigItem)>());
										}
										modUsedInItems[item3].Add((item, (itemDataLocation != null) ? itemDataLocation.GetOrDefault(item.ItemType) : null));
									}
								}
							}
						}
					}
				}
			}
		}

		public static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
		{
			string message = errorArgs.ErrorContext.Error.Message;
			errorArgs.ErrorContext.Handled = true;
			Console.WriteLine(message);
		}

		public HashSet<string> ParseListFromFile(string path)
		{
			HashSet<string> hashSet = new HashSet<string>();
			if (!File.Exists(path))
			{
				return hashSet;
			}
			string[] array = File.ReadAllLines(path);
			foreach (string text in array)
			{
				if (!text.StartsWith("//"))
				{
					string text2 = text;
					if (text.Contains("//"))
					{
						text2 = text.Substring(0, text.IndexOf("//"));
					}
					text2 = text2.Trim();
					if (!string.IsNullOrEmpty(text2))
					{
						hashSet.Add(text2);
					}
				}
			}
			return hashSet;
		}
	}
}
