namespace AlecaFrameClientLib.Data.Types
{
	public class Config
	{
		public string[] Skins { get; set; }

		public Pricol pricol { get; set; }

		public Eyecol eyecol { get; set; }

		public Sigcol sigcol { get; set; }

		public string[] PvpUpgrades { get; set; }

		public string[] Upgrades { get; set; }

		public string Name { get; set; }

		public Song[] Songs { get; set; }

		public bool ugly { get; set; }
	}
}
