using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataSecondaryWeapon : BigItem
	{
		public float[] damagePerShot { get; set; }

		public float totalDamage { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float procChance { get; set; }

		public float fireRate { get; set; }

		public int slot { get; set; }

		public float accuracy { get; set; }

		public string noise { get; set; }

		public string trigger { get; set; }

		public int magazineSize { get; set; }

		public float reloadTime { get; set; }

		public int multishot { get; set; }

		public int buildPrice { get; set; }

		public int buildTime { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int buildQuantity { get; set; }

		public bool consumeOnBuild { get; set; }

		public bool tradable { get; set; }

		public int? ammo { get; set; }

		public object damage { get; set; }

		public Damagetypes damageTypes { get; set; }

		public object flight { get; set; }

		public string[] polarities { get; set; }

		public string projectile { get; set; }

		public string[] tags { get; set; }

		public string wikiaThumbnail { get; set; }

		public int itemCount { get; set; }

		public string[] parents { get; set; }

		public SecondaryMode secondary { get; set; }

		public SecondaryMode secondaryArea { get; set; }

		public float statusChance { get; set; }

		public int maxLevelCap { get; set; }

		public string[] ModularParts { get; set; }

		public WeaponAttack[] attacks { get; set; }

		public int MASTERY_XP_NEEDED()
		{
			int value = 30;
			DataHandler dataHandler = StaticData.dataHandler;
			bool? obj;
			if (dataHandler == null)
			{
				obj = null;
			}
			else
			{
				BasicRemoteData basicRemoteData = dataHandler.basicRemoteData;
				if (basicRemoteData == null)
				{
					obj = null;
				}
				else
				{
					Dictionary<string, int> maxLevelOverrides = basicRemoteData.maxLevelOverrides;
					obj = ((maxLevelOverrides != null) ? new bool?(!maxLevelOverrides.TryGetValue(base.uniqueName, out value)) : ((bool?)null));
				}
			}
			bool? flag = obj;
			if (flag == true)
			{
				value = 30;
			}
			return Misc.GetMasteryXPFromItemLevel(value, isWarframeOrSentinel: false);
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 100;
		}

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(MASTERY_XP_NEEDED());
		}

		public override int GetMasteryLevel(long XP)
		{
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: false, MASTERY_XP_NEEDED());
		}

		public override int GetMaxMasteryLevel()
		{
			return Misc.GetMasteryLevelFromXP(MASTERY_XP_NEEDED(), isWarframeOrSentinel: false, MASTERY_XP_NEEDED());
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			if (StaticData.dataHandler.warframeRootObject.Pistols.Any((Pistol p) => p.ItemType == base.uniqueName))
			{
				return true;
			}
			return false;
		}
	}
}
