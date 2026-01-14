using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONSortie
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public string rewardPool { get; set; }

		public WorldStatusJSONVariant[] variants { get; set; }

		public string boss { get; set; }

		public string faction { get; set; }

		public bool expired { get; set; }

		public string eta { get; set; }
	}
}
