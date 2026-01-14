namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateSyndicatemission
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string Tag { get; set; }

		public int Seed { get; set; }

		public string[] Nodes { get; set; }

		public Job[] Jobs { get; set; }
	}
}
