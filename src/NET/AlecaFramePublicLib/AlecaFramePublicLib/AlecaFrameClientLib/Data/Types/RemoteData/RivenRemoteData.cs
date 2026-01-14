using System;
using System.Collections.Generic;
using System.Linq;

namespace AlecaFrameClientLib.Data.Types.RemoteData
{
	public class RivenRemoteData
	{
		public List<MultipliersBasedOnGoodBadModifiers> modifiersBasedOnTraitCount = new List<MultipliersBasedOnGoodBadModifiers>();

		public Dictionary<string, DataRivenStats> dataByRivenInternalID = new Dictionary<string, DataRivenStats>();

		public Dictionary<string, RivenWeaponData> weaponStats = new Dictionary<string, RivenWeaponData>();

		public string attributions = "'Good rolls' data is based on the work done by 44bananas#9024 and Xennethkeisere#0717 (https://docs.google.com/spreadsheets/d/1zbaeJBuBn44cbVKzJins_E3hTDpnmvOk8heYN-G8yy8)";

		public static bool GetRivenAttrsFromNameWithWeaponName(RivenRemoteData rivenRemoteData, string weaponName, string rivenName, out List<string> attrTags)
		{
			attrTags = new List<string>();
			string text = rivenRemoteData.weaponStats.Values.FirstOrDefault((RivenWeaponData p) => p.name.ToLower() == weaponName.ToLower())?.rivenUID;
			if (text == null)
			{
				return GetRivenAttrsFromNameWithNoWeaponInfo(rivenRemoteData, rivenName, out attrTags);
			}
			return GetRivenAttrsFromNameWithRivenUID(rivenRemoteData, rivenName, text, out attrTags);
		}

