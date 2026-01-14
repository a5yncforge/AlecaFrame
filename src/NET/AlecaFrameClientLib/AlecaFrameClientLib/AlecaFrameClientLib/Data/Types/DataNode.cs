using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataNode : BigItem
	{
		public int factionIndex { get; set; }

		public bool masterable { get; set; }

		public int maxEnemyLevel { get; set; }

		public int minEnemyLevel { get; set; }

		public int missionIndex { get; set; }

		public int nodeType { get; set; }

		public int systemIndex { get; set; }

		public string systemName { get; set; }

		public bool tradable { get; set; }

		public Patchlog[] patchlogs { get; set; }

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 0;
		}

		public int GetCompletionXP()
		{
			return StaticData.dataHandler.basicRemoteData?.nodeXP.GetOrDefault(base.uniqueName) ?? 0;
		}

		public override int GetMasteryLevel(long XP)
		{
			return 0;
		}

		public override int GetMaxMasteryLevel()
		{
			return 0;
		}

		public override bool IsFullyMastered()
		{
			return false;
		}

		public override bool IsOwned()
		{
			return false;
		}
	}
}
