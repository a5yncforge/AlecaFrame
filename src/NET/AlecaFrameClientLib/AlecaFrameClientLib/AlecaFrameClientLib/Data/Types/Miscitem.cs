namespace AlecaFrameClientLib.Data.Types
{
	public class Miscitem
	{
		public int ItemCount { get; set; }

		public string ItemType { get; set; }

		public Lastadded LastAdded { get; set; }

		public string UpgradeFingerprint { get; set; }

		public int XP { get; set; }

		public MiscItemItemId ItemId { get; set; }

		public bool IsWarframePart()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (!ItemType.Contains("/WarframeRecipes/"))
			{
				return false;
			}
			if (ItemType.Contains("Beacon") && ItemType.Contains("Component"))
			{
				return false;
			}
			return true;
		}

		public bool IsFactionOrBaro()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("CephalonSuda/Pistols/CSDroidArray"))
			{
				return false;
			}
			if (!ItemType.Contains("/Prisma") && !ItemType.Contains("/Lotus/Weapons/Syndicates/SteelMeridian") && !ItemType.Contains("/Lotus/Weapons/Syndicates/ArbitersOfHexis") && !ItemType.Contains("/Lotus/Weapons/Syndicates/CephalonSuda") && !ItemType.Contains("/Lotus/Weapons/Syndicates/PerrinSequence") && !ItemType.Contains("/Lotus/Weapons/Syndicates/RedVeil") && !ItemType.Contains("/Lotus/Weapons/Syndicates/NewLoka") && !ItemType.Contains("/VoidTrader") && !ItemType.Contains("/Lotus/Weapons/Corpus/LongGuns/CrpBFG/Vandal/VandalCrpBFG"))
			{
				return ItemType.Contains("/Lotus/Weapons/Tenno/Pistols/ConclaveLeverPistol/ConclaveLeverPistol");
			}
			return true;
		}

		public bool IsWeaponPart()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/Weapons/Ostron") || ItemType.Contains("/Weapons/Corpus"))
			{
				return false;
			}
			if (ItemType.Contains("TeamAmmoBlueprint"))
			{
				return false;
			}
			if (ItemType.Contains("/Weapons/Skins/"))
			{
				return false;
			}
			if (ItemType.Contains("/WeaponParts/"))
			{
				return true;
			}
			if (ItemType.Contains("/SentinelRecipes/") && ItemType.Contains("Blueprint"))
			{
				return true;
			}
			if (ItemType.Contains("/Weapons/") && ItemType.Contains("Blueprint"))
			{
				return true;
			}
			return false;
		}

		public bool IsRelic()
		{
			if (ItemType == null)
			{
				return false;
			}
			return ItemType.Contains("/Projections/");
		}

		public bool IsMisc()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/Fusers/LegendaryModFuser"))
			{
				return true;
			}
			if (ItemType == "/Lotus/Upgrades/Skins/Kubrows/Collars/PrimeKubrowCollarA")
			{
				return true;
			}
			if (ItemType.Contains("/Resources/Mechs/"))
			{
				return true;
			}
			if (ItemType.Contains("/Items/Fish/") && !ItemType.Contains("Boot"))
			{
				return true;
			}
			if (ItemType.Contains("/Lotus/Upgrades/Skins/") && ItemType.Contains("Helmet"))
			{
				return true;
			}
			if (ItemType == "/Lotus/Types/Keys/Nightwave/GlassmakerBossFightKey")
			{
				return true;
			}
			if (ItemType.StartsWith("/Lotus/Types/Items/FusionTreasures"))
			{
				return true;
			}
			if (IsScene())
			{
				return true;
			}
			return false;
		}

		public bool IsScene()
		{
			if (ItemType == null)
			{
				return false;
			}
			return ItemType.StartsWith("/Lotus/Types/Items/MiscItems/PhotoboothTile");
		}

		public bool IsLandingCraftPart()
		{
			if (ItemType == null)
			{
				return false;
			}
			return ItemType.Contains("/Lotus/Types/Recipes/LandingCraftRecipes/");
		}

		public bool IsMod()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/Beginner/"))
			{
				return false;
			}
			if (ItemType.Contains("/CosmeticEnhancers/Peculiars/"))
			{
				return true;
			}
			if (ItemType.Contains("/Lotus/Upgrades/Mods/Railjack/"))
			{
				return true;
			}
			if (IsArcane())
			{
				return false;
			}
			return true;
		}

		public bool IsArcane()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/CosmeticEnhancers/Peculiars/"))
			{
				return false;
			}
			if (ItemType.Contains("/Lotus/Upgrades/CosmeticEnhancers"))
			{
				return true;
			}
			return false;
		}

		public bool IsArchFramePart()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/Lotus/Types/Recipes/ArchwingRecipes/"))
			{
				return true;
			}
			return false;
		}

		public bool IsArchWeaponPart()
		{
			if (ItemType == null)
			{
				return false;
			}
			if (ItemType.Contains("/Archwing/Primary/"))
			{
				return true;
			}
			return false;
		}

		public bool IsRivenMod()
		{
			if (ItemType == null)
			{
				return false;
			}
			return ItemType.StartsWith("/Lotus/Upgrades/Mods/Randomized/");
		}
	}
}
