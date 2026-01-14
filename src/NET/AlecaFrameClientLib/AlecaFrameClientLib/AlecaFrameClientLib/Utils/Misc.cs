using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFramePublicLib;
using AlecaFramePublicLib.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace AlecaFrameClientLib.Utils
{
    public static class Misc
    {
        public enum WarframeLanguage
        {
            English,
            French,
            UNKNOWN,
            Spanish,
            German,
            Russian
        }

        private static Regex inlineIconRegex = new Regex("<[A-Z]\\w+>", RegexOptions.IgnoreCase);

        public static void MinimizeLanguageFile(string pathInput, string pathOutput)
        {
            if (!File.Exists(pathInput))
            {
                Console.WriteLine("MinimizeLanguageFile error: File not found");
                return;
            }
            Dictionary<string, Dictionary<string, SingleLanguageData>> dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, SingleLanguageData>>>(File.ReadAllText(pathInput));
            foreach (string item in (IEnumerable<string>)dictionary.Keys.Where((string p) => p.Contains("/Lotus/Types/Game/Projections/T") || p.Contains("/StoreItems/AvatarImages/") || p.Contains("/Lotus/Upgrades/Skins/") || p.Contains("/Lotus/Types/Items/Emotes") || p.Contains("/Lotus/Types/Items/ShipDecos")).ToList())
            {
                dictionary.Remove(item);
            }
            foreach (ItemComponent value in StaticData.dataHandler.relicDropsRealNames[WarframeLanguage.English].Values)
            {
                if (!dictionary.ContainsKey(value.uniqueName))
                {
                    Console.WriteLine("Can not find translation for: " + value.uniqueName);
                }
            }
            File.WriteAllText(pathOutput, JsonConvert.SerializeObject(dictionary));
        }

        public static float ParseOrDefault(this string input, float defaultValueOnError = 0f)
        {
            if (float.TryParse(input, out var result))
            {
                return result;
            }
            return defaultValueOnError;
        }

        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_").Replace(":", "_");
        }

        public static string GetSIRepresentation(this long number, int decimalPlaces = 0)
        {
            return ((double)number).GetSIRepresentation(decimalPlaces);
        }

        public static string GetFullImagePath(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return string.Empty;
            }
            if (imageName.StartsWith("afRemoteImg://"))
            {
                return imageName.Replace("afRemoteImg://", "https://cdn.alecaframe.com/warframeData/custom/img/");
            }
            if (imageName.StartsWith("https://") || imageName.StartsWith("http://"))
            {
                if (imageName.StartsWith("https://static.wikia."))
                {
                    return imageName.Replace("static.wikia.nocookie.net", "modimg.alecaframe.com");
                }
                return imageName;
            }
            if (imageName == "stock.png")
            {
                return "https://cdn.alecaframe.com/warframeData/custom/imgRemote/Stock.png";
            }
            return StaticData.imageURLPrefix + imageName;
        }

        public static string GetSIRepresentation(this int number, int decimalPlaces = 0)
        {
            return ((double)number).GetSIRepresentation(decimalPlaces);
        }

        public static string GetSIRepresentation(this float number, int decimalPlaces = 2)
        {
            return ((double)number).GetSIRepresentation(decimalPlaces);
        }

        public static string ToTimeString(this TimeSpan timeSpan)
        {
            return ((timeSpan.Days > 0) ? (timeSpan.Days + "d ") : "") + ((timeSpan.Hours > 0) ? (timeSpan.Hours + "h ") : "") + ((timeSpan.Minutes > 0) ? (timeSpan.Minutes + "m ") : "") + ((timeSpan.Seconds > 0) ? (timeSpan.Seconds + "s") : "");
        }

        public static string CreateMD5(string input)
        {
            using MD5 mD = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            byte[] array = mD.ComputeHash(bytes);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        public static string GetSIRepresentation(this double number, int decimalPlaces = 2)
        {
            string text = "0";
            if (number > 1000000.0)
            {
                return Math.Round(number / 1000000.0, decimalPlaces) + "M";
            }
            if (number > 1000.0)
            {
                return Math.Round(number / 1000.0, decimalPlaces) + "K";
            }
            return Math.Round(number, decimalPlaces).ToString();
        }

        public static string GetWarframeMarketURLName(string humanName)
        {
            if (humanName == null)
            {
                return "";
            }
            humanName = humanName.ToLower();
            if (humanName == null)
            {
                humanName = "";
            }
            if (humanName.Contains("radiant") || humanName.Contains("intact") || humanName.Contains("exceptional") || humanName.Contains("flawless"))
            {
                humanName = humanName.Replace(" radiant", "").Replace(" intact", "").Replace(" exceptional", "")
                    .Replace(" flawless", "");
                humanName += " relic";
            }
            humanName = humanName.Trim().Replace("&", "and").Replace("'", "")
                .Replace("-", "_")
                .Replace(" ", "_");
            humanName = humanName.Replace("prisma_dual_decurions", "prisma_dual_decurion");
            if (humanName == "dark_split_sword")
            {
                humanName = "dark_split_sword_(dual_swords)";
            }
            return humanName;
        }

        public static bool UploadRelicFile(string username, int lastRelicLogWorstDelta, string path)
        {
            try
            {
                MyWebClient myWebClient = new MyWebClient();
                myWebClient.Proxy = null;
                myWebClient.UploadFile("https://" + StaticData.LogAPIHostname + "/logs/relic?username=" + username + "&worstDelta=" + lastRelicLogWorstDelta, path);
                return true;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to upload relic log data! " + ex.Message);
                return false;
            }
        }

        public static WarframeLanguage GetWarframeLanguage(bool defaultToEnglish)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warframe\\Launcher.log";
                if (!File.Exists(path))
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check WF language (GetWarframeLanguage) File not found!");
                    return GetWarframeLanguageSecondMethod(defaultToEnglish);
                }
                using StreamReader streamReader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                string text = streamReader.ReadToEnd();
                int num = text.LastIndexOf("-language:");
                if (num == -1)
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find language string in log file. Defaulting to english...");
                    return GetWarframeLanguageSecondMethod(defaultToEnglish);
                }
                string text2 = text.Substring(num + 10, 2);
                WarframeLanguage languageFromLanguageCode = TranslationHelper.GetLanguageFromLanguageCode(text2);
                File.WriteAllText(StaticData.saveFolder + "lastLang.tmp", text2);
                return languageFromLanguageCode;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check WF language (GetWarframeLanguage): " + ex);
                return GetWarframeLanguageSecondMethod(defaultToEnglish);
            }
        }

        public static WarframeLanguage GetWarframeLanguageSecondMethod(bool defaultToEnglish)
        {
            StaticData.Log(OverwolfWrapper.LogType.INFO, "Using second method to get WF language...");
            try
            {
                return TranslationHelper.GetLanguageFromLanguageCode(File.ReadAllText(StaticData.saveFolder + "lastLang.tmp"));
            }
            catch (Exception)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to find language (second method). Defaulting to english...");
                return (!defaultToEnglish) ? WarframeLanguage.UNKNOWN : WarframeLanguage.English;
            }
        }

        public static string FirstCharToUpper(this string input)
        {
            if (input != null)
            {
                if (input == "")
                {
                    throw new ArgumentException("input cannot be empty", "input");
                }
                return input[0].ToString().ToUpper() + input.Substring(1);
            }
            throw new ArgumentNullException("input");
        }

        public static string ReplaceStringWithIcons(string baseString)
        {
            if (baseString == null)
            {
                return string.Empty;
            }
            string text = baseString;
            foreach (Match item in inlineIconRegex.Matches(baseString))
            {
                string value = item.Value;
                text = text.Replace(value, "<img class=\"iicon\" src=\"assets/img/inlineIcons/" + value.Substring(1, item.Length - 2).ToLower() + ".webp\" />");
            }
            return text;
        }

        public static string ReplaceStringWithNothing(string baseString)
        {
            if (baseString == null)
            {
                return string.Empty;
            }
            string text = baseString;
            foreach (Match item in inlineIconRegex.Matches(baseString))
            {
                string value = item.Value;
                text = text.Replace(value, "");
            }
            return text;
        }

        public static string RemoveDiacritics(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }
            string text2 = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(text2.Length);
            foreach (char c in text2)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static bool TryGetScalingModeFromGameSettings(ref ScalingMode scalingMode, ref float scalePerOne)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warframe\\EE.cfg";
                if (!File.Exists(path))
                {
                    return false;
                }
                using StreamReader streamReader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                string text = streamReader.ReadToEnd();
                if (text.Contains("Flash.FlashDrawScaleMode=MSM_CUSTOM"))
                {
                    scalingMode = ScalingMode.Custom;
                    if (text.Contains("Flash.FlashDrawScale="))
                    {
                        string[] array = text.Replace("\r", "").Split('\n');
                        foreach (string text2 in array)
                        {
                            if (text2.Contains("Flash.FlashDrawScale="))
                            {
                                string[] array2 = text2.Split('=');
                                if (array2.Length == 2)
                                {
                                    scalePerOne = float.Parse(array2[1], CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                    return true;
                }
                if (text.Contains("Flash.FlashDrawScaleMode=MSM_MATCH_SCREEN"))
                {
                    scalingMode = ScalingMode.Legacy;
                    return true;
                }
                scalingMode = ScalingMode.Full;
                return true;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check WF DOF: " + ex);
                return true;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static bool IsWarframeInTheForeground(bool inTheForegoundDefault = true)
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    return inTheForegoundDefault;
                }
                Process[] processesByName = Process.GetProcessesByName("Warframe.x64");
                if (!processesByName.Any())
                {
                    return inTheForegoundDefault;
                }
                int id = processesByName.First().Id;
                GetWindowThreadProcessId(foregroundWindow, out var processId);
                return processId == id;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to test if WF is in the foreground!: " + ex);
                return inTheForegoundDefault;
            }
        }

        public static bool IsWarframeRunning()
        {
            return Process.GetProcessesByName("Warframe.x64").Any();
        }

        public static bool IsWarframeUIScaleLegacy()
        {
            return StaticData.WarframeScalingMode == ScalingMode.Legacy;
        }

        public static bool IsAccountLogedIn()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warframe\\EE.log";
                if (!File.Exists(path))
                {
                    return true;
                }
                using StreamReader streamReader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                return streamReader.ReadToEnd().Contains("Logging in as ");
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check if WF is logged in: " + ex);
                return false;
            }
        }

        public static bool IsProcessRunAsAdmin(Process process)
        {
            try
            {
                return new WindowsPrincipal(global::UACHelper.UACHelper.GetProcessOwner(process)).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check if process " + process.ProcessName + " is admin: " + ex.Message);
                return true;
            }
        }

        public static string ReadLastDataFile()
        {
            return ReadAllTextEncrypted(StaticData.saveFolder + "\\lastData.dat");
        }

        public static string ReadAllTextEncrypted(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            string text = File.ReadAllText(path);
            if (text.First() == '{' && text.Last() == '}')
            {
                return text;
            }
            using AesManaged aesManaged = new AesManaged();
            ICryptoTransform transform = aesManaged.CreateDecryptor(StaticData.lastDataKey, StaticData.lastDataIV);
            using MemoryStream stream = new MemoryStream(File.ReadAllBytes(path));
            using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(stream2);
            return streamReader.ReadToEnd();
        }

        public static void WriteLastDataFile(string data)
        {
            WriteAllTextEncrypted(StaticData.saveFolder + "\\lastData.dat", data);
        }

        public static void WriteAllTextEncrypted(string path, string data)
        {
            using AesManaged aesManaged = new AesManaged();
            using ICryptoTransform transform = aesManaged.CreateEncryptor(StaticData.lastDataKey, StaticData.lastDataIV);
            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                streamWriter.Write(data);
            }
            File.WriteAllBytes(path, memoryStream.ToArray());
        }

        public static List<ThemePreset> GetThemeDataFromFiles(List<string> fileList)
        {
            List<ThemePreset> list = new List<ThemePreset>();
            foreach (string file in fileList)
            {
                ThemePreset themePreset = JsonConvert.DeserializeObject<ThemePreset>(File.ReadAllText(file));
                if (!themePreset.colorData.ToLower().Contains("script") && !themePreset.iconBase64.ToLower().Contains("script"))
                {
                    list.Add(themePreset);
                }
            }
            return list;
        }

        public static string GetMasteryLevelIcon(int playerLevel)
        {
            return "https://cdn.alecaframe.com/warframeData/custom/imgRemote/levelIcons/" + playerLevel + ".webp";
        }

        public static int GetMasteryLevelFromXP(double XP, bool isWarframeOrSentinel, double XPCap)
        {
            if (XP < 0.0)
            {
                return 0;
            }
            if (XP > XPCap)
            {
                XP = XPCap;
            }
            XP = ((!isWarframeOrSentinel) ? (XP / 500.0) : (XP / 1000.0));
            return (int)Math.Floor(Math.Sqrt(XP));
        }

        public static int GetMasteryXPFromItemLevel(int level, bool isWarframeOrSentinel)
        {
            return (int)Math.Floor((double)(isWarframeOrSentinel ? 1000 : 500) * Math.Pow(level, 2.0));
        }

        public static int GetMasteryLevelTotalXPRequired(int masteryLevel)
        {
            if (masteryLevel <= 0)
            {
                return 0;
            }
            if (masteryLevel <= 30)
            {
                return 2500 * masteryLevel * masteryLevel;
            }
            return 2250000 + 147500 * (masteryLevel - 30);
        }

        public static int Clamp(int percentLeftNextLevel, int min, int max)
        {
            if (percentLeftNextLevel < min)
            {
                return min;
            }
            if (percentLeftNextLevel > max)
            {
                return max;
            }
            return percentLeftNextLevel;
        }

        public static bool IsWeaponOwned(string uniqueID)
        {
            DataPrimaryWeapon orDefault = StaticData.dataHandler.primaryWeapons.GetOrDefault(uniqueID);
            if (orDefault != null && orDefault.IsOwned())
            {
                return true;
            }
            DataSecondaryWeapon orDefault2 = StaticData.dataHandler.secondaryWeapons.GetOrDefault(uniqueID);
            if (orDefault2 != null && orDefault2.IsOwned())
            {
                return true;
            }
            DataMeleeWeapon orDefault3 = StaticData.dataHandler.meleeWeapons.GetOrDefault(uniqueID);
            if (orDefault3 != null && orDefault3.IsOwned())
            {
                return true;
            }
            DataArchGun orDefault4 = StaticData.dataHandler.archGuns.GetOrDefault(uniqueID);
            if (orDefault4 != null && orDefault4.IsOwned())
            {
                return true;
            }
            DataArchMelee orDefault5 = StaticData.dataHandler.archMelees.GetOrDefault(uniqueID);
            if (orDefault5 != null && orDefault5.IsOwned())
            {
                return true;
            }
            DataSentinelWeapons orDefault6 = StaticData.dataHandler.sentinelWeapons.GetOrDefault(uniqueID);
            if (orDefault6 != null && orDefault6.IsOwned())
            {
                return true;
            }
            DataMisc orDefault7 = StaticData.dataHandler.misc.GetOrDefault(uniqueID);
            if (orDefault7 != null && orDefault7.IsOwned())
            {
                return true;
            }
            return false;
        }

        public static object RelicOrderIndex(string relicType)
        {
            return relicType.ToLower() switch
            {
                "lith" => 0,
                "meso" => 1,
                "neo" => 2,
                "axi" => 3,
                "requiem" => 4,
                _ => 99,
            };
        }

        public static BigItem GetBigItemRefernceOrNull(string uniqueID, bool onlyFoundryItems = false)
        {
            if (uniqueID == null)
            {
                return null;
            }
            BigItem bigItem = (BigItem)(StaticData.dataHandler.primaryWeapons.GetOrDefault(uniqueID) ?? StaticData.dataHandler.secondaryWeapons.GetOrDefault(uniqueID) ?? StaticData.dataHandler.meleeWeapons.GetOrDefault(uniqueID) ?? StaticData.dataHandler.archGuns.GetOrDefault(uniqueID) ?? StaticData.dataHandler.archMelees.GetOrDefault(uniqueID) ?? StaticData.dataHandler.sentinelWeapons.GetOrDefault(uniqueID) ?? StaticData.dataHandler.pets.GetOrDefault(uniqueID) ?? StaticData.dataHandler.warframes.GetOrDefault(uniqueID) ?? StaticData.dataHandler.sentinels.GetOrDefault(uniqueID) ?? StaticData.dataHandler.kdrives.GetOrDefault(uniqueID) ?? ((object)StaticData.dataHandler.amps.GetOrDefault(uniqueID)) ?? ((object)StaticData.dataHandler.archWings.GetOrDefault(uniqueID)));
            if (bigItem == null && StaticData.dataHandler.misc.TryGetValue(uniqueID, out var value) && value.IsModular())
            {
                bigItem = value;
            }
            if (!onlyFoundryItems && bigItem == null)
            {
                bigItem = (BigItem)(StaticData.dataHandler.misc.GetOrDefault(uniqueID) ?? StaticData.dataHandler.resources.GetOrDefault(uniqueID) ?? StaticData.dataHandler.skins.GetOrDefault(uniqueID) ?? StaticData.dataHandler.fish.GetOrDefault(uniqueID) ?? StaticData.dataHandler.mods.GetOrDefault(uniqueID) ?? StaticData.dataHandler.relics.GetOrDefault(uniqueID) ?? ((object)StaticData.dataHandler.arcanes.GetOrDefault(uniqueID)) ?? ((object)StaticData.dataHandler.quests.GetOrDefault(uniqueID)));
            }
            return bigItem;
        }

        public static string GetWikiaURL(string urlPart)
        {
            if (urlPart == null)
            {
                return null;
            }
            return "https://wiki.warframe.com/w/" + urlPart;
        }

        public static string GetReducedRivenWeaponUID(string weaponUID)
        {
            Dictionary<string, RivenWeaponData> weaponStats = StaticData.dataHandler.rivenData.weaponStats;
            string weaponName = weaponStats.GetOrDefault(weaponUID)?.name;
            if (weaponName == null)
            {
                throw new Exception("Can not find weapon name for UID: " + weaponUID);
            }
            return weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> p) => p.Value.name == weaponName.Replace("Prime", "").Trim() && p.Value.name != weaponName).Key ?? weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> p) => p.Value.name == weaponName.Replace("Tenet", "").Trim() && p.Value.name != weaponName).Key ?? weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> p) => p.Value.name == weaponName.Replace("Vandal", "").Trim() && p.Value.name != weaponName).Key ?? weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> p) => p.Value.name == weaponName.Replace("Kuva", "").Trim() && p.Value.name != weaponName).Key ?? weaponUID;
        }

        internal static BigItem GetBigItemRefernceOrNull(object realWeaponUID)
        {
            throw new NotImplementedException();
        }

        public static PossibleOverlaysActive GetPossibleOverlayIssuesCached()
        {
            try
            {
                PossibleOverlaysActive possibleOverlaysActive = DetectOverlayIssues();
                if (possibleOverlaysActive != PossibleOverlaysActive.Other)
                {
                    string path = StaticData.saveFolder + "overlayIssues.cache";
                    int num = (int)possibleOverlaysActive;
                    File.WriteAllText(path, num.ToString());
                    return possibleOverlaysActive;
                }
                return File.Exists(StaticData.saveFolder + "overlayIssues.cache") ? ((PossibleOverlaysActive)int.Parse(File.ReadAllText(StaticData.saveFolder + "overlayIssues.cache"))) : PossibleOverlaysActive.Other;
            }
            catch
            {
                return PossibleOverlaysActive.Other;
            }
        }

        public static PossibleOverlaysActive DetectOverlayIssues()
        {
            try
            {
                Process process = Process.GetProcessesByName("Warframe.x64").FirstOrDefault();
                if (process == null)
                {
                    return PossibleOverlaysActive.Other;
                }
                foreach (ProcessModule module in process.Modules)
                {
                    if (module.FileName.Contains("steamclient64.dll"))
                    {
                        return PossibleOverlaysActive.Steam;
                    }
                    if (module.FileName.Contains("discord_overlay.dll"))
                    {
                        return PossibleOverlaysActive.Discord;
                    }
                }
                return PossibleOverlaysActive.Other;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to check WF launch source: " + ex);
                return PossibleOverlaysActive.Other;
            }
        }

        public static List<RivenWeaponData> GetCompatibleWeapons(RivenWeaponData myWeaponData)
        {
            List<RivenWeaponData> list = new List<RivenWeaponData>();
            if (myWeaponData == null)
            {
                return list;
            }
            list.Add(myWeaponData);
            list.AddRange(StaticData.dataHandler.rivenData.weaponStats.Values.Where((RivenWeaponData p) => (WordSplitCombinationContains(p.name, myWeaponData.name) || p.name == "MK1-" + myWeaponData.name) && !p.name.Contains("Garuda") && !p.name.Contains("Valkyr") && p.name != myWeaponData.name));
            if (list.Count == 1 && myWeaponData?.name == "Pangolin Sword")
            {
                list.Add(StaticData.dataHandler.rivenData.weaponStats["/Lotus/Weapons/Tenno/Melee/Swords/PrimePangolinSword/PrimePangolinSword"]);
            }
            return list;
        }

        public static List<string> GetCompatibleWeaponsUIDs(string myWeaponUID)
        {
            List<string> list = new List<string>();
            if (myWeaponUID == null)
            {
                return list;
            }
            RivenWeaponData myWeaponData = StaticData.dataHandler.rivenData.weaponStats.GetOrDefault(myWeaponUID);
            if (myWeaponData == null)
            {
                return list;
            }
            list.Add(myWeaponUID);
            list.AddRange(from p in StaticData.dataHandler.rivenData.weaponStats
                          where (WordSplitCombinationContains(p.Value.name, myWeaponData.name) || p.Value.name == "MK1-" + myWeaponData.name) && !p.Value.name.Contains("Garuda") && !p.Value.name.Contains("Valkyr") && p.Value.name != myWeaponData.name
                          select p.Key);
            return list;
        }

        public static bool WordSplitCombinationContains(string possibleWeapon, string baseWeapon)
        {
            List<string> list = possibleWeapon.Split(' ').ToList();
            if (list.Contains(baseWeapon))
            {
                return true;
            }
            if (list.Count >= 3)
            {
                if (string.Join(" ", list.Skip(1)).Contains(baseWeapon))
                {
                    return true;
                }
                if (string.Join(" ", list.Take(list.Count - 1)).Contains(baseWeapon))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetSetName(BigItem isPartOf)
        {
            try
            {
                if (StaticData.LazyWfmItemData.Value.data.FirstOrDefault((WFMItemListItem p) => p.item_name == isPartOf.name + " Set").slug.EndsWith("_set"))
                {
                    return isPartOf.name + " Set";
                }
                return isPartOf.name;
            }
            catch (Exception)
            {
                if (isPartOf == null)
                {
                    return "Unknown";
                }
                if (!string.IsNullOrWhiteSpace(isPartOf.releaseDate) && DateTime.ParseExact(isPartOf.releaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) > new DateTime(2024, 8, 15))
                {
                    return isPartOf.name;
                }
                return isPartOf.name + " Set";
            }
        }
    }
}
