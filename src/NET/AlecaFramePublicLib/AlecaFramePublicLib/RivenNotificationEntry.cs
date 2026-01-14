using System;

namespace AlecaFramePublicLib
{
	public class RivenNotificationEntry
	{
		public string realWeaponUID { get; set; }

		public RivenSimilarityRequest data { get; set; }

		public DateTime lastNotificationSent { get; set; }

		public DateTime creationDate { get; set; }

		public bool enabled { get; set; } = true;

		public bool innactivityDisabled { get; set; }

		public string name { get; set; }

		public string id { get; set; }

		public string userToken { get; set; }
	}
}
