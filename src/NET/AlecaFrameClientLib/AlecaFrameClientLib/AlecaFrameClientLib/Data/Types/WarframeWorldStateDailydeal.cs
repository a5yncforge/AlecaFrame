namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateDailydeal
	{
		public string StoreItem { get; set; }

		public WarframeWorldStateDateInside Activation { get; set; }

		public WarframeWorldStateDateInside Expiry { get; set; }

		public int Discount { get; set; }

		public int OriginalPrice { get; set; }

		public int SalePrice { get; set; }

		public int AmountTotal { get; set; }

		public int AmountSold { get; set; }
	}
}
