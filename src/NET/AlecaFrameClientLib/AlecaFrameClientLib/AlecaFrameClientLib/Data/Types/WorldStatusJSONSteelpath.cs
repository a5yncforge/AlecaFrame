using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONSteelpath
	{
		public WorldStatusJSONCurrentreward currentReward { get; set; }

		public DateTime activation { get; set; }

		public DateTime expiry { get; set; }

		public string remaining { get; set; }

		public WorldStatusJSONRotation[] rotation { get; set; }

		public WorldStatusJSONEvergreen[] evergreens { get; set; }

		public WorldStatusJSONIncursions incursions { get; set; }
	}
}