		public static bool GetRivenAttrsFromNameWithRivenUID(RivenRemoteData rivenRemoteData, string rivenName, string rivenUID, out List<string> attrTags)
		{
			attrTags = new List<string>();
			try
			{
				rivenName = rivenName.ToLower();
				string prefix = (Enumerable.Contains(rivenName, '-') ? rivenName.Split(new char[1] { '-' })[0] : "");
				string text = (Enumerable.Contains(rivenName, '-') ? rivenName.Split(new char[1] { '-' })[1] : rivenName);
				string suffix;
				string core;
				for (int i = 2; i <= 4; i++)
				{
					suffix = text.Substring(text.Length - i);
					core = text.Substring(0, text.Length - suffix.Length);
					string text2 = rivenRemoteData.dataByRivenInternalID[rivenUID].rivenStats.Values.FirstOrDefault((DataRivenStatsModifier u) => u.prefixTag == prefix && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))?.modifierTag;
					string text3 = rivenRemoteData.dataByRivenInternalID[rivenUID].rivenStats.Values.FirstOrDefault((DataRivenStatsModifier u) => u.prefixTag == core && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))?.modifierTag;
					string text4 = rivenRemoteData.dataByRivenInternalID[rivenUID].rivenStats.Values.FirstOrDefault((DataRivenStatsModifier u) => u.suffixTag == suffix && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))?.modifierTag;
					if (text3 != null && text4 != null && (text2 != null || !rivenName.Contains("-")))
					{
						if (text2 != null)
						{
							attrTags.Add(text2);
						}
						attrTags.Add(text3);
						attrTags.Add(text4);
						return true;
					}
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		[Obsolete("This method is not accurate, please use the other versions where you can provide the weapon name or, even better, the riven UID")]
		public static bool GetRivenAttrsFromNameWithNoWeaponInfo(RivenRemoteData rivenRemoteData, string rivenName, out List<string> attrTags)
		{
			attrTags = new List<string>();
			try
			{
				rivenName = rivenName.ToLower();
				string prefix = (Enumerable.Contains(rivenName, '-') ? rivenName.Split(new char[1] { '-' })[0] : "");
				string text = (Enumerable.Contains(rivenName, '-') ? rivenName.Split(new char[1] { '-' })[1] : rivenName);
				string suffix;
				string core;
				for (int i = 2; i <= 4; i++)
				{
					suffix = text.Substring(text.Length - i);
					core = text.Substring(0, text.Length - suffix.Length);
					string text2 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.prefixTag == prefix)).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.modifierTag;
					string text3 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.prefixTag == core)).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.modifierTag;
					string text4 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.suffixTag == suffix)).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.modifierTag;
					if (text3 != null && text4 != null && (text2 != null || !rivenName.Contains("-")))
					{
						if (text2 != null)
						{
							attrTags.Add(text2);
						}
						attrTags.Add(text3);
						attrTags.Add(text4);
						return true;
					}
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool GetNameFromAttributes(RivenRemoteData rivenRemoteData, string[] attrIDs, out string name)
		{
			if (attrIDs.Length == 3)
			{
				string text = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.modifierTag == attrIDs[0] && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.prefixTag;
				string text2 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.modifierTag == attrIDs[1] && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.prefixTag;
				string text3 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.modifierTag == attrIDs[2] && !string.IsNullOrEmpty(u.prefixTag) && !string.IsNullOrEmpty(u.suffixTag))).FirstOrDefault((DataRivenStatsModifier p) => p != null && !string.IsNullOrEmpty(p.prefixTag) && !string.IsNullOrEmpty(p.suffixTag))?.suffixTag;
				if (text == null || text2 == null || text3 == null)
				{
					name = null;
					return false;
				}
				name = text + "-" + text2 + text3;
				return true;
			}
			string text4 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.modifierTag == attrIDs[0])).FirstOrDefault((DataRivenStatsModifier p) => p != null)?.prefixTag;
			string text5 = rivenRemoteData.dataByRivenInternalID.Values.Select((DataRivenStats p) => p.rivenStats?.Values.FirstOrDefault((DataRivenStatsModifier u) => u.modifierTag == attrIDs[1])).FirstOrDefault((DataRivenStatsModifier p) => p != null)?.suffixTag;
			if (text4 == null || text5 == null)
			{
				name = null;
				return false;
			}
			name = text4 + text5;
			return true;
		}

		[Obsolete("This method is not accurate, please use the other versions where you can provide the weapon name or, even better, the riven UID")]
		public static bool TryGetAttributeTagFromPrefixAndSuffix(RivenRemoteData rivenRemoteData, string prefix, string suffix, out string attrTag)
		{
			attrTag = null;
			prefix = prefix.ToLower();
			suffix = suffix.ToLower();
			foreach (DataRivenStats item in rivenRemoteData.dataByRivenInternalID.Values.Where((DataRivenStats p) => p.rivenStats != null))
			{
				foreach (KeyValuePair<string, DataRivenStatsModifier> rivenStat in item.rivenStats)
				{
					if (rivenStat.Value.prefixTag == prefix && rivenStat.Value.suffixTag == suffix)
					{
						attrTag = rivenStat.Value.modifierTag;
						return true;
					}
				}
			}
			return false;
		}

		public static bool TryGetAttributeTagFromPrefixAndSuffixWithWeaponName(RivenRemoteData rivenRemoteData, string weaponName, string prefix, string suffix, out string attrTag)
		{
			string text = rivenRemoteData.weaponStats.Values.FirstOrDefault((RivenWeaponData p) => p.name.ToLower().Replace(" prime", "") == weaponName.ToLower().Replace(" prime", ""))?.rivenUID;
			if (text == null)
			{
				return TryGetAttributeTagFromPrefixAndSuffix(rivenRemoteData, prefix, suffix, out attrTag);
			}
			return TryGetAttributeTagFromPrefixAndSuffixWithRivenUID(rivenRemoteData, text, prefix, suffix, out attrTag);
		}

		public static bool TryGetAttributeTagFromPrefixAndSuffixWithRivenUID(RivenRemoteData rivenRemoteData, string rivenUID, string prefix, string suffix, out string attrTag)
		{
			attrTag = null;
			prefix = prefix.ToLower();
			suffix = suffix.ToLower();
			if (!rivenRemoteData.dataByRivenInternalID.ContainsKey(rivenUID))
			{
				return false;
			}
			foreach (KeyValuePair<string, DataRivenStatsModifier> rivenStat in rivenRemoteData.dataByRivenInternalID[rivenUID].rivenStats)
			{
				if (rivenStat.Value.prefixTag == prefix && rivenStat.Value.suffixTag == suffix)
				{
					attrTag = rivenStat.Value.modifierTag;
					return true;
				}
			}
			return false;
		}
	}
}
