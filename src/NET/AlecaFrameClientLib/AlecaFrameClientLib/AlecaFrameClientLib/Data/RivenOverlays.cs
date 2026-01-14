using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AlecaFrameClientLib.Data
{
    public static class RivenOverlays
    {
        private delegate void RivenOverlayOCRStartMethod(Bitmap pictureAlreadyTaken = null);

        private enum RivenOCRDetectionType
        {
            ChatRiven,
            RivenReroll,
            PreCut
        }

        private class ParsedOCRRiven
        {
            public string weaponName;

            public string rivenName;

            public List<(float value, string text, int order)> attributes = new List<(float, string, int)>();

            public override string ToString()
            {
                return "Weapon: " + weaponName + Environment.NewLine + "Riven: " + rivenName + Environment.NewLine + string.Join(Environment.NewLine, attributes.Select(((float value, string text, int order) p) => "\t-" + p.text + " | " + p.value));
            }
        }

        public class RivenOverlayWorkingData
        {
            public class RivenOverlayRiven
            {
                public RivenSummaryData data;

                public bool show;

                public bool loading;

                public bool errorHappened;

                public string errorMessage = "";
            }

            public bool enabled;

            public RivenOverlayRiven rivenLeft = new RivenOverlayRiven();

            public RivenOverlayRiven rivenRight = new RivenOverlayRiven();
        }

        private static object lockObject = new object();

        private static DateTime lastChatRivenTime = DateTime.MinValue;

        public static RivenOverlayWorkingData workingData = new RivenOverlayWorkingData();

        public static bool isRerollUIOpen = false;

        private static RivenOverlayOCRStartMethod lastOCRMethodRun = null;

        private static HashSet<char> riven_OCR_allowedCharts = new HashSet<char>("\n 1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,()+-%");

        public static void RivenSelectedForRerollDetected(Bitmap pictureIfAlreadyTaken = null)
        {
            if (!StaticData.enableRivenOverlay)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "RivenSelectedForRerollDetected cancelled because the riven overlay is disabled.");
                return;
            }
            if (Misc.GetWarframeLanguage(defaultToEnglish: true) != Misc.WarframeLanguage.English)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "RivenSelectedForRerollDetected cancelled because the game is not in English.");
                return;
            }
            bool num = isRerollUIOpen;
            isRerollUIOpen = true;
            workingData.enabled = true;
            workingData.rivenLeft.loading = true;
            workingData.rivenLeft.show = true;
            if (!num)
            {
                workingData.rivenLeft.data = null;
            }
            workingData.rivenLeft.errorMessage = "";
            workingData.rivenLeft.errorHappened = false;
            workingData.rivenRight.data = null;
            workingData.rivenRight.show = false;
            workingData.rivenRight.loading = false;
            workingData.rivenRight.errorMessage = "";
            workingData.rivenRight.errorHappened = false;
            StaticData.overwolfWrappwer.onRivenOverlayOpenCaller();
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
            StaticData.Log(OverwolfWrapper.LogType.INFO, "Riven to reroll detected");
            Thread.Sleep(200);
            try
            {
                lastOCRMethodRun = RivenSelectedForRerollDetected;
                workingData.rivenLeft.data = DetectAndProcessRiven(RivenOCRDetectionType.RivenReroll, pictureIfAlreadyTaken);
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.ERROR, "Error while detecting riven: " + ex);
                workingData.rivenLeft.errorMessage = "Error while detecting reroll riven: " + ex.Message;
                workingData.rivenLeft.errorHappened = true;
            }
            workingData.rivenLeft.loading = false;
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
            AnalyticsHandler.AddMetric("rivenRerollStart", workingData.rivenLeft.errorHappened ? "error" : "success");
        }

        public static void RivenRerollDetected()
        {
            RivenRerollDetected(null);
        }

        public static void RivenRerollDetected(Bitmap pictureIfAlreadyTaken = null)
        {
            if (!StaticData.enableRivenOverlay)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "RivenRerollDetected cancelled because the riven overlay is disabled.");
            }
            else if (Misc.GetWarframeLanguage(defaultToEnglish: true) == Misc.WarframeLanguage.English && isRerollUIOpen)
            {
                workingData.rivenRight.data = null;
                workingData.rivenRight.loading = true;
                workingData.rivenRight.show = true;
                StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
                StaticData.Log(OverwolfWrapper.LogType.INFO, "Riven rerolled detected");
                Thread.Sleep(2750);
                try
                {
                    lastOCRMethodRun = RivenRerollDetected;
                    workingData.rivenRight.data = DetectAndProcessRiven(RivenOCRDetectionType.RivenReroll, pictureIfAlreadyTaken);
                }
                catch (Exception ex)
                {
                    StaticData.Log(OverwolfWrapper.LogType.ERROR, "Error while detecting reroll riven: " + ex);
                    workingData.rivenRight.errorMessage = "Error while detecting reroll riven: " + ex.Message;
                    workingData.rivenRight.errorHappened = true;
                }
                workingData.rivenRight.loading = false;
                StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
                AnalyticsHandler.AddMetric("rivenRerollEnd", workingData.rivenRight.errorHappened ? "error" : "success");
            }
        }

        public static void RivenRerollCycleComplete()
        {
            if (Misc.GetWarframeLanguage(defaultToEnglish: true) == Misc.WarframeLanguage.English)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "Riven reroll cycle complete detected. Starting cycle again...");
                Thread.Sleep(1000);
                if (isRerollUIOpen)
                {
                    RivenSelectedForRerollDetected();
                }
            }
        }

        public static void ChatRivenOpenedDetected(Bitmap pictureIfAlreadyTaken = null)
        {
            if (!StaticData.enableRivenOverlay)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "ChatRivenOpenedDetected cancelled because the riven overlay is disabled.");
                return;
            }
            if (Misc.GetWarframeLanguage(defaultToEnglish: true) != Misc.WarframeLanguage.English)
            {
                StaticData.Log(OverwolfWrapper.LogType.INFO, "ChatRivenOpenedDetected cancelled because the game is not in English.");
                return;
            }
            lock (lockObject)
            {
                if (DateTime.UtcNow - lastChatRivenTime < TimeSpan.FromSeconds(0.4))
                {
                    StaticData.Log(OverwolfWrapper.LogType.INFO, "Chat riven opened detected, but ignored because another one had happened recently");
                    return;
                }
                lastChatRivenTime = DateTime.UtcNow;
            }
            workingData.enabled = true;
            workingData.rivenLeft.loading = true;
            workingData.rivenLeft.show = true;
            workingData.rivenLeft.data = null;
            workingData.rivenLeft.errorMessage = "";
            workingData.rivenLeft.errorHappened = false;
            workingData.rivenRight.data = null;
            workingData.rivenRight.show = false;
            workingData.rivenRight.loading = false;
            workingData.rivenRight.errorMessage = "";
            workingData.rivenRight.errorHappened = false;
            StaticData.overwolfWrappwer.onRivenOverlayOpenCaller();
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
            StaticData.Log(OverwolfWrapper.LogType.INFO, "Chat riven opened detected");
            Thread.Sleep(200);
            try
            {
                lastOCRMethodRun = ChatRivenOpenedDetected;
                workingData.rivenLeft.data = DetectAndProcessRiven(RivenOCRDetectionType.ChatRiven, pictureIfAlreadyTaken);
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.ERROR, "Error while detecting chat riven: " + ex);
                workingData.rivenLeft.errorMessage = "Error while detecting chat riven: " + ex.Message;
                workingData.rivenLeft.errorHappened = true;
            }
            workingData.rivenLeft.loading = false;
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
            AnalyticsHandler.AddMetric("rivenChat", workingData.rivenLeft.errorHappened ? "error" : "success");
        }

        private static RivenSummaryData DetectAndProcessRiven(RivenOCRDetectionType OCRtype, Bitmap pictureIfAlreadyTaken = null)
        {
            using AlecaLogDataLogger alecaLogDataLogger = new AlecaLogDataLogger(AlecaLogDataLogger.AlecaLogType.riven, StaticData.relicLogFolder, delegate
            {
            }, _doNotLog: false);
            int num = 3;
            while (true)
            {
                int num2 = 3;
                while (true)
                {
                    try
                    {
                        if (pictureIfAlreadyTaken == null && !OCRHelper.TakeScreenshotOfWarframeInCSharp(alecaLogDataLogger))
                        {
                            alecaLogDataLogger.AddString("Error while taking screenshot");
                            alecaLogDataLogger.FlagError();
                            return null;
                        }
                        alecaLogDataLogger.AddBitmap(AlecaLogDataLogger.relicLogImageType.original, pictureIfAlreadyTaken ?? StaticData.overwolfWrappwer.lastWarframeScreenshot);
                        string text = null;
                        using (Bitmap bitmap = CutBitmapToRoughSize(OCRtype, pictureIfAlreadyTaken ?? StaticData.overwolfWrappwer.lastWarframeScreenshot, StaticData.WarframeScalingMode, StaticData.customScale))
                        {
                            alecaLogDataLogger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap, "roughtCut");
                            alecaLogDataLogger.AddString("Performing edge detection...");
                            using Bitmap bitmap2 = bitmap.Laplacian3x3Filter();
                            Bitmap bitmap3;
                            if (num <= 1)
                            {
                                alecaLogDataLogger.AddString("Last attempt. Using rough cut");
                                bitmap3 = (Bitmap)bitmap.Clone();
                            }
                            else
                            {
                                alecaLogDataLogger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap2, "roughtCutEdges");
                                alecaLogDataLogger.AddString("Fine cropping riven...");
                                try
                                {
                                    bitmap3 = DetailedRivenCrop(bitmap, bitmap2, alecaLogDataLogger);
                                }
                                catch
                                {
                                    num2--;
                                    if (num2 > 0)
                                    {
                                        alecaLogDataLogger.AddString("Failed to detect edges. Trying again in a few ms...");
                                        Thread.Sleep(200);
                                        continue;
                                    }
                                    alecaLogDataLogger.AddString("Failed to finecrop the riven. Using rough cut");
                                    bitmap3 = (Bitmap)bitmap.Clone();
                                }
                            }
                            using (bitmap3)
                            {
                                alecaLogDataLogger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap3, "fineCropped");
                                alecaLogDataLogger.AddString("Submitting image for OCR");
                                text = OCRHelper.SendRAWRivenOCR(bitmap3);
                                alecaLogDataLogger.AddString("OCR result: " + text);
                            }
                            goto IL_019a;
                        }
                    IL_019a:
                        ParsedOCRRiven parsedOCRRiven = ParseRivenOCR(text, alecaLogDataLogger);
                        alecaLogDataLogger.AddString("Parsed OCR result: " + Environment.NewLine + parsedOCRRiven.ToString());
                        return RivenSummaryDataFromOCRResult(parsedOCRRiven, alecaLogDataLogger);
                    }
                    catch (Exception ex)
                    {
                        alecaLogDataLogger.AddString("Error while taking screenshot: " + ex);
                        num--;
                        if (num > 0)
                        {
                            alecaLogDataLogger.AddString("Trying again...");
                            Thread.Sleep(500);
                            break;
                        }
                        alecaLogDataLogger.FlagError();
                        throw;
                    }
                    finally
                    {
                        StaticData.overwolfWrappwer.lastWarframeScreenshot?.Dispose();
                        StaticData.overwolfWrappwer.lastWarframeScreenshot = null;
                    }
                }
            }
        }

        private static ParsedOCRRiven ParseRivenOCR(string rawOCR, AlecaLogDataLogger logger)
        {
            ParsedOCRRiven parsedOCRRiven = new ParsedOCRRiven();
            string text = string.Concat(from c in rawOCR.Replace("~", "-").Replace("_", "-")
                                        where riven_OCR_allowedCharts.Contains(c)
                                        select c);
            for (int num = 1; num < text.Length; num++)
            {
                if (char.IsUpper(text[num]) && !char.IsUpper(text[num - 1]) && text[num - 1] != ' ')
                {
                    text = text.Insert(num, " ");
                }
            }
            logger.AddString("Parsing OCR result");
            List<string> list = (from p in text.Replace("\r", "").Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 select p.Trim()).ToList();
            bool flag = false;
            while (list.Count > 0)
            {
                string text2 = list[0];
                if (text2.Length < 6 || !text2.Contains(" "))
                {
                    list.RemoveAt(0);
                    continue;
                }
                list.RemoveAt(0);
                if (list.Count > 0 && (text2.EndsWith("-") || (!list[0].Any((char p) => char.IsDigit(p)) && list[0].Length > 3 && !list[0].Contains("%") && !list[0].StartsWith("x"))))
                {
                    if (!text2.EndsWith("-") && text2.Split(' ').Last().Length <= 5)
                    {
                        text2 += "-";
                    }
                    else if (!text2.EndsWith("-"))
                    {
                        text2 += " ";
                    }
                    text2 += list[0];
                    list.RemoveAt(0);
                }
                parsedOCRRiven.weaponName = text2.Substring(0, text2.LastIndexOf(" ")).Trim();
                parsedOCRRiven.rivenName = text2.Substring(text2.LastIndexOf(" ")).Trim();
                flag = true;
                break;
            }
            if (!flag)
            {
                throw new Exception("Failed to find weapon and riven name");
            }
            while (list.Count > 0)
            {
                string text3 = list[0];
                if (text3.Length < 6)
                {
                    list.RemoveAt(0);
                    continue;
                }
                if (!IsNewAttributeLine(text3) || text3.ToLower().Contains("mr") || char.IsDigit(text3.Last()))
                {
                    list.RemoveAt(0);
                    continue;
                }
                list.RemoveAt(0);
                if (list.Count > 0 && list[0].Length > 3 && !IsNewAttributeLine(list[0]) && !list[0].ToLower().StartsWith("mr") && !list[0].ToLower().StartsWith("mb") && !char.IsDigit(list[0].Last()))
                {
                    text3 = text3 + " " + list[0];
                    list.RemoveAt(0);
                }
                text3 = text3.Replace("g%", "9%");
                text3 = text3.Replace("%", " ").Replace(",", ".").Replace("  ", " ")
                    .Trim();
                bool flag2 = !text3.Contains("-");
                text3 = text3.Replace("+", " ").Replace("-", " ").Trim();
                List<string> list2 = (from p in text3.Split(' ')
                                      where p.Length > 0
                                      select p).ToList();
                List<string> list3 = new List<string>();
                float num2 = -1f;
                for (int num3 = 0; num3 < list2.Count; num3++)
                {
                    string text4 = list2[num3].Trim();
                    if (text4.StartsWith("x") && text4 != "x2")
                    {
                        text4 = text4.Substring(1);
                    }
                    float result2;
                    if (num3 < list2.Count - 1 && float.TryParse(text4, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result))
                    {
                        num2 = result;
                        list3.Clear();
                    }
                    else if (num3 < list2.Count - 1 && float.TryParse(text4.Substring(0, text4.Length - 1), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result2))
                    {
                        num2 = result2;
                        list3.Clear();
                        list3.Add(text4.Substring(text4.Length - 1));
                    }
                    else if (text4.Length > 1)
                    {
                        list3.Add(text4.Replace("t0", "to"));
                    }
                }
                if (num2 != -1f && list3.Count != 0)
                {
                    if (!flag2)
                    {
                        num2 = 0f - num2;
                    }
                    string item = string.Join(" ", list3);
                    parsedOCRRiven.attributes.Add((num2, item, parsedOCRRiven.attributes.Count));
                }
            }
            if (parsedOCRRiven.attributes.Count < 2)
            {
                throw new Exception("Failed to find attributes");
            }
            return parsedOCRRiven;
        }

        private static bool IsNewAttributeLine(string line)
        {
            if (line.StartsWith("x2") && line.ToLower().Replace(" ", "").Contains("forheavyattacks"))
            {
                return false;
            }
            if (!line.Contains("%") && !line.Contains("+") && !line.Contains("-") && !line.Contains("."))
            {
                return line.Trim().StartsWith("x");
            }
            return true;
        }

        private static RivenSummaryData RivenSummaryDataFromOCRResult(ParsedOCRRiven ocrResult, AlecaLogDataLogger logger)
        {
            RivenSummaryData rivenSummaryData = new RivenSummaryData();
            if (ocrResult.weaponName.ToLower().Replace(" ", "").Contains("dethmachi") && ocrResult.rivenName.ToLower().StartsWith("rifle-"))
            {
                ocrResult.weaponName += " Rifle";
                ocrResult.rivenName = ocrResult.rivenName.Substring(6);
            }
            if (ocrResult.weaponName == "Euphona")
            {
                ocrResult.weaponName = "Euphona Prime";
            }
            if (ocrResult.weaponName == "Reaper")
            {
                ocrResult.weaponName = "Reaper Prime";
            }
            if (ocrResult.weaponName == "Gotva")
            {
                ocrResult.weaponName = "Gotva Prime";
            }
            List<(BigItem, int)> list = new List<(BigItem, int)>();
            (DataPrimaryWeapon, int) tuple = StaticData.dataHandler.primaryWeapons.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple.Item1, tuple.Item2));
            (DataSecondaryWeapon, int) tuple2 = StaticData.dataHandler.secondaryWeapons.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple2.Item1, tuple2.Item2));
            (DataMeleeWeapon, int) tuple3 = StaticData.dataHandler.meleeWeapons.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple3.Item1, tuple3.Item2));
            (DataSentinelWeapons, int) tuple4 = StaticData.dataHandler.sentinelWeapons.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple4.Item1, tuple4.Item2));
            (DataArchGun, int) tuple5 = StaticData.dataHandler.archGuns.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple5.Item1, tuple5.Item2));
            (DataArchMelee, int) tuple6 = StaticData.dataHandler.archMelees.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple6.Item1, tuple6.Item2));
            (DataMisc, int) tuple7 = StaticData.dataHandler.misc.FirstOrDefaultLevenshtein(ocrResult.weaponName, 3);
            list.Add((tuple7.Item1, tuple7.Item2));
            BigItem item = list.OrderBy<(BigItem, int), int>(((BigItem item, int error) p) => p.error).FirstOrDefault().Item1;
            if (item == null)
            {
                throw new Exception("Failed to find weapon: " + ocrResult.weaponName);
            }
            logger.AddString("Weapon found: " + item.name);
            RivenWeaponData orDefault = StaticData.dataHandler.rivenData.weaponStats.GetOrDefault(item.uniqueName);
            if (orDefault == null)
            {
                throw new Exception("Failed to find weapon riven data: " + item.uniqueName);
            }
            logger.AddString("Weapon riven data found");
            DataRivenStats rivenTypeStats = null;
            if (item is DataPrimaryWeapon)
            {
                if (item.type == "Shotgun")
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusShotgunRandomModRare"];
                }
                else
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusRifleRandomModRare"];
                }
            }
            else if (item is DataSecondaryWeapon)
            {
                rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusPistolRandomModRare"];
            }
            else if (item is DataMeleeWeapon || item is DataArchMelee)
            {
                if (item.type == "Zaw Component")
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusModularMeleeRandomModRare"];
                }
                else
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/PlayerMeleeWeaponRandomModRare"];
                }
            }
            else if (item is DataArchGun)
            {
                rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusArchgunRandomModRare"];
            }
            else if (item is DataMisc)
            {
                rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusModularPistolRandomModRare"];
            }
            else if (item is DataSentinelWeapons)
            {
                if (item.description.ToLower().Contains("shotgun") || item.uniqueName.Contains("shotgun"))
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusShotgunRandomModRare"];
                }
                else if ((item as DataSentinelWeapons).blockingAngle > 0 || item.uniqueName.EndsWith("/SentGlaiveWeapon"))
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/PlayerMeleeWeaponRandomModRare"];
                }
                else
                {
                    rivenTypeStats = StaticData.dataHandler.rivenData.dataByRivenInternalID["/Lotus/Upgrades/Mods/Randomized/LotusRifleRandomModRare"];
                }
            }
            if (rivenTypeStats == null)
            {
                throw new Exception("Failed to find riven type stats for: " + item.uniqueName);
            }
            logger.AddString("Riven mod type found: " + rivenTypeStats.rivenInternalID);
            List<string> attrTags = new List<string>();
            if (!RivenRemoteData.GetRivenAttrsFromNameWithRivenUID(StaticData.dataHandler.rivenData, ocrResult.rivenName, rivenTypeStats.rivenInternalID, out attrTags))
            {
                throw new Exception("Failed to find attributes from name: " + ocrResult.rivenName);
            }
            if (ocrResult.attributes.Count < attrTags.Count || ocrResult.attributes.Count > attrTags.Count + 1)
            {
                throw new Exception("Attribute count from raw OCR isn't feasible with the attribute count extracted from name. " + ocrResult.attributes.Count + " vs " + attrTags.Count);
            }
            List<DataRivenStatsModifier> attributesFromNameData = attrTags.Select((string p) => rivenTypeStats.rivenStats.GetOrDefault(p)).ToList();
            if (attributesFromNameData.Any((DataRivenStatsModifier p) => p == null))
            {
                throw new Exception("Failed to find attributes from attribute names: " + string.Join(" ", attrTags));
            }
            logger.AddString("Matching name attributes to raw OCR attributes:");
            List<(float value, string text, int order)> list2 = ocrResult.attributes.ToList();
            bool negativeAttributeExists = list2.Count > attributesFromNameData.Count;
            if (negativeAttributeExists)
            {
                list2.RemoveAt(list2.Count - 1);
            }
            List<(bool positive, DataRivenStatsModifier modifier, float value, int order)> list3 = new List<(bool, DataRivenStatsModifier, float, int)>();
            foreach (DataRivenStatsModifier item3 in attributesFromNameData)
            {
                logger.AddString("-" + item3.modifierTag);
                string currentAttrLookupName = Misc.ReplaceStringWithNothing(item3.localizationString.Replace("|STAT1|%", "").Replace("|val|%", "").Replace("|val|", "")
                    .ToLower());
                string currentAttrLookupNameWithoutX2ForBows = currentAttrLookupName.Replace(" (x2 for bows)", "");
                ((float value, string text, int order) p, int, int order) bestMatch = (from p in list2
                                                                                       select (p: p, Math.Min(OCRHelper.LevenshteinDistance(p.text.ToLower(), currentAttrLookupName), OCRHelper.LevenshteinDistance(p.text.ToLower(), currentAttrLookupNameWithoutX2ForBows)), order: p.order) into p
                                                                                       orderby p.Item2
                                                                                       select p).First();
                if (bestMatch.Item2 > 4)
                {
                    throw new Exception("Failed to find situable OCR attribute for name attribute");
                }
                list2.RemoveAll(((float value, string text, int order) p) => p.text == bestMatch.p.text);
                list3.Add((true, item3, bestMatch.p.value, bestMatch.order));
                logger.AddString("\t-" + currentAttrLookupName + " -> " + bestMatch.p.text.ToLower() + " (" + bestMatch.p.value + ")");
            }
            if (negativeAttributeExists)
            {
                logger.AddString("Negative attribute found. Trying to match it...");
                (float value, string text, int order) negativeAttributeOCR = ocrResult.attributes.Last();
                (DataRivenStatsModifier, int, int) tuple8 = (from p in rivenTypeStats.rivenStats.Values
                                                             select (p: p, OCRHelper.LevenshteinDistance(Misc.ReplaceStringWithNothing(p.localizationString.Replace("|STAT1|%", "").Replace("|val|%", "").Replace("|val|", "")
                                                                 .ToLower()), negativeAttributeOCR.text.ToLower()), order: negativeAttributeOCR.order) into p
                                                             orderby p.Item2
                                                             select p).First();
                if (tuple8.Item2 > 4)
                {
                    throw new Exception("Failed to find situable OCR attribute for negative attribute");
                }
                list3.Add((false, tuple8.Item1, negativeAttributeOCR.value, tuple8.Item3));
                logger.AddString("\t" + tuple8.Item1.localizationString + " -> " + negativeAttributeOCR.text + " (" + negativeAttributeOCR.value + ")");
            }
            else
            {
                logger.AddString("No negative attribute found");
            }
            list3 = (from p in list3
                     select (positive: p.positive, modifier: p.modifier, p.modifier.localizationString.Contains("Damage to") ? (p.value - 1f) : p.value, order: p.order) into p
                     orderby p.order
                     select p).ToList();
            rivenSummaryData.isUnveiled = true;
            rivenSummaryData.randomID = Guid.NewGuid().ToString();
            rivenSummaryData.weaponType = rivenTypeStats.veiledName.Replace(" Riven Mod", "");
            rivenSummaryData.weaponReference = item;
            rivenSummaryData.maxImprovementLevel = 8;
            rivenSummaryData.minimumMastery = 0;
            rivenSummaryData.currentImprovementLevel = 0;
            rivenSummaryData.polarity = "";
            rivenSummaryData.rerollCount = 0;
            rivenSummaryData.name = ocrResult.rivenName;
            rivenSummaryData.weaponName = orDefault.name;
            rivenSummaryData.disposition = orDefault.omegaAtt;
            rivenSummaryData.weaponPicture = Misc.GetFullImagePath(item?.imageName);
            List<RivenWeaponData> compatibleWeapons = Misc.GetCompatibleWeapons(orDefault);
            List<RivenWeaponData> list4 = (from p in compatibleWeapons
                                           orderby Misc.IsWeaponOwned(StaticData.dataHandler.rivenData.weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> k) => k.Value.name == p.name).Key) descending, p.name.Length
                                           select p).ToList();
            RivenWeaponData rivenWeaponData;
            RivenUnveiledStats rivenUnveiledStats;
            List<double> list5;
            while (true)
            {
            IL_0aa0:
                if (list4.Count == 0)
                {
                    throw new Exception("Only rivens at levels 0 or 8 (max) are supported");
                }
                rivenWeaponData = list4.First();
                list4.RemoveAt(0);
                logger.AddString("Trying weapon: " + rivenWeaponData.name);
                rivenUnveiledStats = new RivenUnveiledStats();
                MultipliersBasedOnGoodBadModifiers multipliersBasedOnGoodBadModifiers = StaticData.dataHandler.rivenData.modifiersBasedOnTraitCount.First((MultipliersBasedOnGoodBadModifiers p) => p.goodModifiersCount == attributesFromNameData.Count && p.badModifiersCount == (negativeAttributeExists ? 1 : 0));
                logger.AddString("Good modifiers: " + multipliersBasedOnGoodBadModifiers.goodModifiersCount + " Bad modifiers: " + multipliersBasedOnGoodBadModifiers.badModifiersCount);
                list5 = new List<double>();
                foreach (var item4 in list3)
                {
                    DataRivenStatsModifier item2 = item4.Item2;
                    double num = item4.Item3;
                    double num2 = 90.0 * item2.baseValue * rivenWeaponData.omegaAtt * (item4.Item1 ? multipliersBasedOnGoodBadModifiers.goodModifierMultiplier : multipliersBasedOnGoodBadModifiers.badModifierMultiplier);
                    if (Math.Sign(num2) != Math.Sign(item4.Item3))
                    {
                        num = 0f - item4.Item3;
                        logger.AddString("Inverted sign of stat value. It probably was missread by the OCR?");
                    }
                    if (item2.localizationString.Contains("|val|%") || item2.localizationString.Contains("|STAT1|%"))
                    {
                        num2 *= 100.0;
                    }
                    double num3 = num2 / 9.0;
                    double num4 = num / num3 - 1.0;
                    list5.Add(num4);
                    if (num4 > 6.5)
                    {
                        num4 = 8.0;
                    }
                    else
                    {
                        if (!(num4 < 1.5))
                        {
                            if (list4.Count == 0)
                            {
                                throw new Exception("Only rivens at levels 0 or 8 (max) are supported");
                            }
                            logger.AddString("Mod rank guess for " + item4.Item2.shortString + " is away from 0 or 8. Trying another weapon variation...");
                            goto IL_0aa0;
                        }
                        num4 = 0.0;
                    }
                    num = num / (num4 + 1.0) * 9.0;
                    double num5 = (num - num2 * 0.9) / (num2 * 0.2);
                    double num6 = Math.Max(0.0, Math.Min(num5, 1.0));
                    double num7 = 0.054999999701976776;
                    if (item2.localizationString.Contains("Damage to") || Math.Abs(num) / 9.0 < 2.5)
                    {
                        num7 = 0.5;
                    }
                    if (Math.Abs(num) / 9.0 < 1.0)
                    {
                        num7 = 2.25;
                    }
                    if (Math.Abs(num5 - num6) > num7)
                    {
                        logger.AddString("Random percent for attr " + item4.Item2.shortString + " is too far from the rounded one. Trying another weapon variation...");
                        goto IL_0aa0;
                    }
                    num = Math.Round(num, 2);
                    if (item4.Item1)
                    {
                        rivenUnveiledStats.positiveTraits.Add(new RivenUnveiledSingleStat(item2.modifierTag, item2.localizationString, num, num2 * 0.9, num2 * 1.1, num6, item2.prefixTag + "|" + item2.suffixTag));
                    }
                    else
                    {
                        rivenUnveiledStats.negativeTraits.Add(new RivenUnveiledSingleStat(item2.modifierTag, item2.localizationString, num, num2 * 1.1, num2 * 0.9, 1.0 - num6, item2.prefixTag + "|" + item2.suffixTag));
                    }
                }
                break;
            }
            logger.AddString("Attribute purities: ");
            logger.AddString("\t POS:\n\t\t" + string.Join("\n\t\t", rivenUnveiledStats.positiveTraits.Select((RivenUnveiledSingleStat p) => p.localizationString + " " + p.grade)));
            logger.AddString("\t NEG:\n\t\t" + string.Join("\n\t\t", rivenUnveiledStats.negativeTraits.Select((RivenUnveiledSingleStat p) => p.localizationString + " " + p.grade)));
            logger.AddString("Detected mod level by attributes: " + string.Join(" ", list5));
            if (list5.Any((double p) => p < -2.0))
            {
                throw new Exception("At least one of the mod level guesses is very negative");
            }
            if (list5.Any((double p) => p > 10.0))
            {
                throw new Exception("At least one of the mod level guesses is above 8");
            }
            float averageModLevel = (float)list5.Average();
            logger.AddString("Average mod level: " + averageModLevel);
            if (list5.Any((double p) => Math.Abs(p - (double)averageModLevel) > 2.0))
            {
                throw new Exception("The mod level guesses are not all the same");
            }
            rivenSummaryData.currentImprovementLevel = (int)list5.First();
            rivenSummaryData.currentCost = 10 + rivenSummaryData.currentImprovementLevel;
            foreach (RivenWeaponData item5 in compatibleWeapons)
            {
                RivenUnveiledStatWithWeapon rivenUnveiledStatWithWeapon = new RivenUnveiledStatWithWeapon();
                rivenUnveiledStatWithWeapon.weaponName = item5.name;
                double num8 = item5.omegaAtt / rivenWeaponData.omegaAtt;
                for (int num9 = 0; num9 <= 8; num9++)
                {
                    double levelMultiplier = num8 * ((double)(num9 + 1) / 9.0);
                    rivenUnveiledStatWithWeapon.byLevel.Add(new RivenUnveiledStats
                    {
                        level = num9,
                        positiveTraits = rivenUnveiledStats.positiveTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList(),
                        negativeTraits = rivenUnveiledStats.negativeTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList()
                    });
                }
                rivenSummaryData.statsPerWeapon.Add(rivenUnveiledStatWithWeapon);
            }
            rivenSummaryData.FillSimilarRivens();
            rivenSummaryData.GradeRiven();
            rivenSummaryData.FillGoodRolls();
            return rivenSummaryData;
        }

        private static (T firstItem, int error) FirstOrDefaultLevenshtein<T>(this Dictionary<string, T> source, string nameToMatch, int maxError) where T : BigItem
        {
            (KeyValuePair<string, T>, int) tuple = (from p in source
                                                    select (p: p, OCRHelper.LevenshteinDistance(p.Value.name.ToLower(), nameToMatch.ToLower())) into p
                                                    orderby p.Item2
                                                    select p).First();
            if (tuple.Item2 > maxError)
            {
                return (firstItem: null, error: int.MaxValue);
            }
            return (firstItem: tuple.Item1.Value, error: tuple.Item2);
        }

        private static Bitmap DetailedRivenCrop(Bitmap roughCut, Bitmap roughtCutEdges, AlecaLogDataLogger logger)
        {
            Bitmap bitmap = (Bitmap)roughCut.Clone();
            List<int> list = new List<int>();
            for (int i = (int)(0.15 * (double)roughCut.Height); (double)i < (double)roughCut.Height * 0.55; i++)
            {
                int num = -1;
                int num2 = -1;
                for (int j = 0; j <= roughCut.Width / 2; j++)
                {
                    if (roughtCutEdges.GetPixel(j, i).R > 130)
                    {
                        bitmap.SetPixel(j, i, Color.Green);
                        num = j;
                        break;
                    }
                }
                for (int num3 = roughCut.Width - 1; num3 > roughCut.Width / 2; num3--)
                {
                    if (roughtCutEdges.GetPixel(num3, i).R > 130)
                    {
                        bitmap.SetPixel(num3, i, Color.Green);
                        num2 = num3;
                        break;
                    }
                }
                if (num != -1 && num2 != -1 && (float)Math.Abs(num + num2 - roughCut.Width) <= (float)roughCut.Width * 0.05f)
                {
                    list.Add(num);
                    bitmap.SetPixel(num, i, Color.Red);
                }
            }
            logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap, "leftRightEdges");
            if (list.Count < 10)
            {
                throw new Exception("Not enough valid left edge guesses");
            }
            int averageXEdge = (int)list.Average();
            if ((float)(list.Sum((int x) => Math.Abs(x - averageXEdge)) / list.Count) > (float)roughCut.Width * 0.03f)
            {
                throw new Exception("Average left edge error too high");
            }
            int num4 = averageXEdge;
            int num5 = roughCut.Width - num4;
            int xstart = num4 + (int)((float)roughCut.Width * 0.15f);
            int xend = num5 - (int)((float)roughCut.Width * 0.15f);
            int num6 = 0;
            for (int num7 = 0; num7 < roughCut.Height; num7++)
            {
                if (IsLineMostlyBlack(roughCut, roughtCutEdges, xstart, xend, num7))
                {
                    num6 = num7;
                    break;
                }
            }
            if (num6 == 0)
            {
                throw new Exception("Could not find top edge");
            }
            int num8 = 0;
            for (int num9 = roughCut.Height - 1; num9 >= 0; num9--)
            {
                if (IsLineMostlyBlack(roughCut, roughtCutEdges, xstart, xend, num9))
                {
                    num8 = num9;
                    break;
                }
            }
            if (num8 == 0)
            {
                throw new Exception("Could not find bottom edge");
            }
            Bitmap bitmap2 = (Bitmap)roughCut.Clone();
            for (int num10 = num4; num10 < num5; num10++)
            {
                bitmap2.SetPixel(num10, num6, Color.Yellow);
                bitmap2.SetPixel(num10, num8, Color.Yellow);
            }
            logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap2, "allEdges");
            return roughCut.Clone(new RectangleF(num4 + 3, num6 + 2, num5 - num4 - 6, num8 - num6 - 2), roughtCutEdges.PixelFormat);
        }

        private static bool IsLineMostlyBlack(Bitmap bitmap, Bitmap bitmapEdgeDetected, int Xstart, int Xend, int y)
        {
            float num = 0f;
            float num2 = 0f;
            for (int i = Xstart; i < Xend; i++)
            {
                Color pixel = bitmap.GetPixel(i, y);
                float num3 = (float)(pixel.R + pixel.G + pixel.B) / 3f;
                float num4 = (int)bitmapEdgeDetected.GetPixel(i, y).R;
                if (num3 > 50f || num4 > 50f)
                {
                    return false;
                }
            }
            num /= (float)(Xend - Xstart);
            num2 /= (float)(Xend - Xstart);
            if (num < 50f)
            {
                return num2 < 50f;
            }
            return false;
        }

        private static Bitmap CutBitmapToRoughSize(RivenOCRDetectionType oCRtype, Bitmap bitmap, ScalingMode scalingMode, float customScale)
        {
            switch (oCRtype)
            {
                case RivenOCRDetectionType.ChatRiven:
                    {
                        float num7 = (float)bitmap.Height * 0.33f;
                        float num8 = num7 * 0.75f;
                        float x2 = (float)bitmap.Width * 0.5f - num8 * 0.5f;
                        float num9 = (float)bitmap.Height * 0.5f - num7 * 0.5f;
                        float num10 = 0.2f * num7;
                        num7 -= num10;
                        num9 += num10;
                        return bitmap.Clone(new RectangleF(x2, num9, num8, num7), bitmap.PixelFormat);
                    }
                case RivenOCRDetectionType.RivenReroll:
                    {
                        float num = Math.Max((float)bitmap.Height - (float)bitmap.Width * 9f / 16f, 0f);
                        float num2 = Math.Max((float)bitmap.Width - (float)bitmap.Height * 16f / 9f, 0f);
                        RectangleF rect = new RectangleF(num2 / 2f, num / 2f, (float)bitmap.Width - num2, (float)bitmap.Height - num);
                        using Bitmap bitmap2 = bitmap.Clone(rect, bitmap.PixelFormat);
                        Bitmap bitmap3 = bitmap2;
                        if (scalingMode == ScalingMode.Legacy)
                        {
                            if (bitmap2.Width > 1536 && bitmap2.Height > 840)
                            {
                                RectangleF rect2 = new RectangleF((bitmap2.Width - 1536) / 2, (bitmap2.Height - 840) / 2, 1536f, 840f);
                                bitmap3 = bitmap2.Clone(rect2, bitmap2.PixelFormat);
                            }
                        }
                        else
                        {
                            bitmap3 = bitmap2;
                        }
                        using (bitmap3)
                        {
                            float num3 = (float)bitmap3.Height * 0.7f;
                            float num4 = num3 * 0.45f;
                            float x = (float)bitmap3.Width * 0.5f - num4 * 0.5f;
                            float num5 = (float)bitmap3.Height * 0.5f - num3 * 0.5f;
                            float num6 = 0.38f * num3;
                            num3 -= num6;
                            num5 += num6;
                            RectangleF rect3 = new RectangleF(x, num5, num4, num3);
                            if (scalingMode == ScalingMode.Custom)
                            {
                                Point point = new Point(bitmap3.Width / 2, bitmap3.Height / 2);
                                rect3.X -= point.X;
                                rect3.Y -= point.Y;
                                rect3.X *= customScale;
                                rect3.Y *= customScale;
                                rect3.Width *= customScale;
                                rect3.Height *= customScale;
                                rect3.X += point.X;
                                rect3.Y += point.Y;
                            }
                            return bitmap3.Clone(rect3, bitmap3.PixelFormat);
                        }
                    }
                case RivenOCRDetectionType.PreCut:
                    return new Bitmap(bitmap);
                default:
                    throw new NotImplementedException($"RivenOCRDetection type {oCRtype} not implemented");
            }
        }

        public static void ChatRivenClosedDetected()
        {
            StaticData.Log(OverwolfWrapper.LogType.INFO, "Chat riven closed detected");
            workingData.enabled = false;
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
        }

        public static void RivenRerollClosedDetected()
        {
            isRerollUIOpen = false;
            StaticData.Log(OverwolfWrapper.LogType.INFO, "Riven reroll closed detected");
            workingData.enabled = false;
            StaticData.overwolfWrappwer.onRivenOverlayChangeCaller();
        }

        public static void RetryLastOCR()
        {
            lastOCRMethodRun?.Invoke();
        }
    }
}
