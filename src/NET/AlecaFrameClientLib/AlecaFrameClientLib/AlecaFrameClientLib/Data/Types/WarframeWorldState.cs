namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldState
	{
		public string WorldSeed { get; set; }

		public int Version { get; set; }

		public string MobileVersion { get; set; }

		public string BuildLabel { get; set; }

		public int Time { get; set; }

		public WarframeWorldStateEvent[] Events { get; set; }

		public WarframeWorldStateGoal[] Goals { get; set; }

		public object[] Alerts { get; set; }

		public WarframeWorldStateSorty[] Sorties { get; set; }

		public WarframeWorldStateLitesorty[] LiteSorties { get; set; }

		public WarframeWorldStateSyndicatemission[] SyndicateMissions { get; set; }

		public WarframeWorldStateActivemission[] ActiveMissions { get; set; }

		public object[] GlobalUpgrades { get; set; }

		public WarframeWorldStateFlashsale[] FlashSales { get; set; }

		public WarframeWorldStateIngamemarket InGameMarket { get; set; }

		public WarframeWorldStateInvasion[] Invasions { get; set; }

		public object[] HubEvents { get; set; }

		public WarframeWorldStateNodeoverride[] NodeOverrides { get; set; }

		public WarframeWorldStateVoidtrader[] VoidTraders { get; set; }

		public WarframeWorldStatePrimevaulttrader[] PrimeVaultTraders { get; set; }

		public WarframeWorldStateVoidstorm[] VoidStorms { get; set; }

		public WarframeWorldStatePrimeaccessavailability PrimeAccessAvailability { get; set; }

		public bool[] PrimeVaultAvailabilities { get; set; }

		public bool PrimeTokenAvailability { get; set; }

		public WarframeWorldStateDailydeal[] DailyDeals { get; set; }

		public WarframeWorldStateLibraryinfo LibraryInfo { get; set; }

		public WarframeWorldStatePvpchallengeinstance[] PVPChallengeInstances { get; set; }

		public object[] PersistentEnemies { get; set; }

		public object[] PVPAlternativeModes { get; set; }

		public object[] PVPActiveTournaments { get; set; }

		public float[] ProjectPct { get; set; }

		public object[] ConstructionProjects { get; set; }

		public object[] TwitchPromos { get; set; }

		public object[] ExperimentRecommended { get; set; }

		public int ForceLogoutVersion { get; set; }

		public WarframeWorldStateFeaturedguild[] FeaturedGuilds { get; set; }

		public WarframeWorldStateSeasoninfo SeasonInfo { get; set; }

		public string Tmp { get; set; }

		public WarframeWorldStateEndlessCoiches[] EndlessXpChoices { get; set; }
	}
}
