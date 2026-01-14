using System;
using System.Collections.Generic;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AF_DamageCalculatorLib.Classes
{
	public class BuildUpgradeData
	{
		public class BuildModBuff
		{
			public class ModBuffCondition
			{
				public ModBufConditions condition;

				public ConditionFilter filter;

				public double value;
			}

			public enum ModBuffType
			{
				None,
				Unknown,
				AbilityRange,
				AbilityStrength,
				AbilityDuration,
				Armor,
				AbilityEfficiency,
				Health,
				StatusChance,
				ReloadSpeed,
				AmmoMaximum,
				Zoom,
				CriticalChance,
				CriticalDamage,
				Multishot,
				MagazineCapacity,
				PunchThrough,
				MeleeDamage,
				AttackSpeed,
				Heat,
				Cold,
				Toxin,
				Electricity,
				Slash,
				Impact,
				Viral,
				Radiation,
				Puncture,
				FinisherDamage,
				ParkourVelocity,
				Slide,
				SprintSpeed,
				AllDamages,
				WeaponRecoil,
				EnergyMax,
				ShieldCapacity,
				ShieldRecharge,
				Range,
				FireRate,
				ChargeRate,
				ComboDuration,
				InitialCombo,
				StatusChancePerComboMultiplier,
				ExplosionRadius,
				Accuracy,
				CastingSpeed,
				StatusDuration,
				ProjectileSpeed,
				DirectDamagePerStatusType,
				BulletJump,
				FireRateX2forBows,
				Hacking,
				Friction,
				LootRadar,
				EnemyRadar,
				HeavyAttackEfficiency,
				CriticalChanceX2forHeavyAttacks,
				FlightSpeed,
				ShieldRechargeDelay,
				ChanceToResistKnockdown,
				FasterKnockdownRecovery,
				DamageOnFirstShotInMagazine,
				FinalStatusChance,
				HeadshotMultiplier,
				Mobility,
				AdditionalComboCountChance,
				ComboCountChance,
				MeleeSlamDamage,
				MeleeDamageOnHeavyAttack,
				HealthPerHit,
				AimGlideAndWallLatchDuration,
				AmmoEfficiency,
				EnergyFilledInSpawn,
				MovementSpeed,
				MeleeRange,
				TauResistance,
				OverguardMax,
				HeadshotDamage,
				BodyshotDamage,
				DamageTaken,
				PercentBaseDamageOfStatus,
				FactionDamage,
				AbilityAugment,
				ExtraPickupAmmo,
				MeleeDamageToJumpKick,
				EnergyRegeneration,
				ExtraParkourDamage,
				DamageConversionToType,
				Regeneration,
				SyndicatePower,
				KnockdownResist,
				AmmoConversion,
				EnergyOnSelfHealthDamaged,
				ComboChanceWhenStatusDealsDamage,
				WeaponAugment,
				HeavyAttackWindUp,
				AuraStrength,
				ProjectileBounces,
				SniperComboDuration,
				InjuryBlockChance,
				DamageIfTargetHasStatus,
				PickupAmount,
				StatusDamage,
				MaxStatusStacks,
				PickupAmountHealthOnly,
				PickupAmountEnergyOnly,
				PickupAmountEnergyToHealthAndViceversa,
				AnotherStatusChanceOnStatus,
				DamageConversion,
				DamagePerStatusStackOfCertainType
			}

			public enum ModBuffOperation
			{
				StackingMultiply,
				Multiply,
				Add,
				Set,
				AddBase
			}

			public enum ModBufScalingType
			{
				Linear
			}

			public enum ModBufConditions
			{
				None,
				Kill,
				Hit,
				Headshot,
				HeadshotKill,
				ReloadFromEmpty,
				Reload,
				Equip,
				LowHealth,
				AbilityCast,
				MeleeKill,
				HeavyAttackHit,
				KillWithSecondaryWeapon,
				Roll,
				GroundSlam,
				WallLatchDuration,
				WallLatch,
				AfterWallLatch,
				HeadshotEximus,
				LandAfterSpecialJump,
				ColdProc,
				DamageToSelf,
				MultiHeadshot,
				MultiHit,
				FourHit,
				HealthDamageSelf,
				SixMeleeKills,
				MeleeChargeAttack,
				ComboTier,
				Lifted,
				GlaiveMeleeKill,
				HackSolved,
				HackStarted,
				ExecutionEnd,
				HealAbility,
				FreezeKillAbility,
				Unknown,
				Finisher,
				ShieldDamageToSelf,
				CriticalHit,
				StatusDamage,
				DealDamage,
				HealthPickup,
				EnergyPickup,
				ApplyStatus,
				ExactZoom
			}

			[Flags]
			public enum ConditionFilter
			{
				None = 0,
				Shotgun = 1,
				Pistol = 2,
				Rifle = 4,
				Sniper = 8,
				Bow = 0x10,
				Melee = 0x20,
				Archgun = 0x40,
				Aim = 0x80,
				InAir = 0x100,
				HeavyMelee = 0x200,
				Knockdown = 0x400,
				Parry = 0x800,
				BulletJump = 0x1000,
				Airborne = 0x2000,
				Dodge = 0x4000,
				EnemyHasHeat = 0x8000,
				FromToxinDamage = 0x10000,
				FromBlastDamage = 0x20000,
				Primary = 0x40000,
				Kitgun = 0x80000,
				FromHeatDamage = 0x100000,
				FromColdDamage = 0x200000
			}

			public ModBuffType type;

			public double value;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public bool isPercentageOfBase;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public ModBuffOperation operation;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public List<ModBuffCondition> conditions;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public double duration;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public DamageType damageType;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public Faction faction;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public bool script;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public bool durationScales;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public double stackChance;

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public int maxStacks;

			public ProcType statusList;

			public int roundTo;

			public string strictCompat;

			public bool dontDisplay;
		}

		public enum ModMaterial
		{
			Bronze,
			Silver,
			Gold,
			Primed,
			Riven,
			Archon,
			Peculiar,
			Amalgam,
			Galvanized,
			Requiem,
			Umbra,
			Tome,
			RailjackBronze,
			RailjackSilver,
			RailjackGold
		}

		public enum ModType
		{
			Aura,
			Stance,
			Warframe,
			Parazon,
			Melee,
			NotEquipable,
			Primary,
			Secondary,
			Kavat,
			Sentinel,
			Kubrow,
			ArchGun,
			ArchWing,
			ArchMelee,
			HelminthCharger,
			ArchonShardOrange,
			ArchonShardRedMythic,
			ArchonShardYellow,
			ArchonShardYellowMythic,
			ArchonShardBlue,
			ArchonShardBlueMythic,
			ArchonShardGreen,
			ArchonShardGreenMythic,
			ArchonShardOrangeMythic,
			ArchonShardPurple,
			ArchonShardPurpleMythic
		}

		public enum ModPolarity
		{
			Universal,
			Madurai,
			Vazarin,
			Naramon,
			Zenurik,
			Unairu,
			Penjaga,
			Umbra,
			Aura,
			Unchanged
		}

		public enum ParseStatus
		{
			Failed,
			Partial,
			Complete
		}

		public string uniqueName;

		public string name;

		public ModMaterial rarity;

		public ModType type;

		public string modCompat;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string compatUID;

		public ModPolarity modPolarity;

		public int baseDrain;

		public int maxLvl;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool conclaveOnly;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public List<BuildModBuff> buffs = new List<BuildModBuff>();

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public List<BuildModBuff> maxLevelBuffs;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public ParseStatus status;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string setUID;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public double[] setScalings;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool exilus;

		public int[] parents;
	}
}
