using System;
using System.Collections.Generic;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib
{
	public class SingleRelicRewardData
	{
		public bool detected;

		public float errorProb;

		public string internalName = "";

		public string name = "";

		public string icon = "";

		public int platinum;

		public int ducats;

		public bool isPartOfOwned;

		public int countOwned;

		public int totalToOwn;

		public bool isItemVaulted;

		public List<FoundryItemComponent> componentData = new List<FoundryItemComponent>();

		public string englishName;

		[NonSerialized]
		public ItemComponent itemReference;

		public int setPlat;

		public bool isFav;
	}
}
