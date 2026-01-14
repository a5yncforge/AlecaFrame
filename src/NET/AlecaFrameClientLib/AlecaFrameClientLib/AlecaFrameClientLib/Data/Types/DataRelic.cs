using System.Collections.Generic;
using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataRelic : BigItem
	{
		public class DataRelicReward
		{
			public class DataRelicRewardItem
			{
				public string name;
			}

			public string rarity;

			public float chance;

			public DataRelicRewardItem item;
		}

		public class RelicDropData
		{
			public class RelicDropDataWithRarity
			{
				public float chance;

				public ItemRarity rarity;

				public ItemComponent item;
			}

			public enum ItemRarity
			{
				Common,
				Uncommon,
				Rare
			}

			public List<RelicDropDataWithRarity> chance = new List<RelicDropDataWithRarity>();
		}

		public enum RelicRarities
		{
			Intact,
			Exceptional,
			Flawless,
			Radiant
		}

		public Dictionary<RelicRarities, RelicDropData> relicRewards = new Dictionary<RelicRarities, RelicDropData>();

		public List<DataRelicReward> rewards = new List<DataRelicReward>();

		public bool tradable { get; set; }

		public override bool IsFullyMastered()
		{
			return false;
		}

		public override int GetMasteryLevel(long XP)
		{
			return 0;
		}

		public override int GetMaxMasteryLevel()
		{
			return 0;
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 0;
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			return StaticData.dataHandler.warframeRootObject.MiscItems.Any((Miscitem p) => p.ItemType == base.uniqueName);
		}
	}
}
