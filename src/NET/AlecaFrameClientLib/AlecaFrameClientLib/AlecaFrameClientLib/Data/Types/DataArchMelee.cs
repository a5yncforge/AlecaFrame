using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataArchMelee : BigItem
	{
		public AttackArchMelee[] attacks { get; set; }

		public int blockingAngle { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public int comboDuration { get; set; }

		public bool consumeOnBuild { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float[] damagePerShot { get; set; }

		public float fireRate { get; set; }

		public float followThrough { get; set; }

		public int heavyAttackDamage { get; set; }

		public int heavySlamAttack { get; set; }

		public Introduced introduced { get; set; }

		public string[] polarities { get; set; }

		public float procChance { get; set; }

		public float range { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int slamAttack { get; set; }

		public int slamRadialDamage { get; set; }

		public int slamRadius { get; set; }

		public int slideAttack { get; set; }

		public int slot { get; set; }

		public string[] tags { get; set; }

		public float totalDamage { get; set; }

		public bool tradable { get; set; }

		public string wikiaThumbnail { get; set; }

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(450000);
		}

		public override int GetMasteryLevel(long XP)
		{
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: false, 450000.0);
		}

		public override int GetMaxMasteryLevel()
		{
			return Misc.GetMasteryLevelFromXP(450000.0, isWarframeOrSentinel: false, 450000.0);
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 100;
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			if (!StaticData.dataHandler.warframeRootObject.SpaceMelee.Any((Spacemelee p) => p.ItemType == base.uniqueName))
			{
				return StaticData.dataHandler.warframeRootObject.Melee.Any((Melee p) => p.ItemType == base.uniqueName);
			}
			return true;
		}
	}
}
