using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONEarthcycle
	{
		public string id { get; set; }

		public DateTime expiry { get; set; }

		public DateTime activation { get; set; }

		public bool isDay { get; set; }

		public string state { get; set; }

		public string timeLeft { get; set; }
	}
}
