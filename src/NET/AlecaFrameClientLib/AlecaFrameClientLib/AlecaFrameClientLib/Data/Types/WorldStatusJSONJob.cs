using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONJob
	{
		public string id { get; set; }

		public string[] rewardPool { get; set; }

		public string type { get; set; }

		public int[] enemyLevels { get; set; }

		public int[] standingStages { get; set; }

		public int minMR { get; set; }

		public DateTime expiry { get; set; }

		public string timeBound { get; set; }

		public bool isVault { get; set; }

		public string locationTag { get; set; }

		public string timeBoound { get; set; }
	}
}
