using System;
using AlecaFramePublicLib.DataTypes;

namespace AlecaFrameClientLib.Data.Types
{
	public class WFMarketProfileOrder
	{
		public string id { get; set; }

		public string type { get; set; }

		public int platinum { get; set; }

		public int quantity { get; set; }

		public int perTrade { get; set; }

		public bool visible { get; set; }

		public DateTime createdAt { get; set; }

		public DateTime updatedAt { get; set; }

		public string itemId { get; set; }

		public string rank { get; set; }

		public string subtype { get; set; }

		public WFMItemListItem GetItemDataObject()
		{
			if (StaticData.LazyWfmItemData.Value.AsDictionaryByID.ContainsKey(itemId))
			{
				return StaticData.LazyWfmItemData.Value.AsDictionaryByID[itemId];
			}
			return null;
		}
	}
}
