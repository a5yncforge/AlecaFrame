using System.Collections.Generic;

namespace AlecaFrameClientLib.Data
{
	public class WorldStateTimerData
	{
		public string cetusState;

		public string cetusTimeLeft;

		public string earthState;

		public string earthTimeLeft;

		public string cambionState;

		public string cambionTimeLeft;

		public string vallisState;

		public string vallisTimeLeft;

		public string sortieTimeLeft;

		public string weeklyTimeLeft;

		public string circuitTimeLeft;

		public string dailyUTCTimeLeft;

		public List<WorldStateRelicDataPointGroup> relicDataPoints;

		public List<WorldStateRelicDataPointGroup> relicDataPointsHard;

		public WorldStateDuviriTimerData duviriTimers;
	}
}
