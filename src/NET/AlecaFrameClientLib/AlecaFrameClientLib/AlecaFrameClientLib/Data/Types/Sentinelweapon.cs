namespace AlecaFrameClientLib.Data.Types
{
	public class Sentinelweapon : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public int Features { get; set; }

		public Polarity4[] Polarity { get; set; }

		public int Polarized { get; set; }
	}
}
