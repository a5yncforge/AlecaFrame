using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataPet : BigItem
	{
		public int armor { get; set; }

		public int health { get; set; }

		public int power { get; set; }

		public int shield { get; set; }

		public int stamina { get; set; }

		public bool tradable { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int criticalChance { get; set; }

		public int criticalMultiplier { get; set; }

		public int[] damagePerShot { get; set; }

		public int fireRate { get; set; }

		public int procChance { get; set; }

		public int skipBuildTimePrice { get; set; }

		public int totalDamage { get; set; }

		public bool excludeFromCodex { get; set; }

		public override bool IsFullyMastered()
		{
			return isFullyMasteredInner(900000);
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			return 200;
		}

		public override int GetMasteryLevel(long XP)
		{
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: true, 900000.0);
		}

		public override int GetMaxMasteryLevel()
		{
			return Misc.GetMasteryLevelFromXP(900000.0, isWarframeOrSentinel: true, 900000.0);
		}

		public bool IsPet()
		{
			if (base.type.ToLower() == "warframe")
			{
				return false;
			}
			if (base.name.ToLower().Contains("antigen"))
			{
				return false;
			}
			if (base.name.ToLower().Contains("mutagen"))
			{
				return false;
			}
			if (base.name.ToLower().Contains("core"))
			{
				return false;
			}
			if (base.name.ToLower().Contains("gyro"))
			{
				return false;
			}
			if (base.name.ToLower().Contains("bracket"))
			{
				return false;
			}
			if (base.name.ToLower().Contains("stabilizer"))
			{
				return false;
			}
			if (base.uniqueName.Contains("/Deimos/WoundedInfested"))
			{
				return false;
			}
			return true;
		}

		public override bool IsOwned()
		{
			if (base.uniqueName == "/Lotus/Powersuits/Khora/Kavat/KhoraPrimeKavatPowerSuit")
			{
				return StaticData.dataHandler.warframeRootObject.Suits.Any((Suit p) => p.ItemType == "/Lotus/Powersuits/Khora/KhoraPrime");
			}
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			if (!StaticData.dataHandler.warframeRootObject.KubrowPets.Any((Kubrowpet p) => p.ItemType == base.uniqueName))
			{
				return StaticData.dataHandler.warframeRootObject.MoaPets.Any((Moapet p) => p.ItemType == base.uniqueName || p.ModularParts.Contains(base.uniqueName));
			}
			return true;
		}
	}
}
