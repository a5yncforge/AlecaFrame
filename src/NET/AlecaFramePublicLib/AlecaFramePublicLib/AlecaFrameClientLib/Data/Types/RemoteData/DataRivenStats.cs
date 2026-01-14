using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types.RemoteData
{
	public class DataRivenStats
	{
		public string rivenInternalID;

		public string veiledName;

		public int baseDrain = 10;

		public int fusionLimit = 8;

		public Dictionary<string, DataRivenStatsModifier> rivenStats = new Dictionary<string, DataRivenStatsModifier>();

		public Dictionary<string, DataRivenChallenge> challenges = new Dictionary<string, DataRivenChallenge>();
	}
}
