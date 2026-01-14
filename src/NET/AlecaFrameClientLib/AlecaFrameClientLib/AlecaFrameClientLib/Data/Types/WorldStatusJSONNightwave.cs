using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONNightwave
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public int season { get; set; }

		public string tag { get; set; }

		public int phase { get; set; }

		public WorldStatusJSONParams _params { get; set; }

		public object[] possibleChallenges { get; set; }

		public WorldStatusJSONActivechallenge[] activeChallenges { get; set; }

		public string[] rewardTypes { get; set; }
	}
}
