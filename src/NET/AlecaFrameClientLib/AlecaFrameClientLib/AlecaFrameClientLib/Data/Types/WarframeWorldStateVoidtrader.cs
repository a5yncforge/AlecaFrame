namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateVoidtrader
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public WarframeWorldStateDateManifest[] Manifest { get; set; }

		public string Character { get; set; }

		public string Node { get; set; }
	}
}
