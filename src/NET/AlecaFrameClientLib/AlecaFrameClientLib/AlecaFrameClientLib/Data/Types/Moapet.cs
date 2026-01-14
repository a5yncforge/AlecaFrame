namespace AlecaFrameClientLib.Data.Types
{
	public class Moapet : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public string[] ModularParts { get; set; }

		public int Features { get; set; }

		public string ItemName { get; set; }

		public Polarity8[] Polarity { get; set; }
	}
}
