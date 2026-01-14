namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateSeasoninfo
	{
		public WarframeWorldStateDateExpiry Activation { get; set; }

		public WarframeWorldStateDateExpiry Expiry { get; set; }

		public string AffiliationTag { get; set; }

		public int Season { get; set; }

		public int Phase { get; set; }

		public string Params { get; set; }

		public Activechallenge[] ActiveChallenges { get; set; }
	}
}
