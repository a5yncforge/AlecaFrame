namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateLitesorty
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Reward { get; set; }

		public int Seed { get; set; }

		public string Boss { get; set; }

		public WarframeWorldStateDateMission2[] Missions { get; set; }
	}
}
