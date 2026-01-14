using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONValliscycle
	{
		public string id { get; set; }

		public DateTime expiry { get; set; }

		public bool isWarm { get; set; }

		public string state { get; set; }

		public DateTime activation { get; set; }

		public string timeLeft { get; set; }

		public string shortString { get; set; }
	}
}
