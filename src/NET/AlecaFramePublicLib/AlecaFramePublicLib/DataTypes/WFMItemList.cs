using System.Collections.Generic;

namespace AlecaFramePublicLib.DataTypes
{
	public class WFMItemList
	{
		public WFMItemListItem[] data { get; set; }

		public Dictionary<string, WFMItemListItem> AsDictionaryByID { get; set; }

		public Dictionary<string, WFMItemListItem> AsDictionaryBySlug { get; set; }

		public void InitializeDictionary()
		{
			AsDictionaryByID = new Dictionary<string, WFMItemListItem>();
			AsDictionaryBySlug = new Dictionary<string, WFMItemListItem>();
			if (data != null && data.Length != 0)
			{
				WFMItemListItem[] array = data;
				foreach (WFMItemListItem wFMItemListItem in array)
				{
					AsDictionaryByID[wFMItemListItem.id] = wFMItemListItem;
				}
				array = data;
				foreach (WFMItemListItem wFMItemListItem2 in array)
				{
					AsDictionaryBySlug[wFMItemListItem2.slug] = wFMItemListItem2;
				}
			}
		}
	}
}
