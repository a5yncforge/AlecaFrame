namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStatePrimevaulttrader
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public bool Completed { get; set; }

		public WarframeWorldStateDateInside InitialStartDate { get; set; }

		public string Node { get; set; }

		public WarframeWorldStateDateManifest[] Manifest { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public WarframeWorldStateDateEvergreenmanifest[] EvergreenManifest { get; set; }

		public Scheduleinfo[] ScheduleInfo { get; set; }
	}
}
