using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AF_DamageCalculatorLib.Classes;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class EnemyInstance
	{
		public class AttackData
		{
			public Dictionary<DamageType, double> weaponDamageByType;

			public bool criticalHit;

			public double critMultiplier = 1.0;

			public bool weakspotHit;

			public double baseTotalDamage = -1.0;

			public double statusChance;

			public int critTier;
		}

		public class AttackMitigationLoggingData
		{
			public class DataPoint
			{
				public int preMitigationDamage;

				public int postMitigationDamage;

				public double mitigationPercentage => 100.0 * ((double)(postMitigationDamage - preMitigationDamage) / (double)preMitigationDamage);
			}

			public Dictionary<DamageType, DataPoint> byDamageType = new Dictionary<DamageType, DataPoint>();

			public AttackMitigationLoggingData()
			{
			}

			public AttackMitigationLoggingData(DamageType damageType, double damageAmount, double damageToDeal)
			{
				byDamageType.Add(damageType, new DataPoint
				{
					preMitigationDamage = (int)damageAmount,
					postMitigationDamage = (int)damageToDeal
				});
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<DamageType, DataPoint> item in byDamageType)
				{
					stringBuilder.AppendLine($"\t\t\t{item.Key}: {item.Value.preMitigationDamage} -> {item.Value.postMitigationDamage} ({item.Value.mitigationPercentage}%)");
				}
				return stringBuilder.ToString();
			}
		}

		public DamageCalculatorInstance simulatorInstance;

		private BuildSourceDataFile sourceData;

		private EnemySetup.EnemyInfo enemyInSetup;

		public StatusEffectHandler statusEffectHandler;

		private BuildEnemyData enemyData;

		public int maxHealth;

		public int health;

		public int maxShield;

		public int shield;

		private double armorWithoutModifiers;

		public long enemyFirstDamageTick = -1L;

		public long enemyDeathTick = -1L;

		private int shieldGateMSremaining;

		public EnemyInstance(BuildSourceDataFile sourceData, DamageCalculatorInstance simulatorInstance, EnemySetup.EnemyInfo enemyInSetup)
		{
			this.sourceData = sourceData;
			this.enemyInSetup = enemyInSetup;
			this.simulatorInstance = simulatorInstance;
			statusEffectHandler = new StatusEffectHandler(this);
			enemyData = sourceData.enemies.GetOrDefault(enemyInSetup.uniqueName);
			if (enemyData == null)
			{
				throw new Exception("Enemy data not found");
			}
			SetEnemyLevelStats();
			Reset();
		}

		public void SetEnemyLevelStats()
		{
			maxHealth = (int)EnemyUtils.GetEnemyHealth(enemyData.health, enemyInSetup.level, enemyData.baseLevel, enemyInSetup.steelPath, enemyData.eximus);
			if (health > maxHealth)
			{
				health = maxHealth;
			}
			maxShield = (int)EnemyUtils.GetEnemyShield(enemyData.shield, enemyInSetup.level, enemyData.baseLevel, enemyInSetup.steelPath, enemyData.eximus);
			if (shield > maxShield)
			{
				shield = maxShield;
			}
			armorWithoutModifiers = EnemyUtils.GetEnemyArmor(enemyData.armor, enemyInSetup.level, enemyData.baseLevel, enemyInSetup.steelPath, enemyData.eximus);
		}

		public void Reset()
		{
			health = maxHealth;
			shield = maxShield;
			shieldGateMSremaining = 0;
			statusEffectHandler.Reset();
			enemyFirstDamageTick = -1L;
			enemyDeathTick = -1L;
		}

		public void ApplyWeaponAttack(Dictionary<WeaponInstance.WeaponStat, StatWorkingData> weaponStats, AttackData attack, DamageSource damageSource)
		{
			if (health == 0)
			{
				return;
			}
			double num = 0.0;
			double num2 = weaponStats.GetOrDefault(EnemyUtils.GetFactionModifierFromFactionName(enemyData.group))?.finalValue ?? 1.0;
			double num3 = ((!attack.weakspotHit) ? 1 : enemyData.weakspotCoeff);
			if (attack.criticalHit && attack.weakspotHit)
			{
				num3 *= 2.0;
			}
			double num4 = num3 * attack.critMultiplier * num2;
			double num5 = attack.baseTotalDamage;
			if (num5 == -1.0)
			{
				num5 = attack.weaponDamageByType.Sum((KeyValuePair<DamageType, double> x) => x.Value);
			}
			double num6 = num5 / 16.0;
			AttackMitigationLoggingData attackMitigationLoggingData = new AttackMitigationLoggingData();
			foreach (KeyValuePair<DamageType, double> item in attack.weaponDamageByType)
			{
				int num7 = (int)Math.Round(item.Value / num6);
				if (num7 > 0)
				{
					double num8 = (double)num7 * num6;
					num8 *= num4;
					double num9 = GetFinalDamageAmount(item.Key, Math.Round(num8));
					if (num9 < 1.0)
					{
						num9 = 1.0;
					}
					num += num9;
					attackMitigationLoggingData.byDamageType.Add(item.Key, new AttackMitigationLoggingData.DataPoint
					{
						preMitigationDamage = (int)item.Value,
						postMitigationDamage = (int)num9
					});
				}
			}
			num = Math.Round(num);
			double statusDamageMultiplier = num4 * num2;
			List<ProcType> statusEffectsAdded = statusEffectHandler.ApplyStatusEffectsFromAttack(weaponStats, attack, statusDamageMultiplier);
			ApplyDamageInternal(num, statusEffectsAdded, attack.critTier, attack.weakspotHit, out var _, out var _, attackMitigationLoggingData, damageSource);
		}

		private void ApplyDamageInternal(double totalDamageToApply, List<ProcType> statusEffectsAdded, int critTier, bool isWeakspotHit, out int damageDoneToHealth, out int damageDoneToShields, AttackMitigationLoggingData mitigationLoggingData, DamageSource damageSource)
		{
			damageDoneToShields = 0;
			damageDoneToHealth = 0;
			if (shield > 0)
			{
				if ((double)shield > totalDamageToApply)
				{
					shield -= (int)totalDamageToApply;
					damageDoneToShields += (int)totalDamageToApply;
				}
				else
				{
					damageDoneToShields += shield;
					int num = (int)totalDamageToApply - shield;
					shield = 0;
					if (shield <= 0 && !isWeakspotHit)
					{
						shieldGateMSremaining = 100;
					}
					double num2 = ((shieldGateMSremaining > 0) ? 0.05000000074505806 : 1.0);
					num = (int)((double)num * num2);
					damageDoneToHealth += num;
				}
			}
			else
			{
				health -= (int)totalDamageToApply;
				damageDoneToHealth += (int)totalDamageToApply;
				if (health < 0)
				{
					health = 0;
				}
			}
			if (enemyFirstDamageTick == -1)
			{
				enemyFirstDamageTick = simulatorInstance.currentTick;
			}
			DamageCalculatorEnemyHitEventData eventData = new DamageCalculatorEnemyHitEventData
			{
				shieldDamage = damageDoneToShields,
				healthDamage = damageDoneToHealth,
				critTier = critTier,
				statusEffectsAdded = statusEffectsAdded,
				damageSource = damageSource,
				mitigationLoggingData = mitigationLoggingData
			};
			simulatorInstance.RaiseEvent(DamageCalculatorEvent.EnemyHit, eventData);
			if (IsDead())
			{
				enemyDeathTick = simulatorInstance.currentTick;
				statusEffectHandler.Reset();
				simulatorInstance.SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions.Kill);
				if (isWeakspotHit)
				{
					simulatorInstance.SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions.HeadshotKill);
				}
				simulatorInstance.RaiseEvent(DamageCalculatorEvent.EnemyKilled, null);
			}
		}

		private double GetFinalDamageAmount(DamageType damageType, double damageAmount)
		{
			if (shield > 0 && !sourceData.shieldBypassDamageTypes.HasFlag(damageType))
			{
				double num = sourceData.damageTable[enemyData.shieldType][damageType];
				damageAmount *= num;
				double magneticShieldDamageMultiplier = statusEffectHandler.GetMagneticShieldDamageMultiplier();
				damageAmount *= magneticShieldDamageMultiplier;
				return damageAmount;
			}
			double num2 = 1.0;
			double num3 = 1.0;
			if (GetCurrentArmor() > 0.0)
			{
				num2 = sourceData.damageTable[enemyData.armorType][damageType];
				num3 = EnemyUtils.GetArmorDamageMultiplier(GetCurrentArmor(), num2);
			}
			double num4 = sourceData.damageTable[enemyData.healthType][damageType];
			double num5 = ((shieldGateMSremaining > 0) ? 0.05000000074505806 : 1.0);
			double num6 = damageAmount * num3 * num2 * num4 * num5;
			double viralHealthDamageMultiplier = statusEffectHandler.GetViralHealthDamageMultiplier();
			return num6 * viralHealthDamageMultiplier;
		}

		public double GetCurrentArmor()
		{
			return armorWithoutModifiers * statusEffectHandler.GetHeatArmorStripMultiplier() * statusEffectHandler.GetCorrosiveArmorStripMultiplier();
		}

		public void Tick(int deltaTimeMS)
		{
			statusEffectHandler.Tick(deltaTimeMS);
			shieldGateMSremaining -= deltaTimeMS;
			if (shieldGateMSremaining < 0)
			{
				shieldGateMSremaining = 0;
			}
		}

		public bool IsDead()
		{
			return health <= 0;
		}

		public void TakeDamage(DamageType damageType, double damageAmount, DamageSource damageSource)
		{
			if (!IsDead())
			{
				double num = GetFinalDamageAmount(damageType, damageAmount);
				if (num < 1.0)
				{
					num = 1.0;
				}
				AttackMitigationLoggingData mitigationLoggingData = new AttackMitigationLoggingData(damageType, damageAmount, num);
				ApplyDamageInternal(num, new List<ProcType>(), 0, isWeakspotHit: false, out var _, out var _, mitigationLoggingData, damageSource);
			}
		}
	}
}
