using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSON
	{
		public DateTime timestamp { get; set; }

		public WorldStatusJSONNews[] news { get; set; }

		public WorldStatusJSONEvent[] events { get; set; }

		public object[] alerts { get; set; }

		public WorldStatusJSONSortie sortie { get; set; }

		public WorldStatusJSONSyndicatemission[] syndicateMissions { get; set; }

		public WorldStatusJSONFissure[] fissures { get; set; }

		public object[] globalUpgrades { get; set; }

		public WorldStatusJSONFlashsale[] flashSales { get; set; }

		public WorldStatusJSONInvasion[] invasions { get; set; }

		public object[] darkSectors { get; set; }

		public WorldStatusJSONVoidtrader voidTrader { get; set; }

		public WorldStatusJSONDailydeal[] dailyDeals { get; set; }

		public WorldStatusJSONSimaris simaris { get; set; }

		public WorldStatusJSONConclavechallenge[] conclaveChallenges { get; set; }

		public object[] persistentEnemies { get; set; }

		public WorldStatusJSONEarthcycle earthCycle { get; set; }

		public WorldStatusJSONCetuscycle cetusCycle { get; set; }

		public WorldStatusJSONCambioncycle cambionCycle { get; set; }

		public object[] weeklyChallenges { get; set; }

		public WorldStatusJSONConstructionprogress constructionProgress { get; set; }

		public WorldStatusJSONValliscycle vallisCycle { get; set; }

		public WorldStatusJSONNightwave nightwave { get; set; }

		public object[] kuva { get; set; }

		public WorldStatusJSONArbitration arbitration { get; set; }

		public WorldStatusJSONSentientoutposts sentientOutposts { get; set; }

		public WorldStatusJSONSteelpath steelPath { get; set; }

		public WorldStatusJSONVaulttrader vaultTrader { get; set; }
	}
}
