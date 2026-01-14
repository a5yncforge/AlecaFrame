using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONDailydeal
	{
		public string item { get; set; }

		public DateTime expiry { get; set; }

		public DateTime activation { get; set; }

		public int originalPrice { get; set; }

		public int salePrice { get; set; }

		public int total { get; set; }

		public int sold { get; set; }

		public string id { get; set; }

		public string eta { get; set; }

		public int discount { get; set; }
	}
}
