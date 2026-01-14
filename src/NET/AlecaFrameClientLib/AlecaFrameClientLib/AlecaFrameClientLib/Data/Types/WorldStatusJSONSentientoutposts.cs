using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONSentientoutposts
	{
		public WorldStatusJSONMission mission { get; set; }

		public DateTime activation { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public string id { get; set; }
	}
}
