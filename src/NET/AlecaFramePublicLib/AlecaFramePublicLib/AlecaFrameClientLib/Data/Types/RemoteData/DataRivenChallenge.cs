using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types.RemoteData
{
	public class DataRivenChallenge
	{
		public string challengeUID;

		public string description;

		public Dictionary<string, DataRivenChallengeComplication> complications = new Dictionary<string, DataRivenChallengeComplication>();
	}
}
