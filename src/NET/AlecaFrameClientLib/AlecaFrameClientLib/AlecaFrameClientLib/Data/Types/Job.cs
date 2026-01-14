namespace AlecaFrameClientLib.Data.Types
{
	public class Job
	{
		public string jobType { get; set; }

		public string rewards { get; set; }

		public int masteryReq { get; set; }

		public int minEnemyLevel { get; set; }

		public int maxEnemyLevel { get; set; }

		public int[] xpAmounts { get; set; }

		public bool endless { get; set; }

		public float bonusXpMultiplier { get; set; }

		public string locationTag { get; set; }

		public bool isVault { get; set; }
	}
}
