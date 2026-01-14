using System;
using System.Linq;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib.Data
{
	public class DeltaResponseItem
	{
		[NonSerialized]
		public bool allOK;

		public InventoryItemData baseData;

		public DeltaResponseItem(string uniqueID, int amount)
		{
			int itemCount = amount;
			Miscitem miscitem = StaticData.dataHandler.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == uniqueID);
			if (miscitem != null)
			{
				itemCount = miscitem.ItemCount;
			}
			Miscitem miscitem2 = new Miscitem
			{
				ItemType = uniqueID,
				ItemCount = itemCount
			};
			if (miscitem2.IsWarframePart())
			{
				baseData = new WarframePartsItemData(miscitem2);
				allOK = true;
			}
			else
			{
				if (!miscitem2.IsWeaponPart())
				{
					return;
				}
				baseData = new WeaponPartsItemData(miscitem2);
				allOK = true;
			}
			if (!baseData.tradeable)
			{
				allOK = false;
			}
			else
			{
				baseData.amountNow = amount;
			}
		}
	}
}
