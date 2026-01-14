using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONSyndicatemission
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public string syndicate { get; set; }

		public string syndicateKey { get; set; }

		public string[] nodes { get; set; }

		public WorldStatusJSONJob[] jobs { get; set; }

		public string eta { get; set; }
	}
}
