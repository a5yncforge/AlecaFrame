using System;

namespace AlecaFrameClientLib.Data
{
	public class WorldStateRelicDataPoint
	{
		public string name;

		public string type;

		public string relicTier;

		public string planet;

		[NonSerialized]
		public string planetAlone;

		public string timeLeft;

		public bool steelPath;

		[NonSerialized]
		public string uniqueKey;

		[NonSerialized]
		public TimeSpan timeLeftRAW;

		public bool matchesNotification;
	}
}
