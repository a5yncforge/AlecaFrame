using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataArchGun : BigItem
	{
		public float accuracy { get; set; }

		public int? ammo { get; set; }

		public AttackArchGun[] attacks { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float[] damagePerShot { get; set; }

		public float fireRate { get; set; }

		public Introduced introduced { get; set; }

		public int magazineSize { get; set; }

		public int multishot { get; set; }

		public string noise { get; set; }

		public string[] polarities { get; set; }

		public float procChance { get; set; }

		public float reloadTime { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int slot { get; set; }

		public string[] tags { get; set; }

		public int totalDamage { get; set; }

		public bool tradable { get; set; }

		public string trigger { get; set; }

		public string wikiaThumbnail { get; set; }

		public int maxLevelCap { get; set; }

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

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(MASTERY_XP_NEEDED());
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 100;
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
			return StaticData.dataHandler.warframeRootObject.SpaceGuns.Any((Spacegun p) => p.ItemType == base.uniqueName);
		}
	}
}
