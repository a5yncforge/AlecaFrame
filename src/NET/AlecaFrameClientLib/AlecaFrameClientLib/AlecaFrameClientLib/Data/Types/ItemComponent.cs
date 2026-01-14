using System.Collections.Generic;
using System.Linq;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class ItemComponent
	{
		public class ItemComponentComparer : EqualityComparer<ItemComponent>
		{
			public override bool Equals(ItemComponent x, ItemComponent y)
			{
				return x.uniqueName == y.uniqueName;
			}

			public override int GetHashCode(ItemComponent obj)
			{
				return obj.uniqueName.GetHashCode();
			}
		}

		public BigItem isPartOf;

		public string uniqueName { get; set; }

		public string name { get; set; }

		public string description { get; set; }

		public int itemCount { get; set; }

		public string imageName { get; set; }

		public bool tradable { get; set; }

		public Drop[] drops { get; set; }

		public float[] damagePerShot { get; set; }

		public float totalDamage { get; set; }

		public float criticalChance { get; set; }

		public float criticalMultiplier { get; set; }

		public float procChance { get; set; }

		public float fireRate { get; set; }

		public int masteryReq { get; set; }

		public string productCategory { get; set; }

		public int slot { get; set; }

		public float accuracy { get; set; }

		public float omegaAttenuation { get; set; }

		public string noise { get; set; }

		public string trigger { get; set; }

		public int magazineSize { get; set; }

		public float reloadTime { get; set; }

		public int multishot { get; set; }

		public int ammo { get; set; }

		public Damagetypes damageTypes { get; set; }

		public int marketCost { get; set; }

		public string[] polarities { get; set; }

		public string projectile { get; set; }

		public string[] tags { get; set; }

		public string type { get; set; }

		public string wikiaThumbnail { get; set; }

		public string wikiaUrl { get; set; }

		public int disposition { get; set; }

		public int flight { get; set; }

		public int primeSellingPrice { get; set; }

		public int ducats { get; set; }

		public string releaseDate { get; set; }

		public string vaultDate { get; set; }

		public string estimatedVaultDate { get; set; }

		public int blockingAngle { get; set; }

		public int comboDuration { get; set; }

		public float followThrough { get; set; }

		public float range { get; set; }

		public int slamAttack { get; set; }

		public int slamRadialDamage { get; set; }

		public int slamRadius { get; set; }

		public int slideAttack { get; set; }

		public int heavyAttackDamage { get; set; }

		public int heavySlamAttack { get; set; }

		public int heavySlamRadialDamage { get; set; }

		public int heavySlamRadius { get; set; }

		public float windUp { get; set; }

		public float channeling { get; set; }

		public string stancePolarity { get; set; }

		public bool vaulted { get; set; }

		public bool excludeFromCodex { get; set; }

		public float statusChance { get; set; }

		public bool IsLandingCraftPart()
		{
			return uniqueName.Contains("/Lotus/Types/Recipes/LandingCraftRecipes/");
		}

		public string GetRealExternalName(bool alwaysProvideTradeablePartName = false)
		{
			string text = name;
			if (text == "Forma")
			{
				text = "Forma Blueprint";
			}
			else
			{
				if (text.Contains("Kavasa Prime") || name.Equals("Orokin Cell") || uniqueName.Contains("/Resources/") || uniqueName.Contains("/Types/Items/") || fireRate > 0f || reloadTime > 0f || uniqueName.Contains("/Resource/"))
				{
					return name;
				}
				if (isPartOf != null)
				{
					text = isPartOf.name + " " + text;
				}
			}
			if (text.Contains("Voidrig"))
			{
				text = text.Replace("Voidrig Voidrig", "Voidrig");
			}
			if (text.Contains("Bonewidow"))
			{
				text = text.Replace("Bonewidow Bonewidow", "Bonewidow");
			}
			if (text.Contains("War war"))
			{
				text = text.Replace("War War", "War");
			}
			if (text.Contains("War War"))
			{
				text = text.Replace("War War", "War");
			}
			if (text.Contains("Decurion decurion"))
			{
				text = text.Replace("Decurion Decurion", "Decurion");
			}
			if (text.Contains("Decurion Decurion"))
			{
				text = text.Replace("Decurion Decurion", "Decurion");
			}
			if (text == "Broken War Blade")
			{
				text = "War Blade";
			}
			if (text == "Broken War Hilt")
			{
				text = "War Hilt";
			}
			if (text.Contains("Dual Decurion") && !text.Contains("Blueprint"))
			{
				text = text.Replace("Dual Decurion", "Decurion");
			}
			if (isPartOf != null && !isPartOf.name.Contains("Ambassador"))
			{
				bool flag = false;
				flag |= isPartOf is DataWarframe;
				if (!flag && StaticData.dataHandler != null)
				{
					List<ExtendedCraftingRemoteDataItemComponent> value = default(List<ExtendedCraftingRemoteDataItemComponent>);
					List<ExtendedCraftingRemoteDataItemComponent> value2 = default(List<ExtendedCraftingRemoteDataItemComponent>);
					if (StaticData.dataHandler?.tradeableCraftingPartsByUID?.TryGetValue(uniqueName, out value) == true)
					{
						if (value.Count > 0)
						{
							flag |= value[0].componentType == ComponentType.SubBlueprint && value[0].tradeable;
							flag |= value[0]?.components.Any((ExtendedCraftingRemoteDataItemComponent p) => p.componentType == ComponentType.SubBlueprint && p.tradeable) ?? false;
						}
					}
					else if (StaticData.dataHandler?.nonTradeableCraftingPartsByUID?.TryGetValue(uniqueName, out value2) == true && value2.Count > 0)
					{
						flag |= value2[0].componentType == ComponentType.SubBlueprint && value2[0].tradeable;
						flag |= value2[0]?.components.Any((ExtendedCraftingRemoteDataItemComponent p) => p.componentType == ComponentType.SubBlueprint && p.tradeable) ?? false;
					}
				}
				if (flag && !text.EndsWith("Blueprint"))
				{
					text += " Blueprint";
				}
			}
			return text;
		}
	}
}
