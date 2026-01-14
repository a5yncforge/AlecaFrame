namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateNodeoverride
	{
		public MiscItemItemId _id { get; set; }

		public string Node { get; set; }

		public bool Hide { get; set; }

		public int Seed { get; set; }

		public string LevelOverride { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Faction { get; set; }

		public string[] CustomNpcEncounters { get; set; }
	}
}
