using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONArbitration
	{
		public DateTime activation { get; set; }

		public DateTime expiry { get; set; }

		public string enemy { get; set; }

		public string type { get; set; }

		public bool archwing { get; set; }

		public bool sharkwing { get; set; }

		public string node { get; set; }
	}
}
