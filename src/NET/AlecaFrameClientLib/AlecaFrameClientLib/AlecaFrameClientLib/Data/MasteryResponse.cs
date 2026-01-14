using System;
using System.Collections.Generic;

namespace AlecaFrameClientLib.Data
{
	public class MasteryResponse
	{
		public class MasteryResponseSummary
		{
			public MasteryResponseCategory cont_warframes;

			public MasteryResponseCategory cont_weapons;

			public MasteryResponseCategory cont_companions;

			public MasteryResponseCategory cont_other;

			public MasteryResponseCategory star_normal;

			public MasteryResponseCategory star_steel;

			public MasteryResponseCategory star_junctions;

			public MasteryResponseCategory star_steel_junctions;

			public MasteryResponseCategory intrinsic_rail;

			public MasteryResponseCategory intrinsic_duviri;

			public float contentPercent;

			public float starPercent;

			public float intrinsicPercent;
		}

		public class MasteryResponseCategory
		{
			public float percent;

			public int current;

			public int max;

			public MasteryResponseCategory(int current, int max)
			{
				percent = (float)Math.Round(100f * ((float)current / (float)max));
				this.current = current;
				this.max = max;
			}
		}

		public class MasteryResponseContentRemainingData
		{
			public int potentialXPToGet;

			[NonSerialized]
			public List<(OverwolfWrapper.ItemPriceSmallResponse price, int amountNeeded)> neccessaryComponentPrices = new List<(OverwolfWrapper.ItemPriceSmallResponse, int)>();

			[NonSerialized]
			public bool canBeBoughtWithPlatinum;

			public int platinumNeededToBuyParts;

			public int remainingPartsCount;

			public float relicProbability;

			public int currentLevel;

			public int maxLevel;
		}

		public string iconurl;

		public int lvl;

		public float percent;

		public long currentLevelXp;

		public long nextLevelXp;

		public int summaryPlatinum;

		public MasteryResponseSummary summary;

		public List<FoundryItemData> topItems;
	}
}
