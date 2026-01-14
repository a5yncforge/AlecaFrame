using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONCambioncycle
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public DateTime expiry { get; set; }

		public string timeLeft { get; set; }

		public string active { get; set; }
	}
}
