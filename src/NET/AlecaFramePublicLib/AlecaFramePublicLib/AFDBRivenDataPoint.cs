using System;
using System.Collections.Generic;
using System.Linq;

namespace AlecaFramePublicLib
{
	public class AFDBRivenDataPoint
	{
		[NonSerialized]
		public RivenSimilaritySource source;

		[NonSerialized]
		private static readonly string[] rivenScaleGrade = new string[11]
		{
			"F", "C-", "C", "C+", "B-", "B", "B+", "A-", "A", "A+",
			"S"
		};

		[NonSerialized]
		private static readonly float rivenScaleStart = -11.5f;

		[NonSerialized]
		private static readonly float rivenScaleEnd = 11.5f;

		public DateTime ts { get; set; }

		public int price { get; set; }

		public string id { get; set; }

		public string ownerName { get; set; }

		public int masteryRank { get; set; }

		public int rerolls { get; set; }

		public string name { get; set; }

		public string polarity { get; set; }

		public AFDBRivenDataPointAttribute[] attrs { get; set; }

		public bool MatchesFilters(RivenSimilarityRequestFilters filters, int similarityPercent = int.MaxValue)
		{
			if (price < filters.minPrice)
			{
				return false;
			}
			if (price > filters.maxPrice)
			{
				return false;
			}
			if (similarityPercent < filters.minSimilarity)
			{
				return false;
			}
			if (rerolls > filters.maxRerolls)
			{
				return false;
			}
			if (rerolls < filters.minRerolls)
			{
				return false;
			}
			if (filters.negativeRequired && !attrs.Any((AFDBRivenDataPointAttribute p) => !p.positive))
			{
				return false;
			}
			return true;
		}

		public float ComputeSimilarity(AFDBRivenDataPoint rivenInDB, RivenSimilarityRequestAttribute[] requestRivenAttrs, out bool allMandatoryAttributesFound, out List<string> DBrivenMatchedList)
		{
			allMandatoryAttributesFound = true;
			float num = 1f / (float)Math.Max(requestRivenAttrs.Length, rivenInDB.attrs.Length);
			float num2 = 0f;
			DBrivenMatchedList = new List<string>();
			foreach (RivenSimilarityRequestAttribute lookupAttr in requestRivenAttrs)
			{
				AFDBRivenDataPointAttribute aFDBRivenDataPointAttribute = null;
				aFDBRivenDataPointAttribute = rivenInDB.attrs.FirstOrDefault((AFDBRivenDataPointAttribute p) => p.name == lookupAttr.name && p.positive == lookupAttr.positive);
				if (aFDBRivenDataPointAttribute != null)
				{
					num2 += num;
					DBrivenMatchedList.Add(aFDBRivenDataPointAttribute.name);
				}
				else if (lookupAttr.required)
				{
					allMandatoryAttributesFound = false;
					break;
				}
			}
			return num2;
		}

		public string GetRivenLinkFromSourceAndID(RivenSimilaritySource source)
		{
			switch (source)
			{
			case RivenSimilaritySource.WFMarket:
				return "https://warframe.market/auction/" + id;
			case RivenSimilaritySource.RivenMarket:
				return "https://riven.market/profile/" + ownerName;
			default:
				return "";
			}
		}

		public static string GetAttrLetterGradeFromRandomPercent(double randomPerOne)
		{
			double num = randomPerOne * 20.0 - 10.0;
			double num2 = rivenScaleStart;
			string result = "??";
			for (int i = 0; i < rivenScaleGrade.Length; i++)
			{
				num2 += 2.0;
				if (rivenScaleGrade[i] == "B")
				{
					num2 += 1.0;
				}
				if (num2 > num)
				{
					result = rivenScaleGrade[i];
					break;
				}
			}
			return result;
		}
	}
}
