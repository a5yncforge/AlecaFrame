using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataSkin : BigItem
	{
		public bool excludeFromCodex { get; set; }

		public bool tradable { get; set; }

		public Hexcolour[] hexColours { get; set; }

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
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			return StaticData.dataHandler.warframeRootObject.WeaponSkins.Any((Weaponskin p) => p.ItemType == base.uniqueName);
		}
	}
}
