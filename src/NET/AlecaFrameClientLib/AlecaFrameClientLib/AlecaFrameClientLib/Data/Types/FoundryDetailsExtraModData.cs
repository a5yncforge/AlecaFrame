using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsExtraModData
	{
		public class TierData
		{
			public int endo;

			public int credits;

			public int levelEndo;

			public int levelCredits;

			public int level;

			public string benefits;
		}

		public string polarity;

		public string rarity;

		public string type;

		public List<TierData> tiers = new List<TierData>();

		public string costRange;

		public bool costIsGains;
	}
}
