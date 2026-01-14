using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data
{
	public class DeltaResponseObject
	{
		public List<DeltaResponseItem> items = new List<DeltaResponseItem>();

		public int totalPlatinum;

		public int totalDucats;

		public int totalHarrows;

		public DeltaResponseObject(Dictionary<string, int> deltas, bool withPrices)
		{
			items = (from p in deltas
				select new DeltaResponseItem(p.Key, p.Value) into p
				where p.allOK
				select p).ToList();
			if (withPrices)
			{
				IEnumerable<string> source = items.Select((DeltaResponseItem p) => Misc.GetWarframeMarketURLName(p.baseData.name));
				OverwolfWrapper.ItemPriceSmallResponse[] prices = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(source.ToArray(), TimeSpan.FromSeconds(7.0));
				items = (from p in items.Select(delegate(DeltaResponseItem p, int index)
					{
						p.baseData.sellPrice = (prices[index]?.post).GetValueOrDefault();
						p.baseData.buyPrice = (prices[index]?.insta).GetValueOrDefault();
						return p;
					})
					orderby p.baseData.sellPrice descending
					select p).ToList();
			}
			totalHarrows = deltas.GetOrDefault("/Lotus/Types/Recipes/WarframeRecipes/PriestChassisBlueprint");
			totalPlatinum = items.Sum((DeltaResponseItem p) => p.baseData.sellPrice * p.baseData.amountNow);
			totalDucats = items.Sum((DeltaResponseItem p) => p.baseData.ducats * p.baseData.amountNow);
		}
	}
}
