using System.Linq;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data.Types
{
	public class DataMisc : BigItem
	{
		public bool tradable { get; set; }

		public bool excludeFromCodex { get; set; }

		public bool showInInventory { get; set; }

		public int itemCount { get; set; }

		public float probability { get; set; }

		public string rarity { get; set; }

		public string rewardName { get; set; }

		public int tier { get; set; }

		public int fusionPoints { get; set; }

		public string[] parents { get; set; }

		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float[] damagePerShot { get; set; }

		public float fireRate { get; set; }

		public float procChance { get; set; }

		public int skipBuildTimePrice { get; set; }

		public float totalDamage { get; set; }

		public float accuracy { get; set; }

		public object ammo { get; set; }

		public Attack[] attacks { get; set; }

		public Introduced introduced { get; set; }

		public int multishot { get; set; }

		public string noise { get; set; }

		public string[] polarities { get; set; }

		public float reloadTime { get; set; }

		public int slot { get; set; }

		public string[] tags { get; set; }

		public string trigger { get; set; }

		public string wikiaThumbnail { get; set; }

		public float primeOmegaAttenuation { get; set; }

		public int blockingAngle { get; set; }

		public int comboDuration { get; set; }

		public float followThrough { get; set; }

		public int heavyAttackDamage { get; set; }

		public int heavySlamAttack { get; set; }

		public int heavySlamRadialDamage { get; set; }

		public int heavySlamRadius { get; set; }

		public float range { get; set; }

		public int slamAttack { get; set; }

		public int slamRadialDamage { get; set; }

		public int slamRadius { get; set; }

		public int slideAttack { get; set; }

		public float windUp { get; set; }

		public int magazineSize { get; set; }

		public int binCapacity { get; set; }

		public int binCount { get; set; }

		public int[] capacityMultiplier { get; set; }

		public int durability { get; set; }

		public float fillRate { get; set; }

		public int repairRate { get; set; }

		public object[] specialities { get; set; }

		public override bool IsFullyMastered()
		{
			if (IsHoverboardComponent())
			{
				return isFullyMasteredInner(900000);
			}
			return isFullyMasteredInner(450000);
		}

		public override int GetAccountMasteryGivenPerLevel()
		{
			if (IsHoverboardComponent())
			{
				return 200;
			}
			return 100;
		}

		public override int GetMasteryLevel(long XP)
		{
			if (IsHoverboardComponent())
			{
				return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: true, 900000.0);
			}
			return Misc.GetMasteryLevelFromXP(XP, isWarframeOrSentinel: false, 450000.0);
		}

		public override int GetMaxMasteryLevel()
		{
			if (IsHoverboardComponent())
			{
				return Misc.GetMasteryLevelFromXP(900000.0, isWarframeOrSentinel: true, 900000.0);
			}
			return Misc.GetMasteryLevelFromXP(450000.0, isWarframeOrSentinel: false, 450000.0);
		}

		public override bool IsOwned()
		{
			if (StaticData.dataHandler.warframeRootObject == null)
			{
				return false;
			}
			if (base.uniqueName.Contains("Modular") && StaticData.dataHandler.warframeRootObject.Pistols.Any((Pistol p) => p.ModularParts?.Contains(base.uniqueName) ?? false))
			{
				return true;
			}
			if (base.uniqueName.Contains("Modular") && StaticData.dataHandler.warframeRootObject.LongGuns.Any((Longgun p) => p.ModularParts?.Contains(base.uniqueName) ?? false))
			{
				return true;
			}
			if (IsHoverboardComponent() && StaticData.dataHandler.warframeRootObject.Hoverboards.Any((Hoverboard p) => p.ModularParts?.Contains(base.uniqueName) ?? false))
			{
				return true;
			}
			if (IsAmp() && StaticData.dataHandler.warframeRootObject.OperatorAmps.Any((Operatoramp p) => p.ModularParts?.Contains(base.uniqueName) ?? false))
			{
				return true;
			}
			Shipdecoration[] shipDecorations = StaticData.dataHandler.warframeRootObject.ShipDecorations;
			if (shipDecorations != null && shipDecorations.Any((Shipdecoration p) => p.ItemType == base.uniqueName))
			{
				return true;
			}
			Miscitem[] flavourItems = StaticData.dataHandler.warframeRootObject.FlavourItems;
			if (flavourItems != null && flavourItems.Any((Miscitem p) => p.ItemType == base.uniqueName))
			{
				return true;
			}
			return false;
		}

		public bool IsModular()
		{
			if (!base.uniqueName.Contains("/Lotus/Weapons/SolarisUnited/") && !base.uniqueName.Contains("/Infested/Pistols/InfKitGun/"))
			{
				return base.uniqueName.Contains("HoverboardParts/PartComponents");
			}
			return true;
		}

		public bool IsHoverboardComponent()
		{
			return base.uniqueName.StartsWith("/Lotus/Types/Vehicles/Hoverboard/HoverboardParts/");
		}

		public bool IsAmp()
		{
			if (!base.uniqueName.StartsWith("/Lotus/Weapons/Sentients/OperatorAmplifiers/"))
			{
				return base.uniqueName.StartsWith("/Lotus/Weapons/Corpus/OperatorAmplifiers/");
			}
			return true;
		}
	}
}
