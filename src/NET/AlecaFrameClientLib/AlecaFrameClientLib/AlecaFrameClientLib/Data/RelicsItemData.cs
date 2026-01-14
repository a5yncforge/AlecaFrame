using System.Linq;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;

namespace AlecaFrameClientLib.Data
{
	public class RelicsItemData : InventoryItemData
	{
		public RelicsItemData(Miscitem miscitem)
		{
			isRelic = true;
			type = "relic";
			SetBase(miscitem.ItemType);
			amountOwned = miscitem.ItemCount;
			isFav = FavouriteHelper.IsFavourite(internalName);
			if (StaticData.dataHandler.relics.ContainsKey(internalName))
			{
				DataRelic dataRelic = StaticData.dataHandler.relics[internalName];
				picture = Misc.GetFullImagePath(dataRelic.imageName);
				tradeable = dataRelic.tradable;
				name = dataRelic.name;
				if (name.ToLower().Contains("requiem"))
				{
					name = name.Replace(" Ii ", " II ").Replace(" Iii ", " III ").Replace(" Iv ", " IV ");
				}
				vaulted = dataRelic.drops == null || !dataRelic.drops.Any();
				ordersPlaced = StaticData.overwolfWrappwer.IsOrderPlaced(name);
			}
			else
			{
				StaticData.RelicsNotFoundThatShouldNotErrorOut.All((string p) => !internalName.Contains(p));
				errorOccurred = true;
			}
			vualtedMakesSense = true;
		}

		public RelicsItemData ShallowClone()
		{
			return (RelicsItemData)MemberwiseClone();
		}
	}
}
