using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AF_DamageCalculatorLib;
using AF_DamageCalculatorLib.Classes;
using AF_DamageCalculatorLib.SimulationObjects;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlecaFrameClientLib
{
	public class BuildHandler
	{
		public class SwitchModPositionRequest
		{
			public int currentItemIndex;

			public string specialSlotName;

			public int fromIndex;

			public int toIndex;

			public bool saveUndoHistory = true;
		}

		public class RemoveModPositionRequest
		{
			public int currentItemIndex;

			public int index;

			public string specialSlotName;

			public bool saveUndoHistory = true;
		}

		public class ModBrowserRequest
		{
			public int currentItemIndex;

			public string category;

			public string search;

			public int selectedModSlotIndex = -1;
		}

		public class ModBrowserResponse
		{
			public enum ModBrowserStatus
			{
				Calculating,
				FinishedOK,
				TooEasy,
				TooHard,
				NoModSelected,
				NoEnemySelected
			}

			public class ModBrowserSingleMod
			{
				public string uniqueName;

				public string name;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildUpgradeData.ModPolarity modPolarity;

				public string picture;

				public double dps;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildUpgradeData.ModMaterial modType;

				public bool owned;

				public BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat[] stats;

				public bool statsShowOther;

				public string placingFilter;

				public string description;

				public bool compatible;
			}

			public ModBrowserSingleMod[] mods;

			public int progressCurrent;

			public int progressTotal;

			[JsonConverter(typeof(StringEnumConverter))]
			public ModBrowserStatus status;

			public double progressPercent => Math.Round((double)(100 * progressCurrent) / (double)progressTotal);
		}

		public class PlaceNewModRequest
		{
			public int currentItemIndex;

			public int index;

			public string specialSlotName;

			public string uniqueID;
		}

		public class SetBuildSettingsRequest
		{
			public int weaponModeID;

			public int weaponZoomLevelID;
		}

		public class EnemySetupListResponse
		{
			public List<string> availablePresetNames;

			public string selectedPreset;
		}

		public class EnemySetupSelectRequest
		{
			public string presetName;
		}

		public class EnemyCustomizationGetEnemiesRequest
		{
			public string search;

			[JsonConverter(typeof(StringEnumConverter))]
			public BuildEnemyData.FilteringGroup faction = BuildEnemyData.FilteringGroup.All;

			public string isBoss;

			public string isEximus;

			public bool showAll;
		}

		public class EnemyCustomizationGetEnemiesResponse
		{
			public class EnemyOutputListData
			{
				public string uniqueName;

				public string name;

				public string picture;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildEnemyData.FilteringGroup faction;

				public bool hasArmor;

				public bool hasShield;

				public bool isBoss;

				public bool isEximus;
			}

			public List<EnemyOutputListData> enemies;
		}

		public class EnemyCustomizationGetCurrentEnemiesStatusResponse
		{
			public class CurrentEnemyOutput
			{
				public string name;

				public string picture;

				public int level;

				public int amount;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildEnemyData.FilteringGroup faction;

				public string armor;

				public string shield;

				public string health;

				public bool isEximus;
			}

			public List<CurrentEnemyOutput> enemies;
		}

		public class EnemyCustomizationAddEnemyRequest
		{
			public string uniqueName;
		}

		public class EnemyCustomizationEditEnemyRequest
		{
			public int index;

			public int level;

			public int amount;
		}

		private bool initialized;

		private DamageCalculatorInstance damageResultsSimulatorInstance;

		private DamageCalculatorInstance mainSimulatorInstance;

		private DamageCalculatorInstance[] modBrowserCalculatingInstances = new DamageCalculatorInstance[3];

		private List<DamageCalculatorInstance> damageCalculatorInstances = new List<DamageCalculatorInstance>();

		private BasicRemoteData basicRemoteData;

		private BuildSourceDataFile sourceDataFile;

		public int weaponModeIndex;

		public int weaponZoomLevelIndex = -1;

		private BuildSource warframeBuildSource = BuildSource.AlecaFrame;

		private WarframeBuild warframeBuild;

		private BuildWarframeData buildWarframeData;

		private BuildSource weaponBuildSource = BuildSource.AlecaFrame;

		private WeaponBuild weaponBuild;

		private BuildWeaponData buildWeaponData;

		private List<EnemySetup> availableEnemySetups = new List<EnemySetup>();

		private EnemySetup currentEnemySetup;

		public static WeaponBuild BratonPrime = new WeaponBuild
		{
			metadata = new BaseBuild.BuildMetadata("/Lotus/Weapons/Tenno/Rifle/BratonPrime"),
			exilusSlot = null,
			arcane = null,
			modsSlots = new List<BaseBuild.UpgradeSlot>
			{
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Rifle/WeaponDamageAmountMod", 9),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Rifle/WeaponFreezeDamageMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Rifle/WeaponFireDamageMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Rifle/WeaponCritChanceMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Rifle/WeaponFireIterationsMod")
			}
		};

		public static WeaponBuild Penta = new WeaponBuild
		{
			metadata = new BaseBuild.BuildMetadata("/Lotus/Weapons/Corpus/LongGuns/GrenadeLauncher/GrenadeLauncher")
		};

		public static WeaponBuild MK1Paris = new WeaponBuild
		{
			metadata = new BaseBuild.BuildMetadata("/Lotus/Weapons/MK1Series/MK1Paris")
		};

		public static WarframeBuild NidusPrimeComplex = new WarframeBuild
		{
			metadata = new BaseBuild.BuildMetadata("/Lotus/Powersuits/Infestation/InfestationPrime"),
			exilusSlot = null,
			auraSlot = new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Aura/AvatarAuraPowerMaxMod"),
			arcaneSlots = new List<BaseBuild.UpgradeSlot>
			{
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/CosmeticEnhancers/Defensive/IncreaseMaxHealthOnHealthPickup", 5)
			},
			itemLevel = 30,
			modsSlots = new List<BaseBuild.UpgradeSlot>
			{
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/DualStat/CorruptedPowerStrengthPowerDurationWarframe"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/AvatarPowerMaxMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/Expert/AvatarAbilityDurationModExpert", 9),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/AvatarHealthMaxMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/AvatarAbilityRangeMod"),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/AvatarAbilityStrengthMod"),
				new BaseBuild.UpgradeSlot(),
				new BaseBuild.UpgradeSlot("/Lotus/Upgrades/Mods/Warframe/AvatarResistanceOnDamageMod", 8)
			}
		};

		public Guid currentModBrowserInstanceGUID = Guid.NewGuid();

		public bool InitializeFromData(BasicRemoteData basicRemoteData, BuildSourceDataFile sourceDataFile, List<EnemySetup> availableEnemySetups)
		{
			if (initialized)
			{
				return true;
			}
			this.basicRemoteData = basicRemoteData;
			this.sourceDataFile = sourceDataFile;
			mainSimulatorInstance = new DamageCalculatorInstance(sourceDataFile);
			damageCalculatorInstances.Add(mainSimulatorInstance);
			damageResultsSimulatorInstance = new DamageCalculatorInstance(sourceDataFile);
			damageCalculatorInstances.Add(damageResultsSimulatorInstance);
			for (int i = 0; i < modBrowserCalculatingInstances.Length; i++)
			{
				modBrowserCalculatingInstances[i] = new DamageCalculatorInstance(sourceDataFile);
				damageCalculatorInstances.Add(modBrowserCalculatingInstances[i]);
			}
			warframeBuild = NidusPrimeComplex;
			buildWarframeData = sourceDataFile.warframes.GetOrDefault(warframeBuild.metadata.itemUID);
			EnsureBuildInitializedCorrectly(warframeBuild);
			foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
			{
				damageCalculatorInstance.SetWarframeBuild(warframeBuild);
			}
			weaponBuild = Penta;
			buildWeaponData = sourceDataFile.weapons.GetOrDefault(weaponBuild.metadata.itemUID);
			EnsureBuildInitializedCorrectly(weaponBuild);
			foreach (DamageCalculatorInstance damageCalculatorInstance2 in damageCalculatorInstances)
			{
				damageCalculatorInstance2.SetWeaponBuild(weaponBuild);
			}
			this.availableEnemySetups = availableEnemySetups;
			if (this.availableEnemySetups.Count > 0)
			{
				currentEnemySetup = this.availableEnemySetups[0];
			}
			else
			{
				currentEnemySetup = new EnemySetup
				{
					metadata = new EnemySetup.Metadata("Custom")
				};
			}
			foreach (DamageCalculatorInstance damageCalculatorInstance3 in damageCalculatorInstances)
			{
				damageCalculatorInstance3.SetEnemySetup(currentEnemySetup);
			}
			initialized = true;
			return true;
		}

		private void EnsureBuildInitializedCorrectly(BaseBuild buildData)
		{
			if (buildData.modsSlots == null)
			{
				buildData.modsSlots = new List<BaseBuild.UpgradeSlot>();
			}
			while (buildData.modsSlots.Count < 8)
			{
				buildData.modsSlots.Add(new BaseBuild.UpgradeSlot());
			}
			if (buildData is WeaponBuild weaponBuild)
			{
				if (weaponBuild.exilusSlot == null)
				{
					weaponBuild.exilusSlot = new BaseBuild.UpgradeSlot();
				}
				if (weaponBuild.stance == null)
				{
					weaponBuild.stance = new BaseBuild.UpgradeSlot();
				}
				if (weaponBuild.arcane == null)
				{
					weaponBuild.arcane = new List<BaseBuild.UpgradeSlot>();
				}
				if (weaponBuild.innateDamages == null)
				{
					weaponBuild.innateDamages = new Dictionary<DamageType, double>();
				}
				return;
			}
			if (buildData is WarframeBuild warframeBuild)
			{
				if (warframeBuild.exilusSlot == null)
				{
					warframeBuild.exilusSlot = new BaseBuild.UpgradeSlot();
				}
				if (warframeBuild.auraSlot == null)
				{
					warframeBuild.auraSlot = new BaseBuild.UpgradeSlot();
				}
				if (warframeBuild.arcaneSlots == null)
				{
					warframeBuild.arcaneSlots = new List<BaseBuild.UpgradeSlot>();
				}
				while (warframeBuild.arcaneSlots.Count < 2)
				{
					warframeBuild.arcaneSlots.Add(new BaseBuild.UpgradeSlot());
				}
				return;
			}
			throw new Exception("Unknown build type");
		}

		public BuildHandlerStatus GetStatus(BuildHandlerStatus.Request request)
		{
			BuildHandlerStatus buildHandlerStatus = new BuildHandlerStatus();
			buildHandlerStatus.items = new BuildHandlerStatus.BuildHandlerStatusItem[2];
			buildHandlerStatus.items[0] = new BuildHandlerStatus.BuildHandlerStatusItem();
			buildHandlerStatus.items[0].name = basicRemoteData.items.GetOrDefault(warframeBuild.metadata.itemUID)?.name ?? "???";
			buildHandlerStatus.items[0].buildName = warframeBuild.metadata.name;
			buildHandlerStatus.items[0].author = warframeBuild.metadata.author;
			buildHandlerStatus.items[0].buildSource = warframeBuildSource;
			buildHandlerStatus.items[0].owned = IsWarframeOwned(warframeBuild.metadata.itemUID);
			buildHandlerStatus.items[0].picture = Misc.GetFullImagePath(basicRemoteData.items.GetOrDefault(warframeBuild.metadata.itemUID)?.pic);
			buildHandlerStatus.items[1] = new BuildHandlerStatus.BuildHandlerStatusItem();
			buildHandlerStatus.items[1].name = basicRemoteData.items.GetOrDefault(weaponBuild.metadata.itemUID)?.name ?? "???";
			buildHandlerStatus.items[1].buildName = weaponBuild.metadata.name;
			buildHandlerStatus.items[1].author = weaponBuild.metadata.author;
			buildHandlerStatus.items[1].buildSource = weaponBuildSource;
			buildHandlerStatus.items[1].owned = IsWeaponOwned(weaponBuild.metadata.itemUID);
			buildHandlerStatus.items[1].picture = Misc.GetFullImagePath(basicRemoteData.items.GetOrDefault(weaponBuild.metadata.itemUID)?.pic);
			buildHandlerStatus.items[1].modes = new List<BuildHandlerStatus.BuildHandlerStatusItem.ModeData>();
			for (int i = 0; i < (buildWeaponData.userModes?.Count ?? 0); i++)
			{
				BuildWeaponData.WeaponUserMode weaponUserMode = buildWeaponData.userModes[i];
				buildHandlerStatus.items[1].modes.Add(new BuildHandlerStatus.BuildHandlerStatusItem.ModeData
				{
					internalName = Regex.Replace(weaponUserMode.name.ToString(), "(\\B[A-Z])", " $1"),
					name = weaponUserMode.name.ToString(),
					modeID = weaponUserMode.modeID,
					selected = (weaponUserMode.modeID == weaponModeIndex)
				});
			}
			buildHandlerStatus.items[1].zoomLevels = new List<BuildHandlerStatus.BuildHandlerStatusItem.ModeData>();
			buildHandlerStatus.items[1].zoomLevels.Add(new BuildHandlerStatus.BuildHandlerStatusItem.ModeData
			{
				name = "Normal",
				internalName = "normal",
				selected = (weaponZoomLevelIndex == -1),
				modeID = -1
			});
			for (int j = 0; j < (buildWeaponData.zoomLevels?.Count ?? 0); j++)
			{
				double num = buildWeaponData.zoomLevels[j];
				buildHandlerStatus.items[1].zoomLevels.Add(new BuildHandlerStatus.BuildHandlerStatusItem.ModeData
				{
					name = "x" + num,
					internalName = "zoom",
					selected = (weaponZoomLevelIndex == weaponModeIndex),
					modeID = j
				});
			}
			buildHandlerStatus.buildResults = new BuildHandlerStatus.BuildHandlerStatusResults();
			List<DamageCalculatorStatOutput> list = ((request.selectedItemIndex == 0) ? mainSimulatorInstance.GetWarframeStats() : mainSimulatorInstance.GetWeaponStats((WeaponInstance.StatRequestType)request.statViewIndexSelected));
			buildHandlerStatus.buildResults.stats = new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleStat[list.Count];
			for (int k = 0; k < list.Count; k++)
			{
				buildHandlerStatus.buildResults.stats[k] = new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleStat();
				buildHandlerStatus.buildResults.stats[k].name = list[k].name;
				buildHandlerStatus.buildResults.stats[k].value = list[k].value;
				buildHandlerStatus.buildResults.stats[k].internalName = list[k].internalName;
			}
			if (request.selectedItemIndex == 0)
			{
				buildHandlerStatus.buildResults.views = new BuildHandlerStatus.BuildHandlerStatusResults.ViewData[1];
				buildHandlerStatus.buildResults.views[0] = new BuildHandlerStatus.BuildHandlerStatusResults.ViewData
				{
					name = "Stats"
				};
				(buildHandlerStatus.buildResults.neededThings, buildHandlerStatus.mods) = DoWarframeSpecificStatusWork(request.neededThingsShowTotal);
			}
			else
			{
				List<BuildHandlerStatus.BuildHandlerStatusResults.ViewData> list2 = new List<BuildHandlerStatus.BuildHandlerStatusResults.ViewData>
				{
					new BuildHandlerStatus.BuildHandlerStatusResults.ViewData
					{
						name = "Stats"
					},
					new BuildHandlerStatus.BuildHandlerStatusResults.ViewData
					{
						name = "Hit"
					}
				};
				if (mainSimulatorInstance.WeaponHasAOE())
				{
					list2.Add(new BuildHandlerStatus.BuildHandlerStatusResults.ViewData
					{
						name = "AOE"
					});
				}
				buildHandlerStatus.buildResults.views = list2.ToArray();
				(buildHandlerStatus.buildResults.neededThings, buildHandlerStatus.mods) = DoWeaponSpecificStatusWork(request.neededThingsShowTotal);
			}
			if (buildHandlerStatus.mods.modCapacity > buildHandlerStatus.mods.totalCapacity)
			{
				buildHandlerStatus.mods.capacityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Red;
			}
			else
			{
				buildHandlerStatus.mods.capacityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Green;
			}
			return buildHandlerStatus;
		}

		private bool IsWeaponOwned(string itemUID)
		{
			return Misc.IsWeaponOwned(itemUID);
		}

		private bool IsWarframeOwned(string itemUID)
		{
			return StaticData.dataHandler?.warframeRootObject?.Suits.Any((Suit x) => x.ItemType == itemUID) == true;
		}

		private (BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing[], BuildHandlerStatus.BuildHandlerStatusMods) DoWeaponSpecificStatusWork(bool showFullAmountOfThings)
		{
			List<BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing> list = new List<BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing>();
			BuildHandlerStatus.BuildHandlerStatusMods buildHandlerStatusMods = new BuildHandlerStatus.BuildHandlerStatusMods();
			buildHandlerStatusMods.mods = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod[8];
			buildHandlerStatusMods.arcanes = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod[1];
			BuildWeaponData orDefault = sourceDataFile.weapons.GetOrDefault(weaponBuild.metadata.itemUID);
			int modDrain = 0;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int endoCost = 0;
			int num5 = 0;
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			for (int i = 0; i < Math.Min(weaponBuild.modsSlots.Count, buildHandlerStatusMods.mods.Length); i++)
			{
				buildHandlerStatusMods.mods[i] = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.polarities[i], weaponBuild.modsSlots[i], ref endoCost, ref modDrain, list2, increasesCapacity: false, out var formaNeeded);
				if (formaNeeded)
				{
					num++;
				}
			}
			for (int j = weaponBuild.modsSlots.Count; j < buildHandlerStatusMods.mods.Length; j++)
			{
				buildHandlerStatusMods.mods[j] = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			}
			if (weaponBuild?.exilusSlot?.full == true)
			{
				num3++;
			}
			buildHandlerStatusMods.exilus = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.exilusPolarity, weaponBuild.exilusSlot, ref endoCost, ref modDrain, list2, increasesCapacity: false, out var formaNeeded2);
			if (formaNeeded2)
			{
				num++;
			}
			if (orDefault.weaponType == BuildWeaponData.WeaponType.Melee)
			{
				buildHandlerStatusMods.stance = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.stancePolarity, weaponBuild.stance, ref endoCost, ref modDrain, list2, increasesCapacity: true, out var formaNeeded3);
				if (formaNeeded3)
				{
					num2++;
				}
			}
			for (int k = 0; k < Math.Min(weaponBuild.arcane?.Count ?? 0, buildHandlerStatusMods.arcanes.Length); k++)
			{
				buildHandlerStatusMods.arcanes[k] = DoStatusWorkWithArcane(showFullAmountOfThings, weaponBuild.arcane[k], list3);
				if (weaponBuild.arcane[k].full)
				{
					num5++;
				}
			}
			for (int l = weaponBuild.arcane?.Count ?? 0; l < buildHandlerStatusMods.arcanes.Length; l++)
			{
				buildHandlerStatusMods.arcanes[l] = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			}
			if (modDrain > 30)
			{
				num4 = 1;
			}
			buildHandlerStatusMods.modCapacity = modDrain;
			buildHandlerStatusMods.totalCapacity = ((num4 > 0) ? 60 : 30);
			if (num4 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "orokinCatalyst",
					tooltip = "Orokin Catalyst",
					amount = num4.ToString()
				});
			}
			if (num3 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "exilusAdapter",
					tooltip = "Exilus Adapter",
					amount = num3.ToString()
				});
			}
			if (num2 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "stanceForma",
					tooltip = "Stance Forma",
					amount = num2.ToString()
				});
			}
			if (num > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "forma",
					tooltip = "Forma",
					amount = num.ToString()
				});
			}
			if (num5 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "arcaneAdapter",
					tooltip = "Arcane Adapter",
					amount = num5.ToString()
				});
			}
			if (endoCost > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "endo",
					tooltip = "Endo",
					amount = endoCost.GetSIRepresentation(1)
				});
			}
			if (list2.Count > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "mods",
					tooltip = string.Join("\n", list2),
					amount = list2.Count.ToString()
				});
			}
			if (list3.Count > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "arcanes",
					tooltip = string.Join("\n", list3),
					amount = list3.Count.ToString()
				});
			}
			return (list.ToArray(), buildHandlerStatusMods);
		}

		private (BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing[], BuildHandlerStatus.BuildHandlerStatusMods) DoWarframeSpecificStatusWork(bool showFullAmountOfThings)
		{
			List<BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing> list = new List<BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing>();
			BuildHandlerStatus.BuildHandlerStatusMods buildHandlerStatusMods = new BuildHandlerStatus.BuildHandlerStatusMods();
			buildHandlerStatusMods.mods = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod[8];
			buildHandlerStatusMods.arcanes = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod[2];
			BuildWarframeData orDefault = sourceDataFile.warframes.GetOrDefault(warframeBuild.metadata.itemUID);
			int modDrain = 0;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int endoCost = 0;
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			for (int i = 0; i < Math.Min(warframeBuild.modsSlots.Count, buildHandlerStatusMods.mods.Length); i++)
			{
				buildHandlerStatusMods.mods[i] = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.polarities[i], warframeBuild.modsSlots[i], ref endoCost, ref modDrain, list2, increasesCapacity: false, out var formaNeeded);
				if (formaNeeded)
				{
					num++;
				}
			}
			for (int j = warframeBuild.modsSlots.Count; j < buildHandlerStatusMods.mods.Length; j++)
			{
				buildHandlerStatusMods.mods[j] = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			}
			if (warframeBuild?.exilusSlot?.full == true)
			{
				num3++;
			}
			buildHandlerStatusMods.exilus = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.exilusPolarity, warframeBuild.exilusSlot, ref endoCost, ref modDrain, list2, increasesCapacity: false, out var formaNeeded2);
			if (formaNeeded2)
			{
				num++;
			}
			buildHandlerStatusMods.aura = DoStatusWorkWithMod(showFullAmountOfThings, orDefault.auraPolarity, warframeBuild.auraSlot, ref endoCost, ref modDrain, list2, increasesCapacity: true, out var formaNeeded3);
			if (formaNeeded3)
			{
				num2++;
			}
			for (int k = 0; k < Math.Min(warframeBuild.arcaneSlots.Count, buildHandlerStatusMods.arcanes.Length); k++)
			{
				buildHandlerStatusMods.arcanes[k] = DoStatusWorkWithArcane(showFullAmountOfThings, warframeBuild.arcaneSlots[k], list3);
			}
			for (int l = warframeBuild.arcaneSlots.Count; l < buildHandlerStatusMods.arcanes.Length; l++)
			{
				buildHandlerStatusMods.arcanes[l] = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			}
			if (modDrain > 30)
			{
				num4 = 1;
			}
			buildHandlerStatusMods.modCapacity = modDrain;
			buildHandlerStatusMods.totalCapacity = ((num4 > 0) ? 60 : 30);
			if (num4 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "orokinCatalyst",
					tooltip = "Orokin Catalyst",
					amount = num4.ToString()
				});
			}
			if (num3 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "exilusAdapter",
					tooltip = "Exilus Adapter",
					amount = num3.ToString()
				});
			}
			if (num2 > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "auraForma",
					tooltip = "Aura Forma",
					amount = num2.ToString()
				});
			}
			if (num > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "forma",
					tooltip = "Forma",
					amount = num.ToString()
				});
			}
			if (endoCost > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "endo",
					tooltip = "Endo",
					amount = endoCost.GetSIRepresentation(1)
				});
			}
			if (list2.Count > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "mods",
					tooltip = string.Join("\n", list2),
					amount = list2.Count.ToString()
				});
			}
			if (list3.Count > 0)
			{
				list.Add(new BuildHandlerStatus.BuildHandlerStatusResults.BuildHandlerStatusSingleNeededThing
				{
					internalName = "arcanes",
					tooltip = string.Join("\n", list3),
					amount = list3.Count.ToString()
				});
			}
			return (list.ToArray(), buildHandlerStatusMods);
		}

		private BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod DoStatusWorkWithArcane(bool showFullAmountOfThings, BaseBuild.UpgradeSlot buildSlot, List<string> unownedArcaneNames)
		{
			BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod buildHandlerStatusSingleMod = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			buildHandlerStatusSingleMod.used = buildSlot.full;
			if (!buildHandlerStatusSingleMod.used)
			{
				return buildHandlerStatusSingleMod;
			}
			BuildUpgradeData orDefault = sourceDataFile.arcanes.GetOrDefault(buildSlot.uniqueName);
			buildHandlerStatusSingleMod.internalName = buildSlot.uniqueName;
			buildHandlerStatusSingleMod.name = orDefault?.name ?? "???";
			buildHandlerStatusSingleMod.modType = orDefault.rarity;
			buildHandlerStatusSingleMod.currentLevel = buildSlot.level;
			buildHandlerStatusSingleMod.maxLevel = orDefault.maxLvl;
			buildHandlerStatusSingleMod.picture = Misc.GetFullImagePath(basicRemoteData.items.GetOrDefault(buildSlot.uniqueName)?.pic);
			buildHandlerStatusSingleMod.longDescription = GetArcaneFullDescription(buildSlot.uniqueName);
			buildHandlerStatusSingleMod.owned = true;
			if (!IsModOwned(buildSlot.uniqueName, buildSlot.level, out var maxLevelUnder) || showFullAmountOfThings)
			{
				if (maxLevelUnder == -1)
				{
					buildHandlerStatusSingleMod.owned = false;
					unownedArcaneNames.Add(orDefault.name + $" (Rank {buildSlot.level})");
				}
				else
				{
					unownedArcaneNames.Add(orDefault.name + $" (Level up from {maxLevelUnder} to {buildSlot.level})");
				}
			}
			return buildHandlerStatusSingleMod;
		}

		private string GetArcaneFullDescription(string uniqueName)
		{
			IEnumerable<string> enumerable = StaticData.dataHandler?.arcanes?.GetOrDefault(uniqueName)?.levelStats?.Last()?.stats;
			return Misc.ReplaceStringWithIcons(string.Join("<br> /", enumerable ?? Enumerable.Empty<string>()) ?? "???").Replace(":", ": ");
		}

		private string GetModFullDescription(string uniqueName)
		{
			IEnumerable<string> enumerable = StaticData.dataHandler?.mods?.GetOrDefault(uniqueName)?.levelStats?.Last()?.stats;
			return Misc.ReplaceStringWithIcons(string.Join("<br />", enumerable ?? Enumerable.Empty<string>()) ?? "???").Replace(":", ": ");
		}

		private BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod DoStatusWorkWithMod(bool showFullAmountOfThings, BuildUpgradeData.ModPolarity baseSlotPolarity, BaseBuild.UpgradeSlot buildSlot, ref int endoCost, ref int modDrain, List<string> unownedModNames, bool increasesCapacity, out bool formaNeeded)
		{
			BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod buildHandlerStatusSingleMod = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod();
			formaNeeded = false;
			if (buildSlot == null)
			{
				buildHandlerStatusSingleMod.slotPolarity = (buildHandlerStatusSingleMod.modPolarity = baseSlotPolarity);
				buildHandlerStatusSingleMod.used = false;
				return buildHandlerStatusSingleMod;
			}
			buildHandlerStatusSingleMod.used = buildSlot.full;
			buildHandlerStatusSingleMod.slotPolarity = baseSlotPolarity;
			if (buildSlot.formaPolarity != BuildUpgradeData.ModPolarity.Unchanged)
			{
				buildHandlerStatusSingleMod.slotPolarity = buildSlot.formaPolarity;
			}
			if (!buildHandlerStatusSingleMod.used)
			{
				return buildHandlerStatusSingleMod;
			}
			int modSlotCapacityUsage = GetModSlotCapacityUsage(buildSlot, baseSlotPolarity, increasesCapacity, out formaNeeded, out buildHandlerStatusSingleMod.polarityColor);
			modDrain += modSlotCapacityUsage;
			BuildUpgradeData orDefault = sourceDataFile.mods.GetOrDefault(buildSlot.uniqueName);
			buildHandlerStatusSingleMod.internalName = buildSlot.uniqueName;
			buildHandlerStatusSingleMod.name = orDefault?.name ?? "???";
			buildHandlerStatusSingleMod.currentLevel = buildSlot.level;
			buildHandlerStatusSingleMod.maxLevel = orDefault.maxLvl;
			buildHandlerStatusSingleMod.drain = Math.Abs(modSlotCapacityUsage);
			buildHandlerStatusSingleMod.modPolarity = orDefault.modPolarity;
			buildHandlerStatusSingleMod.modType = orDefault.rarity;
			buildHandlerStatusSingleMod.picture = Misc.GetFullImagePath(basicRemoteData.items.GetOrDefault(buildSlot.uniqueName)?.pic);
			buildHandlerStatusSingleMod.longDescription = GetModFullDescription(buildSlot.uniqueName);
			buildHandlerStatusSingleMod.owned = true;
			if (!IsModOwned(buildSlot.uniqueName, buildSlot.level, out var maxLevelUnder) || showFullAmountOfThings)
			{
				if (maxLevelUnder == -1)
				{
					unownedModNames.Add(orDefault.name);
					buildHandlerStatusSingleMod.owned = false;
				}
				endoCost += GetTotalEndoNeededAtLvl(orDefault.rarity, buildSlot.level) - GetTotalEndoNeededAtLvl(orDefault.rarity, maxLevelUnder);
			}
			new List<BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat>();
			buildHandlerStatusSingleMod.modStatsShowOther = false;
			buildHandlerStatusSingleMod.stats = GetModStatsArray(buildSlot.level, orDefault, out buildHandlerStatusSingleMod.modStatsShowOther);
			return buildHandlerStatusSingleMod;
		}

		private BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat[] GetModStatsArray(int modLevel, BuildUpgradeData modData, out bool modStatsShowOther)
		{
			modStatsShowOther = false;
			List<BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat> list = new List<BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat>();
			List<BuildUpgradeData.BuildModBuff> list2 = modData.buffs.Where((BuildUpgradeData.BuildModBuff x) => string.IsNullOrEmpty(x.strictCompat) || buildWeaponData.parents.Any((int p) => x.strictCompat == sourceDataFile.parentList[p])).ToList();
			Dictionary<BuildUpgradeData.BuildModBuff.ModBuffType, double> dictionary = null;
			if (modData.buffs.Any((BuildUpgradeData.BuildModBuff x) => x.dontDisplay))
			{
				dictionary = new Dictionary<BuildUpgradeData.BuildModBuff.ModBuffType, double>();
				foreach (BuildUpgradeData.BuildModBuff buff in list2.Where((BuildUpgradeData.BuildModBuff p) => p.dontDisplay))
				{
					if (modData.buffs.Any((BuildUpgradeData.BuildModBuff z) => !z.dontDisplay && z.type == buff.type))
					{
						double orDefault = dictionary.GetOrDefault(buff.type);
						dictionary[buff.type] = orDefault + buff.value;
					}
				}
			}
			foreach (BuildUpgradeData.BuildModBuff item in list2)
			{
				if (item.dontDisplay && dictionary != null && dictionary.ContainsKey(item.type))
				{
					continue;
				}
				string modStatNameOrNull = GetModStatNameOrNull(item.type, item.damageType, item.faction);
				if (modStatNameOrNull == null)
				{
					modStatsShowOther = true;
					continue;
				}
				double num = dictionary?.GetOrDefault(item.type) ?? 0.0;
				dictionary?.Remove(item.type);
				double num2 = (item.value + num) * (double)(1 + modLevel);
				string text = "";
				text = ((item.type != BuildUpgradeData.BuildModBuff.ModBuffType.FactionDamage) ? (((num2 >= 0.0) ? "+" : "") + (item.isPercentageOfBase ? (Math.Round(100.0 * num2, item.roundTo) + "%") : Math.Round(num2, item.roundTo).ToString())) : ("x" + Math.Round(1.0 + num2, item.roundTo)));
				if (item.maxStacks > 1)
				{
					text = $"{item.maxStacks}x" + text;
				}
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions = item.conditions;
				if (conditions != null && conditions.Count > 0)
				{
					text += "*";
				}
				BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat obj = new BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat
				{
					stat = modStatNameOrNull,
					value = text
				};
				List<BuildUpgradeData.BuildModBuff.ModBuffCondition> conditions2 = item.conditions;
				obj.conditional = conditions2 != null && conditions2.Count > 0;
				list.Add(obj);
			}
			return list.ToArray();
		}

		private string GetModStatNameOrNull(BuildUpgradeData.BuildModBuff.ModBuffType type, DamageType damageType = DamageType.None, Faction faction = Faction.None)
		{
			switch (type)
			{
			case BuildUpgradeData.BuildModBuff.ModBuffType.Health:
				return "health";
			case BuildUpgradeData.BuildModBuff.ModBuffType.ShieldCapacity:
				return "shield";
			case BuildUpgradeData.BuildModBuff.ModBuffType.Armor:
				return "armor";
			case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityStrength:
				return "strength";
			case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityRange:
				return "range";
			case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityEfficiency:
				return "efficiency";
			case BuildUpgradeData.BuildModBuff.ModBuffType.AbilityDuration:
				return "duration";
			case BuildUpgradeData.BuildModBuff.ModBuffType.SprintSpeed:
				return "sprintSpeed";
			case BuildUpgradeData.BuildModBuff.ModBuffType.CriticalChance:
				return "criticalChance";
			case BuildUpgradeData.BuildModBuff.ModBuffType.CriticalDamage:
				return "criticalMultiplier";
			case BuildUpgradeData.BuildModBuff.ModBuffType.StatusChance:
				return "statusChance";
			case BuildUpgradeData.BuildModBuff.ModBuffType.Multishot:
				return "multishot";
			case BuildUpgradeData.BuildModBuff.ModBuffType.FactionDamage:
				switch (faction)
				{
				case Faction.Corpus:
					return "damagetocorpus";
				case Faction.Grineer:
					return "damagetogrineer";
				case Faction.Infested:
					return "damagetoinfested";
				case Faction.Orokin:
					return "damagetocorrupted";
				case Faction.Murmur:
					return "damagetomurmur";
				case Faction.Sentient:
					return "damagetosentients";
				}
				break;
			case BuildUpgradeData.BuildModBuff.ModBuffType.AllDamages:
				return "damage";
			case BuildUpgradeData.BuildModBuff.ModBuffType.PercentBaseDamageOfStatus:
				switch (damageType)
				{
				case DamageType.Impact:
					return "impactDamage";
				case DamageType.Puncture:
					return "punctureDamage";
				case DamageType.Slash:
					return "slashDamage";
				case DamageType.Cold:
					return "coldDamage";
				case DamageType.Electricity:
					return "electricityDamage";
				case DamageType.Heat:
					return "heatDamage";
				case DamageType.Toxin:
					return "toxinDamage";
				case DamageType.Viral:
					return "viralDamage";
				case DamageType.Corrosive:
					return "corrosiveDamage";
				case DamageType.Gas:
					return "gasDamage";
				case DamageType.Magnetic:
					return "magneticDamage";
				case DamageType.Radiation:
					return "radiationDamage";
				case DamageType.Blast:
					return "blastDamage";
				}
				break;
			case BuildUpgradeData.BuildModBuff.ModBuffType.EnergyMax:
				return "energyMax";
			case BuildUpgradeData.BuildModBuff.ModBuffType.FireRate:
				return "fireRate";
			case BuildUpgradeData.BuildModBuff.ModBuffType.PunchThrough:
				return "punchThrough";
			case BuildUpgradeData.BuildModBuff.ModBuffType.ReloadSpeed:
				return "reloadTime";
			case BuildUpgradeData.BuildModBuff.ModBuffType.StatusDuration:
				return "statustime";
			case BuildUpgradeData.BuildModBuff.ModBuffType.MagazineCapacity:
				return "magazineCapacity";
			case BuildUpgradeData.BuildModBuff.ModBuffType.AmmoMaximum:
				return "ammomax";
			}
			return null;
		}

		public int GetModSlotCapacityUsage(BaseBuild.UpgradeSlot buildModSlot, BuildUpgradeData.ModPolarity baseSlotPolarity, bool increasesCapacity, out bool formaNeeded, out BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor polarityColor)
		{
			formaNeeded = false;
			polarityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.White;
			if (!buildModSlot.full)
			{
				return 0;
			}
			BuildUpgradeData.ModPolarity modPolarity = baseSlotPolarity;
			if (buildModSlot.formaPolarity != BuildUpgradeData.ModPolarity.Unchanged)
			{
				modPolarity = buildModSlot.formaPolarity;
			}
			formaNeeded = modPolarity != baseSlotPolarity;
			BuildUpgradeData orDefault = sourceDataFile.mods.GetOrDefault(buildModSlot.uniqueName);
			int baseDrain = orDefault.baseDrain;
			baseDrain = ((!increasesCapacity) ? (baseDrain + buildModSlot.level) : (baseDrain - buildModSlot.level));
			if (increasesCapacity)
			{
				if (modPolarity != BuildUpgradeData.ModPolarity.Universal)
				{
					if (orDefault.modPolarity == modPolarity)
					{
						polarityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Green;
						return baseDrain * 2;
					}
					polarityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Red;
					return (int)Math.Round((double)baseDrain * 0.75);
				}
				return baseDrain;
			}
			if (modPolarity != BuildUpgradeData.ModPolarity.Universal)
			{
				if (orDefault.modPolarity == modPolarity)
				{
					polarityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Green;
					return (int)Math.Ceiling((double)baseDrain * 0.5);
				}
				polarityColor = BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.PolarityColor.Red;
				return (int)Math.Round((double)baseDrain * 1.25);
			}
			return baseDrain;
		}

		public int GetHighestOwnedModLevel(string modInternalName, int maxLevel = -1)
		{
			List<int> list = (from x in StaticData.dataHandler?.warframeRootObject?.Upgrades
				where x.ItemType == modInternalName
				select x.TryGetModRank() into p
				where maxLevel == -1 || p >= maxLevel
				orderby p descending
				select p).ToList();
			if (list.Count == 0)
			{
				return -1;
			}
			return list.First();
		}

		public bool IsModOwned(string modInternalName, int desiredLevel, out int maxLevelUnder)
		{
			maxLevelUnder = GetHighestOwnedModLevel(modInternalName, desiredLevel);
			if (maxLevelUnder == desiredLevel)
			{
				return maxLevelUnder != -1;
			}
			return false;
		}

		public int GetTotalEndoNeededAtLvl(BuildUpgradeData.ModMaterial modType, int level)
		{
			return (int)(10.0 * (Math.Pow(2.0, level) - 1.0) * GetModTypeEndoMultipler(modType));
		}

		public static double GetModTypeEndoMultipler(BuildUpgradeData.ModMaterial modType)
		{
			switch (modType)
			{
			case BuildUpgradeData.ModMaterial.Bronze:
				return 1.0;
			case BuildUpgradeData.ModMaterial.Silver:
				return 2.0;
			case BuildUpgradeData.ModMaterial.Gold:
			case BuildUpgradeData.ModMaterial.Riven:
				return 3.0;
			case BuildUpgradeData.ModMaterial.Primed:
			case BuildUpgradeData.ModMaterial.Archon:
			case BuildUpgradeData.ModMaterial.Amalgam:
			case BuildUpgradeData.ModMaterial.Galvanized:
				return 4.0;
			default:
				return 0.0;
			}
		}

		public void SwitchMods(SwitchModPositionRequest request)
		{
			if (request.currentItemIndex < 0 || request.currentItemIndex > 1)
			{
				throw new Exception("Invalid index");
			}
			if (request.fromIndex == request.toIndex)
			{
				return;
			}
			List<BaseBuild.UpgradeSlot> list = ((!(request.specialSlotName == "arcane")) ? ((request.currentItemIndex == 0) ? warframeBuild.modsSlots : weaponBuild.modsSlots) : ((request.currentItemIndex == 0) ? warframeBuild.arcaneSlots : weaponBuild.arcane));
			if (request.fromIndex < 0 || request.fromIndex >= list.Count)
			{
				throw new Exception("Invalid from index");
			}
			if (request.toIndex < 0 || request.toIndex >= list.Count)
			{
				throw new Exception("Invalid to index");
			}
			if (!list[request.fromIndex].full)
			{
				throw new Exception("From index is empty");
			}
			if (request.saveUndoHistory)
			{
				SaveCurrentStateInUndoHistory();
			}
			BuildUpgradeData.ModPolarity formaPolarity = list[request.fromIndex].formaPolarity;
			BuildUpgradeData.ModPolarity formaPolarity2 = list[request.toIndex].formaPolarity;
			BaseBuild.UpgradeSlot value = list[request.fromIndex];
			list[request.fromIndex] = list[request.toIndex];
			list[request.toIndex] = value;
			list[request.fromIndex].formaPolarity = formaPolarity;
			list[request.toIndex].formaPolarity = formaPolarity2;
			if (request.currentItemIndex == 0)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
				{
					damageCalculatorInstance.SetWarframeBuild(warframeBuild);
				}
			}
			else if (request.currentItemIndex == 1)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance2 in damageCalculatorInstances)
				{
					damageCalculatorInstance2.SetWeaponBuild(weaponBuild);
				}
			}
			BuildSetupJustChanged();
		}

		public void RemoveModFromSlot(RemoveModPositionRequest request)
		{
			if (request.currentItemIndex < 0 || request.currentItemIndex > 1)
			{
				throw new Exception("Invalid currentItemIndex");
			}
			BaseBuild.UpgradeSlot upgradeSlot = null;
			if (request.specialSlotName == "arcane")
			{
				List<BaseBuild.UpgradeSlot> list = ((request.currentItemIndex == 0) ? warframeBuild.arcaneSlots : weaponBuild.arcane);
				if (request.index < 0 || request.index >= list.Count)
				{
					throw new Exception("Invalid index");
				}
				upgradeSlot = list[request.index];
			}
			else
			{
				switch (request.specialSlotName)
				{
				case "aura":
					if (request.currentItemIndex == 0)
					{
						upgradeSlot = warframeBuild.auraSlot;
						break;
					}
					throw new Exception("Invalid aura slot for weapon");
				case "exilus":
					upgradeSlot = ((request.currentItemIndex != 0) ? weaponBuild.exilusSlot : warframeBuild.exilusSlot);
					break;
				case "stance":
					if (request.currentItemIndex == 0)
					{
						throw new Exception("Invalid stance slot for warframe");
					}
					upgradeSlot = weaponBuild.stance;
					break;
				case "mod":
				{
					List<BaseBuild.UpgradeSlot> list2 = ((request.currentItemIndex == 0) ? warframeBuild.modsSlots : weaponBuild.modsSlots);
					if (request.index < 0 || request.index >= list2.Count)
					{
						throw new Exception("Invalid index");
					}
					upgradeSlot = list2[request.index];
					break;
				}
				default:
					throw new Exception("Invalid specialSlotName");
				}
			}
			if (upgradeSlot == null)
			{
				throw new Exception("Invalid slot to clear");
			}
			if (!upgradeSlot.full)
			{
				throw new Exception("Slot is already empty");
			}
			if (request.saveUndoHistory)
			{
				SaveCurrentStateInUndoHistory();
			}
			upgradeSlot.ClearMod();
			if (request.currentItemIndex == 0)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
				{
					damageCalculatorInstance.SetWarframeBuild(warframeBuild);
				}
			}
			else if (request.currentItemIndex == 1)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance2 in damageCalculatorInstances)
				{
					damageCalculatorInstance2.SetWeaponBuild(weaponBuild);
				}
			}
			BuildSetupJustChanged();
		}

		public void GetModBrowser(ModBrowserRequest request)
		{
			Guid thisBrowserInstanceGuid = Guid.NewGuid();
			currentModBrowserInstanceGUID = thisBrowserInstanceGuid;
			ModBrowserResponse toReturn = new ModBrowserResponse();
			string category = request.category;
			Dictionary<string, BuildUpgradeData>.ValueCollection valueCollection = ((category == "arcane") ? sourceDataFile.arcanes.Values : ((!(category == "shard")) ? sourceDataFile.mods.Values : sourceDataFile.shards.Values));
			Dictionary<string, BuildUpgradeData>.ValueCollection source = valueCollection;
			HashSet<string> existingUpgradeParents = null;
			if (request.category == "mod")
			{
				BuildUpgradeData[] source2 = (from x in (request.currentItemIndex == 0) ? warframeBuild.modsSlots : weaponBuild.modsSlots
					select sourceDataFile.mods.GetOrDefault(x.uniqueName) into x
					where x != null
					select x).ToArray();
				existingUpgradeParents = source2.Select((BuildUpgradeData x) => x.uniqueName).Union(source2.SelectMany((BuildUpgradeData x) => x.parents.Select((int p) => sourceDataFile.parentList[p]))).ToHashSet();
				existingUpgradeParents.Remove("/Lotus/Types/Game/LotusArtifactUpgrade");
				existingUpgradeParents.Remove("/Lotus/Types/Game/LotusArtifactUpgrades/BaseArtifactUpgrade");
			}
			toReturn.mods = (from p in (from p in source.Where((BuildUpgradeData x) => ShouldShowUpgradeInModBrowser(x, request)).Select(delegate(BuildUpgradeData modData)
					{
						ModBrowserResponse.ModBrowserSingleMod modBrowserSingleMod = new ModBrowserResponse.ModBrowserSingleMod();
						modBrowserSingleMod.name = modData.name;
						modBrowserSingleMod.uniqueName = modData.uniqueName;
						modBrowserSingleMod.picture = Misc.GetFullImagePath(basicRemoteData.items.GetOrDefault(modData.uniqueName)?.pic);
						modBrowserSingleMod.modType = modData.rarity;
						modBrowserSingleMod.description = ((request.category == "arcane") ? GetArcaneFullDescription(modData.uniqueName) : GetModFullDescription(modData.uniqueName));
						modBrowserSingleMod.modPolarity = modData.modPolarity;
						modBrowserSingleMod.owned = IsModOwned(modData.uniqueName, 0, out var maxLevelUnder) || maxLevelUnder != -1;
						modBrowserSingleMod.stats = GetModStatsArray(modData.maxLvl, modData, out modBrowserSingleMod.statsShowOther);
						modBrowserSingleMod.dps = 0.0;
						modBrowserSingleMod.placingFilter = request.category;
						modBrowserSingleMod.compatible = existingUpgradeParents == null || (!existingUpgradeParents.Contains(modData.uniqueName) && !modData.parents.Any((int u) => existingUpgradeParents.Contains(sourceDataFile.parentList[u])));
						return modBrowserSingleMod;
					})
					orderby p.compatible descending, p.dps descending
					select p).ThenByDescending(delegate(ModBrowserResponse.ModBrowserSingleMod p)
				{
					BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat[] stats = p.stats;
					return (stats != null) ? stats.Length : 0;
				}).ThenByDescending((ModBrowserResponse.ModBrowserSingleMod p) => p.name)
				where FinalModBrowserSearchFilter(p, request)
				select p).ToArray();
			toReturn.progressTotal = toReturn.mods.Length;
			if (!StaticData.runOverwolfFunctionsAsync)
			{
				toReturn.status = ModBrowserResponse.ModBrowserStatus.FinishedOK;
			}
			if (request.selectedModSlotIndex == -1)
			{
				toReturn.status = ModBrowserResponse.ModBrowserStatus.NoModSelected;
			}
			if (currentEnemySetup.enemyEntries.Count == 0)
			{
				toReturn.status = ModBrowserResponse.ModBrowserStatus.NoEnemySelected;
			}
			StaticData.overwolfWrappwer.OnBuildsModBrowserUpdateInvoke(true, JsonConvert.SerializeObject(toReturn));
			if (toReturn.status != ModBrowserResponse.ModBrowserStatus.Calculating || !StaticData.runOverwolfFunctionsAsync)
			{
				return;
			}
			CancellationTokenSource modBrowserCancellationTokenSource = new CancellationTokenSource();
			string baseWarframeBuild = JsonConvert.SerializeObject(warframeBuild);
			string baseWeaponBuild = JsonConvert.SerializeObject(weaponBuild);
			ConcurrentQueue<string> modsLeftToAnalyze = new ConcurrentQueue<string>();
			modsLeftToAnalyze.Enqueue(null);
			toReturn.mods.Select((ModBrowserResponse.ModBrowserSingleMod x) => x.uniqueName).ToList().ForEach(delegate(string x)
			{
				modsLeftToAnalyze.Enqueue(x);
			});
			ManualResetEvent baselineReadySemaphore = new ManualResetEvent(initialState: false);
			double baselineTTK = -1.0;
			DamageCalculatorInstance[] array = modBrowserCalculatingInstances;
			foreach (DamageCalculatorInstance instance in array)
			{
				Task.Run(delegate
				{
					while (true)
					{
						if (!modsLeftToAnalyze.TryDequeue(out var newModUID) || !(thisBrowserInstanceGuid == currentModBrowserInstanceGUID) || toReturn.status != ModBrowserResponse.ModBrowserStatus.Calculating)
						{
							break;
						}
						try
						{
							WarframeBuild warframeBuild = JsonConvert.DeserializeObject<WarframeBuild>(baseWarframeBuild);
							WeaponBuild weaponBuild = JsonConvert.DeserializeObject<WeaponBuild>(baseWeaponBuild);
							if (newModUID != null)
							{
								if (request.selectedModSlotIndex == -1)
								{
									request.selectedModSlotIndex = 0;
								}
								if (request.currentItemIndex == 0)
								{
									warframeBuild.SetNewModInSlot(request.category, request.selectedModSlotIndex, newModUID);
								}
								else
								{
									weaponBuild.SetNewModInSlot(request.category, request.selectedModSlotIndex, newModUID);
								}
							}
							instance.SetWarframeBuild(warframeBuild);
							instance.SetWeaponBuild(weaponBuild);
							SimulationResults simulationResults = instance.DoCompleteSimulation(modBrowserCancellationTokenSource.Token, TimeSpan.FromSeconds(1.0), TimeSpan.FromMilliseconds(15.0));
							StaticData.Log(OverwolfWrapper.LogType.INFO, "Simulation for mod " + newModUID + " finished with result: " + simulationResults.state.ToString() + " and TTK: " + simulationResults.elapsedSimulatedTime + "s. Real time: " + simulationResults.elapsedRealTime);
							if (newModUID == null)
							{
								if (simulationResults.state == SimulationResults.SimulationResultState.Finished)
								{
									if (simulationResults.averageTTKTicks <= 1.0)
									{
										toReturn.status = ModBrowserResponse.ModBrowserStatus.TooEasy;
									}
									else if (simulationResults.elapsedRealTime > 500.0)
									{
										toReturn.status = ModBrowserResponse.ModBrowserStatus.TooHard;
									}
									else
									{
										baselineTTK = simulationResults.setupTTK;
									}
								}
								else
								{
									StaticData.Log(OverwolfWrapper.LogType.ERROR, "Baseline TTK failed with result: " + simulationResults.state);
									if (simulationResults.state == SimulationResults.SimulationResultState.Timeout)
									{
										toReturn.status = ModBrowserResponse.ModBrowserStatus.TooHard;
									}
									modBrowserCancellationTokenSource.Cancel();
								}
								StaticData.overwolfWrappwer.OnBuildsModBrowserUpdateInvoke(true, JsonConvert.SerializeObject(toReturn));
								baselineReadySemaphore.Set();
								break;
							}
							if (!baselineReadySemaphore.WaitOne(TimeSpan.FromSeconds(5.0)))
							{
								StaticData.Log(OverwolfWrapper.LogType.ERROR, "Baseline TTK not ready in time");
								break;
							}
							if (toReturn.status != ModBrowserResponse.ModBrowserStatus.Calculating)
							{
								break;
							}
							Interlocked.Increment(ref toReturn.progressCurrent);
							if (toReturn.progressCurrent == toReturn.progressTotal && toReturn.status == ModBrowserResponse.ModBrowserStatus.Calculating)
							{
								toReturn.status = ModBrowserResponse.ModBrowserStatus.FinishedOK;
							}
							if (simulationResults.state == SimulationResults.SimulationResultState.Finished)
							{
								double value = (baselineTTK - simulationResults.setupTTK) / baselineTTK * 100.0;
								toReturn.mods.FirstOrDefault((ModBrowserResponse.ModBrowserSingleMod x) => x.uniqueName == newModUID).dps = Math.Round(value, 1);
							}
							else
							{
								StaticData.Log(OverwolfWrapper.LogType.ERROR, "Simulation for mod " + newModUID + " failed with result: " + simulationResults.state);
							}
							if (thisBrowserInstanceGuid != currentModBrowserInstanceGUID)
							{
								break;
							}
							StaticData.overwolfWrappwer.OnBuildsModBrowserUpdateInvoke(true, JsonConvert.SerializeObject(toReturn));
						}
						catch (Exception ex)
						{
							StaticData.overwolfWrappwer.OnBuildsModBrowserUpdateInvoke(false, ex.Message);
						}
					}
				});
			}
		}

		private bool FinalModBrowserSearchFilter(ModBrowserResponse.ModBrowserSingleMod upgrade, ModBrowserRequest request)
		{
			if (upgrade.name.IndexOf(request.search, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}
			if (upgrade.description.IndexOf(request.search, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}
			if (upgrade.stats.Any((BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat x) => x.stat.Replace(" ", "").IndexOf(request.search.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) != -1))
			{
				return true;
			}
			if (upgrade.stats.Any((BuildHandlerStatus.BuildHandlerStatusMods.BuildHandlerStatusSingleMod.ModStat x) => x.value.IndexOf(request.search, StringComparison.OrdinalIgnoreCase) != -1))
			{
				return true;
			}
			return false;
		}

		private bool ShouldShowUpgradeInModBrowser(BuildUpgradeData upgrade, ModBrowserRequest request)
		{
			if (upgrade.conclaveOnly)
			{
				return false;
			}
			switch (request.category)
			{
			case "aura":
				if (upgrade.type != BuildUpgradeData.ModType.Aura)
				{
					return false;
				}
				break;
			case "exilus":
				if (!upgrade.exilus)
				{
					return false;
				}
				break;
			case "stance":
				if (upgrade.type != BuildUpgradeData.ModType.Stance)
				{
					return false;
				}
				break;
			case "mod":
				if (upgrade.type == BuildUpgradeData.ModType.Aura || upgrade.type == BuildUpgradeData.ModType.Stance)
				{
					return false;
				}
				if (upgrade.exilus)
				{
					return false;
				}
				break;
			default:
				if (upgrade.exilus)
				{
					return false;
				}
				break;
			}
			bool flag = false;
			int[] array = ((request.currentItemIndex == 0) ? buildWarframeData.parents : buildWeaponData.parents);
			for (int i = 0; i < array.Length; i++)
			{
				if (upgrade.compatUID == sourceDataFile.parentList[array[i]] || sourceDataFile.parentList[array[i]].EndsWith("/" + upgrade.compatUID))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			return true;
		}

		private void SaveCurrentStateInUndoHistory()
		{
		}

		public void PlaceNewMod(PlaceNewModRequest request)
		{
			BaseBuild.UpgradeSlot upgradeSlot = null;
			if (request.currentItemIndex == 0)
			{
				switch (request.specialSlotName)
				{
				case "aura":
					upgradeSlot = warframeBuild.auraSlot;
					break;
				case "exilus":
					upgradeSlot = warframeBuild.exilusSlot;
					break;
				case "mod":
				{
					List<BaseBuild.UpgradeSlot> modsSlots = warframeBuild.modsSlots;
					if (request.index < 0 || request.index >= modsSlots.Count)
					{
						throw new Exception("Invalid index");
					}
					upgradeSlot = modsSlots[request.index];
					break;
				}
				case "arcane":
				{
					List<BaseBuild.UpgradeSlot> arcaneSlots = warframeBuild.arcaneSlots;
					if (request.index < 0 || request.index >= arcaneSlots.Count)
					{
						throw new Exception("Invalid index");
					}
					upgradeSlot = arcaneSlots[request.index];
					break;
				}
				default:
					throw new Exception("Invalid specialSlotName");
				}
			}
			else
			{
				if (request.currentItemIndex != 1)
				{
					throw new Exception("Incalid currentItemIndex");
				}
				switch (request.specialSlotName)
				{
				case "stance":
					upgradeSlot = weaponBuild.stance;
					break;
				case "exilus":
					upgradeSlot = weaponBuild.exilusSlot;
					break;
				case "mod":
				{
					List<BaseBuild.UpgradeSlot> modsSlots2 = weaponBuild.modsSlots;
					if (request.index < 0 || request.index >= modsSlots2.Count)
					{
						throw new Exception("Invalid index");
					}
					upgradeSlot = modsSlots2[request.index];
					break;
				}
				case "arcane":
				{
					List<BaseBuild.UpgradeSlot> arcane = weaponBuild.arcane;
					if (request.index < 0 || request.index >= arcane.Count)
					{
						throw new Exception("Invalid index");
					}
					upgradeSlot = arcane[request.index];
					break;
				}
				default:
					throw new Exception("Invalid specialSlotName");
				}
			}
			upgradeSlot.full = true;
			upgradeSlot.uniqueName = request.uniqueID;
			BuildUpgradeData orDefault = sourceDataFile.mods.GetOrDefault(request.uniqueID);
			if (orDefault == null)
			{
				throw new Exception("Mod not found");
			}
			upgradeSlot.level = orDefault.maxLvl;
			if (request.currentItemIndex == 0)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
				{
					damageCalculatorInstance.SetWarframeBuild(warframeBuild);
				}
			}
			else if (request.currentItemIndex == 1)
			{
				foreach (DamageCalculatorInstance damageCalculatorInstance2 in damageCalculatorInstances)
				{
					damageCalculatorInstance2.SetWeaponBuild(weaponBuild);
				}
			}
			BuildSetupJustChanged();
		}

		public void SetBuildSettings(SetBuildSettingsRequest request)
		{
			weaponModeIndex = request.weaponModeID;
			weaponZoomLevelIndex = request.weaponZoomLevelID;
			foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
			{
				damageCalculatorInstance.WeaponChangeSelectedMode(request.weaponModeID);
				damageCalculatorInstance.WeaponChangeSelectedZoomLevel(request.weaponZoomLevelID);
			}
		}

		public EnemySetupListResponse GetEnemySetupList()
		{
			return new EnemySetupListResponse
			{
				availablePresetNames = (from p in availableEnemySetups
					select p.metadata?.name into p
					where p != null
					select p).ToList(),
				selectedPreset = currentEnemySetup?.metadata?.name
			};
		}

		public void SelectEnemySetup(EnemySetupSelectRequest request)
		{
			currentEnemySetup = availableEnemySetups.FirstOrDefault((EnemySetup p) => p.metadata?.name == request.presetName);
			if (currentEnemySetup == null)
			{
				throw new Exception("Preset not found");
			}
			foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
			{
				damageCalculatorInstance.SetEnemySetup(currentEnemySetup);
			}
			BuildSetupJustChanged();
		}

		public EnemyCustomizationGetEnemiesResponse GetEnemyCustomizationEnemies(EnemyCustomizationGetEnemiesRequest request)
		{
			return new EnemyCustomizationGetEnemiesResponse
			{
				enemies = (from p in sourceDataFile.enemies.Values
					where (string.IsNullOrEmpty(request.search) || p.name.IndexOf(request.search, StringComparison.OrdinalIgnoreCase) != -1) ? true : false
					select new EnemyCustomizationGetEnemiesResponse.EnemyOutputListData
					{
						faction = p.@group,
						name = p.name + (p.eximus ? " Eximus" : ""),
						picture = Misc.GetFullImagePath(p.picture),
						hasArmor = (p.armor > 0.0),
						hasShield = (p.shield > 0.0),
						isBoss = (p.armourConstant > 0.0),
						isEximus = p.eximus,
						uniqueName = p.uniqueName
					}).Where(delegate(EnemyCustomizationGetEnemiesResponse.EnemyOutputListData p)
				{
					if (request.faction != BuildEnemyData.FilteringGroup.All && request.faction != p.faction)
					{
						return false;
					}
					if (!string.IsNullOrEmpty(request.isBoss) && bool.Parse(request.isBoss) != p.isBoss)
					{
						return false;
					}
					return (string.IsNullOrEmpty(request.isEximus) || bool.Parse(request.isEximus) == p.isEximus) ? true : false;
				}).Take(request.showAll ? 999999 : 100).ToList()
			};
		}

		public EnemyCustomizationGetCurrentEnemiesStatusResponse GetCurrentEnemyCustomizationStatus()
		{
			return new EnemyCustomizationGetCurrentEnemiesStatusResponse
			{
				enemies = currentEnemySetup.enemyEntries.Select(delegate(EnemySetup.EnemyEntry p)
				{
					BuildEnemyData orDefault = sourceDataFile.enemies.GetOrDefault(p.info.uniqueName);
					return new EnemyCustomizationGetCurrentEnemiesStatusResponse.CurrentEnemyOutput
					{
						name = orDefault?.name + (orDefault.eximus ? " Eximus" : ""),
						picture = Misc.GetFullImagePath(orDefault?.picture),
						level = p.info.level,
						amount = p.amount,
						faction = orDefault.group,
						armor = ((int)EnemyUtils.GetEnemyArmor(orDefault.armor, p.info.level, orDefault.baseLevel, p.info.steelPath, orDefault.eximus)).GetSIRepresentation(1),
						shield = ((int)EnemyUtils.GetEnemyShield(orDefault.shield, p.info.level, orDefault.baseLevel, p.info.steelPath, orDefault.eximus)).GetSIRepresentation(1),
						health = ((int)EnemyUtils.GetEnemyHealth(orDefault.health, p.info.level, orDefault.baseLevel, p.info.steelPath, orDefault.eximus)).GetSIRepresentation(1),
						isEximus = orDefault.eximus
					};
				}).ToList()
			};
		}

		public void AddEnemyToCustomization(EnemyCustomizationAddEnemyRequest request)
		{
			currentEnemySetup.enemyEntries.Add(new EnemySetup.EnemyEntry(new EnemySetup.EnemyInfo(request.uniqueName, 100), 1));
			foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
			{
				damageCalculatorInstance.SetEnemySetup(currentEnemySetup);
			}
			BuildSetupJustChanged();
		}

		public void EditEnemyInCustomization(EnemyCustomizationEditEnemyRequest request)
		{
			if (request.index < 0 || request.index >= currentEnemySetup.enemyEntries.Count)
			{
				throw new Exception("Invalid index");
			}
			if (request.amount <= 0)
			{
				currentEnemySetup.enemyEntries.RemoveAt(request.index);
			}
			else
			{
				currentEnemySetup.enemyEntries[request.index].info.level = request.level;
				currentEnemySetup.enemyEntries[request.index].amount = request.amount;
			}
			foreach (DamageCalculatorInstance damageCalculatorInstance in damageCalculatorInstances)
			{
				damageCalculatorInstance.SetEnemySetup(currentEnemySetup);
			}
			BuildSetupJustChanged();
		}

		public void BuildSetupJustChanged()
		{
			Task.Run(delegate
			{
				StaticData.overwolfWrappwer.OnSimulationResultsUpdateInvoke("running", "{}");
				try
				{
					SimulationResults value = damageResultsSimulatorInstance.DoCompleteSimulation(CancellationToken.None, TimeSpan.FromSeconds(3.0), TimeSpan.FromMilliseconds(15.0));
					StaticData.overwolfWrappwer.OnSimulationResultsUpdateInvoke("results", JsonConvert.SerializeObject(value));
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, $"An error has occurred when simulating build: {ex}");
					StaticData.overwolfWrappwer.OnSimulationResultsUpdateInvoke("error", ex.Message);
				}
			});
		}
	}
}
