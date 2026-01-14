using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class RivenSniperStatus
	{
		public AlecaFrameSubscriptionStatus AlecaFrameSubscriptionStatus { get; set; }

		public List<RivenNotificationEntry> notifications { get; set; }

		public string notificationDiscordWebhook { get; set; }

		public int maxSubscriptions { get; set; }
	}
}
