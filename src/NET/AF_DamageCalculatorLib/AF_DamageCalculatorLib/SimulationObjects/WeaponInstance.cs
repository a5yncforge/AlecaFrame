using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AF_DamageCalculatorLib.Classes;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class WeaponInstance
	{
		public enum WeaponStat
		{
			Accuracy,
			AmmoMax,
			AmmoPickup,
			MagazineSize,
			FalloffStart,
			FalloffEnd,
			FalloffDamageMultiplier,
			FireRate,
			PunchThrough,
			ReloadTime,
			CriticalChance,
			CriticalMultiplier,
			Multishot,
			StatusChance,
			DamageToCorpus,
			DamageToSentients,
			DamageToMurmur,
			DamageToCorrupted,
			DamageToInfested,
			DamageToGrineer,
			None,
			StatusTime
		}

		public class WeaponCurrentStats
		{
			public class DamageHandler
			{
				public class FalloffData
				{
					public double startDistance;

					public double endDistance;

					public double falloffAtEnd;

					public FalloffData(double startDistance, double endDistance, double falloffAtEnd)
					{
						this.startDistance = startDistance;
						this.endDistance = endDistance;
						this.falloffAtEnd = falloffAtEnd;
					}

					public double GetDamageMultiplier(double distance)
					{
						if (distance < startDistance)
						{
							return 1.0;
						}
						if (distance > endDistance)
						{
							return 1.0 - falloffAtEnd;
						}
						return 1.0 - (distance - startDistance) / (endDistance - startDistance) * falloffAtEnd;
					}
				}

				private WeaponInstance weaponInstance;

				public Dictionary<DamageType, StatWorkingData> damages = new Dictionary<DamageType, StatWorkingData>();

				public StatWorkingData statusChance = new StatWorkingData();

				public FalloffData falloffData;

				private Dictionary<DamageType, DamageType> DamageMergingPairs = new Dictionary<DamageType, DamageType>
				{
					{
						DamageType.Cold | DamageType.Heat,
						DamageType.Blast
					},
					{
						DamageType.Toxin | DamageType.Electricity,
						DamageType.Corrosive
					},
					{
						DamageType.Heat | DamageType.Toxin,
						DamageType.Gas
					},
					{
						DamageType.Cold | DamageType.Electricity,
						DamageType.Magnetic
					},
					{
						DamageType.Heat | DamageType.Electricity,
						DamageType.Radiation
					},
					{
						DamageType.Cold | DamageType.Toxin,
						DamageType.Viral
					}
				};

				public DamageHandler(WeaponInstance weaponInstance)
				{
					this.weaponInstance = weaponInstance;
				}

				internal void FinalizeCalculations()
				{
					statusChance.UpdateFinalValue();
					foreach (StatWorkingData value in damages.Values)
					{
						value.UpdateFinalValue();
					}
					MergeDamageTypes();
					foreach (StatWorkingData value2 in damages.Values)
					{
						value2.UpdateFinalValue();
					}
				}

				private void MergeDamageTypes()
				{
					List<KeyValuePair<DamageType, StatWorkingData>> list = (from p in damages
						where (p.Key & DamageType.BaseElemental) > DamageType.None && p.Value.finalValue > 0.0
						orderby p.Value.firstModIndexThatAffectsThisStat
						select p).ToList();
					if (list.Count((KeyValuePair<DamageType, StatWorkingData> p) => !p.Value.modIndexSet) > 1)
					{
						throw new Exception("Multiple Elemental Damage types found without mod index");
					}
					for (int num = 0; num < list.Count && num + 1 < list.Count; num += 2)
					{
						KeyValuePair<DamageType, StatWorkingData> keyValuePair = list[num];
						KeyValuePair<DamageType, StatWorkingData> keyValuePair2 = list[num + 1];
						DamageType key = DamageMergingPairs[keyValuePair.Key | keyValuePair2.Key];
						damages[key].SetExtraBaseValue(keyValuePair.Value.finalValue + keyValuePair2.Value.finalValue);
						damages[keyValuePair.Key].ClearLists();
						damages[keyValuePair2.Key].ClearLists();
					}
				}

				public EnemyInstance.AttackData GetNewAttackInfo(DamageCalculatorInstance.TargetInfo targetInfo, DamageSource damageSource)
				{
					EnemyInstance.AttackData attackData = new EnemyInstance.AttackData();
					attackData.baseTotalDamage = damages.Values.Sum((StatWorkingData p) => p.finalPreAdditionsValue);
					attackData.weakspotHit = targetInfo.isHeadshot;
					attackData.weaponDamageByType = damages.ToDictionary((KeyValuePair<DamageType, StatWorkingData> p) => p.Key, (KeyValuePair<DamageType, StatWorkingData> p) => p.Value.finalValue);
					attackData.statusChance = statusChance.finalValue;
					double num = weaponInstance.currentStats.stats[WeaponStat.CriticalChance].finalValue;
					if (damageSource != DamageSource.WeaponAOE)
					{
						num += targetInfo.enemy.statusEffectHandler.GetAdditivePunctureCriticalChance();
					}
					attackData.criticalHit = false;
					attackData.critMultiplier = 1.0;
					attackData.critTier = 0;
					if (num > 0.0)
					{
						int num2 = 0;
						num2 += (int)Math.Floor(num);
						num -= (double)num2;
						if (weaponInstance.simulatorInstance.GetRandomDouble() < num)
						{
							num2++;
						}
						if (num2 > 0)
						{
							attackData.criticalHit = true;
							double finalValue = weaponInstance.currentStats.stats[WeaponStat.CriticalMultiplier].finalValue;
							if (damageSource != DamageSource.WeaponAOE)
							{
								num += targetInfo.enemy.statusEffectHandler.GetAdditiveColdCriticalMultiplier();
							}
							attackData.critTier = num2;
							attackData.critMultiplier = 1.0 + (double)num2 * (finalValue - 1.0);
						}
					}
					return attackData;
				}
			}

			public Dictionary<WeaponStat, StatWorkingData> stats = new Dictionary<WeaponStat, StatWorkingData>();

			public DamageHandler directDamageHandler;

			public DamageHandler aoeDamageHandler;

			public bool requiresReinitialization = true;

			public void Invalidate()
			{
				requiresReinitialization = true;
			}

			public void ClearState()
			{
				foreach (StatWorkingData value in stats.Values)
				{
					value.ClearLists();
				}
				directDamageHandler?.statusChance.ClearLists();
				IEnumerable<StatWorkingData> enumerable = directDamageHandler?.damages.Values;
				foreach (StatWorkingData item in enumerable ?? Enumerable.Empty<StatWorkingData>())
				{
					item.ClearLists();
				}
				aoeDamageHandler?.statusChance.ClearLists();
				enumerable = aoeDamageHandler?.damages.Values;
				foreach (StatWorkingData item2 in enumerable ?? Enumerable.Empty<StatWorkingData>())
				{
					item2.ClearLists();
				}
			}

			public void FinalizeCalculations()
			{
				foreach (StatWorkingData value in stats.Values)
				{
					value.UpdateFinalValue();
				}
				directDamageHandler?.FinalizeCalculations();
				aoeDamageHandler?.FinalizeCalculations();
			}
		}

		public enum StatRequestType
		{
			Stats,
			DirectDamage,
			AOE
		}

		private const double CRITICAL_DAMAGE_QUANTIZATION_COEFFICIENT = 0.007814408279955387;

		private BuildSourceDataFile sourceDataFile;

		public readonly DamageCalculatorInstance simulatorInstance;

		private BuildWeaponData weaponData;

		private List<UpgradeInstance> upgrades;

		private Dictionary<DamageType, double> innateDamages;

		private int currentMode;

		private int currentZoomLevel;

		private WeaponCurrentStats currentStats = new WeaponCurrentStats();

		private int nextFireAvailableMS;

		private int reloadRemainingMS;

		private int currentMagazineAmmoLeft;

		private DamageType[] DamageTypeEnumList = Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToArray();

		public WeaponInstance(BuildSourceDataFile sourceDataFile, WeaponBuild build, DamageCalculatorInstance simulatorInstance)
		{
			this.sourceDataFile = sourceDataFile;
			this.simulatorInstance = simulatorInstance;
			weaponData = sourceDataFile.weapons.GetOrDefault(build.metadata.itemUID);
			if (weaponData == null)
			{
				throw new Exception("Weapon data not found");
			}
			upgrades = new List<UpgradeInstance>();
			IEnumerable<BuildUpgradeData.BuildModBuff> baseBuffs = weaponData.baseBuffs;
			foreach (BuildUpgradeData.BuildModBuff item in baseBuffs ?? Enumerable.Empty<BuildUpgradeData.BuildModBuff>())
			{
				upgrades.Add(new UpgradeInstance(item, 0, weaponData.parents));
			}
			if (build.modsSlots != null)
			{
				foreach (BaseBuild.UpgradeSlot modsSlot in build.modsSlots)
				{
					if (modsSlot.full)
					{
						upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, modsSlot, weaponData.parents));
					}
				}
			}
			if (build.arcane != null)
			{
				foreach (BaseBuild.UpgradeSlot item2 in build.arcane)
				{
					if (item2.full)
					{
						upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Arcane, item2, weaponData.parents));
					}
				}
			}
			if (build.exilusSlot != null && build.exilusSlot.full)
			{
				upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, build.exilusSlot, weaponData.parents));
			}
			if (build.stance != null && build.stance.full)
			{
				upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, build.stance, weaponData.parents));
			}
			innateDamages = build.innateDamages?.ToDictionary((KeyValuePair<DamageType, double> k) => k.Key, (KeyValuePair<DamageType, double> k) => k.Value);
			currentStats.Invalidate();
			CalculateStats();
			currentMagazineAmmoLeft = (int)currentStats.stats[WeaponStat.MagazineSize].finalValue;
		}

		public void Reset()
		{
			currentStats.Invalidate();
			nextFireAvailableMS = 0;
			reloadRemainingMS = 0;
			currentMagazineAmmoLeft = (int)currentStats.stats[WeaponStat.MagazineSize].finalValue;
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.Reset();
			}
		}

		public void ChangeSelectedMode(int mode)
		{
			if (currentMode != mode)
			{
				if (mode >= weaponData.modes.Count)
				{
					throw new Exception("Mode not found");
				}
				currentMode = mode;
				currentStats.Invalidate();
			}
		}

		public void ChangeSelectedZoomLevel(int zoomLevelIndex)
		{
			if (zoomLevelIndex >= 0 && weaponData.zoomLevels == null)
			{
				throw new Exception("Invalid zoom level");
			}
			if (currentZoomLevel != zoomLevelIndex)
			{
				if (zoomLevelIndex != -1 && zoomLevelIndex >= weaponData.zoomLevels.Count)
				{
					throw new Exception("Zoom level not found");
				}
				currentZoomLevel = zoomLevelIndex;
				currentStats.Invalidate();
			}
		}

		public void CalculateStats(DamageCalculatorInstance.TargetInfo targetInfo = null)
		{
			if (currentStats.requiresReinitialization)
			{
				foreach (WeaponStat item in Enum.GetValues(typeof(WeaponStat)).Cast<WeaponStat>().ToList())
				{
					currentStats.stats[item] = new StatWorkingData();
				}
				currentStats.stats[WeaponStat.AmmoMax].SetBaseValue(weaponData.ammoCapacity);
				currentStats.stats[WeaponStat.MagazineSize].SetBaseValue(weaponData.magazineCapacity);
				currentStats.stats[WeaponStat.AmmoPickup].SetBaseValue(weaponData.ammoPickUpCount);
				BuildWeaponData.WeaponMode weaponMode = weaponData.modes[currentMode];
				currentStats.stats[WeaponStat.FireRate].SetBaseValue(weaponMode.fireRate);
				currentStats.stats[WeaponStat.ReloadTime].SetBaseValue(weaponMode.reloadTime);
				currentStats.stats[WeaponStat.CriticalChance].SetBaseValue(weaponMode.critChance);
				currentStats.stats[WeaponStat.CriticalMultiplier].SetBaseValue(weaponMode.critMultiplier, -1.0, 0.007814408279955387);
				currentStats.stats[WeaponStat.Multishot].SetBaseValue(weaponMode.baseMultishot);
				currentStats.stats[WeaponStat.PunchThrough].SetBaseValue(weaponMode.punchThrough);
				currentStats.stats[WeaponStat.DamageToCorpus].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.DamageToSentients].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.DamageToMurmur].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.DamageToCorrupted].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.DamageToInfested].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.DamageToGrineer].SetBaseValue(1.0);
				currentStats.stats[WeaponStat.StatusTime].SetBaseValue(1.0);
				double damageSum = weaponMode.directDamage.damages.Values.Sum();
				Dictionary<DamageType, double> dictionary = innateDamages?.Select((KeyValuePair<DamageType, double> p) => (Key: p.Key, p.Value * damageSum)).ToDictionary(((DamageType Key, double) p) => p.Key, ((DamageType Key, double) p) => p.Item2) ?? new Dictionary<DamageType, double>();
				damageSum += dictionary.Values.Sum();
				if (weaponMode.directDamage != null)
				{
					currentStats.directDamageHandler = new WeaponCurrentStats.DamageHandler(this);
					foreach (DamageType item2 in Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList())
					{
						currentStats.directDamageHandler.damages[item2] = new StatWorkingData();
						double value = weaponMode.directDamage.damages.GetOrDefault(item2) + dictionary.GetOrDefault(item2);
						currentStats.directDamageHandler.damages[item2].SetBaseValue(value, damageSum);
					}
					currentStats.directDamageHandler.statusChance.SetBaseValue(weaponMode.directDamage.statusChance);
					currentStats.directDamageHandler.falloffData = new WeaponCurrentStats.DamageHandler.FalloffData(weaponMode.directDamage.fallofStartAt, weaponMode.directDamage.fallofAt, weaponMode.directDamage.fallofLoss);
				}
				if (weaponMode.aoeAfterDirect != null)
				{
					currentStats.aoeDamageHandler = new WeaponCurrentStats.DamageHandler(this);
					foreach (DamageType item3 in Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList())
					{
						currentStats.aoeDamageHandler.damages[item3] = new StatWorkingData();
						currentStats.aoeDamageHandler.damages[item3].SetBaseValue(weaponMode.aoeAfterDirect.damages.GetOrDefault(item3) + dictionary.GetOrDefault(item3), ((DamageType.Physical & item3) > DamageType.None) ? 0.0 : damageSum);
					}
					currentStats.aoeDamageHandler.statusChance.SetBaseValue(weaponMode.aoeAfterDirect.statusChance);
					currentStats.aoeDamageHandler.falloffData = new WeaponCurrentStats.DamageHandler.FalloffData(weaponMode.aoeAfterDirect.fallofStartAt, weaponMode.aoeAfterDirect.fallofAt, weaponMode.aoeAfterDirect.fallofLoss);
				}
				currentStats.requiresReinitialization = false;
			}
			currentStats.ClearState();
			int num = 0;
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.ApplyWeaponStats(ApplyStatBuffCallback, ApplyDamageBuffCallback, targetInfo, (upgrade.upgradeType == UpgradeInstance.UpgradeType.Mod) ? num : 999999);
				if (upgrade.upgradeType == UpgradeInstance.UpgradeType.Mod)
				{
					num++;
				}
			}
			currentStats.FinalizeCalculations();
		}

		private void ApplyStatBuffCallback(WeaponStat stat, BuildUpgradeData.BuildModBuff.ModBuffOperation operation, double value, bool isBuffAPercentageOfTheBase)
		{
			if (stat == WeaponStat.StatusChance)
			{
				currentStats.directDamageHandler?.statusChance.AddBuff(operation, value, isBuffAPercentageOfTheBase);
				currentStats.aoeDamageHandler?.statusChance.AddBuff(operation, value, isBuffAPercentageOfTheBase);
			}
			else
			{
				currentStats.stats[stat].AddBuff(operation, value, isBuffAPercentageOfTheBase);
			}
		}

		private void ApplyDamageBuffCallback(DamageType type, BuildUpgradeData.BuildModBuff.ModBuffOperation operation, double value, bool isBuffAPercentageOfTheBase, int modIndex)
		{
			if (type == DamageType.AF_All)
			{
				DamageType[] damageTypeEnumList = DamageTypeEnumList;
				foreach (DamageType key in damageTypeEnumList)
				{
					currentStats.directDamageHandler?.damages[key].AddBuff(operation, value, isBuffAPercentageOfTheBase);
					currentStats.aoeDamageHandler?.damages[key].AddBuff(operation, value, isBuffAPercentageOfTheBase);
				}
			}
			else
			{
				currentStats.directDamageHandler?.damages[type].AddBuff(operation, value, isBuffAPercentageOfTheBase, modIndex);
				currentStats.aoeDamageHandler?.damages[type].AddBuff(operation, value, isBuffAPercentageOfTheBase, modIndex);
			}
		}

		public string GetStatString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Mode: {currentMode}");
			stringBuilder.AppendLine($"Zoom: {currentZoomLevel}");
			stringBuilder.AppendLine("Stats: ");
			foreach (WeaponStat item in Enum.GetValues(typeof(WeaponStat)).Cast<WeaponStat>().ToList())
			{
				stringBuilder.AppendLine($"\t{item}: {currentStats.stats[item].finalValue}");
			}
			stringBuilder.AppendLine("Direct Damage: ");
			if (currentStats.directDamageHandler != null)
			{
				foreach (DamageType item2 in Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList())
				{
					stringBuilder.AppendLine($"\t{item2}: {currentStats.directDamageHandler.damages[item2].finalValue}");
				}
				stringBuilder.AppendLine($"\tStatus Chance: {currentStats.directDamageHandler.statusChance.finalValue}");
				stringBuilder.AppendLine($"\tFalloff Start: {currentStats.directDamageHandler.falloffData.startDistance}");
				stringBuilder.AppendLine($"\tFalloff End: {currentStats.directDamageHandler.falloffData.endDistance}");
				stringBuilder.AppendLine($"\tFalloff Damage Multiplier: {currentStats.directDamageHandler.falloffData.falloffAtEnd}");
			}
			else
			{
				stringBuilder.AppendLine("\tNo direct damage");
			}
			stringBuilder.AppendLine("AOE Damage: ");
			if (currentStats.aoeDamageHandler != null)
			{
				foreach (DamageType item3 in Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList())
				{
					stringBuilder.AppendLine($"\t{item3}: {currentStats.aoeDamageHandler.damages[item3].finalValue}");
				}
				stringBuilder.AppendLine($"\tStatus Chance: {currentStats.aoeDamageHandler.statusChance.finalValue}");
				stringBuilder.AppendLine($"\tFalloff Start: {currentStats.aoeDamageHandler.falloffData.startDistance}");
				stringBuilder.AppendLine($"\tFalloff End: {currentStats.aoeDamageHandler.falloffData.endDistance}");
				stringBuilder.AppendLine($"\tFalloff Damage Multiplier: {currentStats.aoeDamageHandler.falloffData.falloffAtEnd}");
			}
			else
			{
				stringBuilder.AppendLine("\tNo AOE damage");
			}
			return stringBuilder.ToString();
		}

		public void Tick(int deltaTimeMS)
		{
			if (currentMagazineAmmoLeft > 0 && nextFireAvailableMS == 0)
			{
				FireWeapon();
			}
			else
			{
				CalculateStats();
			}
			if (currentMagazineAmmoLeft == 0 && reloadRemainingMS == 0)
			{
				reloadRemainingMS = (int)(currentStats.stats[WeaponStat.ReloadTime].finalValue * 1000.0);
				simulatorInstance.RaiseEvent(DamageCalculatorEvent.WeaponReloadStarted, null);
			}
			if (reloadRemainingMS > 0)
			{
				reloadRemainingMS -= deltaTimeMS;
				if (reloadRemainingMS <= 0)
				{
					reloadRemainingMS = 0;
					currentMagazineAmmoLeft = (int)currentStats.stats[WeaponStat.MagazineSize].finalValue;
					simulatorInstance.RaiseEvent(DamageCalculatorEvent.WeaponReloadFinished, null);
				}
			}
			if (nextFireAvailableMS > 0)
			{
				nextFireAvailableMS -= deltaTimeMS;
				if (nextFireAvailableMS <= 0)
				{
					nextFireAvailableMS = 0;
				}
			}
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.Tick(deltaTimeMS);
			}
		}

		private void FireWeapon()
		{
			DamageCalculatorInstance.TargetInfo targetInfo = simulatorInstance.AcquireNextTargetInfo();
			CalculateStats(targetInfo);
			if (targetInfo == null)
			{
				return;
			}
			if (currentStats.directDamageHandler != null)
			{
				double finalValue = currentStats.stats[WeaponStat.Multishot].finalValue;
				int num = (int)Math.Floor(finalValue);
				if (simulatorInstance.GetRandomDouble() < finalValue - (double)num)
				{
					num++;
				}
				for (int i = 0; i < num; i++)
				{
					EnemyInstance.AttackData newAttackInfo = currentStats.directDamageHandler.GetNewAttackInfo(targetInfo, DamageSource.Weapon);
					targetInfo.enemy.ApplyWeaponAttack(currentStats.stats, newAttackInfo, DamageSource.Weapon);
					simulatorInstance.SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions.Hit);
					if (newAttackInfo.criticalHit)
					{
						simulatorInstance.SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions.CriticalHit);
					}
					if (newAttackInfo.weakspotHit)
					{
						simulatorInstance.SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions.Headshot);
					}
				}
			}
			_ = currentStats.aoeDamageHandler;
			double finalValue2 = currentStats.stats[WeaponStat.FireRate].finalValue;
			nextFireAvailableMS = (int)(1000.0 / finalValue2);
			currentMagazineAmmoLeft--;
		}

		public void SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions eventType)
		{
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.ConditionalEventHappened(eventType, currentStats.stats[WeaponStat.StatusTime].finalValue);
			}
		}

		public List<DamageCalculatorStatOutput> GetStats(StatRequestType request)
		{
			List<DamageCalculatorStatOutput> list = new List<DamageCalculatorStatOutput>();
			CalculateStats();
			if (request == StatRequestType.Stats)
			{
				foreach (KeyValuePair<WeaponStat, StatWorkingData> stat in currentStats.stats)
				{
					DamageCalculatorStatOutput damageCalculatorStatOutput = new DamageCalculatorStatOutput();
					DamageCalculatorStatOutput damageCalculatorStatOutput2 = damageCalculatorStatOutput;
					string name;
					switch (stat.Key)
					{
					case WeaponStat.FalloffStart:
						name = null;
						break;
					case WeaponStat.FalloffDamageMultiplier:
						name = null;
						break;
					case WeaponStat.FalloffEnd:
						name = null;
						break;
					case WeaponStat.AmmoMax:
						name = "Ammo";
						break;
					case WeaponStat.AmmoPickup:
						name = null;
						break;
					case WeaponStat.MagazineSize:
						name = null;
						break;
					case WeaponStat.Accuracy:
						name = null;
						break;
					case WeaponStat.None:
						name = null;
						break;
					case WeaponStat.StatusChance:
						name = null;
						break;
					default:
						name = Regex.Replace(stat.Key.ToString(), "(\\B[A-Z])", " $1");
						break;
					}
					damageCalculatorStatOutput2.name = name;
					int num;
					switch (stat.Key)
					{
					case WeaponStat.DamageToMurmur:
						num = 2;
						break;
					case WeaponStat.DamageToCorpus:
						num = 2;
						break;
					case WeaponStat.DamageToSentients:
						num = 2;
						break;
					case WeaponStat.DamageToCorrupted:
						num = 2;
						break;
					case WeaponStat.DamageToInfested:
						num = 2;
						break;
					case WeaponStat.DamageToGrineer:
						num = 2;
						break;
					case WeaponStat.StatusChance:
						num = 2;
						break;
					case WeaponStat.StatusTime:
						num = 0;
						break;
					default:
						num = 1;
						break;
					}
					int digits = num;
					damageCalculatorStatOutput.internalName = stat.Key.ToString();
					bool flag;
					switch (stat.Key)
					{
					case WeaponStat.StatusChance:
						flag = true;
						break;
					case WeaponStat.CriticalChance:
						flag = true;
						break;
					case WeaponStat.StatusTime:
						flag = true;
						break;
					default:
						flag = false;
						break;
					}
					bool flag2 = flag;
					name = ((stat.Key != WeaponStat.AmmoMax) ? Math.Round((double)((!flag2) ? 1 : 100) * stat.Value.finalValue, digits).ToString() : $"{Math.Round(currentStats.stats[WeaponStat.MagazineSize].finalValue, 1)}/{Math.Round(currentStats.stats[WeaponStat.AmmoMax].finalValue, 1)}");
					damageCalculatorStatOutput.value = name;
					if (flag2)
					{
						damageCalculatorStatOutput.value += "%";
					}
					if (damageCalculatorStatOutput.name != null && damageCalculatorStatOutput.value != null && (!damageCalculatorStatOutput.name.Contains("Damage To ") || !(damageCalculatorStatOutput.value == "1")))
					{
						list.Add(damageCalculatorStatOutput);
					}
				}
			}
			else
			{
				WeaponCurrentStats.DamageHandler damageHandler = ((request == StatRequestType.DirectDamage) ? currentStats.directDamageHandler : currentStats.aoeDamageHandler);
				if (damageHandler == null)
				{
					return list;
				}
				list.Add(new DamageCalculatorStatOutput
				{
					internalName = "falloffStart",
					name = "Falloff",
					value = $"{Math.Round(damageHandler.falloffData.startDistance, 1)} - {Math.Round(damageHandler.falloffData.endDistance, 1)}" + $"({Math.Round(100.0 * (1.0 - damageHandler.falloffData.falloffAtEnd))}%)"
				});
				list.Add(new DamageCalculatorStatOutput
				{
					internalName = "statusChance",
					name = "Status Chance",
					value = Math.Round(100.0 * damageHandler.statusChance.finalValue, 1) + "%"
				});
				foreach (KeyValuePair<DamageType, StatWorkingData> damage in damageHandler.damages)
				{
					if (damage.Value.finalValue != 0.0)
					{
						list.Add(new DamageCalculatorStatOutput
						{
							internalName = damage.Key.ToString().ToLower() + "Damage",
							name = damage.Key.ToString(),
							value = Math.Round(damage.Value.finalValue, 1).ToString()
						});
					}
				}
			}
			return list;
		}

		public bool HasAOE()
		{
			return currentStats.aoeDamageHandler != null;
		}
	}
}
