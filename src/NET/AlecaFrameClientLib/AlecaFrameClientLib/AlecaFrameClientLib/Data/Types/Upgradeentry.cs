namespace AlecaFrameClientLib.Data.Types
{
	public class Upgradeentry
	{
		public string tag { get; set; }

		public string prefixTag { get; set; }

		public string suffixTag { get; set; }

		public Upgradevalue[] upgradeValues { get; set; }
	}
}
