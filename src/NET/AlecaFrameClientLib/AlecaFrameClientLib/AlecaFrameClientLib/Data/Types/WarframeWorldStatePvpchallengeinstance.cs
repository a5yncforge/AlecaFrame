namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStatePvpchallengeinstance
	{
		public MiscItemItemId _id { get; set; }

		public string challengeTypeRefID { get; set; }

		public WarframeWorldStateDateInside startDate { get; set; }

		public WarframeWorldStateDateInside endDate { get; set; }

		public WarframeWorldStateDateParam[] _params { get; set; }

		public bool isGenerated { get; set; }

		public string PVPMode { get; set; }

		public MiscItemItemId[] subChallenges { get; set; }

		public string Category { get; set; }
	}
}
