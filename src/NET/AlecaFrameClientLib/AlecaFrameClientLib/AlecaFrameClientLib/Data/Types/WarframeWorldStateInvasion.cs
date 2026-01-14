namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateInvasion
	{
		public MiscItemItemId _id { get; set; }

		public string Faction { get; set; }

		public string DefenderFaction { get; set; }

		public string Node { get; set; }

		public int Count { get; set; }

		public int Goal { get; set; }

		public string LocTag { get; set; }

		public bool Completed { get; set; }

		public WarframeWorldStateDateChainid ChainID { get; set; }

		public object AttackerReward { get; set; }

		public WarframeWorldStateDateAttackermissioninfo AttackerMissionInfo { get; set; }

		public WarframeWorldStateDateDefenderreward DefenderReward { get; set; }

		public WarframeWorldStateDateDefendermissioninfo DefenderMissionInfo { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }
	}
}
