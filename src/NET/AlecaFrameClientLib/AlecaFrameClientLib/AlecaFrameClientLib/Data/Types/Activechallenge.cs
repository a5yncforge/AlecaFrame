namespace AlecaFrameClientLib.Data.Types
{
	public class Activechallenge
	{
		public MiscItemItemId _id { get; set; }

		public bool Daily { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Challenge { get; set; }
	}
}
