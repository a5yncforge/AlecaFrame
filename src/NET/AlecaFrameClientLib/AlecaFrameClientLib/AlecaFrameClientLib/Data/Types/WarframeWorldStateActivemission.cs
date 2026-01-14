namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateActivemission
	{
		public MiscItemItemId _id { get; set; }

		public int Region { get; set; }

		public int Seed { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Node { get; set; }

		public string MissionType { get; set; }

		public string Modifier { get; set; }

		public bool Hard { get; set; }
	}
}
