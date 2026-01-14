using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONNews
	{
		public string id { get; set; }

		public string message { get; set; }

		public string link { get; set; }

		public string imageLink { get; set; }

		public bool priority { get; set; }

		public DateTime date { get; set; }

		public string eta { get; set; }

		public bool update { get; set; }

		public bool primeAccess { get; set; }

		public bool stream { get; set; }

		public WorldStatusJSONTranslations translations { get; set; }

		public string asString { get; set; }
	}
}
