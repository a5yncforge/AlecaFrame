using System;
using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class PlayerStatsData
	{
		public List<PlayerStatsDataPoint> generalDataPoints { get; set; } = new List<PlayerStatsDataPoint>();

		public List<PlayerStatsTrade> trades { get; set; } = new List<PlayerStatsTrade>();

		public DateTime lastUpdate { get; set; } = DateTime.MinValue;

		public string userHash { get; set; }

		public PublicLinkParts publicParts { get; set; }

		public string usernameWhenPublic { get; set; }
	}
}
