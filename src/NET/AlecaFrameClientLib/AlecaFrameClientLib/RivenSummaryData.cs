using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Data.Types.WFM;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class RivenSummaryData
{
	[NonSerialized]
	public string uniqueID;

	public bool isUnveiled;

	public string name;

	public string randomID;

	public string weaponType;

	public int tradePrice;

	public int amountOwned = 1;

	[NonSerialized]
	public string challengeID;

	[NonSerialized]
	public string simpleChallengeName;

	[NonSerialized]
	public string challengeDescription;

	public string challengeStringWithComplication;

	public double challengeCurrentProgress;

	public double challengeMaxProgress;

	public string weaponName;

	[NonSerialized]
	public BigItem weaponReference;

	public string weaponPicture;

	public List<RivenUnveiledStatWithWeapon> statsPerWeapon = new List<RivenUnveiledStatWithWeapon>();

	public double disposition;

	public int minimumMastery;

	public int currentImprovementLevel;

	public int maxImprovementLevel;

	public int currentCost;

	public string polarity;

	public int rerollCount;

	public string editWFMdescription = "";

	public int editWFMsellingPrice;

	public int editWFMstartingPrice;

	public int editWFMbuyoutPrice;

	public int editWFMminReputation;

	public string editlistType = "direct";

	public string editlistVisibility = "public";

	public List<RivenUISimilarRiven> similarRivens = new List<RivenUISimilarRiven>();

	public GoodRollDataEvaluated goodRollData;

	[NonSerialized]
	public bool errorOccurred;

	[NonSerialized]
	public bool isPreVeiled;

	public bool listedInWFMarket;

	[JsonConverter(typeof(StringEnumConverter))]
	public AlecaFrameRivenGrade grade;

	public RivenSummaryData()
	{
	}

	public RivenSummaryData(WFMRivenDataAuction wfmRivenData, RivenAtttrResponse wfmRivenAttrData, WFMRivenItemsResponse wfmRivenItemsData)
	{
		isUnveiled = true;
		randomID = wfmRivenData.id;
		WFMRivenItemsResponseItem wfmRivenObject = wfmRivenItemsData.data.First((WFMRivenItemsResponseItem p) => p.slug == wfmRivenData.item.weapon_url_name);
		minimumMastery = wfmRivenData.item.mastery_level;
		currentImprovementLevel = wfmRivenData.item.mod_rank;
		maxImprovementLevel = 8;
		currentCost = 10 + currentImprovementLevel;
		polarity = wfmRivenData.item.polarity;
		rerollCount = wfmRivenData.item.re_rolls;
		name = wfmRivenData.item.name;
		editWFMdescription = wfmRivenData.note.Replace("<p>", "").Replace("</p>", "").Trim();
		editWFMsellingPrice = wfmRivenData.starting_price;
		editWFMstartingPrice = wfmRivenData.starting_price;
		editWFMbuyoutPrice = wfmRivenData.buyout_price.GetValueOrDefault();
		editWFMminReputation = wfmRivenData.minimal_reputation;
		editlistType = (wfmRivenData.is_direct_sell ? "direct" : "auction");
		editlistVisibility = (wfmRivenData.visible ? "public" : "private");
		weaponReference = StaticData.dataHandler.primaryWeapons.Values.FirstOrDefault((DataPrimaryWeapon p) => p.uniqueName == wfmRivenObject.gameRef);
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.secondaryWeapons.Values.FirstOrDefault((DataSecondaryWeapon p) => p.uniqueName == wfmRivenObject.gameRef);
		}
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.meleeWeapons.Values.FirstOrDefault((DataMeleeWeapon p) => p.uniqueName == wfmRivenObject.gameRef);
		}
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.sentinelWeapons.Values.FirstOrDefault((DataSentinelWeapons p) => p.uniqueName == wfmRivenObject.gameRef);
		}
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.archGuns.Values.FirstOrDefault((DataArchGun p) => p.uniqueName == wfmRivenObject.gameRef);
		}
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.misc.FirstOrDefault((KeyValuePair<string, DataMisc> p) => p.Value.uniqueName == wfmRivenObject.gameRef).Value;
		}
		if (weaponReference == null)
		{
			weaponReference = StaticData.dataHandler.archMelees.Values.FirstOrDefault((DataArchMelee p) => p.uniqueName == wfmRivenObject.gameRef);
		}
		RivenWeaponData myWeaponData = null;
		myWeaponData = StaticData.dataHandler.rivenData.weaponStats.FirstOrDefault((KeyValuePair<string, RivenWeaponData> p) => p.Key == wfmRivenObject.gameRef).Value;
		if (myWeaponData != null)
		{
			weaponName = myWeaponData.name;
		}
		if (myWeaponData != null)
		{
			disposition = myWeaponData.omegaAtt;
		}
		if (weaponReference == null || myWeaponData == null)
		{
			StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to find weapon: " + wfmRivenObject.slug + " (" + wfmRivenObject.gameRef + ") for riven mod");
			errorOccurred = true;
			return;
		}
		DataRivenStats value = StaticData.dataHandler.rivenData.dataByRivenInternalID.FirstOrDefault((KeyValuePair<string, DataRivenStats> p) => p.Key == myWeaponData.rivenUID).Value;
		if (value == null)
		{
			StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to find riven data for: " + wfmRivenObject.gameRef);
			errorOccurred = true;
			return;
		}
		weaponType = value.veiledName.Replace(" Riven Mod", "");
		weaponPicture = ((weaponReference == null) ? "" : Misc.GetFullImagePath(weaponReference?.imageName));
		RivenUnveiledStats rivenUnveiledStats = new RivenUnveiledStats();
		MultipliersBasedOnGoodBadModifiers multipliersBasedOnGoodBadModifiers = StaticData.dataHandler.rivenData.modifiersBasedOnTraitCount.FirstOrDefault((MultipliersBasedOnGoodBadModifiers p) => p.goodModifiersCount == wfmRivenData.item.attributes.Count((WFMRivenDataAttribute u) => u.positive) && p.badModifiersCount == wfmRivenData.item.attributes.Count((WFMRivenDataAttribute u) => !u.positive));
		if (multipliersBasedOnGoodBadModifiers == null)
		{
			StaticData.Log(OverwolfWrapper.LogType.WARN, "Invalid WFM riven attr count. The closest one will be chosen. " + wfmRivenData.item.weapon_url_name + " " + wfmRivenData.item.name);
			multipliersBasedOnGoodBadModifiers = (from p in StaticData.dataHandler.rivenData.modifiersBasedOnTraitCount
				orderby Math.Abs(p.goodModifiersCount - wfmRivenData.item.attributes.Count((WFMRivenDataAttribute u) => u.positive)), Math.Abs(p.badModifiersCount - wfmRivenData.item.attributes.Count((WFMRivenDataAttribute u) => !u.positive))
				select p).First();
		}
		WFMRivenDataAttribute[] attributes = wfmRivenData.item.attributes;
		foreach (WFMRivenDataAttribute goodStat in attributes)
		{
			RivenAtttrResponseAttribute currentWFMAttrData = wfmRivenAttrData.data.First((RivenAtttrResponseAttribute p) => p.slug == goodStat.url_name);
			DataRivenStatsModifier value2 = value.rivenStats.FirstOrDefault((KeyValuePair<string, DataRivenStatsModifier> p) => p.Value.suffixTag == (currentWFMAttrData.suffix ?? "").ToLower() && p.Value.prefixTag == (currentWFMAttrData.prefix ?? "").ToLower()).Value;
			if (value2 == null)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Invalid WFM riven attr: " + wfmRivenData.item.weapon_url_name + " " + wfmRivenData.item.name);
				continue;
			}
			double num2 = 90.0 * value2.baseValue * myWeaponData.omegaAtt * (goodStat.positive ? multipliersBasedOnGoodBadModifiers.goodModifierMultiplier : multipliersBasedOnGoodBadModifiers.badModifierMultiplier);
			if (value2.localizationString.Contains("|val|%") || value2.localizationString.Contains("|STAT1|%"))
			{
				num2 *= 100.0;
			}
			double num3 = goodStat.value;
			if (goodStat.url_name.StartsWith("damage_vs"))
			{
				num3 -= 1.0;
			}
			if (wfmRivenData.item.mod_rank == 0 && Math.Abs(num3 - num2) < 0.5 * num3)
			{
				wfmRivenData.item.mod_rank = 8;
			}
			num3 = num3 / (double)(wfmRivenData.item.mod_rank + 1) * 9.0;
			double num4 = 0.0;
			if (!(num3 > num2 * 1.125))
			{
				_ = num2 * 0.875;
			}
			num4 = (num3 - num2 * 0.9) / (num2 * 0.2);
			num4 = Math.Max(0.0, Math.Min(num4, 1.0));
			num3 = Math.Round(num3, 1);
			if (goodStat.positive)
			{
				rivenUnveiledStats.positiveTraits.Add(new RivenUnveiledSingleStat(value2.modifierTag, value2.localizationString, num3, num2 * 0.9, num2 * 1.1, num4, value2.prefixTag + "|" + value2.suffixTag));
			}
			else
			{
				rivenUnveiledStats.negativeTraits.Add(new RivenUnveiledSingleStat(value2.modifierTag, value2.localizationString, num3, num2 * 1.1, num2 * 0.9, 1.0 - num4, value2.prefixTag + "|" + value2.suffixTag));
			}
		}
		List<RivenWeaponData> list = new List<RivenWeaponData>();
		list.Add(myWeaponData);
		list.AddRange(StaticData.dataHandler.rivenData.weaponStats.Values.Where((RivenWeaponData p) => p.name.Split(' ').Contains(myWeaponData.name) && !p.name.Contains("Garuda") && !p.name.Contains("Valkyr") && p.name != myWeaponData.name));
		foreach (RivenWeaponData item in list)
		{
			RivenUnveiledStatWithWeapon rivenUnveiledStatWithWeapon = new RivenUnveiledStatWithWeapon
			{
				weaponName = item.name
			};
			double num5 = item.omegaAtt / myWeaponData.omegaAtt;
			for (int num6 = 0; num6 <= 8; num6++)
			{
				double levelMultiplier = num5 * ((double)(num6 + 1) / 9.0);
				rivenUnveiledStatWithWeapon.byLevel.Add(new RivenUnveiledStats
				{
					level = num6,
					positiveTraits = rivenUnveiledStats.positiveTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList(),
					negativeTraits = rivenUnveiledStats.negativeTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList()
				});
			}
			statsPerWeapon.Add(rivenUnveiledStatWithWeapon);
		}
		GradeRiven();
	}

	public RivenSummaryData(Miscitem miscData, bool isUnveiled)
	{
		this.isUnveiled = isUnveiled;
		uniqueID = miscData.ItemType;
		randomID = miscData.ItemId?.oid;
		if (randomID == null)
		{
			isPreVeiled = true;
			randomID = miscData.ItemType + "%%PREVEILED";
			amountOwned = miscData.ItemCount;
		}
		DataRivenStats remoteData = StaticData.dataHandler.rivenData.dataByRivenInternalID[miscData.ItemType];
		ExtraModData extraModData = ExtraModData.DeserializeFromString(miscData.UpgradeFingerprint);
		weaponType = remoteData.veiledName.Replace(" Riven Mod", "");
		if ((!isPreVeiled || isUnveiled) && extraModData.IsRivenUnveiled() != isUnveiled)
		{
			errorOccurred = true;
			return;
		}
		if (isUnveiled)
		{
			string text = extraModData.compat;
			if (text == "/Lotus/Weapons/Tenno/Melee/Dagger/DarkDaggerBase")
			{
				text = "/Lotus/Weapons/Tenno/Melee/Dagger/DarkDagger";
			}
			if (text == "/Lotus/Weapons/Tenno/Shotgun/QuadShotgunBase")
			{
				text = "/Lotus/Weapons/Tenno/Shotgun/QuadShotgun";
			}
			weaponReference = StaticData.dataHandler.primaryWeapons.GetOrDefault(text);
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.secondaryWeapons.GetOrDefault(text);
			}
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.meleeWeapons.GetOrDefault(text);
			}
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.sentinelWeapons.GetOrDefault(text);
			}
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.archGuns.GetOrDefault(text);
			}
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.misc.GetOrDefault(text);
			}
			if (weaponReference == null)
			{
				weaponReference = StaticData.dataHandler.archMelees.GetOrDefault(text);
			}
			RivenWeaponData myWeaponData = null;
			if (StaticData.dataHandler.rivenData.weaponStats.ContainsKey(text))
			{
				myWeaponData = StaticData.dataHandler.rivenData.weaponStats[text];
			}
			if (myWeaponData == null)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to find weapon: " + extraModData.compat + " for riven mod");
				errorOccurred = true;
				return;
			}
			weaponName = myWeaponData.name;
			weaponPicture = ((weaponReference == null) ? "" : Misc.GetFullImagePath(weaponReference?.imageName));
			disposition = myWeaponData.omegaAtt;
			minimumMastery = extraModData.lvlReq;
			currentImprovementLevel = extraModData.lvl;
			maxImprovementLevel = 8;
			currentCost = 10 + currentImprovementLevel;
			switch (extraModData.pol)
			{
			case "AP_ATTACK":
				polarity = "madurai";
				break;
			case "AP_DEFENSE":
				polarity = "vazarin";
				break;
			case "AP_TACTIC":
				polarity = "naramon";
				break;
			default:
				polarity = "";
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Can not find polarity translation for: " + extraModData.pol);
				break;
			}
			rerollCount = extraModData.rerolls;
			RivenUnveiledStats rivenUnveiledStats = new RivenUnveiledStats();
			MultipliersBasedOnGoodBadModifiers multipliersBasedOnGoodBadModifiers = StaticData.dataHandler.rivenData.modifiersBasedOnTraitCount.First(delegate(MultipliersBasedOnGoodBadModifiers p)
			{
				if (p.goodModifiersCount == extraModData.buffs.Length)
				{
					int badModifiersCount = p.badModifiersCount;
					ExtraModDataCurse[] curses = extraModData.curses;
					return badModifiersCount == ((curses != null) ? curses.Length : 0);
				}
				return false;
			});
			ExtraModDataBuff[] array = extraModData.buffs ?? new ExtraModDataBuff[0];
			foreach (ExtraModDataBuff extraModDataBuff in array)
			{
				DataRivenStatsModifier dataRivenStatsModifier = remoteData.rivenStats[extraModDataBuff.Tag];
				double num2 = Math.Min(Math.Max(0.9 + (double)extraModDataBuff.Value / 53687091.0 / 100.0, 0.9), 1.1);
				double num3 = 90.0 * dataRivenStatsModifier.baseValue * myWeaponData.omegaAtt * multipliersBasedOnGoodBadModifiers.goodModifierMultiplier;
				if (dataRivenStatsModifier.localizationString.Contains("|val|%") || dataRivenStatsModifier.localizationString.Contains("|STAT1|%"))
				{
					num3 *= 100.0;
				}
				double currentValue = num3 * num2;
				RivenUnveiledSingleStat item = new RivenUnveiledSingleStat(dataRivenStatsModifier.modifierTag, dataRivenStatsModifier.localizationString, currentValue, num3 * 0.9, num3 * 1.1, (num2 - 0.9) * 5.0, dataRivenStatsModifier.prefixTag + "|" + dataRivenStatsModifier.suffixTag);
				rivenUnveiledStats.positiveTraits.Add(item);
			}
			ExtraModDataCurse[] array2 = extraModData.curses ?? new ExtraModDataCurse[0];
			foreach (ExtraModDataCurse extraModDataCurse in array2)
			{
				DataRivenStatsModifier dataRivenStatsModifier2 = remoteData.rivenStats[extraModDataCurse.Tag];
				double num4 = Math.Min(Math.Max(0.9 + (double)extraModDataCurse.Value / 53687091.0 / 100.0, 0.9), 1.1);
				double num5 = 90.0 * dataRivenStatsModifier2.baseValue * myWeaponData.omegaAtt * multipliersBasedOnGoodBadModifiers.badModifierMultiplier;
				if (dataRivenStatsModifier2.modifierTag == "WeaponMeleeComboPointsOnHitMod" && num5 > 0.0)
				{
					num5 = 0.0 - num5;
				}
				if (dataRivenStatsModifier2.localizationString.Contains("|val|%") || dataRivenStatsModifier2.localizationString.Contains("|STAT1|%"))
				{
					num5 *= 100.0;
				}
				double currentValue2 = num5 * num4;
				RivenUnveiledSingleStat item2 = new RivenUnveiledSingleStat(dataRivenStatsModifier2.modifierTag, dataRivenStatsModifier2.localizationString, currentValue2, num5 * 1.1, num5 * 0.9, 1.0 - (num4 - 0.9) * 5.0, dataRivenStatsModifier2.prefixTag + "|" + dataRivenStatsModifier2.suffixTag);
				rivenUnveiledStats.negativeTraits.Add(item2);
			}
			List<RivenWeaponData> list = new List<RivenWeaponData>();
			list.Add(myWeaponData);
			list.AddRange(StaticData.dataHandler.rivenData.weaponStats.Values.Where((RivenWeaponData p) => p.name.Split(' ').Contains(myWeaponData.name) && !p.name.Contains("Garuda") && !p.name.Contains("Valkyr") && p.name != myWeaponData.name));
			foreach (RivenWeaponData item3 in list)
			{
				RivenUnveiledStatWithWeapon rivenUnveiledStatWithWeapon = new RivenUnveiledStatWithWeapon
				{
					weaponName = item3.name
				};
				double num6 = item3.omegaAtt / myWeaponData.omegaAtt;
				for (int num7 = 0; num7 <= 8; num7++)
				{
					double levelMultiplier = num6 * ((double)(num7 + 1) / 9.0);
					rivenUnveiledStatWithWeapon.byLevel.Add(new RivenUnveiledStats
					{
						level = num7,
						positiveTraits = rivenUnveiledStats.positiveTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList(),
						negativeTraits = rivenUnveiledStats.negativeTraits.Select((RivenUnveiledSingleStat p) => new RivenUnveiledSingleStat(p.uniqueID, p.localizationString, p.currentValue * levelMultiplier, p.worstCase * levelMultiplier, p.bestCase * levelMultiplier, p.rawRandomValue, p.prefixSufixCombo)).ToList()
					});
				}
				statsPerWeapon.Add(rivenUnveiledStatWithWeapon);
			}
			string[] array3 = new string[extraModData.buffs.Length];
			List<DataRivenStatsModifier> list2 = (from p in extraModData.buffs
				orderby p.Value descending
				select remoteData.rivenStats[p.Tag]).ToList();
			if (array3.Length == 2)
			{
				name = list2[0].prefixTag.FirstCharToUpper() + list2[1].suffixTag.ToLower();
			}
			else
			{
				if (array3.Length != 3)
				{
					errorOccurred = true;
					StaticData.Log(OverwolfWrapper.LogType.ERROR, "Found riven mod with an unknown buff quantity!");
					return;
				}
				name = list2[0].prefixTag.FirstCharToUpper() + "-" + list2[1].prefixTag.ToLower() + list2[2].suffixTag.ToLower();
			}
			GradeRiven();
			return;
		}
		name = remoteData.veiledName;
		if (isPreVeiled)
		{
			challengeID = "PREVEILED_ID";
			challengeDescription = "Unseen (???) rivens";
			challengeStringWithComplication = "Equip these rivens to reveal their challenge";
			challengeCurrentProgress = -1.0;
			challengeMaxProgress = -1.0;
			return;
		}
		challengeID = extraModData.challenge.Type;
		challengeDescription = remoteData.challenges[challengeID].description.Replace("|COUNT| ", "");
		challengeStringWithComplication = Misc.ReplaceStringWithIcons(remoteData.challenges[challengeID].description.Replace("|COUNT|", extraModData.challenge.Required.ToString()));
		if (extraModData.challenge.Complication != null)
		{
			try
			{
				challengeStringWithComplication = challengeStringWithComplication + " " + remoteData.challenges[challengeID].complications[extraModData.challenge.Complication].description;
			}
			catch (Exception)
			{
				challengeStringWithComplication += " (Unknown complication)";
			}
		}
		challengeCurrentProgress = extraModData.challenge.Progress;
		challengeMaxProgress = extraModData.challenge.Required;
	}

	public void GradeRiven()
	{
		if (weaponReference == null)
		{
			grade = AlecaFrameRivenGrade.Unknown;
			{
				foreach (RivenUnveiledStatWithWeapon item in statsPerWeapon)
				{
					foreach (RivenUnveiledStats item2 in item.byLevel)
					{
						for (int i = 0; i < item2.positiveTraits.Count; i++)
						{
							item2.positiveTraits[i].rollGrade = AlecaFrameAttributeGrade.Unknown;
						}
						for (int j = 0; j < item2.negativeTraits.Count; j++)
						{
							item2.negativeTraits[j].rollGrade = AlecaFrameAttributeGrade.Unknown;
						}
					}
				}
				return;
			}
		}
		RivenUnveiledStats rivenUnveiledStats = statsPerWeapon[0].byLevel[0];
		grade = RivenExplorerHelper.GetRivenGradeFromRAWAttrList(weaponReference.uniqueName, rivenUnveiledStats.positiveTraits.Select((RivenUnveiledSingleStat p) => p.uniqueID).ToList(), rivenUnveiledStats.negativeTraits.Select((RivenUnveiledSingleStat p) => p.uniqueID).ToList(), out var positiveGrades, out var negativeGrades);
		foreach (RivenUnveiledStatWithWeapon item3 in statsPerWeapon)
		{
			foreach (RivenUnveiledStats item4 in item3.byLevel)
			{
				for (int num = 0; num < item4.positiveTraits.Count; num++)
				{
					item4.positiveTraits[num].rollGrade = positiveGrades[num];
				}
				for (int num2 = 0; num2 < item4.negativeTraits.Count; num2++)
				{
					item4.negativeTraits[num2].rollGrade = negativeGrades[num2];
				}
			}
		}
	}

	public bool IsRoughlyEqual(RivenSummaryData otherRiven)
	{
		if (name.ToLower().Replace(" ", "") == otherRiven.name?.ToLower().Replace(" ", "") && minimumMastery == otherRiven.minimumMastery && rerollCount == otherRiven.rerollCount)
		{
			return weaponName.ToLower().Replace(" ", "") == otherRiven.weaponName?.ToLower().Replace(" ", "");
		}
		return false;
	}

	public void FillSimilarRivens(RivenSimilaritySource source = RivenSimilaritySource.WFMarket | RivenSimilaritySource.RivenMarket)
	{
		similarRivens.Clear();
		List<RivenSimilarityRequestAttribute> list = statsPerWeapon.First().byLevel.First().positiveTraits.Select((RivenUnveiledSingleStat p) => new RivenSimilarityRequestAttribute
		{
			positive = true,
			name = p.uniqueID,
			required = false
		}).ToList();
		list.AddRange(statsPerWeapon.First().byLevel.First().negativeTraits.Select((RivenUnveiledSingleStat p) => new RivenSimilarityRequestAttribute
		{
			positive = false,
			name = p.uniqueID,
			required = false
		}));
		similarRivens = RivenExplorerHelper.GetSimilarRivens(source, weaponReference, list, new RivenSimilarityRequestFilters());
	}

	public void FillGoodRolls()
	{
		goodRollData = RivenExplorerHelper.GetGoodRollData(weaponReference.uniqueName, statsPerWeapon.FirstOrDefault()?.byLevel.FirstOrDefault());
	}
}
