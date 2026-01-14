namespace AlecaFrameClientLib.Data.Types
{
	public class Specialitem : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public int Features { get; set; }

		public Polarity7[] Polarity { get; set; }

		public int Polarized { get; set; }
	}
}
