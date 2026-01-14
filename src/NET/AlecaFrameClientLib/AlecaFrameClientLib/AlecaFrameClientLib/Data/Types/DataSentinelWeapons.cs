using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataSentinelWeapons : BigItem
	{
		public AttackSentinelWeapon[] attacks { get; set; }

		public int blockingAngle { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float[] damagePerShot { get; set; }

		public bool excludeFromCodex { get; set; }

		public float fireRate { get; set; }

		public Introduced introduced { get; set; }

		public string[] polarities { get; set; }

		public float procChance { get; set; }

		public bool sentinel { get; set; }

		public int slot { get; set; }

		public string[] tags { get; set; }

		public float totalDamage { get; set; }

		public bool tradable { get; set; }

		public string wikiaThumbnail { get; set; }

		public float accuracy { get; set; }

		public object ammo { get; set; }

		public int magazineSize { get; set; }

		public int multishot { get; set; }

		public string noise { get; set; }

		public float reloadTime { get; set; }

		public string trigger { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int skipBuildTimePrice { get; set; }

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
			return StaticData.dataHandler.warframeRootObject.SentinelWeapons?.Any((Sentinelweapon p) => p.ItemType == base.uniqueName) ?? false;
		}
	}
}
