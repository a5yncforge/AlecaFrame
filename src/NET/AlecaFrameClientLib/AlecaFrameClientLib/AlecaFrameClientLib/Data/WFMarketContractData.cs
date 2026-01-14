using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types.WFM;

namespace AlecaFrameClientLib.Data
{
	public class WFMarketContractData
	{
		public string picture;

		public string name;

		public string weaponName;

		public bool orderVisible = true;

		public bool showWarning;

		public string randomID;

		public bool isAuction;

		public string platinumText;

		public RivenUnveiledStats stats = new RivenUnveiledStats();

		[NonSerialized]
		public int orderingPrice;

		[NonSerialized]
		private WFMRivenDataAuction wfmarketContract;

		[NonSerialized]
		public string weaponType;

		public WFMarketContractData(WFMRivenDataAuction contract, IEnumerable<RivenSummaryData> itemType)
		{
			wfmarketContract = contract;
			orderVisible = contract.visible;
			RivenSummaryData rivenData = RivenExplorerHelper.GetSingleRivenDetailsFromWFM(contract);
			picture = rivenData.weaponPicture;
			name = rivenData.name;
			weaponName = rivenData.weaponName;
			randomID = contract.id;
			stats = rivenData.statsPerWeapon.FirstOrDefault()?.byLevel.LastOrDefault();
			isAuction = !contract.is_direct_sell;
			weaponType = rivenData.weaponType;
			orderingPrice = contract.starting_price;
			if (isAuction)
			{
				platinumText = string.Format("{0}/{1}", contract.starting_price, (contract.buyout_price == 0) ? "-" : contract.buyout_price.ToString());
			}
			else
			{
				platinumText = contract.starting_price.ToString();
			}
			bool flag = itemType.Any((RivenSummaryData p) => p.IsRoughlyEqual(rivenData));
			showWarning = !flag;
		}
	}
}
