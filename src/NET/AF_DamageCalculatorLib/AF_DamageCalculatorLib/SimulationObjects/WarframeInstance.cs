using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AF_DamageCalculatorLib.Classes;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class WarframeInstance
	{
		public enum WarframeStat
		{
			Health,
			Shield,
			Armor,
			EnergyMax,
			Strength,
			Duration,
			Range,
			Efficiency,
			Regeneration,
			SprintSpeed,
			Invalid
		}

		public enum WarframeCheckEnum
		{
			Hydroid
		}

		private DamageCalculatorInstance simulatorInstance;

		private BuildSourceDataFile sourceDataFile;

		private BuildWarframeData warframeData;

		private List<UpgradeInstance> upgrades;

		private List<UpgradeInstance> levelUpgrades;

		private Dictionary<WarframeStat, StatWorkingData> stats = new Dictionary<WarframeStat, StatWorkingData>();

		private string itemUID;

		private int itemLevel;

		private bool statsRequiresReinitialization = true;

		private List<WarframeStat> WarframeStatEnumList = Enum.GetValues(typeof(WarframeStat)).Cast<WarframeStat>().ToList();

		private static readonly ReadOnlyDictionary<WarframeStat, bool> DisplayStatAsPercentage = new ReadOnlyDictionary<WarframeStat, bool>(new Dictionary<WarframeStat, bool>
		{
			{
				WarframeStat.Duration,
				true
			},
			{
				WarframeStat.Efficiency,
				true
			},
			{
				WarframeStat.Range,
				true
			},
			{
				WarframeStat.Strength,
				true
			}
		});

		public WarframeInstance(BuildSourceDataFile sourceDataFile, WarframeBuild build, DamageCalculatorInstance damageCalculatorInstance)
		{
			this.sourceDataFile = sourceDataFile;
			simulatorInstance = damageCalculatorInstance;
			warframeData = sourceDataFile.warframes.GetOrDefault(build.metadata.itemUID);
			if (warframeData == null)
			{
				throw new Exception("Warframe data not found");
			}
			upgrades = new List<UpgradeInstance>();
			if (build.arcaneSlots != null)
			{
				foreach (BaseBuild.UpgradeSlot arcaneSlot in build.arcaneSlots)
				{
					if (arcaneSlot.full)
					{
						upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Arcane, arcaneSlot, warframeData.parents));
					}
				}
			}
			if (build.auraSlot != null && build.auraSlot.full)
			{
				upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, build.auraSlot, warframeData.parents));
			}
			if (build.exilusSlot != null && build.exilusSlot.full)
			{
				upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, build.exilusSlot, warframeData.parents));
			}
			if (build.modsSlots != null)
			{
				foreach (BaseBuild.UpgradeSlot modsSlot in build.modsSlots)
				{
					if (modsSlot.full)
					{
						upgrades.Add(new UpgradeInstance(sourceDataFile, UpgradeInstance.UpgradeType.Mod, modsSlot, warframeData.parents));
					}
				}
			}
			levelUpgrades = new List<UpgradeInstance>();
			for (int i = 0; i <= build.itemLevel && i < warframeData.levelUpBuffs.Count; i++)
			{
				BuildUpgradeData.BuildModBuff buff = warframeData.levelUpBuffs[i];
				levelUpgrades.Add(new UpgradeInstance(buff, 0, warframeData.parents));
			}
			itemUID = build.metadata.itemUID;
			itemLevel = build.itemLevel;
			statsRequiresReinitialization = true;
		}

		public void Reset()
		{
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.Reset();
			}
			statsRequiresReinitialization = true;
		}

		public void CalculateStats()
		{
			if (statsRequiresReinitialization)
			{
				stats.Clear();
				foreach (WarframeStat warframeStatEnum in WarframeStatEnumList)
				{
					stats.Add(warframeStatEnum, new StatWorkingData());
				}
				stats[WarframeStat.Duration].SetBaseValue(1.0);
				stats[WarframeStat.Efficiency].SetBaseValue(1.0);
				stats[WarframeStat.Range].SetBaseValue(1.0);
				stats[WarframeStat.Strength].SetBaseValue(1.0);
				stats[WarframeStat.Regeneration].SetBaseValue(0.0);
				stats[WarframeStat.Health].SetBaseValue(warframeData.health);
				stats[WarframeStat.Shield].SetBaseValue(warframeData.shield);
				stats[WarframeStat.Armor].SetBaseValue(warframeData.armor);
				stats[WarframeStat.EnergyMax].SetBaseValue(warframeData.energy);
				stats[WarframeStat.SprintSpeed].SetBaseValue(warframeData.sprintSpeed);
				statsRequiresReinitialization = false;
			}
			foreach (WarframeStat warframeStatEnum2 in WarframeStatEnumList)
			{
				stats[warframeStatEnum2].ClearLists();
			}
			foreach (UpgradeInstance levelUpgrade in levelUpgrades)
			{
				levelUpgrade.ApplyPassiveWarframeStats(stats);
			}
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.ApplyPassiveWarframeStats(stats);
			}
			foreach (WarframeStat warframeStatEnum3 in WarframeStatEnumList)
			{
				stats[warframeStatEnum3].UpdateFinalValue();
			}
		}

		public string GetStatString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Stats for {warframeData.name} at level {itemLevel}:");
			foreach (KeyValuePair<WarframeStat, StatWorkingData> stat in stats)
			{
				if (stat.Key != WarframeStat.Invalid)
				{
					if (DisplayStatAsPercentage.TryGetValue(stat.Key, out var value) && value)
					{
						stringBuilder.Append(stat.Key.ToString() + ": " + (100.0 * stat.Value.finalValue).ToString("0.0"));
						stringBuilder.Append("%");
					}
					else
					{
						stringBuilder.Append(stat.Key.ToString() + ": " + stat.Value.finalValue.ToString("0.00"));
					}
					stringBuilder.Append("\n");
				}
			}
			return stringBuilder.ToString();
		}

		public void SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions eventType)
		{
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.ConditionalEventHappened(eventType, 1.0);
			}
		}

		public void Tick(int deltaTimeMS)
		{
			CalculateStats();
			foreach (UpgradeInstance upgrade in upgrades)
			{
				upgrade.Tick(deltaTimeMS);
			}
		}

		public bool IsWarframe(WarframeCheckEnum warframe)
		{
			if (warframe == WarframeCheckEnum.Hydroid)
			{
				if (!(itemUID == "/Lotus/Powersuits/Pirate/Pirate"))
				{
					return itemUID == "/Lotus/Powersuits/Pirate/HydroidPrime";
				}
				return true;
			}
			throw new Exception("Invalid warframe");
		}

		public List<DamageCalculatorStatOutput> GetStats()
		{
			List<DamageCalculatorStatOutput> list = new List<DamageCalculatorStatOutput>();
			CalculateStats();
			foreach (KeyValuePair<WarframeStat, StatWorkingData> stat in stats)
			{
				DamageCalculatorStatOutput damageCalculatorStatOutput = new DamageCalculatorStatOutput();
				DamageCalculatorStatOutput damageCalculatorStatOutput2 = damageCalculatorStatOutput;
				string name;
				switch (stat.Key)
				{
				case WarframeStat.EnergyMax:
					name = "Max Energy";
					break;
				case WarframeStat.SprintSpeed:
					name = "Sprint Speed";
					break;
				default:
					name = stat.Key.ToString();
					break;
				}
				damageCalculatorStatOutput2.name = name;
				damageCalculatorStatOutput.internalName = stat.Key.ToString();
				bool value;
				bool flag = DisplayStatAsPercentage.TryGetValue(stat.Key, out value) && value;
				int num;
				switch (stat.Key)
				{
				case WarframeStat.Regeneration:
					num = 2;
					break;
				case WarframeStat.SprintSpeed:
					num = 2;
					break;
				default:
					num = 0;
					break;
				}
				int digits = num;
				damageCalculatorStatOutput.value = Math.Round((double)((!flag) ? 1 : 100) * stat.Value.finalValue, digits).ToString();
				if (flag)
				{
					damageCalculatorStatOutput.value += "%";
				}
				if (stat.Key != WarframeStat.Invalid)
				{
					list.Add(damageCalculatorStatOutput);
				}
			}
			return list;
		}
	}
}
