using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AlecaFrameClientLib.Utils;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public static class PriceHelper
	{
		public class PriceHelperItem
		{
			private bool updateRequested;

			private DateTime validUntil = DateTime.MinValue;

			private readonly OverwolfWrapper.ItemPriceSmallResponse data = new OverwolfWrapper.ItemPriceSmallResponse();

			public OverwolfWrapper.ItemPriceSmallResponse Value => data;

			public bool IsUpToDate()
			{
				return validUntil > DateTime.UtcNow;
			}

			public void UpdateValue(OverwolfWrapper.ItemPriceSmallResponse newData)
			{
				data.post = newData.post;
				data.insta = newData.insta;
				data.postMax = newData.postMax;
				data.minR0 = newData.minR0;
				data.minRMax = newData.minRMax;
				data.volume = newData.volume;
				validUntil = DateTime.UtcNow.Add(MAX_VALID_TIME);
				updateRequested = false;
			}

			public void AcknowledgeUpdateRequest()
			{
				updateRequested = false;
			}

			public bool IsUpdateRequested()
			{
				return updateRequested;
			}

			public void RequestUpdate()
			{
				updateRequested = true;
			}

			public PriceHelperItem()
			{
			}

			public PriceHelperItem(DateTime validUntil, OverwolfWrapper.ItemPriceSmallResponse data)
			{
				this.validUntil = validUntil;
				this.data = data;
			}
		}

		public static readonly TimeSpan MAX_VALID_TIME = TimeSpan.FromMinutes(15.0);

		private static ConcurrentDictionary<string, PriceHelperItem> dataCache = new ConcurrentDictionary<string, PriceHelperItem>();

		private static PriceHelperItem zeroPriceObject = new PriceHelperItem(DateTime.MaxValue, new OverwolfWrapper.ItemPriceSmallResponse());

		public static OverwolfWrapper.ItemPriceSmallResponse GetLazyItemPrice(string itemName, bool requestUpdate = true)
		{
			string warframeMarketURLName = Misc.GetWarframeMarketURLName(itemName);
			if (itemName.ToLower().Contains("orokin") || itemName.ToLower().Contains("forma") || itemName.Contains("’"))
			{
				return zeroPriceObject.Value;
			}
			PriceHelperItem orAdd = dataCache.GetOrAdd(warframeMarketURLName, new PriceHelperItem());
			if (requestUpdate)
			{
				orAdd.RequestUpdate();
			}
			return orAdd.Value;
		}

		public static void Flush(TimeSpan timeout)
		{
			lock (dataCache)
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, PriceHelperItem> item in dataCache)
				{
					if (item.Value.IsUpdateRequested())
					{
						item.Value.AcknowledgeUpdateRequest();
						if (!item.Value.IsUpToDate())
						{
							list.Add(item.Key);
						}
					}
				}
				if (list.Count > 0)
				{
					string dataToSend = JsonConvert.SerializeObject(list);
					Console.WriteLine("Requesting item price data...");
					OverwolfWrapper.ItemPriceSmallResponse[] array = JsonConvert.DeserializeObject<OverwolfWrapper.ItemPriceSmallResponse[]>(HTTPHandler.MakePOSTRequest(StaticData.PricesAPIHostname + "/priceData", dataToSend, (int)timeout.TotalMilliseconds, 1));
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] != null)
						{
							dataCache[list[i]].UpdateValue(array[i]);
							continue;
						}
						dataCache[list[i]].UpdateValue(new OverwolfWrapper.ItemPriceSmallResponse
						{
							post = 0,
							insta = 0
						});
					}
				}
				else
				{
					Console.WriteLine("Not requesting any price because all data was cached!");
				}
			}
		}
	}
}
