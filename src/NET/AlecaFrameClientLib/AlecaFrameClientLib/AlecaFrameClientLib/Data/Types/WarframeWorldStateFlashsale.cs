namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateFlashsale
	{
		public string TypeName { get; set; }

		public WarframeWorldStateDateInside StartDate { get; set; }

		public WarframeWorldStateDateInside EndDate { get; set; }

		public bool Featured { get; set; }

		public bool Popular { get; set; }

		public bool ShowInMarket { get; set; }

		public bool ShowWithRecommended { get; set; }

		public bool SupporterPack { get; set; }

		public int BannerIndex { get; set; }

		public int Discount { get; set; }

		public int RegularOverride { get; set; }

		public int PremiumOverride { get; set; }

		public int BogoBuy { get; set; }

		public int BogoGet { get; set; }

		public bool HideFromMarket { get; set; }

		public bool VoidEclipse { get; set; }
	}
}
