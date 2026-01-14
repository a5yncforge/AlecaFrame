using System;

namespace AlecaFrameClientLib.Data.Types.WFM
{
	public class WFMarketNewChatPayload
	{
		public string chat_id { get; set; }

		public string message { get; set; }

		public string raw_message { get; set; }

		public string message_from { get; set; }

		public DateTime send_date { get; set; }

		public string id { get; set; }
	}
}
