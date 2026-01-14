using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONFissure
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public string node { get; set; }

		public string missionType { get; set; }

		public string missionKey { get; set; }

		public string enemy { get; set; }

		public string enemyKey { get; set; }

		public string nodeKey { get; set; }

		public string tier { get; set; }

		public int tierNum { get; set; }

		public bool expired { get; set; }

		public string eta { get; set; }

		public bool isStorm { get; set; }
	}
}
