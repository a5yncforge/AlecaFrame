namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateSorty
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Reward { get; set; }

		public int Seed { get; set; }

		public string Boss { get; set; }

		public object[] ExtraDrops { get; set; }

		public Variant[] Variants { get; set; }

		public bool Twitter { get; set; }
	}
}
