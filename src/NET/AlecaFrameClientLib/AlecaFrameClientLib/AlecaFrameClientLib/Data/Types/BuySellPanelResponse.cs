using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class BuySellPanelResponse
	{
		public List<BuySellPanelResponseItem> sellListings = new List<BuySellPanelResponseItem>();

		public List<BuySellPanelResponseItem> buyListings = new List<BuySellPanelResponseItem>();

		public BuySellPanelResponseSettings postingSettings = new BuySellPanelResponseSettings();
	}
}
