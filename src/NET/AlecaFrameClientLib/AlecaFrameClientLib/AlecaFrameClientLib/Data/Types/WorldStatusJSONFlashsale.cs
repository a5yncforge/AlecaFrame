using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONFlashsale
	{
		public string item { get; set; }

		public DateTime expiry { get; set; }

		public DateTime activation { get; set; }

		public int discount { get; set; }

		public int regularOverride { get; set; }

		public int premiumOverride { get; set; }

		public bool isShownInMarket { get; set; }

		public bool isFeatured { get; set; }

		public bool isPopular { get; set; }

		public string id { get; set; }

		public bool expired { get; set; }

		public string eta { get; set; }
	}
}
