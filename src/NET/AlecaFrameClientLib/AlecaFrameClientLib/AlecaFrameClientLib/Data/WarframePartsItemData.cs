using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class WarframePartsItemData : InventoryItemData
	{
		public WarframePartsItemData(Miscitem miscitem)
		{
			SetBase(miscitem.ItemType);
			amountOwned = miscitem.ItemCount;
			componentSearchName = internalName;
			if (StaticData.dataHandler.tradeableCraftingPartsByUID.ContainsKey(componentSearchName))
			{
				ExtendedCraftingRemoteDataItemComponent extendedCraftingRemoteDataItemComponent = StaticData.dataHandler.tradeableCraftingPartsByUID[componentSearchName][0];
				ItemComponent itemComponentComponentProblemAware = extendedCraftingRemoteDataItemComponent.GetItemComponentComponentProblemAware();
				if (itemComponentComponentProblemAware == null)
				{
					errorOccurred = true;
				}
				picture = Misc.GetFullImagePath(itemComponentComponentProblemAware.imageName);
				ducats = itemComponentComponentProblemAware.ducats;
				tradeable = extendedCraftingRemoteDataItemComponent.tradeable;
				isFav = FavouriteHelper.IsFavourite(internalName) || FavouriteHelper.IsFavourite(componentSearchName);
				isFav = isFav || FavouriteHelper.IsFavourite(itemComponentComponentProblemAware.isPartOf?.uniqueName);
				name = itemComponentComponentProblemAware.GetRealExternalName();
				goalItemOwned = false;
				if (itemComponentComponentProblemAware.isPartOf != null)
				{
					goalItemOwned = itemComponentComponentProblemAware.isPartOf.IsFullyMastered() || itemComponentComponentProblemAware.isPartOf.IsOwned();
					vaulted = itemComponentComponentProblemAware.isPartOf.vaulted;
				}
				ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
			}
			else if (miscitem.IsLandingCraftPart())
			{
				componentSearchName = internalName.Replace("Blueprint", "Component");
				if (StaticData.dataHandler.warframeParts.TryGetValue(componentSearchName, out var value))
				{
					picture = Misc.GetFullImagePath(value.imageName);
					ducats = value.ducats;
					tradeable = value.tradable;
					isFav = FavouriteHelper.IsFavourite(internalName) || FavouriteHelper.IsFavourite(componentSearchName);
					isFav = isFav || FavouriteHelper.IsFavourite(value.isPartOf?.uniqueName);
					name = value.GetRealExternalName();
					goalItemOwned = false;
					if (value.isPartOf != null)
					{
						goalItemOwned = value.isPartOf.IsFullyMastered() || value.isPartOf.IsOwned();
						vaulted = value.isPartOf.vaulted;
					}
					ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
				}
				else
				{
					errorOccurred = true;
				}
			}
			else
			{
				errorOccurred = true;
			}
			vualtedMakesSense = name?.ToLower().Contains("prime") ?? false;
		}
	}
}
