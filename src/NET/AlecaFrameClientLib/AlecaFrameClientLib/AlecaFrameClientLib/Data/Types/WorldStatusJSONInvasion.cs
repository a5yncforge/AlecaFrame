using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONInvasion
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public string node { get; set; }

		public string nodeKey { get; set; }

		public string desc { get; set; }

		public WorldStatusJSONAttackerreward attackerReward { get; set; }

		public string attackingFaction { get; set; }

		public WorldStatusJSONAttacker attacker { get; set; }

		public WorldStatusJSONDefenderreward defenderReward { get; set; }

		public string defendingFaction { get; set; }

		public WorldStatusJSONDefender defender { get; set; }

		public bool vsInfestation { get; set; }

		public int count { get; set; }

		public int requiredRuns { get; set; }

		public float completion { get; set; }

		public bool completed { get; set; }

		public string eta { get; set; }

		public string[] rewardTypes { get; set; }
	}
}
