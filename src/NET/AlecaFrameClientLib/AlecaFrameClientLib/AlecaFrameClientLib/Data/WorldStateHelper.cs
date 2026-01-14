using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public static class WorldStateHelper
	{
		public static TimeZoneInfo easternTimeZone = null;

		public static WarframeWorldState lastWorldStatusJSON_raw = null;

		private static DateTime worldJSON_RAW_requestSentTimestamp = DateTime.MinValue;

		private static object RAWWorldStateLockObject = new object();

		public static bool notiEarthDay = false;

		public static bool notiEarthDayDone = false;

		public static bool notiEarthNight = false;

		public static bool notiEarthNightDone = false;

		public static bool notiCetusDay = false;

		public static bool notiCetusDayDone = false;

		public static bool notiCetusNight = false;

		public static bool notiCetusNightDone = false;

		public static bool notiVallisWarm = false;

		public static bool notiVallisWarmDone = false;

		public static bool notiVallisCold = false;

		public static bool notiVallisColdDone = false;

		public static bool notiCambionFass = false;

		public static bool notiCambionFassDone = false;

		public static bool notiCambionVome = false;

		public static bool notiCambionVomeDone = false;

		public static bool fissureNotificationsEnabled = false;

		public static List<FissureNotificationSetting> fissureNotificationSettings = new List<FissureNotificationSetting>();

		public static Dictionary<string, DateTime> fissuresAlreadySeen = new Dictionary<string, DateTime>();

		private static readonly WorkdStateDuviriSourceData[] duviriStatesSource = new WorkdStateDuviriSourceData[5]
		{
			new WorkdStateDuviriSourceData
			{
				name = "Fear",
				pictureName = "fear"
			},
			new WorkdStateDuviriSourceData
			{
				name = "Joy",
				pictureName = "joy"
			},
			new WorkdStateDuviriSourceData
			{
				name = "Anger",
				pictureName = "anger"
			},
			new WorkdStateDuviriSourceData
			{
				name = "Envy",
				pictureName = "envy"
			},
			new WorkdStateDuviriSourceData
			{
				name = "Sorrow",
				pictureName = "sorrow"
			}
		};

		private static readonly DateTimeOffset duviriTimeReferenceCicleStart = DateTimeOffset.FromUnixTimeSeconds(1735819200L);

		private static readonly TimeSpan duviriSingleStateDuration = TimeSpan.FromHours(2.0);

		public static WorldStatePrimeResurgenceData GetPrimeResurgenceData()
		{
			WarframeWorldState rAWWorldState = GetRAWWorldState();
			if (rAWWorldState == null)
			{
				return null;
			}
			WorldStatePrimeResurgenceData worldStatePrimeResurgenceData = new WorldStatePrimeResurgenceData();
			WarframeWorldStatePrimevaulttrader warframeWorldStatePrimevaulttrader = rAWWorldState.PrimeVaultTraders.FirstOrDefault((WarframeWorldStatePrimevaulttrader p) => !p.Completed && p.Activation.date.numberLong < DateTime.UtcNow && p.Expiry.date.numberLong > DateTime.UtcNow);
			if (warframeWorldStatePrimevaulttrader != null)
			{
				worldStatePrimeResurgenceData.primeResurgenceEnabled = true;
				TimeSpan timeSpan = warframeWorldStatePrimevaulttrader.Expiry.date.numberLong - DateTime.UtcNow;
				worldStatePrimeResurgenceData.primeResurgenceEndsIn = $"{timeSpan.Days}d {timeSpan.Hours}h";
				worldStatePrimeResurgenceData.items = (from p in warframeWorldStatePrimevaulttrader.Manifest.Select(delegate(WarframeWorldStateDateManifest p)
					{
						string key = p.ItemType.Replace("StoreItems/", "");
						DataPrimaryWeapon dataPrimaryWeapon = StaticData.dataHandler?.primaryWeapons?.GetOrDefault(key);
						if (dataPrimaryWeapon != null)
						{
							return new FoundryItemData(dataPrimaryWeapon)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataSecondaryWeapon dataSecondaryWeapon = StaticData.dataHandler?.secondaryWeapons?.GetOrDefault(key);
						if (dataSecondaryWeapon != null)
						{
							return new FoundryItemData(dataSecondaryWeapon)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataMeleeWeapon dataMeleeWeapon = StaticData.dataHandler?.meleeWeapons?.GetOrDefault(key);
						if (dataMeleeWeapon != null)
						{
							return new FoundryItemData(dataMeleeWeapon)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataWarframe dataWarframe = StaticData.dataHandler?.warframes?.GetOrDefault(key);
						if (dataWarframe != null)
						{
							return new FoundryItemData(dataWarframe)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataArchwing dataArchwing = StaticData.dataHandler?.archWings?.GetOrDefault(key);
						if (dataArchwing != null)
						{
							return new FoundryItemData(dataArchwing)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataArchGun dataArchGun = StaticData.dataHandler?.archGuns?.GetOrDefault(key);
						if (dataArchGun != null)
						{
							return new FoundryItemData(dataArchGun)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataArchMelee dataArchMelee = StaticData.dataHandler?.archMelees?.GetOrDefault(key);
						if (dataArchMelee != null)
						{
							return new FoundryItemData(dataArchMelee)?.SetPremiumCurrencyCost(p.PrimePrice);
						}
						DataSentinel dataSentinel = StaticData.dataHandler?.sentinels?.GetOrDefault(key);
						return (dataSentinel != null) ? new FoundryItemData(dataSentinel)?.SetPremiumCurrencyCost(p.PrimePrice) : null;
					})
					where p != null
					select p).ToList();
			}
			return worldStatePrimeResurgenceData;
		}

		public static WorldStateBaroData GetBaroData()
		{
			WarframeWorldState rAWWorldState = GetRAWWorldState();
			if (rAWWorldState == null)
			{
				return null;
			}
			WorldStateBaroData worldStateBaroData = new WorldStateBaroData();
			WarframeWorldStateVoidtrader warframeWorldStateVoidtrader = rAWWorldState.VoidTraders.FirstOrDefault();
			if (warframeWorldStateVoidtrader != null)
			{
				worldStateBaroData.baroEnabled = DateTime.UtcNow > warframeWorldStateVoidtrader.Activation.date.numberLong && DateTime.UtcNow < warframeWorldStateVoidtrader.Expiry.date.numberLong;
				if (!worldStateBaroData.baroEnabled)
				{
					worldStateBaroData.baroArrivesOrEndsIn = $"Baro in {(warframeWorldStateVoidtrader.Activation.date.numberLong - DateTime.UtcNow).Days}d {(warframeWorldStateVoidtrader.Activation.date.numberLong - DateTime.UtcNow).Hours}h";
				}
				else if ((warframeWorldStateVoidtrader.Expiry.date.numberLong - DateTime.UtcNow).TotalHours >= 1.0)
				{
					worldStateBaroData.baroArrivesOrEndsIn = $"Baro leaves in {Math.Floor((warframeWorldStateVoidtrader.Expiry.date.numberLong - DateTime.UtcNow).TotalHours)}h";
				}
				else
				{
					worldStateBaroData.baroArrivesOrEndsIn = $"Baro leaves in {Math.Floor((warframeWorldStateVoidtrader.Expiry.date.numberLong - DateTime.UtcNow).TotalMinutes)}m";
				}
				worldStateBaroData.itemGroups.Add(new BaroReturnGroup
				{
					groupName = "Mods"
				});
				worldStateBaroData.itemGroups.Add(new BaroReturnGroup
				{
					groupName = "Weapons"
				});
				worldStateBaroData.itemGroups.Add(new BaroReturnGroup
				{
					groupName = "Cosmetics"
				});
				worldStateBaroData.itemGroups.Add(new BaroReturnGroup
				{
					groupName = "Others"
				});
				if (warframeWorldStateVoidtrader.Manifest != null)
				{
					WarframeWorldStateDateManifest[] manifest = warframeWorldStateVoidtrader.Manifest;
					foreach (WarframeWorldStateDateManifest p in manifest)
					{
						(string category, FoundryItemData item) itemData = GetFroundryItemFromBaro(p);
						if (!worldStateBaroData.itemGroups.Any((BaroReturnGroup q) => q.groupName == itemData.category))
						{
							worldStateBaroData.itemGroups.Add(new BaroReturnGroup
							{
								groupName = itemData.category
							});
						}
						worldStateBaroData.itemGroups.FirstOrDefault((BaroReturnGroup q) => q.groupName == itemData.category).items.Add(itemData.item);
					}
				}
				worldStateBaroData.itemGroups.RemoveAll((BaroReturnGroup baroReturnGroup) => baroReturnGroup.items.Count == 0);
			}
			return worldStateBaroData;
		}

		public static (string category, FoundryItemData item) GetFroundryItemFromBaro(WarframeWorldStateDateManifest p)
		{
			string nameToLookup = p.ItemType.Replace("StoreItems/", "");
			DataPrimaryWeapon dataPrimaryWeapon = StaticData.dataHandler?.primaryWeapons?.GetOrDefault(nameToLookup);
			if (dataPrimaryWeapon != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataPrimaryWeapon)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataSecondaryWeapon dataSecondaryWeapon = StaticData.dataHandler?.secondaryWeapons?.GetOrDefault(nameToLookup);
			if (dataSecondaryWeapon != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataSecondaryWeapon)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataMeleeWeapon dataMeleeWeapon = StaticData.dataHandler?.meleeWeapons?.GetOrDefault(nameToLookup);
			if (dataMeleeWeapon != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataMeleeWeapon)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataWarframe dataWarframe = StaticData.dataHandler?.warframes?.GetOrDefault(nameToLookup);
			if (dataWarframe != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataWarframe)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataArchwing dataArchwing = StaticData.dataHandler?.archWings?.GetOrDefault(nameToLookup);
			if (dataArchwing != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataArchwing)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataArchGun dataArchGun = StaticData.dataHandler?.archGuns?.GetOrDefault(nameToLookup);
			if (dataArchGun != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataArchGun)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataArchMelee dataArchMelee = StaticData.dataHandler?.archMelees?.GetOrDefault(nameToLookup);
			if (dataArchMelee != null)
			{
				return (category: "Weapons", item: new FoundryItemData(dataArchMelee)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataSkin dataSkin = StaticData.dataHandler?.skins?.GetOrDefault(nameToLookup);
			if (dataSkin != null)
			{
				return (category: "Cosmetics", item: new FoundryItemData(dataSkin)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataMod dataMod = StaticData.dataHandler?.mods?.GetOrDefault(nameToLookup);
			if (dataMod != null)
			{
				return (category: "Mods", item: new FoundryItemData(dataMod)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataQuest dataQuest = StaticData.dataHandler?.quests?.GetOrDefault(nameToLookup);
			if (dataQuest != null)
			{
				return (category: "Others", item: new FoundryItemData(dataQuest)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			ItemComponent itemComponent = StaticData.dataHandler?.questParts?.GetOrDefault(nameToLookup);
			if (itemComponent != null)
			{
				return (category: "Others", item: new FoundryItemData(itemComponent)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			DataMisc dataMisc = StaticData.dataHandler?.misc?.GetOrDefault(nameToLookup);
			if (dataMisc != null)
			{
				return (category: "Others", item: new FoundryItemData(dataMisc)?.SetPremiumCurrencyCost(p.PrimePrice));
			}
			return (category: "Others", item: new FoundryItemData
			{
				name = (nameToLookup.Contains("/AvatarImages/") ? "Icon" : (nameToLookup.Contains("/Boosters/") ? "Booster" : "Other")),
				internalName = nameToLookup,
				type = "Unknown",
				premiumCost = p.PrimePrice,
				picture = "assets/img/question.png",
				owned = (StaticData.dataHandler?.warframeRootObject?.FlavourItems?.Any((Miscitem l) => l.ItemType == nameToLookup) == true || StaticData.dataHandler?.warframeRootObject?.MiscItems?.Any((Miscitem l) => l.ItemType == nameToLookup) == true)
			});
		}

		public static WorldStateTimerData GetWorldTimerDetails()
		{
			if (easternTimeZone == null)
			{
				try
				{
					easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to find Eastern Standard Time timezone: " + ex);
					easternTimeZone = TimeZoneInfo.Utc;
				}
			}
			WorldStateTimerData worldStateTimerData = new WorldStateTimerData();
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime dateTime2 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime dateTime3 = new DateTime(2018, 11, 10, 8, 13, 48, 0, DateTimeKind.Utc);
			WarframeWorldState rAWWorldState = GetRAWWorldState();
			if (rAWWorldState != null)
			{
				WarframeWorldStateSyndicatemission warframeWorldStateSyndicatemission = rAWWorldState?.SyndicateMissions?.FirstOrDefault((WarframeWorldStateSyndicatemission p) => p.Tag == "CetusSyndicate");
				if (warframeWorldStateSyndicatemission != null)
				{
					dateTime2 = warframeWorldStateSyndicatemission.Activation.date.numberLong;
				}
			}
			TimeSpan timeSpan = TimeSpan.FromTicks((DateTime.UtcNow - dateTime).Ticks % TimeSpan.FromHours(8.0).Ticks);
			if (timeSpan > TimeSpan.FromHours(4.0))
			{
				worldStateTimerData.earthState = "night";
				timeSpan -= TimeSpan.FromHours(4.0);
			}
			else
			{
				worldStateTimerData.earthState = "day";
			}
			TimeSpan timeSpan2 = TimeSpan.FromHours(4.0) - timeSpan;
			worldStateTimerData.earthTimeLeft = (((timeSpan2.Hours > 0) ? (timeSpan2.Hours + "h ") : "") + ((timeSpan2.Minutes > 0) ? (timeSpan2.Minutes + "m ") : "") + ((timeSpan2.Seconds > 0) ? (timeSpan2.Seconds + "s ") : "")).Trim();
			TimeSpan timeSpan3 = TimeSpan.FromTicks((DateTime.UtcNow - dateTime2).Ticks % TimeSpan.FromMinutes(150.0).Ticks);
			TimeSpan timeSpan4;
			if (timeSpan3 > TimeSpan.FromMinutes(100.0))
			{
				worldStateTimerData.cetusState = "night";
				worldStateTimerData.cambionState = "vome";
				timeSpan4 = TimeSpan.FromMinutes(150.0) - timeSpan3;
			}
			else
			{
				worldStateTimerData.cetusState = "day";
				worldStateTimerData.cambionState = "fass";
				timeSpan4 = TimeSpan.FromMinutes(100.0) - timeSpan3;
			}
			worldStateTimerData.cetusTimeLeft = (((timeSpan4.Hours > 0) ? (timeSpan4.Hours + "h ") : "") + ((timeSpan4.Minutes > 0) ? (timeSpan4.Minutes + "m ") : "") + ((timeSpan4.Seconds > 0) ? (timeSpan4.Seconds + "s ") : "")).Trim();
			worldStateTimerData.cambionTimeLeft = (((timeSpan4.Hours > 0) ? (timeSpan4.Hours + "h ") : "") + ((timeSpan4.Minutes > 0) ? (timeSpan4.Minutes + "m ") : "") + ((timeSpan4.Seconds > 0) ? (timeSpan4.Seconds + "s ") : "")).Trim();
			TimeSpan timeSpan5 = TimeSpan.FromTicks((DateTime.UtcNow - dateTime3).Ticks % TimeSpan.FromMilliseconds(1600000.0).Ticks);
			TimeSpan timeSpan6;
			if (timeSpan5 > TimeSpan.FromMilliseconds(400000.0))
			{
				worldStateTimerData.vallisState = "cold";
				timeSpan6 = TimeSpan.FromMinutes(20.0) + TimeSpan.FromMinutes(6.0) + TimeSpan.FromSeconds(40.0) - timeSpan5;
			}
			else
			{
				worldStateTimerData.vallisState = "warm";
				timeSpan6 = TimeSpan.FromMinutes(6.0) + TimeSpan.FromSeconds(40.0) - timeSpan5;
			}
			worldStateTimerData.vallisTimeLeft = (((timeSpan6.Hours > 0) ? (timeSpan6.Hours + "h ") : "") + ((timeSpan6.Minutes > 0) ? (timeSpan6.Minutes + "m ") : "") + ((timeSpan6.Seconds > 0) ? (timeSpan6.Seconds + "s ") : "")).Trim();
			if (easternTimeZone.BaseUtcOffset.TotalMinutes == 0.0)
			{
				worldStateTimerData.sortieTimeLeft = "N/A";
			}
			else
			{
				DateTime dateTime4 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternTimeZone);
				DateTime dateTime5 = new DateTime(dateTime4.Year, dateTime4.Month, dateTime4.Day, 12, 0, 0, DateTimeKind.Unspecified);
				if (dateTime4 > dateTime5)
				{
					dateTime5 = dateTime5.AddDays(1.0);
				}
				TimeSpan timeSpan7 = dateTime5 - dateTime4;
				worldStateTimerData.sortieTimeLeft = (((timeSpan7.Hours > 0) ? (timeSpan7.Hours + "h ") : "") + ((timeSpan7.Minutes > 0) ? (timeSpan7.Minutes + "m ") : "")).Trim();
			}
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime6 = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Unspecified);
			do
			{
				dateTime6 = dateTime6.AddDays(1.0);
			}
			while (dateTime6.DayOfWeek != DayOfWeek.Monday);
			DateTime dateTime7 = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Unspecified);
			if (utcNow > dateTime7)
			{
				dateTime7 = dateTime7.AddDays(1.0);
			}
			TimeSpan timeSpan8 = dateTime6 - utcNow;
			worldStateTimerData.weeklyTimeLeft = (((timeSpan8.Days > 0) ? (timeSpan8.Days + "d ") : "") + ((timeSpan8.Hours > 0) ? (timeSpan8.Hours + "h ") : "") + ((timeSpan8.Minutes > 0) ? (timeSpan8.Minutes + "m ") : "")).Trim();
			TimeSpan timeSpan9 = dateTime7 - utcNow;
			worldStateTimerData.dailyUTCTimeLeft = (((timeSpan9.Hours > 0) ? (timeSpan9.Hours + "h ") : "") + ((timeSpan9.Minutes > 0) ? (timeSpan9.Minutes + "m ") : "")).Trim();
			worldStateTimerData.duviriTimers = GetDuviriTimerData();
			worldStateTimerData.relicDataPoints = new List<WorldStateRelicDataPointGroup>();
			List<WorldStateRelicDataPoint> list = new List<WorldStateRelicDataPoint>();
			if (StaticData.dataHandler != null)
			{
				WarframeWorldStateActivemission[] activeMissions = rAWWorldState.ActiveMissions;
				foreach (WarframeWorldStateActivemission relic in activeMissions)
				{
					WorldStateRelicDataPoint worldStateRelicDataPoint = new WorldStateRelicDataPoint();
					KeyValuePair<string, DataNode> keyValuePair = StaticData.dataHandler.nodes.FirstOrDefault((KeyValuePair<string, DataNode> p) => p.Key == relic.Node);
					worldStateRelicDataPoint.steelPath = relic.Hard;
					worldStateRelicDataPoint.uniqueKey = relic._id.oid;
					if (keyValuePair.Value != null)
					{
						worldStateRelicDataPoint.planetAlone = keyValuePair.Value.systemName;
						if (worldStateRelicDataPoint.planetAlone == "Kuva Fortress")
						{
							worldStateRelicDataPoint.planetAlone = "Kuva Fort.";
						}
						worldStateRelicDataPoint.planet = keyValuePair.Value.name + ", " + worldStateRelicDataPoint.planetAlone;
					}
					else
					{
						worldStateRelicDataPoint.planet = "???";
					}
					switch (relic.Modifier)
					{
					case "VoidT1":
						worldStateRelicDataPoint.relicTier = "lith";
						break;
					case "VoidT2":
						worldStateRelicDataPoint.relicTier = "meso";
						break;
					case "VoidT3":
						worldStateRelicDataPoint.relicTier = "neo";
						break;
					case "VoidT4":
						worldStateRelicDataPoint.relicTier = "axi";
						break;
					case "VoidT5":
						worldStateRelicDataPoint.relicTier = "requiem";
						break;
					case "VoidT6":
						worldStateRelicDataPoint.relicTier = "omnia";
						break;
					default:
						worldStateRelicDataPoint.relicTier = "???";
						break;
					}
					TimeSpan timeSpan10 = (worldStateRelicDataPoint.timeLeftRAW = relic.Expiry.date.numberLong - DateTime.UtcNow);
					worldStateRelicDataPoint.timeLeft = (((timeSpan10.Hours > 0) ? (timeSpan10.Hours + "h ") : "") + ((timeSpan10.Minutes > 0) ? (timeSpan10.Minutes + "m ") : "") + ((timeSpan10.Seconds > 0) ? (timeSpan10.Seconds + "s ") : "")).Trim();
					switch (relic.MissionType)
					{
					case "MT_EXCAVATE":
						worldStateRelicDataPoint.type = "Excavation";
						break;
					case "MT_CAPTURE":
						worldStateRelicDataPoint.type = "Capture";
						break;
					case "MT_DEFENSE":
						worldStateRelicDataPoint.type = "Defense";
						break;
					case "MT_MOBILE_DEFENSE":
						worldStateRelicDataPoint.type = "Mobile Def.";
						break;
					case "MT_SURVIVAL":
						worldStateRelicDataPoint.type = "Survival";
						break;
					case "MT_RESCUE":
						worldStateRelicDataPoint.type = "Rescue";
						break;
					case "MT_SABOTAGE":
						worldStateRelicDataPoint.type = "Sabotage";
						break;
					case "MT_CROSSFIRE":
						worldStateRelicDataPoint.type = "Crossfire";
						break;
					case "MT_EXTERMINATION":
						worldStateRelicDataPoint.type = "Extermin.";
						break;
					case "MT_HIVE":
						worldStateRelicDataPoint.type = "Hive";
						break;
					case "MT_INTEL":
						worldStateRelicDataPoint.type = "Spy";
						break;
					case "MT_TERRITORY":
						worldStateRelicDataPoint.type = "Intercep.";
						break;
					case "MT_ASSAULT":
						worldStateRelicDataPoint.type = "Assault";
						break;
					case "MT_ARTIFACT":
						worldStateRelicDataPoint.type = "Disruption";
						break;
					case "MT_ALCHEMY":
						worldStateRelicDataPoint.type = "Alchemy";
						break;
					case "MT_VOID_CASCADE":
						worldStateRelicDataPoint.type = "Void Cascade";
						break;
					case "MT_CORRUPTION":
						worldStateRelicDataPoint.type = "Void Flood";
						break;
					default:
						worldStateRelicDataPoint.type = "???";
						break;
					}
					list.Add(worldStateRelicDataPoint);
				}
			}
			bool flag = fissuresAlreadySeen.Count == 0;
			lock (list)
			{
				foreach (WorldStateRelicDataPoint fissure in list)
				{
					if (fissureNotificationsEnabled && fissureNotificationSettings.Any(delegate(FissureNotificationSetting p)
					{
						if (p.type != "all" && p.type != fissure.relicTier)
						{
							return false;
						}
						if (p.mode != "all" && p.mode != fissure.type)
						{
							return false;
						}
						if (p.location != "all" && p.location != fissure.planetAlone)
						{
							return false;
						}
						return (!(p.steelPath != "all") || fissure.steelPath == (p.steelPath == "steelPath")) ? true : false;
					}))
					{
						fissure.matchesNotification = true;
						if (!flag && !fissuresAlreadySeen.ContainsKey(fissure.uniqueKey))
						{
							OCRHelper.SendFissureNotification(fissure.type, fissure.planet, fissure.relicTier, fissure.timeLeftRAW, fissure.steelPath);
						}
					}
					fissuresAlreadySeen[fissure.uniqueKey] = DateTime.UtcNow;
				}
			}
			if (fissuresAlreadySeen.Count > 250)
			{
				fissuresAlreadySeen = fissuresAlreadySeen.Where((KeyValuePair<string, DateTime> p) => (DateTime.UtcNow - p.Value).TotalHours < 3.0).ToDictionary((KeyValuePair<string, DateTime> p) => p.Key, (KeyValuePair<string, DateTime> p) => p.Value);
			}
			worldStateTimerData.relicDataPoints = (from p in list
				where !string.IsNullOrWhiteSpace(p.timeLeft)
				group p by p.relicTier into p
				select new WorldStateRelicDataPointGroup
				{
					relicType = p.Key,
					fissures = p.ToList()
				} into p
				orderby Misc.RelicOrderIndex(p.relicType)
				select p).ToList();
			if (notiVallisCold)
			{
				if (worldStateTimerData.vallisState == "warm")
				{
					if (timeSpan6 < StaticData.timeAheadTimerNotifications && !notiVallisColdDone)
					{
						SendTimerNotification("Vallis", "Cold");
						notiVallisColdDone = true;
					}
				}
				else
				{
					notiVallisColdDone = false;
				}
			}
			else
			{
				notiVallisColdDone = false;
			}
			if (notiVallisWarm)
			{
				if (worldStateTimerData.vallisState == "cold")
				{
					if (timeSpan6 < StaticData.timeAheadTimerNotifications && !notiVallisWarmDone)
					{
						SendTimerNotification("Vallis", "Warm");
						notiVallisWarmDone = true;
					}
				}
				else
				{
					notiVallisWarmDone = false;
				}
			}
			else
			{
				notiVallisWarmDone = false;
			}
			if (notiEarthDay)
			{
				if (worldStateTimerData.earthState == "night")
				{
					if (timeSpan2 < StaticData.timeAheadTimerNotifications && !notiEarthDayDone)
					{
						SendTimerNotification("Earth", "Day");
						notiEarthDayDone = true;
					}
				}
				else
				{
					notiEarthDayDone = false;
				}
			}
			else
			{
				notiEarthDayDone = false;
			}
			if (notiEarthNight)
			{
				if (worldStateTimerData.earthState == "day")
				{
					if (timeSpan2 < StaticData.timeAheadTimerNotifications && !notiEarthNightDone)
					{
						SendTimerNotification("Earth", "Night");
						notiEarthNightDone = true;
					}
				}
				else
				{
					notiEarthNightDone = false;
				}
			}
			else
			{
				notiEarthNightDone = false;
			}
			if (notiCetusDay)
			{
				if (worldStateTimerData.cetusState == "night")
				{
					if (timeSpan4 < StaticData.timeAheadTimerNotifications && !notiCetusDayDone)
					{
						SendTimerNotification("Cetus", "Day");
						notiCetusDayDone = true;
					}
				}
				else
				{
					notiCetusDayDone = false;
				}
			}
			else
			{
				notiCetusDayDone = false;
			}
			if (notiCetusNight)
			{
				if (worldStateTimerData.cetusState == "day")
				{
					if (timeSpan4 < StaticData.timeAheadTimerNotifications && !notiCetusNightDone)
					{
						SendTimerNotification("Cetus", "Night");
						notiCetusNightDone = true;
					}
				}
				else
				{
					notiCetusNightDone = false;
				}
			}
			else
			{
				notiCetusNightDone = false;
			}
			if (notiCambionFass)
			{
				if (worldStateTimerData.cambionState == "vome")
				{
					if (timeSpan4 < StaticData.timeAheadTimerNotifications && !notiCambionFassDone)
					{
						SendTimerNotification("Cambion", "Fass");
						notiCambionFassDone = true;
					}
				}
				else
				{
					notiCambionFassDone = false;
				}
			}
			else
			{
				notiCambionFassDone = false;
			}
			if (notiCambionVome)
			{
				if (worldStateTimerData.cambionState == "fass")
				{
					if (timeSpan4 < StaticData.timeAheadTimerNotifications && !notiCambionVomeDone)
					{
						SendTimerNotification("Cambion", "Vome");
						notiCambionVomeDone = true;
					}
				}
				else
				{
					notiCambionVomeDone = false;
				}
			}
			else
			{
				notiCambionVomeDone = false;
			}
			return worldStateTimerData;
		}

		private static WarframeWorldState GetRAWWorldState()
		{
			lock (RAWWorldStateLockObject)
			{
				bool num = (DateTime.UtcNow - worldJSON_RAW_requestSentTimestamp).TotalSeconds > 30.0;
				bool flag = lastWorldStatusJSON_raw != null;
				if (num || !flag)
				{
					worldJSON_RAW_requestSentTimestamp = DateTime.UtcNow;
					Task task = Task.Run(delegate
					{
						try
						{
							lastWorldStatusJSON_raw = JsonConvert.DeserializeObject<WarframeWorldState>(HTTPHandler.MakeGETRequest("https://content.warframe.com/dynamic/worldState.php", 30000, 1), new JsonSerializerSettings
							{
								Error = DataHandler.HandleDeserializationError
							});
							worldJSON_RAW_requestSentTimestamp = DateTime.UtcNow;
						}
						catch (Exception ex)
						{
							StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to request world state: " + ex);
						}
					});
					if (!flag)
					{
						task.Wait();
					}
				}
				return lastWorldStatusJSON_raw;
			}
		}

		public static void SendTimerNotification(string worldName, string stateToChangeTo)
		{
			OCRHelper.SendTimerNotification(worldName, stateToChangeTo, StaticData.timeAheadTimerNotifications.TotalMinutes + " minutes");
		}

		public static WorldStateCircuitData GetCircuitData()
		{
			WarframeWorldState rAWWorldState = GetRAWWorldState();
			if (rAWWorldState?.EndlessXpChoices == null || (rAWWorldState != null && rAWWorldState.EndlessXpChoices?.Length < 2))
			{
				return new WorldStateCircuitData();
			}
			WorldStateCircuitData worldStateCircuitData = new WorldStateCircuitData();
			worldStateCircuitData.normal = new List<WorldStateCircuitData.WorldStateCircuitDataItem>();
			worldStateCircuitData.steel = new List<WorldStateCircuitData.WorldStateCircuitDataItem>();
			string[] array = ((rAWWorldState != null) ? rAWWorldState.EndlessXpChoices[0].Choices : null);
			foreach (string item in array)
			{
				WorldStateCircuitData.WorldStateCircuitDataItem worldStateCircuitDataItem = new WorldStateCircuitData.WorldStateCircuitDataItem();
				BigItem bigItem = StaticData.dataHandler?.warframes?.Values?.FirstOrDefault((DataWarframe p) => p.name.Replace(" ", "").Replace("&", "And") == item);
				if (bigItem != null)
				{
					worldStateCircuitDataItem.name = bigItem.name;
					worldStateCircuitDataItem.picture = Misc.GetFullImagePath(bigItem.imageName);
					worldStateCircuitDataItem.uniqueID = bigItem.uniqueName;
					FoundryItemData foundryItemData = new FoundryItemData((DataWarframe)bigItem);
					worldStateCircuitDataItem.owned = foundryItemData.owned || foundryItemData.mastered;
				}
				else
				{
					worldStateCircuitDataItem.name = "???";
					worldStateCircuitDataItem.picture = "assets/img/question.png";
					worldStateCircuitDataItem.uniqueID = "???";
				}
				worldStateCircuitData.normal.Add(worldStateCircuitDataItem);
			}
			array = ((rAWWorldState != null) ? rAWWorldState.EndlessXpChoices[1].Choices : null);
			foreach (string item2 in array)
			{
				WorldStateCircuitData.WorldStateCircuitDataItem worldStateCircuitDataItem2 = new WorldStateCircuitData.WorldStateCircuitDataItem();
				BigItem bigItem2 = (BigItem)(StaticData.dataHandler?.primaryWeapons?.Values?.FirstOrDefault((DataPrimaryWeapon p) => p.name.Replace(" ", "").Replace("&", "And") == item2) ?? ((object)StaticData.dataHandler?.secondaryWeapons?.Values?.FirstOrDefault((DataSecondaryWeapon p) => p.name.Replace(" ", "").Replace("&", "And") == item2)) ?? ((object)StaticData.dataHandler?.meleeWeapons?.Values?.FirstOrDefault((DataMeleeWeapon p) => p.name.Replace(" ", "").Replace("&", "And") == item2)));
				if (bigItem2 != null)
				{
					worldStateCircuitDataItem2.name = bigItem2.name;
					worldStateCircuitDataItem2.picture = Misc.GetFullImagePath(bigItem2.imageName);
					worldStateCircuitDataItem2.uniqueID = bigItem2.uniqueName;
					string incarnonAdapterName = worldStateCircuitDataItem2.name + " Incarnon Genesis";
					string incarnonUniqueID = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc p) => p.name == incarnonAdapterName)?.uniqueName;
					worldStateCircuitDataItem2.owned = StaticData.dataHandler?.warframeRootObject?.MiscItems?.Any((Miscitem l) => l.ItemType == incarnonUniqueID) == true;
					if (!worldStateCircuitDataItem2.owned)
					{
						foreach (BigItem item3 in (from p in Misc.GetCompatibleWeaponsUIDs(bigItem2.uniqueName)
							select Misc.GetBigItemRefernceOrNull(p) into p
							where p != null
							select p).ToList())
						{
							if (item3 is DataPrimaryWeapon)
							{
								worldStateCircuitDataItem2.owned = worldStateCircuitDataItem2.owned || new FoundryItemData((DataPrimaryWeapon)item3).incarnion;
							}
							else if (item3 is DataSecondaryWeapon)
							{
								worldStateCircuitDataItem2.owned = worldStateCircuitDataItem2.owned || new FoundryItemData((DataSecondaryWeapon)item3).incarnion;
							}
							else if (item3 is DataMeleeWeapon)
							{
								worldStateCircuitDataItem2.owned = worldStateCircuitDataItem2.owned || new FoundryItemData((DataMeleeWeapon)item3).incarnion;
							}
						}
					}
				}
				else
				{
					worldStateCircuitDataItem2.name = "???";
					worldStateCircuitDataItem2.picture = "assets/img/question.png";
					worldStateCircuitDataItem2.uniqueID = "???";
				}
				worldStateCircuitData.steel.Add(worldStateCircuitDataItem2);
			}
			return worldStateCircuitData;
		}

		private static WorldStateDuviriTimerData.WorldStateDuviryTimerDataState GetSingleDuviriState(DateTimeOffset utcTime)
		{
			long num = (long)(utcTime - duviriTimeReferenceCicleStart).TotalSeconds;
			int num2 = (int)duviriSingleStateDuration.TotalSeconds * duviriStatesSource.Length;
			int num3 = (int)(num % num2) / (int)duviriSingleStateDuration.TotalSeconds;
			int num4 = (int)((double)num % duviriSingleStateDuration.TotalSeconds);
			TimeSpan timeSpan = duviriSingleStateDuration - TimeSpan.FromSeconds(num4);
			WorldStateDuviriTimerData.WorldStateDuviryTimerDataState obj = new WorldStateDuviriTimerData.WorldStateDuviryTimerDataState
			{
				name = duviriStatesSource[num3].name,
				picture = duviriStatesSource[num3].pictureName,
				startTime = utcTime.AddSeconds(-num4)
			};
			obj.endTime = obj.startTime.Add(duviriSingleStateDuration);
			obj.timeLeft = (((timeSpan.Hours > 0) ? (timeSpan.Hours + "h ") : "") + ((timeSpan.Minutes > 0) ? (timeSpan.Minutes + "m ") : "") + ((timeSpan.Seconds > 0) ? (timeSpan.Seconds + "s ") : "")).Trim();
			return obj;
		}

		public static WorldStateDuviriTimerData GetDuviriTimerData()
		{
			WorldStateDuviriTimerData obj = new WorldStateDuviriTimerData
			{
				current = GetSingleDuviriState(DateTime.UtcNow)
			};
			obj.next = GetSingleDuviriState(obj.current.endTime.AddSeconds(1.0));
			return obj;
		}
	}
}
