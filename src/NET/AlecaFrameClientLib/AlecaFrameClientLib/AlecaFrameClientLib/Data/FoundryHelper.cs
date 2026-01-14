using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class FoundryHelper
	{
		public class FoundryComponentTooltip
		{
			public string componentName;

			public List<PlayerRelicsForItem> relics;

			public bool showingAll = true;
		}

		public class PlayerRelicsForItem
		{
			public string relicName;

			public string imageURL;

			public int ownedAmount;

			public int percentDrop;

			public FoundryDetailsComponentDrop.DropType dropType;

			public string relicUID;
		}

		public static DateTime lastTimeUsernameFetched = DateTime.MinValue;

		public static string lastFetchedUsername = "AlecaFrame";

		public static DateTime lastTimeUnlockPercentUpdate = DateTime.MinValue;

		public static string lastCachedUnlockPercentage = "-%";

		public static int lastMasteryLevel = 0;

		public static FoundryPlayerStatsResponse GetPlayerStats()
		{
			try
			{
				if (DateTime.UtcNow - lastTimeUsernameFetched >= TimeSpan.FromMinutes(5.0))
				{
					lastTimeUsernameFetched = DateTime.UtcNow;
					if (TryGetUsernameFromAppdata(out var playerName))
					{
						if (playerName != lastFetchedUsername)
						{
							StaticData.Log(OverwolfWrapper.LogType.INFO, "Username changed from " + lastFetchedUsername + " to " + playerName);
						}
						lastFetchedUsername = playerName;
						try
						{
							File.WriteAllText(StaticData.saveFolder + "/lastUsername.txt", lastFetchedUsername);
						}
						catch
						{
						}
					}
					else
					{
						lastFetchedUsername = playerName;
						if (File.Exists(StaticData.saveFolder + "/lastUsername.txt"))
						{
							lastFetchedUsername = File.ReadAllText(StaticData.saveFolder + "/lastUsername.txt");
						}
						else
						{
							lastFetchedUsername = "AlecaFrame";
						}
					}
				}
			}
			catch (Exception ex)
			{
				lastFetchedUsername = "AlecaFrame";
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Error fetching username: " + ex.Message);
			}
			if (StaticData.dataHandler?.warframeRootObject == null)
			{
				return new FoundryPlayerStatsResponse
				{
					dataReady = false
				};
			}
			FoundryPlayerStatsResponse foundryPlayerStatsResponse = new FoundryPlayerStatsResponse();
			foundryPlayerStatsResponse.dataReady = true;
			foundryPlayerStatsResponse.playerMasteryLevel = StaticData.dataHandler.warframeRootObject.PlayerLevel.GetSIRepresentation();
			lastMasteryLevel = StaticData.dataHandler.warframeRootObject.PlayerLevel;
			foundryPlayerStatsResponse.platinum = StaticData.dataHandler.warframeRootObject.PremiumCredits.ToString();
			foundryPlayerStatsResponse.credits = StaticData.dataHandler.warframeRootObject.RegularCredits.GetSIRepresentation(2);
			foundryPlayerStatsResponse.endo = StaticData.dataHandler.warframeRootObject.FusionPoints.GetSIRepresentation(2);
			foundryPlayerStatsResponse.questsDone = StaticData.dataHandler.warframeRootObject.QuestKeys.Count((Questkey p) => p.Completed).GetSIRepresentation();
			foundryPlayerStatsResponse.questsTotal = StaticData.dataHandler.warframeRootObject.QuestKeys.Count().GetSIRepresentation();
			foundryPlayerStatsResponse.iconurl = Misc.GetMasteryLevelIcon(StaticData.dataHandler.warframeRootObject.PlayerLevel);
			if (DateTime.UtcNow - lastTimeUnlockPercentUpdate > TimeSpan.FromMinutes(3.0))
			{
				int unowned = 0;
				int owned = 0;
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.warframes.Values.Where((DataWarframe p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.primaryWeapons.Values.Where((DataPrimaryWeapon p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.secondaryWeapons.Values.Where((DataSecondaryWeapon p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.meleeWeapons.Values.Where((DataMeleeWeapon p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.archWings.Values.Where((DataArchwing p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.archMelees.Values.Where((DataArchMelee p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.archGuns.Values.Where((DataArchGun p) => p.IsPrime()));
				CountOwnedNotOwned(ref owned, ref unowned, StaticData.dataHandler.pets.Values.Where((DataPet p) => p.IsPrime()));
				if (StaticData.HideFoundersPackItemsEnabled)
				{
					DataWarframe orDefault = StaticData.dataHandler.warframes.GetOrDefault("/Lotus/Powersuits/Excalibur/ExcaliburPrime");
					if (orDefault == null || !orDefault.IsOwned())
					{
						unowned--;
					}
					DataSecondaryWeapon orDefault2 = StaticData.dataHandler.secondaryWeapons.GetOrDefault("/Lotus/Weapons/Tenno/Pistol/LatoPrime");
					if (orDefault2 == null || !orDefault2.IsOwned())
					{
						unowned--;
					}
					DataMeleeWeapon orDefault3 = StaticData.dataHandler.meleeWeapons.GetOrDefault("/Lotus/Weapons/Tenno/Melee/LongSword/SkanaPrime");
					if (orDefault3 == null || !orDefault3.IsOwned())
					{
						unowned--;
					}
				}
				if (unowned < 0)
				{
					unowned = 0;
				}
				lastCachedUnlockPercentage = Math.Floor((float)owned / (float)(unowned + owned) * 100f) + "%";
				lastTimeUnlockPercentUpdate = DateTime.UtcNow;
			}
			foundryPlayerStatsResponse.unlockPercent = lastCachedUnlockPercentage;
			try
			{
				foundryPlayerStatsResponse.ducados = (StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem p) => p.ItemType == "/Lotus/Types/Items/MiscItems/PrimeBucks")?.ItemCount ?? 0).ToString();
			}
			catch
			{
				foundryPlayerStatsResponse.ducados = "0";
			}
			foundryPlayerStatsResponse.playerName = lastFetchedUsername;
			return foundryPlayerStatsResponse;
		}

		private static void CountOwnedNotOwned(ref int owned, ref int unowned, IEnumerable<BigItem> toAnalyze)
		{
			int num = toAnalyze.Count((BigItem p) => p.IsOwned() || p.IsFullyMastered());
			owned += num;
			unowned += toAnalyze.Count() - num;
		}

		private static bool TryGetUsernameFromAppdata(out string playerName)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Warframe/EE.log";
			if (File.Exists(path))
			{
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using StreamReader streamReader = new StreamReader(stream, Encoding.Default);
					while (!streamReader.EndOfStream)
					{
						string text = streamReader.ReadLine();
						if (text != null && text.Contains("Logged in "))
						{
							playerName = text.Substring(text.IndexOf("Logged in") + 10).Split('(')[0].Trim();
							return true;
						}
					}
				}
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Username line not found in EE.log file");
			}
			StaticData.Log(OverwolfWrapper.LogType.WARN, "No EE.log file found for username check");
			playerName = "AlecaFrame";
			return false;
		}

		public static FoundryWorldStatusResponse GetWorldStats(bool includeDetailed)
		{
			FoundryWorldStatusResponse foundryWorldStatusResponse = new FoundryWorldStatusResponse();
			foundryWorldStatusResponse.timerData = WorldStateHelper.GetWorldTimerDetails();
			if (includeDetailed)
			{
				foundryWorldStatusResponse.dataLoaded = StaticData.dataHandler?.isInitialized ?? false;
				foundryWorldStatusResponse.primeResurgenceData = WorldStateHelper.GetPrimeResurgenceData();
				foundryWorldStatusResponse.baroData = WorldStateHelper.GetBaroData();
				foundryWorldStatusResponse.circuitData = WorldStateHelper.GetCircuitData();
			}
			return foundryWorldStatusResponse;
		}

		public static FoundryComponentTooltip GetPlayerRelicsToGetItem(string componentUniqueID, string componentName, bool showAll)
		{
			FoundryComponentTooltip foundryComponentTooltip = new FoundryComponentTooltip();
			foundryComponentTooltip.relics = new List<PlayerRelicsForItem>();
			foundryComponentTooltip.componentName = componentName;
			ItemComponent itemComponent = null;
			itemComponent = StaticData.dataHandler.warframeParts.GetOrDefault(componentUniqueID);
			if (itemComponent == null)
			{
				itemComponent = StaticData.dataHandler.weaponParts.GetOrDefault(componentUniqueID);
			}
			if (itemComponent == null)
			{
				return foundryComponentTooltip;
			}
			IEnumerable<Drop> drops = itemComponent.drops;
			foreach (Drop drop in drops ?? Enumerable.Empty<Drop>())
			{
				if (drop == null || !drop.location.ToLower().Contains("relic"))
				{
					continue;
				}
				if (!drop.location.Contains("("))
				{
					drop.location += " (Intact)";
				}
				string text = drop.location.Split('(')[0].Trim();
				drop.location.Split('(')[1].Replace(")", "").Trim();
				string key = text.Replace("Relic", "").Trim();
				if (!StaticData.dataHandler.relicsByShortName.ContainsKey(key))
				{
					continue;
				}
				DataRelic relicData = StaticData.dataHandler.relicsByShortName[key].FirstOrDefault((DataRelic p) => p.name == drop.location.Replace("Relic", "").Replace("  ", " ").Replace("(", "")
					.Replace(")", "")
					.Trim());
				if (relicData != null && !foundryComponentTooltip.relics.Any((PlayerRelicsForItem p) => p.relicUID == relicData.uniqueName))
				{
					int valueOrDefault = (StaticData.dataHandler.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == relicData.uniqueName)?.ItemCount).GetValueOrDefault();
					if (valueOrDefault != 0)
					{
						PlayerRelicsForItem item = new PlayerRelicsForItem
						{
							relicName = text,
							dropType = FoundryDetailsComponentDrop.DropType.Relic,
							imageURL = Misc.GetFullImagePath(relicData.imageName),
							ownedAmount = valueOrDefault,
							percentDrop = (int)Math.Round(drop.chance.GetValueOrDefault() * 100f, 1),
							relicUID = relicData.uniqueName
						};
						foundryComponentTooltip.relics.Add(item);
					}
				}
			}
			if (foundryComponentTooltip.relics.Count > 5 && !showAll)
			{
				foundryComponentTooltip.relics = foundryComponentTooltip.relics.OrderByDescending((PlayerRelicsForItem p) => p.ownedAmount).Take(5).ToList();
				foundryComponentTooltip.showingAll = false;
			}
			return foundryComponentTooltip;
		}

		public static bool IsARelicForThisComponentOwnedFAST(string componentUniqueID)
		{
			ItemComponent itemComponent = StaticData.dataHandler.warframeParts.GetOrDefault(componentUniqueID) ?? StaticData.dataHandler.weaponParts.GetOrDefault(componentUniqueID);
			if (itemComponent == null)
			{
				return false;
			}
			IEnumerable<Drop> drops = itemComponent.drops;
			foreach (Drop item in drops ?? Enumerable.Empty<Drop>())
			{
				if (item == null || (!item.location.EndsWith("Relic") && !item.location.EndsWith("(Intact)")))
				{
					continue;
				}
				string key = item.location.Split('(')[0].Trim().Replace("Relic", "").Trim();
				if (!StaticData.dataHandler.relicsByShortName.TryGetValue(key, out var value))
				{
					continue;
				}
				foreach (DataRelic item2 in value)
				{
					if ((StaticData.dataHandler.warframeRootObject?.MiscItemsLookup?[item2.uniqueName]?.Sum((Miscitem p) => p.ItemCount)).GetValueOrDefault() > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void Initialize()
		{
			if (File.Exists(StaticData.saveFolder + "/lastUsername.txt"))
			{
				lastFetchedUsername = File.ReadAllText(StaticData.saveFolder + "/lastUsername.txt");
			}
		}
	}
}
