using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsResponse
	{
		public enum DetailsFoundryType
		{
			UNKNOWN,
			Warframe,
			Primary,
			Secondary,
			Melee
		}

		public string title;

		public string imageURL;

		public string description;

		public string wikiLink;

		public bool isFav;

		public string internalName;

		public List<FoundryDetailsComponentsItem> components = new List<FoundryDetailsComponentsItem>();

		public string itemType;

		public FoundryDetailsExtraWarframeData extraWarframeData;

		public FoundryDetailsExtraWeaponShootData extraWeaponShootData;

		public FoundryDetailsExtraWeaponMeleeData extraWeaponMeleeData;

		public FoundryDetailsExtraModData extraModData;

		public bool neededForOtherStuff;

		public string neededForOtherStuffInfo;

		[NonSerialized]
		private BigItem referencedPart;

		[NonSerialized]
		public bool initializationSuccessful;

		public FoundryDetailsResponse(string uniqueID)
		{
			referencedPart = FindPartObjectFromUniqueID(uniqueID);
			if (referencedPart != null)
			{
				FillDetailsFromReferencedPart();
				if (referencedPart is DataWarframe)
				{
					itemType = "warframe";
					FillWarframeDetailsFromReferencedPart();
				}
				if (referencedPart is DataPrimaryWeapon)
				{
					itemType = "weaponShoot";
					FillPrimaryWeaponDetailsFromReferencedPart();
				}
				if (referencedPart is DataSecondaryWeapon)
				{
					itemType = "weaponShoot";
					FillSecondaryWeaponDetailsFromReferencedPart();
				}
				if (referencedPart is DataMeleeWeapon)
				{
					itemType = "weaponMelee";
					FillMeleeWeaponDetailsFromReferencedPart();
				}
				if (referencedPart is DataArchwing)
				{
					itemType = "warframe";
					FillArchwingDetailsFromReferencedPart();
				}
				if (referencedPart is DataArchGun)
				{
					itemType = "weaponShoot";
					FillShootArchDetailsFromReferencedPart();
				}
				if (referencedPart is DataArchMelee)
				{
					itemType = "weaponMelee";
					FillMeleeArchDetailsFromReferencedPart();
				}
				if (referencedPart is DataMod)
				{
					itemType = "mod";
					FillModDetailsFromReferencedPart();
				}
				if (referencedPart is DataArcane)
				{
					itemType = "arcane";
					FillModDetailsFromReferencedPart();
				}
				initializationSuccessful = true;
			}
		}

		private void FillMeleeArchDetailsFromReferencedPart()
		{
			extraWeaponMeleeData = new FoundryDetailsExtraWeaponMeleeData();
			DataArchMelee dataArchMelee = referencedPart as DataArchMelee;
			extraWeaponMeleeData.auraPolarity = null;
			extraWeaponMeleeData.polarities = dataArchMelee.polarities;
			extraWeaponMeleeData.rivenDisposition = (int)dataArchMelee.disposition;
			extraWeaponMeleeData.comboDuration = dataArchMelee.comboDuration;
			extraWeaponMeleeData.heavyAttackTime = "- ";
			extraWeaponMeleeData.range = Math.Round(dataArchMelee.range, 1).ToString();
			extraWeaponMeleeData.blockingAngle = dataArchMelee.blockingAngle;
			IEnumerable<AttackArchMelee> attacks = dataArchMelee.attacks;
			foreach (AttackArchMelee item in attacks ?? Enumerable.Empty<AttackArchMelee>())
			{
				FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot foundryDetailsExtraWeaponMeleeDataMiniShoot = new FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot();
				foundryDetailsExtraWeaponMeleeDataMiniShoot.critDamage = dataArchMelee.criticalMultiplier;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.critChance = dataArchMelee.criticalMultiplier;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.statusChance = dataArchMelee.procChance;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.fireRate = dataArchMelee.fireRate;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.name = item.name;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType = "";
				if (string.IsNullOrEmpty(foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType))
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType = "Melee";
				}
				if (item.crit_mult != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.critDamage = item.crit_mult;
				}
				if (item.crit_chance != -1)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.critChance = item.crit_chance;
				}
				if (item.status_chance != -1)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.statusChance = item.status_chance;
				}
				if (item.speed != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.fireRate = item.speed;
				}
				float dmgSum = item.damage.Sum((KeyValuePair<string, float> p) => p.Value);
				foundryDetailsExtraWeaponMeleeDataMiniShoot.damages = item.damage.Select((KeyValuePair<string, float> p) => new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					value = p.Value,
					damageType = p.Key,
					damage = (float)Math.Round(100f * (p.Value / dmgSum))
				}).ToList();
				extraWeaponMeleeData.attacks.Add(foundryDetailsExtraWeaponMeleeDataMiniShoot);
			}
		}

		private void FillModDetailsFromReferencedPart()
		{
			extraModData = new FoundryDetailsExtraModData();
			DataMod dataMod = referencedPart as DataMod;
			bool flag = dataMod is DataArcane;
			ModsItemData modsItemData = new ModsItemData(new Miscitem
			{
				ItemType = dataMod.uniqueName,
				ItemCount = 1
			}, isArcane: false);
			extraModData.polarity = dataMod.polarity;
			extraModData.type = dataMod.compatName?.ToLower();
			extraModData.rarity = modsItemData.modType;
			if (dataMod.baseDrain == 0)
			{
				extraModData.costRange = "-";
			}
			else
			{
				if (dataMod.baseDrain < 0)
				{
					extraModData.costRange = -dataMod.baseDrain + ((dataMod.fusionLimit > 0) ? (" - " + (-dataMod.baseDrain + dataMod.fusionLimit)) : "");
				}
				else
				{
					extraModData.costRange = dataMod.baseDrain + ((dataMod.fusionLimit > 0) ? (" - " + (dataMod.baseDrain + dataMod.fusionLimit)) : "");
				}
				extraModData.costIsGains = dataMod.baseDrain < 0;
			}
			extraModData.tiers = new List<FoundryDetailsExtraModData.TierData>();
			int maxModLevel = dataMod.GetMaxModLevel();
			foreach (Levelstat item in dataMod.levelStats?.Take(maxModLevel + 1) ?? new Levelstat[maxModLevel + 1])
			{
				FoundryDetailsExtraModData.TierData tierData = new FoundryDetailsExtraModData.TierData();
				tierData.level = extraModData.tiers.Count;
				if (item == null && extraModData.costIsGains)
				{
					tierData.benefits = "Provides " + (-dataMod.baseDrain + tierData.level) + " extra capacity points";
				}
				else if (item != null && item.stats != null)
				{
					tierData.benefits = Misc.ReplaceStringWithIcons(string.Join("</br>", item.stats)).Replace(":", ": ");
				}
				else
				{
					tierData.benefits = "";
				}
				if (flag)
				{
					tierData.credits = (tierData.level + 1) * (tierData.level + 2) / 2;
				}
				else
				{
					tierData.endo = (int)(10.0 * (Math.Pow(2.0, tierData.level) - 1.0) * DataMod.GetModTypeEndoMultipler(modsItemData.modType));
					tierData.credits = (int)(483.0 * (Math.Pow(2.0, tierData.level) - 1.0) * DataMod.GetModTypeEndoMultipler(modsItemData.modType));
				}
				if (tierData.level > 0)
				{
					if (flag)
					{
						tierData.levelCredits = tierData.level + 1;
					}
					else
					{
						tierData.levelEndo = (int)(10.0 * Math.Pow(2.0, tierData.level - 1) * DataMod.GetModTypeEndoMultipler(modsItemData.modType));
						tierData.levelCredits = (int)(483.0 * Math.Pow(2.0, tierData.level - 1) * DataMod.GetModTypeEndoMultipler(modsItemData.modType));
					}
				}
				else
				{
					tierData.levelEndo = 0;
					tierData.levelCredits = 0;
				}
				extraModData.tiers.Add(tierData);
			}
		}

		private void FillShootArchDetailsFromReferencedPart()
		{
			extraWeaponShootData = new FoundryDetailsExtraWeaponShootData();
			DataArchGun dataArchGun = referencedPart as DataArchGun;
			extraWeaponShootData.ammo = "-";
			extraWeaponShootData.accuracy = dataArchGun.accuracy;
			extraWeaponShootData.polarities = dataArchGun.polarities;
			extraWeaponShootData.magazineSize = dataArchGun.magazineSize;
			extraWeaponShootData.triggerType = dataArchGun.trigger;
			extraWeaponShootData.weaponType = dataArchGun.type;
			extraWeaponShootData.rivenDisposition = dataArchGun.disposition;
			extraWeaponShootData.reloadTime = Math.Round(dataArchGun.reloadTime, 1).ToString();
			extraWeaponShootData.noise = dataArchGun.noise;
			if (title == "Astilla Prime")
			{
				extraWeaponShootData.weaponType = "Shotgun";
			}
			IEnumerable<AttackArchGun> attacks = dataArchGun.attacks;
			foreach (AttackArchGun item in attacks ?? Enumerable.Empty<AttackArchGun>())
			{
				FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot foundryDetailsExtraWeaponShootDataMiniShoot = new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot();
				foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = dataArchGun.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.critChance = dataArchGun.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = dataArchGun.procChance;
				foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(dataArchGun.fireRate, 2).ToString();
				foundryDetailsExtraWeaponShootDataMiniShoot.name = item.name;
				foundryDetailsExtraWeaponShootDataMiniShoot.shotType = item.shot_type;
				if (item.crit_mult != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = item.crit_mult;
				}
				if (item.crit_chance != -1)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critChance = item.crit_chance;
				}
				if (item.status_chance != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = item.status_chance;
				}
				if (item.speed != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(item.speed, 2).ToString();
				}
				float dmgSum = item.damage.Sum((KeyValuePair<string, float> p) => p.Value);
				foundryDetailsExtraWeaponShootDataMiniShoot.damages = item.damage.Select((KeyValuePair<string, float> p) => new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					damageType = p.Key,
					damage = (float)Math.Round(100f * (p.Value / dmgSum))
				}).ToList();
				extraWeaponShootData.attacks.Add(foundryDetailsExtraWeaponShootDataMiniShoot);
			}
		}

		private void FillArchwingDetailsFromReferencedPart()
		{
			extraWarframeData = new FoundryDetailsExtraWarframeData();
			DataArchwing dataArchwing = referencedPart as DataArchwing;
			extraWarframeData.abilities = dataArchwing.abilities;
			if (extraWarframeData.abilities == null)
			{
				extraWarframeData.abilities = new Ability[0];
			}
			Ability[] abilities = extraWarframeData.abilities;
			foreach (Ability ability in abilities)
			{
				while (ability.description.Contains("<") && ability.description.Contains(">"))
				{
					int num = ability.description.IndexOf("<");
					int length = ability.description.IndexOf(">") - num;
					string oldValue = ability.description.Substring(num, length);
					ability.description = ability.description.Replace(oldValue, "").Replace("  ", "").Replace(".", ". ")
						.Trim();
				}
			}
			extraWarframeData.baseHealth = dataArchwing.health;
			extraWarframeData.baseShield = dataArchwing.shield;
			extraWarframeData.auraPolarity = null;
			switch (title)
			{
			case "Odonata Prime":
				extraWarframeData.polarities = new string[4] { "madurai", "vazarin", "naramon", "naramon" };
				break;
			case "Odonata":
				extraWarframeData.polarities = new string[3] { "madurai", "vazarin", "naramon" };
				break;
			case "Amesha":
				extraWarframeData.polarities = new string[3] { "naramon", "vazarin", "madurai" };
				break;
			case "Elytron":
				extraWarframeData.polarities = new string[2] { "madurai", "madurai" };
				break;
			case "Itzal":
				extraWarframeData.polarities = new string[3] { "madurai", "vazarin", "naramon" };
				break;
			}
			extraWarframeData.runSpeed = dataArchwing.sprintSpeed;
			extraWarframeData.passive = "-";
			extraWarframeData.baseArmor = dataArchwing.armor;
			extraWarframeData.baseEnergy = dataArchwing.power;
			if (string.IsNullOrEmpty(extraWarframeData.passive))
			{
				extraWarframeData.passive = "-";
			}
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DURATION|s", "a certain duration");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|CHANCE|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|STRENGTH|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RANGE|m", "a certain distance");
			extraWarframeData.passive = extraWarframeData.passive.Replace("<DT_FIRE>", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|PERCENT|%", "a %");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|SPEED|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RANGE|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DURATION|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|MULT|x", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|CRIT|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DAMAGE|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RADIUS|m", "a certain distance");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|ENERGY|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|TIME|s", "x amount of seconds");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|PCT|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|STACKS|", "15");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|HEAL|%", "a %");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|HEALTH|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DELAY|%", "%");
			if (extraWarframeData.passive.Length > 0 && extraWarframeData.passive.ToCharArray()[0] == 'a')
			{
				extraWarframeData.passive = "A" + extraWarframeData.passive.Substring(1);
			}
			extraWarframeData.passive = extraWarframeData.passive.Replace(".", ". ").Trim();
		}

		private void FillPrimaryWeaponDetailsFromReferencedPart()
		{
			extraWeaponShootData = new FoundryDetailsExtraWeaponShootData();
			DataPrimaryWeapon dataPrimaryWeapon = referencedPart as DataPrimaryWeapon;
			extraWeaponShootData.ammo = dataPrimaryWeapon.ammo.ToString();
			extraWeaponShootData.accuracy = dataPrimaryWeapon.accuracy;
			extraWeaponShootData.polarities = dataPrimaryWeapon.polarities;
			extraWeaponShootData.magazineSize = dataPrimaryWeapon.magazineSize;
			extraWeaponShootData.triggerType = dataPrimaryWeapon.trigger;
			extraWeaponShootData.weaponType = dataPrimaryWeapon.type;
			extraWeaponShootData.rivenDisposition = dataPrimaryWeapon.disposition;
			extraWeaponShootData.reloadTime = Math.Round(dataPrimaryWeapon.reloadTime, 1).ToString();
			extraWeaponShootData.noise = dataPrimaryWeapon.noise;
			if (title == "Astilla Prime")
			{
				extraWeaponShootData.weaponType = "Shotgun";
			}
			WeaponAttack[] array = dataPrimaryWeapon.attacks ?? new WeaponAttack[0];
			foreach (WeaponAttack weaponAttack in array)
			{
				FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot foundryDetailsExtraWeaponShootDataMiniShoot = new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot();
				foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = dataPrimaryWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.critChance = dataPrimaryWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = dataPrimaryWeapon.statusChance;
				foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(dataPrimaryWeapon.fireRate, 2).ToString();
				foundryDetailsExtraWeaponShootDataMiniShoot.name = weaponAttack.name;
				foundryDetailsExtraWeaponShootDataMiniShoot.shotType = weaponAttack.shot_type;
				if (weaponAttack.crit_mult != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = weaponAttack.crit_mult;
				}
				if (weaponAttack.crit_chance != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critChance = weaponAttack.crit_chance;
				}
				if (weaponAttack.status_chance != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = weaponAttack.status_chance;
				}
				if (weaponAttack.speed != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(weaponAttack.speed).ToString();
				}
				float dmgSum = weaponAttack.damage.Sum((KeyValuePair<string, float> p) => p.Value);
				foundryDetailsExtraWeaponShootDataMiniShoot.damages = weaponAttack.damage.Select((KeyValuePair<string, float> p) => new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					damageType = p.Key,
					damage = (float)Math.Round(100f * (p.Value / dmgSum))
				}).ToList();
				extraWeaponShootData.attacks.Add(foundryDetailsExtraWeaponShootDataMiniShoot);
			}
		}

		private void FillSecondaryWeaponDetailsFromReferencedPart()
		{
			extraWeaponShootData = new FoundryDetailsExtraWeaponShootData();
			DataSecondaryWeapon dataSecondaryWeapon = referencedPart as DataSecondaryWeapon;
			extraWeaponShootData.ammo = dataSecondaryWeapon.ammo.ToString();
			extraWeaponShootData.accuracy = dataSecondaryWeapon.accuracy;
			extraWeaponShootData.polarities = dataSecondaryWeapon.polarities;
			extraWeaponShootData.magazineSize = dataSecondaryWeapon.magazineSize;
			extraWeaponShootData.triggerType = dataSecondaryWeapon.trigger;
			extraWeaponShootData.weaponType = dataSecondaryWeapon.type;
			extraWeaponShootData.weaponType = "Pistol";
			extraWeaponShootData.rivenDisposition = dataSecondaryWeapon.disposition;
			extraWeaponShootData.reloadTime = Math.Round(dataSecondaryWeapon.reloadTime, 1).ToString();
			extraWeaponShootData.noise = dataSecondaryWeapon.noise;
			IEnumerable<WeaponAttack> attacks = dataSecondaryWeapon.attacks;
			foreach (WeaponAttack item in attacks ?? Enumerable.Empty<WeaponAttack>())
			{
				FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot foundryDetailsExtraWeaponShootDataMiniShoot = new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot();
				foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = dataSecondaryWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.critChance = dataSecondaryWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = dataSecondaryWeapon.statusChance;
				foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(dataSecondaryWeapon.fireRate, 2).ToString();
				foundryDetailsExtraWeaponShootDataMiniShoot.name = item.name;
				foundryDetailsExtraWeaponShootDataMiniShoot.shotType = item.shot_type;
				if (item.crit_mult != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critDamage = item.crit_mult;
				}
				if (item.crit_chance != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.critChance = item.crit_chance;
				}
				if (item.status_chance != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.statusChance = item.status_chance;
				}
				if (item.speed != -1f)
				{
					foundryDetailsExtraWeaponShootDataMiniShoot.fireRate = Math.Round(item.speed, 2).ToString();
				}
				float dmgSum = item.damage.Sum((KeyValuePair<string, float> p) => p.Value);
				foundryDetailsExtraWeaponShootDataMiniShoot.damages = item.damage.Select((KeyValuePair<string, float> p) => new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					damageType = p.Key,
					damage = (float)Math.Round(100f * (p.Value / dmgSum))
				}).ToList();
				extraWeaponShootData.attacks.Add(foundryDetailsExtraWeaponShootDataMiniShoot);
			}
		}

		private void FillMeleeWeaponDetailsFromReferencedPart()
		{
			extraWeaponMeleeData = new FoundryDetailsExtraWeaponMeleeData();
			DataMeleeWeapon dataMeleeWeapon = referencedPart as DataMeleeWeapon;
			extraWeaponMeleeData.auraPolarity = dataMeleeWeapon.stancePolarity;
			extraWeaponMeleeData.polarities = dataMeleeWeapon.polarities;
			extraWeaponMeleeData.polarities = dataMeleeWeapon.polarities;
			extraWeaponMeleeData.rivenDisposition = (int)dataMeleeWeapon.disposition;
			extraWeaponMeleeData.comboDuration = dataMeleeWeapon.comboDuration;
			extraWeaponMeleeData.heavyAttackTime = Math.Round(dataMeleeWeapon.windUp, 1).ToString();
			extraWeaponMeleeData.range = Math.Round(dataMeleeWeapon.range, 1).ToString();
			extraWeaponMeleeData.blockingAngle = dataMeleeWeapon.blockingAngle;
			IEnumerable<Attack> attacks = dataMeleeWeapon.attacks;
			foreach (Attack item in attacks ?? Enumerable.Empty<Attack>())
			{
				FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot foundryDetailsExtraWeaponMeleeDataMiniShoot = new FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot();
				foundryDetailsExtraWeaponMeleeDataMiniShoot.critDamage = dataMeleeWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.critChance = dataMeleeWeapon.criticalMultiplier;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.statusChance = dataMeleeWeapon.procChance;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.fireRate = dataMeleeWeapon.fireRate;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.name = item.name;
				foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType = item.shot_type;
				if (string.IsNullOrEmpty(foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType))
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.shotType = "Melee";
				}
				if (item.crit_mult != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.critDamage = item.crit_mult;
				}
				if (item.crit_chance != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.critChance = item.crit_chance;
				}
				if (item.status_chance != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.statusChance = item.status_chance;
				}
				if (item.speed != -1f)
				{
					foundryDetailsExtraWeaponMeleeDataMiniShoot.fireRate = item.speed;
				}
				float dmgSum = item.damage.Sum((KeyValuePair<string, float> p) => p.Value);
				foundryDetailsExtraWeaponMeleeDataMiniShoot.damages = item.damage.Select((KeyValuePair<string, float> p) => new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					value = p.Value,
					damageType = p.Key,
					damage = (float)Math.Round(100f * (p.Value / dmgSum))
				}).ToList();
				extraWeaponMeleeData.attacks.Add(foundryDetailsExtraWeaponMeleeDataMiniShoot);
			}
			FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot foundryDetailsExtraWeaponMeleeDataMiniShoot2 = extraWeaponMeleeData.attacks?.FirstOrDefault((FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot p) => p.name == "Normal Attack");
			if (foundryDetailsExtraWeaponMeleeDataMiniShoot2 == null || dataMeleeWeapon.attacks == null || dataMeleeWeapon.attacks.Length == 0 || dataMeleeWeapon.attacks[0].slam == null)
			{
				return;
			}
			FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot foundryDetailsExtraWeaponMeleeDataMiniShoot3 = new FoundryDetailsExtraWeaponMeleeData.FoundryDetailsExtraWeaponMeleeDataMiniShoot();
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.critDamage = foundryDetailsExtraWeaponMeleeDataMiniShoot2.critDamage;
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.critChance = foundryDetailsExtraWeaponMeleeDataMiniShoot2.critChance;
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.statusChance = foundryDetailsExtraWeaponMeleeDataMiniShoot2.statusChance;
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.fireRate = foundryDetailsExtraWeaponMeleeDataMiniShoot2.fireRate;
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.name = "Slam attack";
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.shotType = "Radial slam";
			foreach (FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage damage in foundryDetailsExtraWeaponMeleeDataMiniShoot2.damages)
			{
				foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages.Add(new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					damage = damage.damage,
					damageType = damage.damageType,
					value = damage.value
				});
			}
			if (dataMeleeWeapon.attacks[0].slam.radial.element != null)
			{
				foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages.Add(new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					value = dataMeleeWeapon.attacks[0].slam.radial.damage,
					damageType = dataMeleeWeapon.attacks[0].slam.radial.element.ToLower()
				});
			}
			float num = foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages.Sum((FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage p) => p.value);
			foreach (FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage damage2 in foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages)
			{
				damage2.damage = (float)Math.Round(100f * (damage2.value / num));
			}
			foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages = (from p in foundryDetailsExtraWeaponMeleeDataMiniShoot3.damages
				group p by p.damageType into p
				select new FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage
				{
					damageType = p.Key,
					damage = p.Sum((FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage y) => y.damage),
					value = p.Sum((FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage y) => y.value)
				}).ToList();
			extraWeaponMeleeData.attacks.Insert(1, foundryDetailsExtraWeaponMeleeDataMiniShoot3);
		}

		private void FillWarframeDetailsFromReferencedPart()
		{
			extraWarframeData = new FoundryDetailsExtraWarframeData();
			DataWarframe dataWarframe = referencedPart as DataWarframe;
			extraWarframeData.abilities = dataWarframe.abilities;
			if (extraWarframeData.abilities == null)
			{
				extraWarframeData.abilities = new Ability[0];
			}
			Ability[] abilities = extraWarframeData.abilities;
			foreach (Ability ability in abilities)
			{
				while (ability.description.Contains("<") && ability.description.Contains(">"))
				{
					int num = ability.description.IndexOf("<");
					int num2 = ability.description.IndexOf(">") - num;
					if (num2 < 0)
					{
						ability.description = ability.description.Remove(ability.description.IndexOf(">"), 1);
						continue;
					}
					string oldValue = ability.description.Substring(num, num2);
					ability.description = ability.description.Replace(oldValue, "").Replace("  ", "").Replace(".", ". ")
						.Trim();
				}
			}
			extraWarframeData.baseHealth = dataWarframe.health;
			extraWarframeData.baseShield = dataWarframe.shield;
			extraWarframeData.auraPolarity = dataWarframe.aura?.FirstOrDefault();
			extraWarframeData.polarities = dataWarframe.polarities;
			extraWarframeData.runSpeed = dataWarframe.sprintSpeed;
			extraWarframeData.passive = dataWarframe.passiveDescription;
			extraWarframeData.baseArmor = dataWarframe.armor;
			extraWarframeData.baseEnergy = dataWarframe.power;
			if (string.IsNullOrEmpty(extraWarframeData.passive))
			{
				extraWarframeData.passive = "-";
			}
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DURATION|s", "a certain duration");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|CHANCE|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|STRENGTH|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RANGE|m", "a certain distance");
			extraWarframeData.passive = extraWarframeData.passive.Replace("<DT_FIRE>", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|PERCENT|%", "a %");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|SPEED|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RANGE|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DURATION|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|MULT|x", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|CRIT|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DAMAGE|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|RADIUS|m", "a certain distance");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|ENERGY|", "");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|TIME|s", "x amount of seconds");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|PCT|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|STACKS|", "15");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|HEAL|%", "a %");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|HEALTH|%", "%");
			extraWarframeData.passive = extraWarframeData.passive.Replace("|DELAY|%", "%");
			if (extraWarframeData.passive.Length > 0 && extraWarframeData.passive.ToCharArray()[0] == 'a')
			{
				extraWarframeData.passive = "A" + extraWarframeData.passive.Substring(1);
			}
			extraWarframeData.passive = extraWarframeData.passive.Replace(".", ". ").Trim();
		}

		public BigItem FindPartObjectFromUniqueID(string uniqueID)
		{
			BigItem bigItem = null;
			bigItem = StaticData.dataHandler.warframes.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.primaryWeapons.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.secondaryWeapons.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.meleeWeapons.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.archWings.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.archGuns.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.archMelees.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.pets.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.pets.Values.FirstOrDefault((DataPet p) => p.components != null && p.components.Any((ItemComponent u) => u.uniqueName == uniqueID));
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.sentinels.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.sentinelWeapons.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			bigItem = StaticData.dataHandler.sentinelWeapons.GetOrDefault(uniqueID);
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.misc.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.misc.FirstOrDefault((KeyValuePair<string, DataMisc> p) => p.Key == uniqueID).Value;
			}
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.mods.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.mods.FirstOrDefault((KeyValuePair<string, DataMod> p) => p.Key == uniqueID).Value;
			}
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.arcanes.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.arcanes.FirstOrDefault((KeyValuePair<string, DataArcane> p) => p.Key == uniqueID).Value;
			}
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.skins.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.skins.FirstOrDefault((KeyValuePair<string, DataSkin> p) => p.Key == uniqueID).Value;
			}
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.warframeParts.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.warframeParts[uniqueID]?.isPartOf;
			}
			if (bigItem != null)
			{
				return bigItem;
			}
			if (StaticData.dataHandler.weaponParts.ContainsKey(uniqueID))
			{
				bigItem = StaticData.dataHandler.weaponParts[uniqueID]?.isPartOf;
			}
			return bigItem;
		}

		private void FillDetailsFromReferencedPart()
		{
			title = referencedPart.name;
			imageURL = Misc.GetFullImagePath(referencedPart.imageName);
			isFav = FavouriteHelper.IsFavourite(referencedPart.uniqueName);
			internalName = referencedPart.uniqueName;
			if (referencedPart is DataMod)
			{
				DataMod dataMod = referencedPart as DataMod;
				if (!string.IsNullOrEmpty(dataMod.wikiaThumbnail))
				{
					imageURL = Misc.GetFullImagePath(dataMod.wikiaThumbnail);
				}
				if (!string.IsNullOrEmpty(referencedPart.description))
				{
					List<char> list = referencedPart.description.ToCharArray().ToList();
					for (int i = 1; i < list.Count; i++)
					{
						if (list[i] >= 'A' && list[i] <= 'Z')
						{
							list.Insert(i, ',');
							list.Insert(i + 1, ' ');
							i += 2;
						}
					}
					referencedPart.description = new string(list.ToArray());
				}
			}
			description = referencedPart.description;
			wikiLink = referencedPart.wikiaUrl?.Replace("https://warframe.fandom.com/wiki/", "https://wiki.warframe.com/w/");
			if (title.Contains("Bhaira Hound"))
			{
				description = "This model devastates enemies with the Lacerten weapon and comes equipped with the 'Null Audit' precept.";
			}
			if (title.Contains("Dorma Hound"))
			{
				description = "This model pierces through enemies with the Batoten weapon and comes equipped with the 'Repo Audit' precept.";
			}
			if (title.Contains("Hec Hound"))
			{
				description = "This model eviscerates enemies with the Akaten weapon and comes equipped with the 'Equilibrium Audit' precept.";
			}
			if (string.IsNullOrEmpty(wikiLink))
			{
				wikiLink = "-";
			}
			components = new List<FoundryDetailsComponentsItem>();
			if ((StaticData.dataHandler?.craftingData?.craftsByUUID?.GetOrDefault(referencedPart.uniqueName)?.components?.Count((ExtendedCraftingRemoteDataItemComponent p) => p.tradeable || (p.components?.Any((ExtendedCraftingRemoteDataItemComponent k) => k.tradeable) ?? false))).GetValueOrDefault() >= 2 || (referencedPart.components != null && referencedPart.components.Length != 0 && referencedPart.name.Contains("Prime")))
			{
				ItemComponent itemComponent = new ItemComponent();
				itemComponent.drops = new Drop[0];
				itemComponent.name = Misc.GetSetName(referencedPart);
				itemComponent.uniqueName = "SET_" + referencedPart.uniqueName;
				itemComponent.description = "Set of " + referencedPart.name;
				itemComponent.imageName = referencedPart.imageName;
				components.Add(new FoundryDetailsComponentsItem(itemComponent, _isSet: true));
			}
			if (referencedPart.components != null)
			{
				ItemComponent[] array = referencedPart.components;
				foreach (ItemComponent component in array)
				{
					components.Add(new FoundryDetailsComponentsItem(component));
				}
			}
			if (referencedPart is DataMod)
			{
				ItemComponent itemComponent2 = new ItemComponent();
				itemComponent2.drops = (referencedPart as DataMod).drops;
				itemComponent2.name = referencedPart.name;
				itemComponent2.uniqueName = referencedPart.uniqueName;
				itemComponent2.description = referencedPart.description;
				itemComponent2.imageName = referencedPart.imageName;
				components.Add(new FoundryDetailsComponentsItem(itemComponent2));
			}
			if (referencedPart.isPartOf.Any())
			{
				neededForOtherStuff = true;
				neededForOtherStuffInfo = "<strong>Used for crafting: </strong> " + string.Join(", ", referencedPart.isPartOf.Select((BigItem p) => p.name));
			}
			else
			{
				neededForOtherStuff = false;
			}
			FoundryItemComponent.ApplyMultipleRecipeSlotsFix(components.Select((FoundryDetailsComponentsItem p) => p.baseData).ToList());
		}
	}
}
