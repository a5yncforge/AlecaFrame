using System;
using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataMod : BigItem
	{
		public int baseDrain { get; set; }

		public string compatName { get; set; }

		public int fusionLimit { get; set; }

		public Introduced introduced { get; set; }

		public bool isAugment { get; set; }

		public Levelstat[] levelStats { get; set; }

		public string polarity { get; set; }

		public string rarity { get; set; }

		public bool tradable { get; set; }

		public bool transmutable { get; set; }

		public string wikiaThumbnail { get; set; }

		public bool isUtility { get; set; }

		public string modSet { get; set; }

		public bool isExilus { get; set; }

		public bool excludeFromCodex { get; set; }

		public Availablechallenge[] availableChallenges { get; set; }

		public Upgradeentry[] upgradeEntries { get; set; }

		public float[] modSetValues { get; set; }

		public override bool IsFullyMastered()
		{
			return false;
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 0;
		}

		public override int GetMasteryLevel(long XP)
		{
			return 0;
		}

		public override int GetMaxMasteryLevel()
		{
			return 0;
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler?.warframeRootObject?.Upgrades?.Any((Upgrade p) => p.ItemType == base.uniqueName) != true)
			{
				return StaticData.dataHandler?.warframeRootObject?.RawUpgrades?.Any((Miscitem p) => p.ItemType == base.uniqueName) == true;
			}
			return true;
		}

		public static double GetModTypeEndoMultipler(string modType)
		{
			switch (modType)
			{
			case "bronze":
				return 1.0;
			case "silver":
				return 2.0;
			case "gold":
			case "riven":
				return 3.0;
			case "primed":
				return 4.0;
			default:
				return 0.0;
			}
		}

		public int GetMaxModLevel()
		{
			if (base.uniqueName.StartsWith("/Lotus/Upgrades/Mods/Railjack/"))
			{
				return fusionLimit;
			}
			int val = fusionLimit;
			Levelstat[] array = levelStats;
			return Math.Max(val, ((array == null) ? 1 : array.Length) - 1);
		}
	}
}
