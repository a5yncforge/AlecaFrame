using System;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public class InventoryItemData
	{
		public string name;

		public string internalName;

		public string picture;

		public int ducats;

		public int buyPrice;

		public int sellPrice;

		public int amountOwned;

		public bool isFav;

		public int amountNow;

		public bool shouldDisplayPlusInPrice;

		[NonSerialized]
		public int totalAmountIncludingRanks;

		public bool vaulted;

		public bool vualtedMakesSense;

		public string type = "";

		public bool isRelic;

		public bool isMod;

		public bool goalItemOwned;

		public bool ordersPlaced;

		public string modUsedBy = "";

		public int currentModRank;

		public int modRankMax;

		public string modType;

		[JsonIgnore]
		public bool tradeable;

		[JsonIgnore]
		public bool errorOccurred;

		[JsonIgnore]
		public string componentSearchName = "";

		public void SetBase(string internalWarframeName)
		{
			internalName = internalWarframeName;
			componentSearchName = internalName;
		}
	}
}
