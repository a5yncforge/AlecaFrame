using System;
using System.Collections.Generic;
using System.Linq;
using AF_DamageCalculatorLib.Classes;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class UpgradeInstance
	{
		private class BuffInternalStateData
		{
			public bool buffEnabledInItem;

			public double currentStacks;

			public int stackDurationMS;

			public int stacksTimeLeftMS = -1;

			public void Reset()
			{
				currentStacks = 0.0;
				stackDurationMS = 0;
				stacksTimeLeftMS = -1;
			}
		}

		public enum UpgradeType
		{
			Mod,
			ModBuff,
			Arcane,
			Shard,
			ModSet,
			WeaponBuff
		}

		private BuildSourceDataFile sourceDataFile;

		public UpgradeType upgradeType;

		private BuildUpgradeData upgradeData;

		private BuffInternalStateData[] buffInternalState;

		private int slotLevel;

		public UpgradeInstance(BuildSourceDataFile sourceDataFile, UpgradeType type, BaseBuild.UpgradeSlot config, int[] itemCompatibilityUIDIndices = null)
		{
			upgradeType = type;
			this.sourceDataFile = sourceDataFile;
			switch (type)
			{
			case UpgradeType.Mod:
				upgradeData = sourceDataFile.mods.GetOrDefault(config.uniqueName);
				if (upgradeData == null)
				{
					throw new Exception("Mod data not found");
				}
				break;
			case UpgradeType.Arcane:
				upgradeData = sourceDataFile.arcanes.GetOrDefault(config.uniqueName);
				if (upgradeData == null)
				{
					throw new Exception("Arcane data not found");
				}
				break;
			case UpgradeType.Shard:
				upgradeData = sourceDataFile.shards.GetOrDefault(config.uniqueName);
				if (upgradeData == null)
				{
					throw new Exception("Shard data not found");
				}
				break;
			case UpgradeType.ModSet:
				upgradeData = sourceDataFile.modSets.GetOrDefault(config.uniqueName);
				if (upgradeData == null)
				{
					throw new Exception("ModSet data not found");
				}
				break;
			default:
				throw new Exception("Invalid upgrade type");
			}
			slotLevel = config.level;
			if (slotLevel == -1)
			{
				slotLevel = upgradeData.maxLvl;
			}
			InitializeBuffsInternalState(itemCompatibilityUIDIndices);
		}

		public UpgradeInstance(BuildUpgradeData.BuildModBuff buff, int level = 0, int[] itemCompatibilityUIDIndices = null)
		{
			upgradeType = UpgradeType.ModBuff;
			slotLevel = level;
			upgradeData = new BuildUpgradeData();
			upgradeData.buffs.Add(buff);
			InitializeBuffsInternalState(itemCompatibilityUIDIndices);
		}

		public void InitializeBuffsInternalState(int[] itemCompatibilityUIDIndices)
		{
			buffInternalState = new BuffInternalStateData[upgradeData.buffs?.Count ?? 0];
			int i;
			for (i = 0; i < buffInternalState.Length; i++)
			{
				buffInternalState[i] = new BuffInternalStateData();
				buffInternalState[i].buffEnabledInItem = itemCompatibilityUIDIndices == null || string.IsNullOrEmpty(upgradeData.buffs[i].strictCompat) || itemCompatibilityUIDIndices.Any((int p) => sourceDataFile.parentList[p] == upgradeData.buffs[i].strictCompat);
			}
		}

		public void Reset()
		{
			for (int i = 0; i < buffInternalState.Length; i++)
			{
				buffInternalState[i].Reset();
			}
		}

		public bool IsModBuffConditionMet(BuildUpgradeData.BuildModBuff.ModBuffCondition modBuffCondition, DamageCalculatorInstance.TargetInfo targetInfo = null)
		{
			return true;
		}

		public void ConditionalEventHappened(BuildUpgradeData.BuildModBuff.ModBufConditions modBuffCondition, double durationMultiplier)
		{
			for (int i = 0; i < upgradeData.buffs.Count; i++)
			{
				if (!buffInternalState[i].buffEnabledInItem)
				{
					continue;
				}
				BuildUpgradeData.BuildModBuff buildModBuff = upgradeData.buffs[i];
				if (buildModBuff.maxStacks <= 0)
				{
					continue;
				}
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions = buildModBuff.conditions;
				if (conditions != null && !conditions.Any((BuildUpgradeData.BuildModBuff.ModBuffCondition p) => p.condition == modBuffCondition))
				{
					continue;
				}
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions2 = buildModBuff.conditions;
				if (conditions2 == null || conditions2.All((BuildUpgradeData.BuildModBuff.ModBuffCondition p) => IsModBuffConditionMet(p)))
				{
					buffInternalState[i].currentStacks += 1.0;
					double num = buildModBuff.duration;
					if (buildModBuff.durationScales)
					{
						num *= (double)(slotLevel + 1);
					}
					buffInternalState[i].stacksTimeLeftMS = (buffInternalState[i].stackDurationMS = (int)(num * durationMultiplier * 1000.0));
				}
			}
		}

		public void ApplyPassiveWarframeStats(Dictionary<WarframeInstance.WarframeStat, StatWorkingData> stats)
		{
			for (int i = 0; i < upgradeData.buffs.Count; i++)
			{
				BuildUpgradeData.BuildModBuff buildModBuff = upgradeData.buffs[i];
				if (!buffInternalState[i].buffEnabledInItem)
				{
					continue;
				}
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions = buildModBuff.conditions;
				if (conditions == null || conditions.All((BuildUpgradeData.BuildModBuff.ModBuffCondition p) => IsModBuffConditionMet(p)))
				{
					WarframeInstance.WarframeStat key = WarframeInstance.WarframeStat.Invalid;
					switch (buildModBuff.type)
					{
					case BuildUpgradeData.BuildModBuff.ModBuffType.Armor:
						key = WarframeInstance.WarframeStat.Armor;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.Health:
						key = WarframeInstance.WarframeStat.Health;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.ShieldCapacity:
						key = WarframeInstance.WarframeStat.Shield;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityStrength:
						key = WarframeInstance.WarframeStat.Strength;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityDuration:
						key = WarframeInstance.WarframeStat.Duration;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityEfficiency:
						key = WarframeInstance.WarframeStat.Efficiency;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityRange:
						key = WarframeInstance.WarframeStat.Range;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.Regeneration:
						key = WarframeInstance.WarframeStat.Regeneration;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.EnergyMax:
						key = WarframeInstance.WarframeStat.EnergyMax;
						break;
					case BuildUpgradeData.BuildModBuff.ModBuffType.None:
						continue;
					}
					_ = 10;
					stats[key].AddBuff(buildModBuff.operation, buildModBuff.value * (double)(slotLevel + 1), buildModBuff.isPercentageOfBase);
				}
			}
		}

		public void ApplyWeaponStats(Action<WeaponInstance.WeaponStat, BuildUpgradeData.BuildModBuff.ModBuffOperation, double, bool> applyStatCallback, Action<DamageType, BuildUpgradeData.BuildModBuff.ModBuffOperation, double, bool, int> addDamageCallback, DamageCalculatorInstance.TargetInfo targetInfo = null, int modIndex = 999999)
		{
			for (int i = 0; i < upgradeData.buffs.Count; i++)
			{
				BuildUpgradeData.BuildModBuff buildModBuff = upgradeData.buffs[i];
				if (!buffInternalState[i].buffEnabledInItem)
				{
					continue;
				}
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions = buildModBuff.conditions;
				if (conditions != null && !conditions.All((BuildUpgradeData.BuildModBuff.ModBuffCondition p) => IsModBuffConditionMet(p)))
				{
					continue;
				}
				double num = buildModBuff.value * (double)(slotLevel + 1);
				if (buildModBuff.maxStacks > 0)
				{
					num *= buffInternalState[i].currentStacks;
				}
				switch (buildModBuff.type)
				{
				case BuildUpgradeData.BuildModBuff.ModBuffType.Accuracy:
					applyStatCallback(WeaponInstance.WeaponStat.Accuracy, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.AmmoMaximum:
					applyStatCallback(WeaponInstance.WeaponStat.AmmoMax, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.ExtraPickupAmmo:
					applyStatCallback(WeaponInstance.WeaponStat.AmmoPickup, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.MagazineCapacity:
					applyStatCallback(WeaponInstance.WeaponStat.MagazineSize, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.FireRate:
					applyStatCallback(WeaponInstance.WeaponStat.FireRate, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.PunchThrough:
					applyStatCallback(WeaponInstance.WeaponStat.PunchThrough, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.ReloadSpeed:
					applyStatCallback(WeaponInstance.WeaponStat.ReloadTime, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.CriticalChance:
					applyStatCallback(WeaponInstance.WeaponStat.CriticalChance, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.CriticalDamage:
					applyStatCallback(WeaponInstance.WeaponStat.CriticalMultiplier, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.Multishot:
					applyStatCallback(WeaponInstance.WeaponStat.Multishot, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.StatusChance:
					applyStatCallback(WeaponInstance.WeaponStat.StatusChance, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.AllDamages:
					addDamageCallback(DamageType.AF_All, buildModBuff.operation, num, buildModBuff.isPercentageOfBase, modIndex);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.FactionDamage:
					switch (buildModBuff.faction)
					{
					case Faction.Corpus:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToCorpus, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					case Faction.Grineer:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToGrineer, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					case Faction.Infested:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToInfested, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					case Faction.Orokin:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToCorrupted, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					case Faction.Murmur:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToMurmur, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					case Faction.Sentient:
						applyStatCallback(WeaponInstance.WeaponStat.DamageToSentients, buildModBuff.operation, num, buildModBuff.isPercentageOfBase);
						break;
					}
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.PercentBaseDamageOfStatus:
					if (buildModBuff.damageType == DamageType.None)
					{
						throw new Exception("Damage type not set");
					}
					addDamageCallback(buildModBuff.damageType, buildModBuff.operation, num, buildModBuff.isPercentageOfBase, modIndex);
					break;
				case BuildUpgradeData.BuildModBuff.ModBuffType.DirectDamagePerStatusType:
				{
					int num2 = targetInfo?.enemy.statusEffectHandler.GetDifferentStatusCount() ?? 0;
					applyStatCallback(WeaponInstance.WeaponStat.StatusChance, buildModBuff.operation, num * (double)num2, buildModBuff.isPercentageOfBase);
					break;
				}
				}
			}
		}

		public void Tick(int deltaMS)
		{
			for (int i = 0; i < buffInternalState.Length; i++)
			{
				if (upgradeData.buffs[i].maxStacks <= 0 || buffInternalState[i].stacksTimeLeftMS == -1)
				{
					continue;
				}
				buffInternalState[i].stacksTimeLeftMS -= deltaMS;
				if (buffInternalState[i].stacksTimeLeftMS <= 0)
				{
					buffInternalState[i].currentStacks -= 1.0;
					if (buffInternalState[i].currentStacks < 0.0)
					{
						buffInternalState[i].currentStacks = 0.0;
						buffInternalState[i].stacksTimeLeftMS = -1;
					}
					else
					{
						buffInternalState[i].stacksTimeLeftMS = buffInternalState[i].stacksTimeLeftMS;
					}
				}
			}
		}
	}
}
