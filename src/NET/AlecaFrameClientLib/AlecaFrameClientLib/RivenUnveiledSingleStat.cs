using System;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class RivenUnveiledSingleStat
{
	public string description;

	public double worstCase;

	public double bestCase;

	public double currentValue;

	public double rawRandomValue;

	public string bestCaseText;

	public string worstCaseText;

	public string grade;

	[JsonConverter(typeof(StringEnumConverter))]
	public AlecaFrameAttributeGrade rollGrade = AlecaFrameAttributeGrade.Unknown;

	[NonSerialized]
	public string noMarkupDescription;

	[NonSerialized]
	public string prefixSufixCombo;

	[NonSerialized]
	public string localizationString;

	[NonSerialized]
	public string uniqueID;

	[NonSerialized]
	public double currentValueBeingShownInUI;

	public RivenUnveiledSingleStat(string uniqueID, string localizationString, double currentValue, double worstCase, double bestCase, double rawRandomValue, string prefixSufixCombo)
	{
		this.uniqueID = uniqueID;
		this.worstCase = worstCase;
		this.bestCase = bestCase;
		this.currentValue = currentValue;
		this.localizationString = localizationString;
		this.rawRandomValue = rawRandomValue;
		this.prefixSufixCombo = prefixSufixCombo;
		string text;
		if (localizationString.Contains("Damage to"))
		{
			currentValueBeingShownInUI = Math.Round(currentValue + 1.0, 2);
			text = "x" + Math.Round(currentValue + 1.0, 2);
			bestCaseText = "x" + Math.Round(bestCase + 1.0, 2);
			worstCaseText = "x" + Math.Round(worstCase + 1.0, 2);
		}
		else
		{
			currentValueBeingShownInUI = Math.Round(currentValue, 1);
			text = Math.Round(currentValue, 1).ToString();
			if (currentValue > 0.0)
			{
				text = "+" + text;
			}
			bestCaseText = Math.Round(bestCase, 1).ToString();
			worstCaseText = Math.Round(worstCase, 1).ToString();
		}
		description = Misc.ReplaceStringWithIcons(localizationString.Replace("|val|", text).Replace("|STAT1|", text));
		noMarkupDescription = Misc.ReplaceStringWithNothing(localizationString.Replace("|val|", text).Replace("|STAT1|", text));
		grade = AFDBRivenDataPoint.GetAttrLetterGradeFromRandomPercent(rawRandomValue);
	}
}
