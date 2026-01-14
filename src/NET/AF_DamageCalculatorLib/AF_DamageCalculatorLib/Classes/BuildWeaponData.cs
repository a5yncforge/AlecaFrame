using System.Collections.Generic;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.Classes
{
	public class BuildWeaponData
	{
		public class WeaponUserMode
		{
			public string name;

			public int modeID;
		}

		public class WeaponMode
		{
			public class SpinUpData
			{
				public int maxShots;

				public double minAttackSpeed;

				public double maxMultishot;

				public double timeToDown;

				public double maxAttackSpeed;
			}

			public enum FireRateMode
			{
				Default,
				Fixed
			}

			public enum FireMode
			{
				Unknown,
				Beam,
				Projectile,
				Melee,
				HitScan,
				MeleeRadius,
				HitScanAndProjectileOnMiss,
				GunBlade,
				Ranged,
				HitScanDampenedSpread,
				Glaive,
				RevenantKatana,
				Basic,
				TentableOnKill,
				Continuous,
				CustomMesa,
				HomingGlaive
			}

			public enum ImpactMode
			{
				Unknown,
				Beam,
				Projectile,
				Melee,
				ProjectileOnMiss,
				Sniper,
				PyranaPrime,
				GunBlade,
				CustomBattacor,
				CustomOccucor,
				Continuous
			}

			public enum StateMode
			{
				Unknown,
				Beam,
				SemiAuto,
				MeleeCombo,
				Auto,
				EmptyMagazineFast,
				MeleeRadius,
				Burst,
				LockOnBurst,
				AutoShotgun,
				RemoteMine,
				Shotgun,
				Charged,
				GunBlade,
				BeamBurst,
				Basic,
				CreateEnemyCopiesOnKills,
				ChargeBeam,
				GlaiveGrenade,
				Glaive,
				Melee,
				Grenade,
				AutoBurst,
				HomingBeacon,
				SpearGun,
				ChargedRemoteMine,
				IceHammer,
				GlaiveAsAlternateFire,
				FireAndToggle,
				CustomBattacor,
				Continuous,
				CustomMesa
			}

			public class DamageData
			{
				public Dictionary<DamageType, double> damages;

				public double statusChance;

				public double fallofAt;

				public double fallofLoss;

				public double fallofStartAt;
			}

			public bool ignoreMultishot;

			public double baseMultishot;

			public bool useAmmo;

			public bool useWholeClip;

			public FireMode fireMode;

			public ImpactMode impactMode;

			public StateMode stateMode;

			public DamageData directDamage;

			public DamageData aoeAfterDirect;

			public FireRateMode fireRateMode;

			public double fireRate;

			public double forcedMultiTrigger;

			public double punchThrough;

			public double reloadTime;

			public double critMultiplier;

			public double critChance;

			public double ammoDivider;

			public bool scaleAmmoWithMS;

			public SpinUpData spinUp;
		}

		public enum WeaponType
		{
			Melee,
			Secondary,
			Primary,
			ArchGun,
			ArchMelee,
			OperatorAmp,
			SentinelWeapon,
			SpecialItem
		}

		public enum CompatibilityTag
		{
			None,
			Projectile,
			SecondaryShotgun,
			SwordsStance,
			AssaultAmmo,
			AOE,
			HammersStance,
			TonfaStance,
			Deployable,
			SingleShot,
			Thrown,
			NikanasStance,
			HeavyBladeStance,
			ScythesStance,
			FirstStance,
			NunchakuStance,
			PolearmsStance,
			DaggersStance,
			SniperAmmo,
			WarfanStance,
			SwordsAndShieldStance,
			RapierStance,
			LongKatanaStance,
			DualDaggersStance,
			GlaivesStance,
			DualSwordsStance,
			MachetesStance,
			PowerWeapon,
			ClawsStance,
			Battery,
			WhipsStance,
			SparringStance,
			BladeAndWhipStance,
			StavesStance,
			FistStance,
			HeavyScytheStance,
			GunbladeStance,
			ImpactExplode,
			Crossbow,
			Beam,
			HoundWeapon,
			SentinelWeapon,
			DualKatanasStance,
			CRPBow,
			Miter,
			BladesawStance,
			Unknown,
			TNJETTurbinePistol,
			Attica,
			Zhuge,
			InfBow,
			Omicrus,
			Vectis,
			Daikyu,
			InfCernos,
			GrnBow
		}

		public string name;

		public string uniqueName;

		public int magazineCapacity;

		public int ammoPickUpCount;

		public string color;

		public CompatibilityTag compat;

		public WeaponType weaponType;

		public List<List<List<string>>> evolutions;

		public double meleeEmpoweredChance;

		public double meleeEmpoweredMeleeChancePerCombo;

		public double stealthDamageBonus;

		public BuildUpgradeData.ModPolarity[] polarities;

		public BuildUpgradeData.ModPolarity exilusPolarity;

		public BuildUpgradeData.ModPolarity stancePolarity;

		public List<WeaponMode> modes;

		public List<BuildUpgradeData.BuildModBuff> baseBuffs;

		public List<double> zoomLevels;

		public int ammoCapacity;

		public int[] parents;

		public List<WeaponUserMode> userModes;
	}
}
