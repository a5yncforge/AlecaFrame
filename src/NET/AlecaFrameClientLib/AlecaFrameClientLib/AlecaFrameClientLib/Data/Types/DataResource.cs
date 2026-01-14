namespace AlecaFrameClientLib.Data.Types
{
	public class DataResource : BigItem
	{
		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int itemCount { get; set; }

		public string[] parents { get; set; }

		public int skipBuildTimePrice { get; set; }

		public bool tradable { get; set; }

		public bool excludeFromCodex { get; set; }

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
			return false;
		}
	}
}
