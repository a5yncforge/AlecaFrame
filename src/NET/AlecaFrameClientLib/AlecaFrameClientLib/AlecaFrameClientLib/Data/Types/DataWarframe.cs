using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataWarframe : BigItem
	{
		public int health { get; set; }

		public int shield { get; set; }

		public int armor { get; set; }

		public int stamina { get; set; }

		public int power { get; set; }

		public float sprintSpeed { get; set; }

		public string passiveDescription { get; set; }

		public Ability[] abilities { get; set; }

		public int buildPrice { get; set; }

		public int buildTime { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int buildQuantity { get; set; }

		public bool consumeOnBuild { get; set; }

		public bool tradable { get; set; }

		public string[] aura { get; set; }

		public bool conclave { get; set; }

		public int color { get; set; }

		public string[] polarities { get; set; }

		public string sex { get; set; }

		public float sprint { get; set; }

		public string wikiaThumbnail { get; set; }

		public string[] exalted { get; set; }

		public Introduced introduced { get; set; }

		public int MASTERY_XP_NEEDED()
		{
			if (!base.uniqueName.Contains("EntratiMech"))
			{
				return 900000;
			}
			return 1600000;
		}

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(MASTERY_XP_NEEDED());
		}

		public override int GetMasteryLevel(long XP)
		{
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: true, MASTERY_XP_NEEDED());
		}

		public override int GetMaxMasteryLevel()
		{
			return Misc.GetMasteryLevelFromXP(MASTERY_XP_NEEDED(), isWarframeOrSentinel: true, MASTERY_XP_NEEDED());
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
			Suit[] suits = StaticData.dataHandler.warframeRootObject.Suits;
			if (suits == null || !suits.Any((Suit p) => p.ItemType == base.uniqueName))
			{
				return StaticData.dataHandler.warframeRootObject.MechSuits?.Any((Suit p) => p.ItemType == base.uniqueName) ?? false;
			}
			return true;
		}
	}
}
