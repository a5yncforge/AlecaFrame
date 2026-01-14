using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataMeleeWeapon : BigItem
	{
		public Attack[] attacks { get; set; }

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

		public int heavySlamRadialDamage { get; set; }

		public int heavySlamRadius { get; set; }

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

		public string stancePolarity { get; set; }

		public string[] tags { get; set; }

		public float totalDamage { get; set; }

		public bool tradable { get; set; }

		public string wikiaThumbnail { get; set; }

		public float windUp { get; set; }

		public int itemCount { get; set; }

		public string[] parents { get; set; }

		public int maxLevelCap { get; set; }

		public string[] ModularParts { get; set; }

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
			if (StaticData.dataHandler.warframeRootObject.Melee.Any((Melee p) => p.ItemType == base.uniqueName))
			{
				return true;
			}
			if (base.uniqueName.Contains("Modular") && StaticData.dataHandler.warframeRootObject.Melee.Any((Melee p) => p.ModularParts?.Contains(base.uniqueName) ?? false))
			{
				return true;
			}
			return false;
		}

		public bool IsModular()
		{
			if (base.uniqueName.Contains("/Melee/Modular"))
			{
				return !base.uniqueName.Contains("PvPVariant");
			}
			return false;
		}

		public bool IsModularOrModularVariant()
		{
			return base.uniqueName.Contains("/Melee/Modular");
		}
	}
}
