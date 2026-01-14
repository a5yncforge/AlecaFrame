using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONEvent
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public int maximumScore { get; set; }

		public int currentScore { get; set; }

		public object smallInterval { get; set; }

		public object largeInterval { get; set; }

		public string faction { get; set; }

		public string description { get; set; }

		public string tooltip { get; set; }

		public string node { get; set; }

		public object[] concurrentNodes { get; set; }

		public object[] rewards { get; set; }

		public bool expired { get; set; }

		public object[] interimSteps { get; set; }

		public object[] progressSteps { get; set; }

		public bool isPersonal { get; set; }

		public object[] regionDrops { get; set; }

		public object[] archwingDrops { get; set; }

		public string asString { get; set; }

		public WorldStatusJSONMetadata metadata { get; set; }

		public object[] completionBonuses { get; set; }

		public DateTime altExpiry { get; set; }

		public DateTime altActivation { get; set; }

		public WorldStatusJSONNextalt nextAlt { get; set; }
	}
}
