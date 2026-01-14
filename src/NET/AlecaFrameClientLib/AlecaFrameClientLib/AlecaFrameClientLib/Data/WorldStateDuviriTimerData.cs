using System;

namespace AlecaFrameClientLib.Data
{
	public class WorldStateDuviriTimerData
	{
		public class WorldStateDuviryTimerDataState
		{
			public string name;

			public string picture;

			public string timeLeft;

			public DateTimeOffset startTime;

			public DateTimeOffset endTime;
		}

		public WorldStateDuviryTimerDataState current;

		public WorldStateDuviryTimerDataState next;
	}
}
