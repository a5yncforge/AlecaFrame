using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class FoundryItemComponent
	{
		public string name;

		public string picture;

		public string quantityOwned = "0";

		public int neccessaryAmount = 1;

		public bool isFav;

		public bool isFavOnlyPart;

		[NonSerialized]
		public int quantity;

		[NonSerialized]
		private readonly string highlightIfUniqueIDisThisOne;

		public string uniqueName;

		[NonSerialized]
		public int ducats;

		public bool recipeNeccessaryComponents;

		public bool recipeHighlightedComponent;

		public bool parentOwned;

		[NonSerialized]
		public ItemComponent componentReference;

		public bool anyRelicsOwned;

		public FoundryItemComponent(ItemComponent itemComponent, string highlightIfUniqueIDisThisOne = "JUAJUASJUASJUAS6969")
		{
			componentReference = itemComponent;
			uniqueName = itemComponent.uniqueName;
			name = itemComponent.GetRealExternalName();
			isFav = FavouriteHelper.IsFavourite(itemComponent.isPartOf?.uniqueName) || FavouriteHelper.IsFavourite(itemComponent.uniqueName);
			isFavOnlyPart = FavouriteHelper.IsFavourite(itemComponent.uniqueName);
			ducats = itemComponent.ducats;
			if (itemComponent.name == "Engine")
			{
				BigItem isPartOf = itemComponent.isPartOf;
				if (isPartOf != null && isPartOf.name.Contains("Ghoulsaw"))
				{
					picture = Misc.GetFullImagePath(itemComponent.imageName.Replace("engine", "grip"));
					goto IL_01bb;
				}
			}
			if (!(itemComponent.name == "Chassis"))
			{
				goto IL_0186;
			}
			BigItem isPartOf2 = itemComponent.isPartOf;
			if (isPartOf2 == null || !isPartOf2.name.Contains("Spectra Vandal"))
			{
				BigItem isPartOf3 = itemComponent.isPartOf;
				if (isPartOf3 == null || !isPartOf3.name.Contains("Shedu"))
				{
					BigItem isPartOf4 = itemComponent.isPartOf;
					if (isPartOf4 == null || !isPartOf4.name.Contains("Ghoulsaw"))
					{
						BigItem isPartOf5 = itemComponent.isPartOf;
						if (isPartOf5 == null || !isPartOf5.name.Contains("Miter"))
						{
							goto IL_0186;
						}
					}
				}
			}
			picture = Misc.GetFullImagePath(itemComponent.imageName.Replace("chassis", "stock"));
			goto IL_01bb;
			IL_0186:
			if (name.Contains("Forma Blueprint"))
			{
				picture = Misc.GetFullImagePath("afRemoteImg://forma.png");
			}
			else
			{
				picture = Misc.GetFullImagePath(itemComponent.imageName);
			}
			goto IL_01bb;
			IL_01bb:
			if (StaticData.dataHandler.warframeRootObject != null)
			{
				int num = 0;
				if (uniqueName.Contains("CrpArSniper") && !uniqueName.Contains("Blueprint"))
				{
					num += CountAmount(uniqueName.Replace("CrpArSniper", "Ambassador") + "Blueprint");
				}
				else if (name.Contains("Forma"))
				{
					num += CountAmount("/Lotus/Types/Items/MiscItems/Forma");
					num += CountAmount("/Lotus/Types/Recipes/Components/FormaBlueprint");
				}
				else
				{
					num += CountAmount(itemComponent.uniqueName);
					if (uniqueName.Contains("/WeaponParts/") && !uniqueName.Contains("Prime") && !uniqueName.EndsWith("Blueprint") && !uniqueName.EndsWith("Component"))
					{
						num += CountAmount(itemComponent.uniqueName + "Blueprint");
					}
				}
				quantity = num;
				quantityOwned = num.GetSIRepresentation();
				neccessaryAmount = itemComponent.itemCount;
			}
			recipeHighlightedComponent = itemComponent.uniqueName == highlightIfUniqueIDisThisOne;
			this.highlightIfUniqueIDisThisOne = highlightIfUniqueIDisThisOne;
			UpdateVisibleFields();
			if (itemComponent.isPartOf != null && (itemComponent.isPartOf.IsOwned() || itemComponent.isPartOf.IsFullyMastered()))
			{
				parentOwned = true;
			}
			anyRelicsOwned = FoundryHelper.IsARelicForThisComponentOwnedFAST(itemComponent.uniqueName);
		}

		public FoundryItemComponent(string uniqueName, BasicRemoteDataItemData basicInfo, int neccessaryAmount)
		{
			this.uniqueName = uniqueName;
			name = basicInfo.name;
			isFav = FavouriteHelper.IsFavourite(uniqueName);
			isFavOnlyPart = FavouriteHelper.IsFavourite(uniqueName);
			if (name.Contains("Forma Blueprint"))
			{
				picture = Misc.GetFullImagePath("afRemoteImg://forma.png");
			}
			else
			{
				picture = Misc.GetFullImagePath(basicInfo.pic);
			}
			if (StaticData.dataHandler.warframeRootObject != null)
			{
				int num = 0;
				if (uniqueName.Contains("CrpArSniper") && !uniqueName.Contains("Blueprint"))
				{
					num += CountAmount(uniqueName.Replace("CrpArSniper", "Ambassador") + "Blueprint");
				}
				else if (name.Contains("Forma"))
				{
					num += CountAmount("/Lotus/Types/Items/MiscItems/Forma");
					num += CountAmount("/Lotus/Types/Recipes/Components/FormaBlueprint");
				}
				else
				{
					num += CountAmount(uniqueName);
					if (uniqueName.Contains("/WeaponParts/") && !uniqueName.Contains("Prime") && !uniqueName.EndsWith("Blueprint") && !uniqueName.EndsWith("Component"))
					{
						num += CountAmount(uniqueName + "Blueprint");
					}
				}
				quantity = num;
				quantityOwned = num.GetSIRepresentation();
				this.neccessaryAmount = neccessaryAmount;
			}
			UpdateVisibleFields();
			anyRelicsOwned = FoundryHelper.IsARelicForThisComponentOwnedFAST(uniqueName);
		}

		public FoundryItemComponent()
		{
		}

		public void UpdateVisibleFields()
		{
			quantityOwned = quantity.GetSIRepresentation();
			recipeNeccessaryComponents = quantity >= neccessaryAmount;
			recipeHighlightedComponent = uniqueName == highlightIfUniqueIDisThisOne;
		}

		private int CountAmount(string uniqueName)
		{
			uniqueName.ToLower().Contains("chroma");
			if (uniqueName.Contains("Blueprint"))
			{
				Miscitem miscitem = StaticData.dataHandler.warframeRootObject.Recipes.FirstOrDefault((Miscitem p) => p.ItemType == uniqueName);
				if (miscitem != null)
				{
					return miscitem.ItemCount;
				}
			}
			else
			{
				int num = 0;
				if (uniqueName.Contains("Weapons"))
				{
					num = StaticData.dataHandler.warframeRootObject.Melee.Count((Melee p) => p.ItemType == uniqueName);
					if (num == 0)
					{
						num = StaticData.dataHandler.warframeRootObject.Pistols.Count((Pistol p) => p.ItemType == uniqueName);
					}
					if (num == 0)
					{
						num = StaticData.dataHandler.warframeRootObject.LongGuns.Count((Longgun p) => p.ItemType == uniqueName);
					}
					if (num > 0)
					{
						return num;
					}
				}
				Miscitem miscitem2 = StaticData.dataHandler.warframeRootObject.MiscItems.FirstOrDefault((Miscitem p) => p.ItemType == uniqueName);
				if (miscitem2 != null)
				{
					return miscitem2.ItemCount;
				}
				if (uniqueName.Contains("Component"))
				{
					uniqueName = uniqueName.Replace("Component", "Blueprint");
					Miscitem miscitem3 = StaticData.dataHandler.warframeRootObject.Recipes.FirstOrDefault((Miscitem p) => p.ItemType == uniqueName);
					if (miscitem3 != null)
					{
						return miscitem3.ItemCount;
					}
				}
			}
			return 0;
		}

		public static void ApplyMultipleRecipeSlotsFix(List<FoundryItemComponent> components)
		{
			if (components == null)
			{
				return;
			}
			Dictionary<string, int> dictionary = (from p in components
				group p by p.uniqueName).ToDictionary((IGrouping<string, FoundryItemComponent> p) => p.Key, (IGrouping<string, FoundryItemComponent> p) => p.Max((FoundryItemComponent k) => k.quantity));
			foreach (FoundryItemComponent component in components)
			{
				if (dictionary[component.uniqueName] < component.neccessaryAmount)
				{
					component.quantity = dictionary[component.uniqueName];
					dictionary[component.uniqueName] = 0;
				}
				else
				{
					dictionary[component.uniqueName] -= component.neccessaryAmount;
				}
				component.UpdateVisibleFields();
			}
		}
	}
}
