namespace AlecaFrameClientLib.Data.Types
{
	public class Upgrade : Miscitem
	{
		public int TryGetModRank(int defaultModRank = 0)
		{
			try
			{
				if (!string.IsNullOrEmpty(base.UpgradeFingerprint))
				{
					return ExtraModData.DeserializeFromString(base.UpgradeFingerprint)?.lvl ?? defaultModRank;
				}
				return defaultModRank;
			}
			catch
			{
				return defaultModRank;
			}
		}
	}
}
