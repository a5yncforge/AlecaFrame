namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateVoidstorm
	{
		public MiscItemItemId _id { get; set; }

		public string Node { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public string ActiveMissionTier { get; set; }
	}
}
