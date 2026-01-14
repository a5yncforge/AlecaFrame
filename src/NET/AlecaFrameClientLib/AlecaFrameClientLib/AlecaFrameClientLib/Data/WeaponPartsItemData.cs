using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class WeaponPartsItemData : InventoryItemData
	{
		public WeaponPartsItemData(Miscitem miscitem)
		{
			SetBase(miscitem.ItemType);
			amountOwned = miscitem.ItemCount;
			componentSearchName = internalName;
			isFav = FavouriteHelper.IsFavourite(internalName) || FavouriteHelper.IsFavourite(componentSearchName);
			ExtendedCraftingRemoteDataItemComponent extendedCraftingRemoteDataItemComponent;
			ItemComponent itemComponentComponentProblemAware;
			if (StaticData.dataHandler.tradeableCraftingPartsByUID.ContainsKey(componentSearchName))
			{
				extendedCraftingRemoteDataItemComponent = StaticData.dataHandler.tradeableCraftingPartsByUID[componentSearchName][0];
				itemComponentComponentProblemAware = extendedCraftingRemoteDataItemComponent.GetItemComponentComponentProblemAware();
				if (itemComponentComponentProblemAware == null)
				{
					errorOccurred = true;
				}
				picture = Misc.GetFullImagePath(itemComponentComponentProblemAware.imageName);
				isFav = isFav || FavouriteHelper.IsFavourite(itemComponentComponentProblemAware.isPartOf?.uniqueName);
				if (itemComponentComponentProblemAware.name == "Chassis")
				{
					BigItem isPartOf = itemComponentComponentProblemAware.isPartOf;
					if (isPartOf == null || !isPartOf.name.Contains("Spectra Vandal"))
					{
						BigItem isPartOf2 = itemComponentComponentProblemAware.isPartOf;
						if (isPartOf2 == null || !isPartOf2.name.Contains("Shedu"))
						{
							BigItem isPartOf3 = itemComponentComponentProblemAware.isPartOf;
							if (isPartOf3 == null || !isPartOf3.name.Contains("Ghoulsaw"))
							{
								BigItem isPartOf4 = itemComponentComponentProblemAware.isPartOf;
								if (isPartOf4 == null || !isPartOf4.name.Contains("Miter"))
								{
									goto IL_0173;
								}
							}
						}
					}
					picture = picture.Replace("chassis", "stock");
				}
				goto IL_0173;
			}
			errorOccurred = true;
			goto IL_0220;
			IL_0220:
			vualtedMakesSense = name?.ToLower().Contains("prime") ?? false;
			return;
			IL_0173:
			ducats = itemComponentComponentProblemAware.ducats;
			name = itemComponentComponentProblemAware.GetRealExternalName();
			if (name == "Shedu Chassis")
			{
				picture = "https://cdn.alecaframe.com/warframeData/custom/imgRemote/Stock.png";
			}
			tradeable = extendedCraftingRemoteDataItemComponent.tradeable;
			goalItemOwned = false;
			if (itemComponentComponentProblemAware.isPartOf != null)
			{
				goalItemOwned = goalItemOwned || itemComponentComponentProblemAware.isPartOf.IsOwned() || itemComponentComponentProblemAware.isPartOf.IsFullyMastered();
				vaulted = itemComponentComponentProblemAware.isPartOf.vaulted;
			}
			ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
			goto IL_0220;
		}
	}
}
