namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateGoal
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Node { get; set; }

		public string ScoreVar { get; set; }

		public string ScoreLocTag { get; set; }

		public int Count { get; set; }

		public float HealthPct { get; set; }

		public int[] Regions { get; set; }

		public string Desc { get; set; }

		public string ToolTip { get; set; }

		public bool OptionalInMission { get; set; }

		public string Tag { get; set; }

		public MiscItemItemId[] UpgradeIds { get; set; }

		public bool Personal { get; set; }

		public bool Community { get; set; }

		public int Goal { get; set; }

		public Reward Reward { get; set; }

		public int[] InterimGoals { get; set; }

		public Interimreward[] InterimRewards { get; set; }
	}
}
