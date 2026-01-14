using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataQuest : BigItem
	{
		public bool tradable { get; set; }

		public Patchlog[] patchlogs { get; set; }

		public bool excludeFromCodex { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int skipBuildTimePrice { get; set; }

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
			return StaticData.dataHandler?.warframeRootObject?.QuestKeys?.Any((Questkey p) => p.ItemType == base.uniqueName) == true;
		}
	}
}
