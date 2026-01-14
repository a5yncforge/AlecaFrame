namespace AlecaFrameClientLib.Data.Types
{
	public class DataFish : BigItem
	{
		public Patchlog[] patchlogs { get; set; }

		public bool tradable { get; set; }

		public bool excludeFromCodex { get; set; }

		public override bool IsFullyMastered()
		{
			return false;
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 0;
		}

		public override bool IsOwned()
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
	}
}
