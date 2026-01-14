using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class StatusEffectHandler
	{
		private readonly EnemyInstance enemyInstance;

		private Dictionary<ProcType, List<SingleStatusEffect>> effects = new Dictionary<ProcType, List<SingleStatusEffect>>();

		private int timeUntilNextAOEElectricityProcMS;

		private int timeUntilNextAOEGasProcMS;

		private int timeSinceLastHeatPresenceChangeMS;

		private Dictionary<ProcType, int> StatusEffectBaseDurationsMS = new Dictionary<ProcType, int>
		{
			{
				ProcType.Impact,
				-1
			},
			{
				ProcType.Puncture,
				6000
			},
			{
				ProcType.Slash,
				6000
			},
			{
				ProcType.Heat,
				6000
			},
			{
				ProcType.Cold,
				6000
			},
			{
				ProcType.Electricity,
				6000
			},
			{
				ProcType.Poison,
				6000
			},
			{
				ProcType.Void,
				3000
			},
			{
				ProcType.Blast,
				6000
			},
			{
				ProcType.Corrosive,
				8000
			},
			{
				ProcType.Gas,
				6000
			},
			{
				ProcType.Magnetic,
				6000
			},
			{
				ProcType.Radiation,
				12000
			},
			{
				ProcType.Viral,
				6000
			}
		};

		private Dictionary<ProcType, int> StatusEffectStackMax = new Dictionary<ProcType, int>
		{
			{
				ProcType.Impact,
				10
			},
			{
				ProcType.Puncture,
				5
			},
			{
				ProcType.Slash,
				-1
			},
			{
				ProcType.Heat,
				-1
			},
			{
				ProcType.Cold,
				9
			},
			{
				ProcType.Electricity,
				-1
			},
			{
				ProcType.Poison,
				-1
			},
			{
				ProcType.Void,
				-1
			},
			{
				ProcType.Blast,
				10
			},
			{
				ProcType.Corrosive,
				10
			},
			{
				ProcType.Gas,
				10
			},
			{
				ProcType.Magnetic,
				10
			},
			{
				ProcType.Radiation,
				10
			},
			{
				ProcType.Viral,
				10
			}
		};

		private Dictionary<ProcType, bool> StatusEffectRefreshesAllStackDuration = new Dictionary<ProcType, bool>
		{
			{
				ProcType.Impact,
				false
			},
			{
				ProcType.Puncture,
				false
			},
			{
				ProcType.Slash,
				false
			},
			{
				ProcType.Heat,
				true
			},
			{
				ProcType.Cold,
				false
			},
			{
				ProcType.Electricity,
				false
			},
			{
				ProcType.Poison,
				false
			},
			{
				ProcType.Void,
				false
			},
			{
				ProcType.Blast,
				false
			},
			{
				ProcType.Corrosive,
				false
			},
			{
				ProcType.Gas,
				false
			},
			{
				ProcType.Magnetic,
				false
			},
			{
				ProcType.Radiation,
				false
			},
			{
				ProcType.Viral,
				false
			}
		};

		public List<double> HeatArmorMultiplierByHeatFAKEDelayedStack = new List<double> { 1.0, 0.85, 0.7, 0.6, 0.5 };

		public StatusEffectHandler(EnemyInstance weaponInstance)
		{
			enemyInstance = weaponInstance;
		}

		public void Reset()
		{
			effects.Clear();
		}

		public void Tick(int deltaMS)
		{
			if ((effects.GetOrDefault(ProcType.Electricity)?.Count ?? 0) > 0)
			{
				if (timeUntilNextAOEElectricityProcMS == 0)
				{
					double num = effects[ProcType.Electricity].Sum((SingleStatusEffect x) => x.typeDamage);
					enemyInstance.simulatorInstance.ApplyAOEDamage(DamageType.Electricity, 0.5 * num, 3.0, DamageSource.StatusEffect);
					timeUntilNextAOEElectricityProcMS = 1000;
				}
				timeUntilNextAOEElectricityProcMS -= deltaMS;
				if (timeUntilNextAOEElectricityProcMS < 0)
				{
					timeUntilNextAOEElectricityProcMS = 0;
				}
			}
			else
			{
				timeUntilNextAOEElectricityProcMS = 0;
			}
			if ((effects.GetOrDefault(ProcType.Gas)?.Count ?? 0) > 0)
			{
				if (timeUntilNextAOEGasProcMS == 0)
				{
					double num2 = effects[ProcType.Gas].Sum((SingleStatusEffect x) => x.baseTotalDamage);
					double radius = 3.0 + (double)effects[ProcType.Gas].Count * 0.3;
					enemyInstance.simulatorInstance.ApplyAOEDamage(DamageType.Gas, 0.5 * num2, radius, DamageSource.StatusEffect);
					timeUntilNextAOEGasProcMS = 1000;
				}
				timeUntilNextAOEGasProcMS -= deltaMS;
				if (timeUntilNextAOEGasProcMS < 0)
				{
					timeUntilNextAOEGasProcMS = 0;
				}
			}
			else
			{
				timeUntilNextAOEGasProcMS = 0;
			}
			foreach (List<SingleStatusEffect> item in effects.Values.ToList())
			{
				foreach (SingleStatusEffect item2 in item)
				{
					item2.Tick(deltaMS);
				}
			}
			foreach (List<SingleStatusEffect> item3 in effects.Values.ToList())
			{
				for (int num3 = 0; num3 < item3.Count; num3++)
				{
					SingleStatusEffect singleStatusEffect = item3[num3];
					if (singleStatusEffect.ShouldGetRemoved())
					{
						item3.RemoveAt(num3);
						num3--;
						if (singleStatusEffect.procType == ProcType.Heat && GetSingleStatusCount(ProcType.Heat) == 0)
						{
							timeSinceLastHeatPresenceChangeMS = 0;
						}
					}
				}
			}
		}

		public List<ProcType> ApplyStatusEffectsFromAttack(Dictionary<WeaponInstance.WeaponStat, StatWorkingData> weaponStats, EnemyInstance.AttackData attack, double statusDamageMultiplier)
		{
			List<ProcType> list = new List<ProcType>();
			int num = (int)Math.Floor(attack.statusChance);
			if (enemyInstance.simulatorInstance.GetRandomDouble() < attack.statusChance - (double)num)
			{
				num++;
			}
			for (int i = 0; i < num; i++)
			{
				List<DamageType> list2 = (from p in attack.weaponDamageByType
					where p.Value > 0.0
					select p.Key).ToList();
				double num2 = enemyInstance.simulatorInstance.GetRandomDouble() * (double)list2.Count;
				DamageType damageType = list2[(int)num2];
				ProcType procTypeFromDamageType = DamageTypeUtils.GetProcTypeFromDamageType(damageType);
				if (procTypeFromDamageType == ProcType.None)
				{
					throw new Exception("Invalid proc type");
				}
				int num3 = StatusEffectBaseDurationsMS[procTypeFromDamageType];
				if (num3 != -1)
				{
					num3 = (int)((double)num3 * weaponStats[WeaponInstance.WeaponStat.StatusTime].finalValue);
				}
				AddStatusEffect(procTypeFromDamageType, num3, attack.weaponDamageByType[damageType] * statusDamageMultiplier, attack.baseTotalDamage * statusDamageMultiplier);
				list.Add(procTypeFromDamageType);
			}
			return list;
		}

		private void AddStatusEffect(ProcType procType, int durationMS, double typeDamage, double baseTotalDamage)
		{
			if (!effects.ContainsKey(procType))
			{
				effects.Add(procType, new List<SingleStatusEffect>());
			}
			SingleStatusEffect item = new SingleStatusEffect(enemyInstance, procType, durationMS, typeDamage, baseTotalDamage);
			int num = StatusEffectStackMax[procType];
			if (num != -1 && effects[procType].Count >= num)
			{
				effects[procType].RemoveAt(0);
			}
			if (StatusEffectRefreshesAllStackDuration[procType])
			{
				foreach (SingleStatusEffect item2 in effects[procType])
				{
					item2.RefreshDuration();
				}
			}
			if (GetSingleStatusCount(ProcType.Heat) == 0 && procType == ProcType.Heat)
			{
				timeSinceLastHeatPresenceChangeMS = 0;
			}
			effects[procType].Add(item);
		}

		public int GetSingleStatusCount(ProcType procType)
		{
			return effects.GetOrDefault(procType)?.Count ?? 0;
		}

		public int GetDifferentStatusCount()
		{
			return effects.Count;
		}

		public List<ProcType> GetActiveStatusEffects()
		{
			return effects.Keys.ToList();
		}

		public double GetAdditivePunctureCriticalChance()
		{
			return (double)GetSingleStatusCount(ProcType.Puncture) * 0.05;
		}

		public double GetAdditiveColdCriticalMultiplier()
		{
			int singleStatusCount = GetSingleStatusCount(ProcType.Cold);
			if (singleStatusCount == 0)
			{
				return 0.0;
			}
			return 0.1 + (double)(singleStatusCount - 1) * 0.05;
		}

		public double GetCorrosiveArmorStripMultiplier()
		{
			int singleStatusCount = GetSingleStatusCount(ProcType.Corrosive);
			if (singleStatusCount == 0)
			{
				return 1.0;
			}
			enemyInstance.simulatorInstance.warframeInstance.IsWarframe(WarframeInstance.WarframeCheckEnum.Hydroid);
			double num = 0.26 + (double)(singleStatusCount - 1) * 0.06;
			return 1.0 - num;
		}

		public double GetHeatArmorStripMultiplier()
		{
			if (GetSingleStatusCount(ProcType.Heat) > 0)
			{
				int val = timeSinceLastHeatPresenceChangeMS / 500;
				val = Math.Min(val, HeatArmorMultiplierByHeatFAKEDelayedStack.Count - 1);
				return HeatArmorMultiplierByHeatFAKEDelayedStack[val];
			}
			int val2 = timeSinceLastHeatPresenceChangeMS / 1500;
			val2 = Math.Min(val2, HeatArmorMultiplierByHeatFAKEDelayedStack.Count - 1);
			return HeatArmorMultiplierByHeatFAKEDelayedStack[HeatArmorMultiplierByHeatFAKEDelayedStack.Count - 1 - val2];
		}

		public double GetViralHealthDamageMultiplier()
		{
			int singleStatusCount = GetSingleStatusCount(ProcType.Viral);
			if (singleStatusCount == 0)
			{
				return 1.0;
			}
			return 2.0 + (double)(singleStatusCount - 1) * 0.25;
		}

		public double GetMagneticShieldDamageMultiplier()
		{
			int singleStatusCount = GetSingleStatusCount(ProcType.Magnetic);
			if (singleStatusCount == 0)
			{
				return 1.0;
			}
			return 2.0 + (double)(singleStatusCount - 1) * 0.25;
		}
	}
}
