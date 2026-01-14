using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public class FoundryItemData
	{
		public class FoundryArchonShard
		{
			public string type;

			public string text;

			private static readonly Dictionary<string, string> shardTypes = new Dictionary<string, string>
			{
				{ "ACC_RED", "red" },
				{ "ACC_YELLOW", "yellow" },
				{ "ACC_BLUE", "blue" },
				{ "ACC_RED_MYTHIC", "red_mythic" },
				{ "ACC_YELLOW_MYTHIC", "yellow_mythic" },
				{ "ACC_BLUE_MYTHIC", "blue_mythic" },
				{ "ACC_GREEN", "green" },
				{ "ACC_GREEN_MYTHIC", "green_mythic" },
				{ "ACC_ORANGE", "orange" },
				{ "ACC_ORANGE_MYTHIC", "orange_mythic" },
				{ "ACC_PURPLE", "purple" },
				{ "ACC_PURPLE_MYTHIC", "purple_mythic" }
			};

			private static readonly Dictionary<string, string> shardUpgradeMessages = new Dictionary<string, string>
			{
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeHealthMax", "+150 Health" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeHealthMaxMythic", "+225 Health" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeShieldMax", "+150 Shield" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeShieldMaxMythic", "+225 Shield" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeEnergyMax", "+50 Energy max" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeEnergyMaxMythic", "+75 Energy max" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeArmourMax", "+150 Armor" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeArmourMaxMythic", "+225 Armor" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeRegen", "+5 Regen" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeRegenMythic", "+7.5 Regen" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeMeleeCritDamage", "+25% Melee crit DMG" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeMeleeCritDamageMythic", "+37.5% Melee crit DMG" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradePrimaryStatusChance", "+25% Primary status %" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradePrimaryStatusChanceMythic", "+37.5% Primary status %" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeSecondaryCritChance", "+25% Secondary crit %" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeSecondaryCritChanceMythic", "+37.5% Secondary crit %" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeAbilityDuration", "+10% Duration" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeAbilityDurationMythic", "+15% Duration" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeAbilityStrength", "+10% Strength" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeAbilityStrengthMythic", "+15% Strength" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeStartingEnergy", "+30% Spawn energy" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeStartingEnergyMythic", "+45% Spawn energy" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeGlobeEffectHealth", "+100% Orb health" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeGlobeEffectHealthMythic", "+150% Orb health" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeGlobeEffectEnergy", "+50% Orb energy" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeGlobeEffectEnergyMythic", "+75% Orb energy" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCastingSpeed", "+25% Cast speed" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCastingSpeedMythic", "+37.5% Cast speed" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeParkourVelocity", "+15% Parkour velocity" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeParkourVelocityMythic", "+22.5% Parkour velocity" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeElectricDamageBoost", "+10% Ab. DMG on enemies with by <DT_ELECTRICITY>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeElectricDamageBoostMythic", "+15% Ab. DMG on enemies with by <DT_ELECTRICITY>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeElectricDamage", "+30% Primary <DT_ELECTRICITY> (+10% per extra Shard)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeElectricDamageMythic", "+45% Primary <DT_ELECTRICITY> (+15% per extra Shard)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCritDamageBoost", "+25% Melee Crit DMG (2x over 500 Energy)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCritDamageBoostMythic", "+37% Melee Crit DMG (2x over 500 Energy)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeEquilibrium", "+20% Energy from Health pickups and vice versa" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeEquilibriumMythic", "+30% Energy from Health pickups and vice versa" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeHPBoostFromImpact", "+1 Health per enemy killed with <DT_BLAST> (Max 300 Health)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeHPBoostFromImpactMythic", "+2 Health per enemy killed with <DT_BLAST> (Max 450 Health)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeBlastProc", "+5 Shields on inflicting <DT_BLAST>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeBlastProcMythic", "+7.5 Shields on inflicting <DT_BLAST>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWeaponCritBoostFromHeat", "+1% Secondary Crit Chance per enemy with <DT_HEAT> killed (Max 50%)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWeaponCritBoostFromHeatMythic", "+1.5% Secondary Crit Chance per enemy with <DT_HEAT> killed (Max 75%)" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeRadiationDamageBoost", "+10% Ab. DMG on enemies with <DT_RADIATION>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeRadiationDamageBoostMythic", "+15% Ab DMG on enemies with <DT_RADIATION>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeToxinDamage", "+30% <DT_TOXIN> Status Effect damage" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeToxinDamageMythic", "+45% <DT_TOXIN> Status Effect damage" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeToxinHeal", "+2 Health on damaging enemies with <DT_TOXIN>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeToxinHealMythic", "+3 Health on damaging enemies with <DT_TOXIN>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCorrosiveDamageBoost", "+10% Ab. DMG on enemies with <DT_CORROSIVE>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCorrosiveDamageBoostMythic", "+15% Ab. DMG on enemies with <DT_CORROSIVE>" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCorrosiveStack", "+2 <DT_CORROSIVE> Corrosion Status max stacks" },
				{ "/Lotus/Upgrades/Invigorations/ArchonCrystalUpgrades/ArchonCrystalUpgradeWarframeCorrosiveStackMythic", "+3 <DT_CORROSIVE> Corrosion Status max stacks" }
			};

			public FoundryArchonShard(SuitArchonCrystalUpgrades waframeShardData)
			{
				type = shardTypes.GetOrDefault(waframeShardData.Color) ?? "???";
				text = Misc.ReplaceStringWithIcons(shardUpgradeMessages.GetOrDefault(waframeShardData.UpgradeType)) ?? "???";
			}
		}

		public string name;

		public string picture;

		public int minimumMastery;

		public string minimumMasteryText = "";

		public bool minimumMasteryAchieved;

		public string debugInfo = "";

		public bool vaulted;

		public bool vualtedMakesSense;

		public string extraVaultedInfo = "";

		public bool mastered;

		public bool owned;

		public bool pendingInFoundry;

		public bool neededForOtherStuff;

		public string neededForOtherStuffInfo = "";

		public string internalName = "";

		public bool isFav;

		public List<FoundryItemComponent> components;

		public int premiumCost;

		public string type = "";

		public string modularType = "";

		public bool helminthDone;

		public bool supportsHelminth;

		public bool incarnion;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<List<FoundryArchonShard>> archonShards;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public MasteryResponse.MasteryResponseContentRemainingData masteryViewData;

		[NonSerialized]
		public long XP;

		[NonSerialized]
		public BigItem itemReference;

		public FoundryItemData(DataWarframe warframeData)
		{
			type = "warframe";
			SetBasicItemData(warframeData);
			supportsHelminth = type == "warframe" && !name.Contains("Prime") && name != "Excalibur Umbra" && name != "Voidrig" && name != "Bonewidow";
			helminthDone = supportsHelminth && StaticData.dataHandler?.warframeRootObject?.InfestedFoundry?.ConsumedSuits?.Any((ConsumedSuit p) => p.s == warframeData.uniqueName) == true;
			archonShards = StaticData.dataHandler?.warframeRootObject?.Suits?.Where((Suit p) => p.ItemType == warframeData.uniqueName && p.ArchonCrystalUpgrades != null && p.ArchonCrystalUpgrades.Length != 0)?.Select((Suit p) => p?.ArchonCrystalUpgrades.Select((SuitArchonCrystalUpgrades k) => new FoundryArchonShard(k)).ToList()).ToList();
		}

		public FoundryItemData(ItemComponent component)
		{
			type = "component";
			isFav = FavouriteHelper.IsFavourite(component.uniqueName);
			name = component.name;
			picture = Misc.GetFullImagePath(component.imageName);
			internalName = component.uniqueName;
			owned = StaticData.dataHandler?.warframeRootObject?.MiscItems?.Any((Miscitem p) => p.ItemType == component.uniqueName) == true;
			owned = owned || StaticData.dataHandler?.warframeRootObject?.QuestKeys?.Any((Questkey p) => p.ItemType == component.uniqueName) == true;
			name = component.GetRealExternalName();
		}

		public FoundryItemData(DataSkin weaponData)
		{
			type = "companion";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataQuest quest)
		{
			type = "quest";
			SetBasicItemData(quest);
		}

		public FoundryItemData(DataMod modData)
		{
			type = "mod";
			SetBasicItemData(modData);
		}

		public FoundryItemData(DataPrimaryWeapon weaponData)
		{
			type = "primary";
			incarnion = StaticData.dataHandler?.warframeRootObject?.LongGuns?.Any((Longgun p) => p.ItemType == weaponData.uniqueName && !string.IsNullOrEmpty(p.SkillTree)) == true;
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataSecondaryWeapon weaponData)
		{
			type = "secondary";
			incarnion = StaticData.dataHandler?.warframeRootObject?.Pistols?.Any((Pistol p) => p.ItemType == weaponData.uniqueName && !string.IsNullOrEmpty(p.SkillTree)) == true;
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataMeleeWeapon weaponData)
		{
			if (weaponData.IsModular())
			{
				type = "modular";
				modularType = "Zaw";
			}
			else
			{
				type = "melee";
				incarnion = StaticData.dataHandler?.warframeRootObject?.Melee?.Any((Melee p) => p.ItemType == weaponData.uniqueName && !string.IsNullOrEmpty(p.SkillTree)) == true;
			}
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataArchGun weaponData)
		{
			type = "arch";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataArchMelee weaponData)
		{
			type = "arch";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataArchwing weaponData)
		{
			type = "arch";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataMisc weaponData)
		{
			if (weaponData.uniqueName.Contains("Hoverboard"))
			{
				type = "modular";
				modularType = "K-Drive";
				SetBasicItemData(weaponData);
			}
			else if (weaponData.uniqueName.Contains("/OperatorAmplifiers/"))
			{
				type = "modular";
				modularType = "Amp";
				SetBasicItemData(weaponData);
			}
			else
			{
				type = "modular";
				modularType = "Kitgun";
				SetBasicItemData(weaponData);
			}
		}

		public FoundryItemData(DataPet weaponData)
		{
			type = "companion";
			modularType = "pet";
			SetBasicItemData(weaponData);
			if (!owned && !mastered && weaponData.uniqueName.Contains("/Lotus/Types/Friendly/Pets/ZanukaPets/ZanukaPetParts/ZanukaPetPartHead"))
			{
				string fullZanucaName = $"/Lotus/Types/Friendly/Pets/ZanukaPets/ZanukaPet{weaponData.uniqueName.Last()}PowerSuit";
				if (StaticData.dataHandler.warframeRootObject != null)
				{
					owned = owned || StaticData.dataHandler.warframeRootObject.MoaPets.Any((Moapet k) => k.ItemType == fullZanucaName);
					mastered = mastered || StaticData.dataHandler.warframeRootObject.MoaPets.Any((Moapet k) => k.ItemType == fullZanucaName && k.XP > 900000);
				}
			}
			debugInfo = weaponData.uniqueName;
		}

		public FoundryItemData(DataSentinel weaponData)
		{
			type = "companion";
			modularType = "sentinel";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData(DataSentinelWeapons weaponData)
		{
			type = "companion";
			modularType = "sentinel_weapon";
			SetBasicItemData(weaponData);
		}

		public FoundryItemData()
		{
		}

		private void SetBasicItemData(BigItem bigItemData)
		{
			itemReference = bigItemData;
			isFav = FavouriteHelper.IsFavourite(bigItemData.uniqueName);
			name = bigItemData.name;
			picture = Misc.GetFullImagePath(bigItemData.imageName);
			internalName = bigItemData.uniqueName;
			if (bigItemData.masteryReq > 1)
			{
				minimumMastery = bigItemData.masteryReq;
				minimumMasteryText = "Minimum mastery level: " + bigItemData.masteryReq.ToString().InClass("tooltipDate");
			}
			else
			{
				minimumMastery = -1;
				minimumMasteryText = "";
			}
			mastered = bigItemData.IsFullyMastered();
			components = bigItemData.components?.Select((ItemComponent p) => new FoundryItemComponent(p)).ToList();
			FoundryItemComponent.ApplyMultipleRecipeSlotsFix(components);
			owned = bigItemData.IsOwned();
			XP = bigItemData.GetXP();
			pendingInFoundry = components?.Any((FoundryItemComponent p) => StaticData.dataHandler?.warframeRootObject?.PendingRecipes?.Any((Pendingrecipe u) => u.ItemType == p.uniqueName) == true) ?? false;
			owned = owned || pendingInFoundry;
			minimumMasteryAchieved = StaticData.dataHandler.warframeRootObject?.PlayerLevel >= bigItemData.masteryReq;
			vaulted = bigItemData.vaulted && name.Contains("Prime");
			if (vaulted)
			{
				if (!string.IsNullOrEmpty(bigItemData.vaultDate))
				{
					extraVaultedInfo = "Vaulted on " + bigItemData.vaultDate.Replace(" ", "-").InClass("tooltipDate");
				}
				extraVaultedInfo = "Vaulted";
			}
			else
			{
				if (!string.IsNullOrEmpty(bigItemData.estimatedVaultDate))
				{
					extraVaultedInfo = "Available. Estimated vault date: " + bigItemData.estimatedVaultDate.Replace(" ", "-").InClass("tooltipDate");
				}
				extraVaultedInfo = "Not vaulted (Available)";
			}
			if (bigItemData.isPartOf.Any())
			{
				neededForOtherStuff = true;
				neededForOtherStuffInfo = "<strong>Used for crafting:</strong> " + string.Join(", ", bigItemData.isPartOf.Select((BigItem p) => p.name));
			}
			else
			{
				neededForOtherStuff = false;
			}
			vualtedMakesSense = name?.ToLower().Contains("prime") ?? false;
			if (name == "Excalibur Prime" || name == "Lato Prime" || name == "Skana Prime")
			{
				vaulted = true;
			}
		}

		public FoundryItemData SetPremiumCurrencyCost(int cost)
		{
			premiumCost = cost;
			return this;
		}

		public bool IsPartOfFoundersPack()
		{
			if (internalName == "/Lotus/Powersuits/Excalibur/ExcaliburPrime")
			{
				return true;
			}
			if (internalName == "/Lotus/Weapons/Tenno/Pistol/LatoPrime")
			{
				return true;
			}
			if (internalName == "/Lotus/Weapons/Tenno/Melee/LongSword/SkanaPrime")
			{
				return true;
			}
			return false;
		}
	}
}
