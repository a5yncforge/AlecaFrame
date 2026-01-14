using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataSentinel : BigItem
	{
		public int armor { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int health { get; set; }

		public int power { get; set; }

		public int shield { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int stamina { get; set; }

		public bool tradable { get; set; }

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(900000);
		}

		public override int GetMasteryLevel(long XP)
		{
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: true, 900000.0);
		}

		public override int GetMaxMasteryLevel()
		{
			return Misc.GetMasteryLevelFromXP(900000.0, isWarframeOrSentinel: true, 900000.0);
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 200;
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			return StaticData.dataHandler.warframeRootObject.Sentinels.Any((Sentinel p) => p.ItemType == base.uniqueName);
		}
	}
}
