using System;
using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsComponentDrop
	{
		public class RelicLevelDropPercentages
		{
			public string type;

			public string chance;
		}

		public enum DropType
		{
			Normal,
			Relic,
			Market
		}

		public string imageURL;

		public string dropPlace;

		public int ownedAmount;

		public string dropPercent;

		public DropType dropType;

		public string relicUID;

		[NonSerialized]
		public float rawDropChance;

		public List<RelicLevelDropPercentages> levels = new List<RelicLevelDropPercentages>();

		public bool vaulted;
	}
}
