using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public static class StatsHandler
	{
		private static Dictionary<Misc.WarframeLanguage, TradeLogMessages> tradeLogMessagesByLanguage = new Dictionary<Misc.WarframeLanguage, TradeLogMessages> { 
		{
			Misc.WarframeLanguage.English,
			new TradeLogMessages
			{
				detectLine = "description=Are you sure you want to accept this trade? You are offering",
				willReceiveLineFirstPart = "and will receive from ",
				willReceiveLineSecondPart = " the following:",
				platinumName = "Platinum"
			}
		} };

		private static List<(string, string)> arcaneLogUTF8Pairs = new List<(string, string)>
		{
			("î‚ƒ", "î‚†"),
			("\ue070", "\ue071"),
			("\ue080", "\ue081"),
			("\ue072", "\ue0a2"),
			("\ue070", "\ue071"),
			(Encoding.UTF8.GetString(new byte[3] { 238, 129, 176 }), Encoding.UTF8.GetString(new byte[3] { 238, 129, 177 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 129, 173 }), Encoding.UTF8.GetString(new byte[3] { 238, 129, 170 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 129, 173 }), Encoding.UTF8.GetString(new byte[3] { 238, 129, 188 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 131, 170 }), Encoding.UTF8.GetString(new byte[3] { 238, 131, 171 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 129, 184 }), Encoding.UTF8.GetString(new byte[3] { 238, 129, 191 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 129, 174 }), Encoding.UTF8.GetString(new byte[3] { 238, 129, 175 })),
			(Encoding.UTF8.GetString(new byte[3] { 238, 130, 158 }), Encoding.UTF8.GetString(new byte[3] { 238, 130, 159 }))
		};

		private static int relicOpenedDelta = 0;

		private static int tradeDelta = 0;

		private static List<string> currentTradeLog = new List<string>();

		private static PlayerStatsTrade currentProcessingTrade = new PlayerStatsTrade();

		private static DateTime tradeProcessedTimestamp = DateTime.MinValue;

		public static void RelicJustOpened_BG()
		{
			relicOpenedDelta++;
			NewDataJustReceived_BG();
		}

		public static bool IsBeginninigOfTradeLog(string message)
		{
			Misc.WarframeLanguage warframeLanguage = Misc.GetWarframeLanguage(defaultToEnglish: false);
			if (warframeLanguage == Misc.WarframeLanguage.UNKNOWN)
			{
				return false;
			}
			if (!tradeLogMessagesByLanguage.ContainsKey(warframeLanguage))
			{
				return false;
			}
			return message.Contains(tradeLogMessagesByLanguage[warframeLanguage].detectLine);
		}

		public static void StartTradeLog(string message)
		{
			currentTradeLog.Clear();
			ReceivedTradeLogMessage(message);
		}

		public static void ReceivedTradeLogMessage(string message)
		{
			currentTradeLog.Add(message);
		}

		public static void TradeLogsFinished()
		{
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Processing trade log: " + Environment.NewLine + string.Join(Environment.NewLine, currentTradeLog));
			List<string> list = currentTradeLog.ToList();
			currentProcessingTrade = new PlayerStatsTrade();
			currentProcessingTrade.ts = DateTime.UtcNow;
			try
			{
				Misc.WarframeLanguage warframeLanguage = Misc.GetWarframeLanguage(defaultToEnglish: false);
				if (warframeLanguage == Misc.WarframeLanguage.UNKNOWN || !tradeLogMessagesByLanguage.ContainsKey(warframeLanguage))
				{
					return;
				}
				string[] array = list[0].Split('\n');
				list.RemoveAt(0);
				for (int i = 0; i < array.Length; i++)
				{
					list.Insert(i, array[i]);
				}
				bool flag = true;
				foreach (string item in list)
				{
					if (string.IsNullOrEmpty(item) || item == "\n" || item.Contains(tradeLogMessagesByLanguage[warframeLanguage].detectLine))
					{
						continue;
					}
					if (item.Contains(tradeLogMessagesByLanguage[warframeLanguage].willReceiveLineFirstPart) && item.Contains(tradeLogMessagesByLanguage[warframeLanguage].willReceiveLineSecondPart))
					{
						currentProcessingTrade.user = item.Replace(tradeLogMessagesByLanguage[warframeLanguage].willReceiveLineFirstPart, "").Replace(tradeLogMessagesByLanguage[warframeLanguage].willReceiveLineSecondPart, "").Trim();
						flag = false;
						continue;
					}
					string text = item;
					if (item.Contains(", leftItem=/"))
					{
						text = item.Substring(0, item.IndexOf(", leftItem=/"));
					}
					text = text.Replace("\r", "").Replace("\n", "");
					string itemName = "";
					int cnt = 1;
					if (text.Contains(" x "))
					{
						itemName = text.Substring(0, text.IndexOf(" x "));
						cnt = int.Parse(text.Substring(text.IndexOf(" x ") + 3));
					}
					else
					{
						itemName = text;
					}
					itemName = itemName.Trim();
					if (itemName == tradeLogMessagesByLanguage[warframeLanguage].platinumName)
					{
						itemName = "plat";
					}
					if (string.IsNullOrEmpty(itemName))
					{
						continue;
					}
					if (flag)
					{
						if (currentProcessingTrade.tx.Any((PlayerStatsTradeTradedObjectInfo p) => p.name == itemName))
						{
							currentProcessingTrade.tx.First((PlayerStatsTradeTradedObjectInfo p) => p.name == itemName).cnt++;
							continue;
						}
						currentProcessingTrade.tx.Add(new PlayerStatsTradeTradedObjectInfo
						{
							name = itemName,
							cnt = cnt,
							displayName = itemName
						});
					}
					else if (currentProcessingTrade.rx.Any((PlayerStatsTradeTradedObjectInfo p) => p.name == itemName))
					{
						currentProcessingTrade.rx.First((PlayerStatsTradeTradedObjectInfo p) => p.name == itemName).cnt++;
					}
					else
					{
						currentProcessingTrade.rx.Add(new PlayerStatsTradeTradedObjectInfo
						{
							name = itemName,
							cnt = cnt,
							displayName = itemName
						});
					}
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Trade log processed: " + currentProcessingTrade.ToString());
				ProcessCurrentTrade();
				tradeProcessedTimestamp = DateTime.UtcNow;
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to parse trade logs:" + ex);
			}
		}

		private static void ProcessCurrentTrade()
		{
			if (currentProcessingTrade == null)
			{
				return;
			}
			int num = 0;
			int num2 = currentProcessingTrade.tx.Count + currentProcessingTrade.rx.Count;
			new List<(string, string)>();
			new List<(string, string)>();
			new List<(string, int)>();
			new List<(string, int)>();
			foreach (PlayerStatsTradeTradedObjectInfo item in currentProcessingTrade.rx)
			{
				if (!ConvertItemNameToId(item))
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find RX trade item: " + item.name);
					num++;
				}
			}
			foreach (PlayerStatsTradeTradedObjectInfo item2 in currentProcessingTrade.tx)
			{
				if (!ConvertItemNameToId(item2))
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find TX trade item: " + item2.name);
					num++;
				}
			}
			bool num3 = currentProcessingTrade.rx.Any((PlayerStatsTradeTradedObjectInfo p) => p.name == "/AF_Special/Platinum");
			bool flag = currentProcessingTrade.tx.Any((PlayerStatsTradeTradedObjectInfo p) => p.name == "/AF_Special/Platinum");
			if (num3 && currentProcessingTrade.rx.Count == 1)
			{
				currentProcessingTrade.type = TradeClassification.Sale;
			}
			else if (flag && currentProcessingTrade.tx.Count == 1)
			{
				currentProcessingTrade.type = TradeClassification.Purchase;
			}
			else
			{
				currentProcessingTrade.type = TradeClassification.Trade;
			}
			StaticData.Log(OverwolfWrapper.LogType.WARN, $"Trade log converted. {num2 - num}/{num2} OK. Classification: {currentProcessingTrade.type}");
		}

		public static bool ConvertItemNameToId(PlayerStatsTradeTradedObjectInfo tradedObjectInfo)
		{
			tradedObjectInfo.rank = -1;
			try
			{
				if (tradedObjectInfo.name == "plat")
				{
					tradedObjectInfo.name = "/AF_Special/Platinum";
					return true;
				}
				if (tradedObjectInfo.name.StartsWith("Imprint of"))
				{
					tradedObjectInfo.name = "/AF_Special/Imprint/" + tradedObjectInfo.name.Replace("Imprint of ", "");
					return true;
				}
				if (tradedObjectInfo.name.StartsWith("Legendary Core"))
				{
					tradedObjectInfo.name = "/AF_Special/Legendary Fusion Core";
					return true;
				}
				if (tradedObjectInfo.name.StartsWith("Ancient Core"))
				{
					tradedObjectInfo.name = "/AF_Special/Legendary Ancient Core";
					return true;
				}
				Dictionary<string, ItemComponent> dictionary = StaticData.dataHandler.relicDropsRealNames[Misc.GetWarframeLanguage(defaultToEnglish: false)];
				Dictionary<string, ItemComponent> dictionary2 = StaticData.dataHandler.relicDropsRealNames[Misc.WarframeLanguage.English];
				if (dictionary.ContainsKey(tradedObjectInfo.name.ToLower()))
				{
					tradedObjectInfo.name = dictionary[tradedObjectInfo.name.ToLower()].uniqueName;
					return true;
				}
				if (dictionary2.ContainsKey(tradedObjectInfo.name.ToLower()))
				{
					tradedObjectInfo.name = dictionary2[tradedObjectInfo.name.ToLower()].uniqueName;
					return true;
				}
				DataMisc dataMisc = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc p) => p.name == tradedObjectInfo.name);
				if (dataMisc != null)
				{
					tradedObjectInfo.name = dataMisc.uniqueName;
					return true;
				}
				if (tradedObjectInfo.name.Contains("(") && tradedObjectInfo.name.EndsWith(")"))
				{
					string text = tradedObjectInfo.name.Substring(tradedObjectInfo.name.LastIndexOf("("));
					string namePart = tradedObjectInfo.name.Substring(0, tradedObjectInfo.name.LastIndexOf("(") - 1);
					if (text.Length > 3)
					{
						string[] array = text.Replace("(", "").Replace(")", "").Split(' ');
						for (int num = 0; num < array.Length; num++)
						{
							if (int.TryParse(array[num], out var result))
							{
								tradedObjectInfo.rank = result;
								break;
							}
						}
						if (text.Contains("(RIVEN RANK "))
						{
							if (namePart.Contains(" Riven Mod"))
							{
								tradedObjectInfo.displayName = namePart + " (Veiled)";
								tradedObjectInfo.name = (from p in StaticData.dataHandler.mods?.Where((KeyValuePair<string, DataMod> p) => p.Value.name.Contains(namePart))
									orderby p.Key.Contains("/Raw") descending
									select p).FirstOrDefault().Key;
							}
							else
							{
								string text2 = namePart.Substring(0, namePart.LastIndexOf(" "));
								string text3 = namePart.Substring(namePart.LastIndexOf(" ") + 1);
								tradedObjectInfo.name = "/AF_Special/Riven/" + text2 + "/" + text3;
							}
							return true;
						}
						namePart = namePart.ToLower().Replace(" defiled", "");
						DataMod dataMod = StaticData.dataHandler.mods.Values.FirstOrDefault((DataMod p) => p.name.ToLower() == namePart);
						if (dataMod != null)
						{
							tradedObjectInfo.name = dataMod.uniqueName;
							tradedObjectInfo.displayName = dataMod.name;
							return true;
						}
					}
					else
					{
						DataFish dataFish = StaticData.dataHandler.fish.Values.FirstOrDefault((DataFish p) => p.name == namePart);
						string text4 = text.Replace("(", "").Replace(")", "");
						if (text4.Length == 1)
						{
							tradedObjectInfo.rank = text4.ToCharArray()[0];
						}
						if (dataFish != null)
						{
							tradedObjectInfo.name = dataFish.uniqueName;
							return true;
						}
					}
				}
				if (Encoding.UTF8.GetByteCount(tradedObjectInfo.name) != tradedObjectInfo.name.Length)
				{
					tradedObjectInfo.name.Substring(tradedObjectInfo.name.LastIndexOf(" ") + 1);
					string arcaneNamePart = tradedObjectInfo.name.Substring(0, tradedObjectInfo.name.LastIndexOf(" "));
					DataArcane dataArcane = StaticData.dataHandler.arcanes.Values.FirstOrDefault((DataArcane p) => p.name == arcaneNamePart);
					if (dataArcane != null)
					{
						tradedObjectInfo.name = dataArcane.uniqueName;
						tradedObjectInfo.displayName = dataArcane.name;
						return true;
					}
				}
				if (tradedObjectInfo.name == "Enter Nihil's Oubliette")
				{
					tradedObjectInfo.name = "/AF_Special/Other/Nihil's Oubliette (Key)";
					tradedObjectInfo.displayName = "Nihil's Oubliette (Key)";
					return true;
				}
				ItemComponent itemComponent = (from p in StaticData.dataHandler.warframeParts.Values
					where p.GetRealExternalName().Contains(tradedObjectInfo.name.Replace(" Blueprint", ""))
					orderby p.GetRealExternalName() == tradedObjectInfo.name descending
					select p).FirstOrDefault();
				if (itemComponent != null)
				{
					tradedObjectInfo.name = itemComponent.uniqueName;
					return true;
				}
				ItemComponent itemComponent2 = (from p in StaticData.dataHandler.weaponParts.Values
					where p.GetRealExternalName().Contains(tradedObjectInfo.name.Replace(" Blueprint", ""))
					orderby p.GetRealExternalName() == tradedObjectInfo.name descending
					select p).FirstOrDefault();
				if (itemComponent2 != null)
				{
					tradedObjectInfo.name = itemComponent2.uniqueName;
					return true;
				}
				DataMeleeWeapon dataMeleeWeapon = StaticData.dataHandler.meleeWeapons.Values.FirstOrDefault((DataMeleeWeapon p) => p.name == tradedObjectInfo.name);
				if (dataMeleeWeapon != null)
				{
					tradedObjectInfo.name = dataMeleeWeapon.uniqueName;
					return true;
				}
				DataPrimaryWeapon dataPrimaryWeapon = StaticData.dataHandler.primaryWeapons.Values.FirstOrDefault((DataPrimaryWeapon p) => p.name == tradedObjectInfo.name);
				if (dataPrimaryWeapon != null)
				{
					tradedObjectInfo.name = dataPrimaryWeapon.uniqueName;
					return true;
				}
				DataSecondaryWeapon dataSecondaryWeapon = StaticData.dataHandler.secondaryWeapons.Values.FirstOrDefault((DataSecondaryWeapon p) => p.name == tradedObjectInfo.name);
				if (dataSecondaryWeapon != null)
				{
					tradedObjectInfo.name = dataSecondaryWeapon.uniqueName;
					return true;
				}
				DataArchGun dataArchGun = StaticData.dataHandler.archGuns.Values.FirstOrDefault((DataArchGun p) => p.name == tradedObjectInfo.name);
				if (dataArchGun != null)
				{
					tradedObjectInfo.name = dataArchGun.uniqueName;
					return true;
				}
				DataArchwing dataArchwing = StaticData.dataHandler.archWings.Values.FirstOrDefault((DataArchwing p) => p.name == tradedObjectInfo.name);
				if (dataArchwing != null)
				{
					tradedObjectInfo.name = dataArchwing.uniqueName;
					return true;
				}
				DataArchMelee dataArchMelee = StaticData.dataHandler.archMelees.Values.FirstOrDefault((DataArchMelee p) => p.name == tradedObjectInfo.name);
				if (dataArchMelee != null)
				{
					tradedObjectInfo.name = dataArchMelee.uniqueName;
					return true;
				}
				if (tradedObjectInfo.name.Contains("Relic"))
				{
					string text5 = tradedObjectInfo.name.Replace(" Relic", "");
					if (text5.Split(' ').Length == 2)
					{
						text5 += " [INTACT]";
					}
					string compareName = text5.Replace("[", "").Replace("]", "").ToLower();
					DataRelic dataRelic = StaticData.dataHandler.relics.Values.FirstOrDefault((DataRelic p) => p.name.ToLower() == compareName);
					if (dataRelic != null)
					{
						tradedObjectInfo.name = dataRelic.uniqueName;
						return true;
					}
				}
				DataSkin dataSkin = StaticData.dataHandler.skins.Values.FirstOrDefault((DataSkin p) => p.name == tradedObjectInfo.name);
				if (dataSkin != null)
				{
					tradedObjectInfo.name = dataSkin.uniqueName;
					return true;
				}
				DataMisc misc2 = StaticData.dataHandler.misc.Values.Where((DataMisc p) => tradedObjectInfo.name.Contains(p.name)).FirstOrDefault((DataMisc p) => p.components?.Any((ItemComponent u) => p.name + " " + u.name == tradedObjectInfo.name.Replace(" Blueprint", "")) ?? false);
				if (misc2 != null)
				{
					ItemComponent itemComponent3 = misc2.components.FirstOrDefault((ItemComponent u) => misc2.name + " " + u.name == tradedObjectInfo.name.Replace(" Blueprint", ""));
					if (itemComponent3 != null)
					{
						tradedObjectInfo.name = itemComponent3.uniqueName;
						return true;
					}
				}
				DataMisc dataMisc2 = StaticData.dataHandler.misc.Values.FirstOrDefault((DataMisc p) => p.name == tradedObjectInfo.name.Replace(" Blueprint", ""));
				if (dataMisc2 != null)
				{
					tradedObjectInfo.name = dataMisc2.uniqueName;
					return true;
				}
				DataPet dataPet = StaticData.dataHandler.pets.Values.FirstOrDefault((DataPet p) => p.name == tradedObjectInfo.name.Replace(" Blueprint", ""));
				if (dataPet != null)
				{
					tradedObjectInfo.name = dataPet.uniqueName;
					return true;
				}
				DataResource dataResource = StaticData.dataHandler.resources.Values.FirstOrDefault((DataResource p) => p.name == tradedObjectInfo.name);
				if (dataResource != null)
				{
					tradedObjectInfo.name = dataResource.uniqueName;
					return true;
				}
				tradedObjectInfo.name = "/AF_Special/Other/" + tradedObjectInfo.name;
				return false;
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Error in GetTradedObjectInfo: " + ex.Message);
				tradedObjectInfo.name = "/AF_Special/Other/" + tradedObjectInfo.name;
				return false;
			}
		}

		public static void NewDataJustReceived_BG()
		{
			if (!StaticData.statsTabEnabled)
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "New stats data just received, but stats tab is disabled. Ignoring it...");
				return;
			}
			Task.Run(delegate
			{
				try
				{
					if (StaticData.dataHandler?.warframeRootObject != null)
					{
						FoundryPlayerStatsResponse playerStats = FoundryHelper.GetPlayerStats();
						PlayerStatsDataPoint value = new PlayerStatsDataPoint
						{
							plat = StaticData.dataHandler.warframeRootObject.PremiumCredits,
							credits = StaticData.dataHandler.warframeRootObject.RegularCredits,
							endo = StaticData.dataHandler.warframeRootObject.FusionPoints,
							ducats = (StaticData.dataHandler?.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == "/Lotus/Types/Items/MiscItems/PrimeBucks")?.ItemCount).GetValueOrDefault(),
							mr = StaticData.dataHandler.warframeRootObject.PlayerLevel,
							percentageCompletion = int.Parse(playerStats.unlockPercent.Replace("%", "")),
							relicOpened = relicOpenedDelta,
							trades = tradeDelta,
							ts = DateTime.UtcNow,
							aya = (StaticData.dataHandler?.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == "/Lotus/Types/Items/MiscItems/SchismKey")?.ItemCount).GetValueOrDefault()
						};
						WebClient webClient = new MyWebClient
						{
							Proxy = null
						};
						string playerUserHash = GetPlayerUserHash();
						if (!string.IsNullOrEmpty(playerUserHash))
						{
							webClient.Headers.Add("Content-Type", "application/json");
							webClient.UploadString(StaticData.StatsAPIHostname + "/api/stats/" + playerUserHash + "/addDataPoint?currentAppVersion=" + StaticData.overwolfWrappwer.currentVersion, JsonConvert.SerializeObject(value));
							relicOpenedDelta = 0;
							tradeDelta = 0;
							CreateAndSendRelicInventory();
							StaticData.overwolfWrappwer?.StatsUpdateNeededCaller();
						}
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to submit player stats: " + ex);
				}
			});
		}

		public static void CreateAndSendRelicInventory()
		{
			if (StaticData.dataHandler?.warframeRootObject?.MiscItemsLookup == null)
			{
				return;
			}
			List<byte> list = new List<byte>();
			var list2 = (from p in StaticData.dataHandler.relics.Values.ToList()
				select new
				{
					relic = p,
					amountOwned = StaticData.dataHandler?.warframeRootObject?.MiscItemsLookup[p.uniqueName].Sum((Miscitem miscitem) => miscitem.ItemCount)
				} into p
				where p.amountOwned > 0
				select p).ToList();
			list.AddRange(BitConverter.GetBytes(list2.Count));
			foreach (var item in list2)
			{
				string[] array = item.relic.name.Replace("Relic", "").Trim().Split(' ');
				if (array.Length != 3)
				{
					continue;
				}
				if (array[1].Length > 3)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Invalid relic name when saving them to the DB: " + item.relic.name);
					continue;
				}
				switch (array[0])
				{
				case "Lith":
					list.Add(0);
					break;
				case "Meso":
					list.Add(1);
					break;
				case "Neo":
					list.Add(2);
					break;
				case "Axi":
					list.Add(3);
					break;
				case "Requiem":
					list.Add(4);
					break;
				}
				switch (array[2])
				{
				case "Intact":
					list.Add(0);
					break;
				case "Exceptional":
					list.Add(1);
					break;
				case "Flawless":
					list.Add(2);
					break;
				case "Radiant":
					list.Add(3);
					break;
				}
				string text = array[1];
				while (text.Length < 3)
				{
					text = " " + text;
				}
				list.AddRange(Encoding.ASCII.GetBytes(text));
				list.AddRange(BitConverter.GetBytes((uint)item.amountOwned.Value));
			}
			WebClient webClient = new MyWebClient();
			webClient.Proxy = null;
			string playerUserHash = GetPlayerUserHash();
			if (!string.IsNullOrEmpty(playerUserHash))
			{
				webClient.Headers.Add("Content-Type", "application/octet-stream");
				webClient.UploadData(StaticData.StatsAPIHostname + "/api/stats/" + playerUserHash + "/addRelicInventory?currentAppVersion=" + StaticData.overwolfWrappwer.currentVersion + "&secretToken=" + GetTokenSecret(), list.ToArray());
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Relic inventory sent to the server");
			}
		}

		public static string GetPlayerUserHash()
		{
			string lastFetchedUsername = FoundryHelper.lastFetchedUsername;
			if (lastFetchedUsername == null || lastFetchedUsername == "AlecaFrame")
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to get player username!");
				return null;
			}
			long valueOrDefault = (StaticData.dataHandler?.warframeRootObject?.Created?.date?.numberLong.Ticks).GetValueOrDefault();
			if (valueOrDefault == 0L)
			{
				return null;
			}
			string s = $"hashStart-{lastFetchedUsername}-%_._%-{valueOrDefault}-hashEnd";
			using SHA256 sHA = SHA256.Create();
			byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(s));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static string GetTokenSecret()
		{
			string text = StaticData.dataHandler?.warframeRootObject?.DataKnives?.OrderByDescending((Dataknife1 p) => (p.ItemType == "/Lotus/Weapons/Tenno/HackingDevices/TnHackingDevice/TnHackingDeviceWeapon") ? 1 : 0)?.FirstOrDefault()?.ItemId?.oid;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string s = "hashStart2|-" + text + "-|hashEnd2";
			using SHA256 sHA = SHA256.Create();
			byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(s));
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = 0; num < array.Length; num++)
			{
				stringBuilder.Append(array[num].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static PlayerStatsData GetLastStatsData()
		{
			WebClient webClient = new MyWebClient();
			webClient.Proxy = null;
			string playerUserHash = GetPlayerUserHash();
			if (string.IsNullOrEmpty(playerUserHash))
			{
				return null;
			}
			webClient.Headers.Add("Content-Type", "application/json");
			return JsonConvert.DeserializeObject<PlayerStatsData>(webClient.DownloadString(StaticData.StatsAPIHostname + "/api/stats/" + playerUserHash));
		}

		public static void TradeAccepted()
		{
			Task.Run(delegate
			{
				if ((DateTime.UtcNow - tradeProcessedTimestamp).TotalMinutes > 15.0)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Not submitting trade because it took too long!");
				}
				else if (!StaticData.statsTabEnabled)
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Trade accepted, but stats tab is disabled. Ignoring it...");
				}
				else
				{
					if (currentProcessingTrade.type == TradeClassification.Sale)
					{
						try
						{
							WFMarketHelper.ItemsWereJustTraded(currentProcessingTrade.tx, currentProcessingTrade.user, selling: true, currentProcessingTrade.rx.FirstOrDefault((PlayerStatsTradeTradedObjectInfo p) => p.name == "/AF_Special/Platinum")?.cnt ?? 0);
						}
						catch (Exception ex)
						{
							StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to submit sold items to market helper: " + ex);
						}
					}
					else if (currentProcessingTrade.type == TradeClassification.Purchase)
					{
						try
						{
							WFMarketHelper.ItemsWereJustTraded(currentProcessingTrade.rx, currentProcessingTrade.user, selling: false, currentProcessingTrade.tx.FirstOrDefault((PlayerStatsTradeTradedObjectInfo p) => p.name == "/AF_Special/Platinum")?.cnt ?? 0);
						}
						catch (Exception ex2)
						{
							StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to submit purchased items to market helper: " + ex2);
						}
					}
					string text = "";
					try
					{
						foreach (PlayerStatsTradeTradedObjectInfo item in currentProcessingTrade?.tx)
						{
							item.displayName = null;
						}
						foreach (PlayerStatsTradeTradedObjectInfo item2 in currentProcessingTrade?.rx)
						{
							item2.displayName = null;
						}
						MyWebClient myWebClient = new MyWebClient
						{
							Proxy = null
						};
						string playerUserHash = GetPlayerUserHash();
						if (string.IsNullOrEmpty(playerUserHash))
						{
							return;
						}
						myWebClient.Headers.Add("Content-Type", "application/json");
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Submitting trade log for user " + playerUserHash + " with appVersion " + StaticData.overwolfWrappwer.currentVersion + "...");
						text = JsonConvert.SerializeObject(currentProcessingTrade);
						myWebClient.UploadString(StaticData.StatsAPIHostname + "/api/stats/" + playerUserHash + "/addTrade?currentAppVersion=" + StaticData.overwolfWrappwer.currentVersion, text);
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Trade log submitted");
						StaticData.overwolfWrappwer?.StatsUpdateNeededCaller();
						ProAnalytics.NewTradeMade();
					}
					catch (Exception ex3)
					{
						StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to submit trade: " + ex3?.ToString() + Environment.NewLine + text);
						if (ex3 is WebException)
						{
							try
							{
								WebException ex4 = ex3 as WebException;
								if (ex4.Response != null)
								{
									string text2 = new StreamReader(ex4.Response.GetResponseStream()).ReadToEnd();
									StaticData.Log(OverwolfWrapper.LogType.ERROR, "Response: " + text2);
								}
							}
							catch
							{
							}
						}
					}
					tradeDelta++;
					NewDataJustReceived_BG();
				}
			});
		}
	}
}
