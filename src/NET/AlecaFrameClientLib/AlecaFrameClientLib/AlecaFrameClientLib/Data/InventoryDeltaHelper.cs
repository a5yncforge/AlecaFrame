using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib.Data
{
	public static class InventoryDeltaHelper
	{
		private static DeltaSaveObject currentDeltaSave = new DeltaSaveObject();

		private static object lockObject = new object();

		public static void Initialize()
		{
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Initializing DeltaHelper...");
			currentDeltaSave = DeltaSaveObject.Load();
		}

		public static void ApplyNewData(WarframeRootObject warframeRootObject)
		{
			lock (lockObject)
			{
				ILookup<string, Miscitem> oldMiscDataInDictForm = currentDeltaSave.previousMiscState.ToLookup((Miscitem p) => p.ItemType);
				List<Miscitem> list = warframeRootObject.MiscItems.Union(warframeRootObject.Recipes).ToList();
				if (currentDeltaSave.savedCleanly)
				{
					foreach (Miscitem item in (from p in list
						select new Miscitem
						{
							ItemType = p.ItemType,
							ItemCount = p.ItemCount - (oldMiscDataInDictForm[p.ItemType]?.FirstOrDefault()?.ItemCount).GetValueOrDefault()
						} into p
						where p.ItemCount > 0
						select p).ToList())
					{
						if (!currentDeltaSave.currentDeltas.ContainsKey(item.ItemType))
						{
							currentDeltaSave.currentDeltas.Add(item.ItemType, 0);
						}
						currentDeltaSave.currentDeltas[item.ItemType] += item.ItemCount;
					}
				}
				else
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Ingoring new data when applying deltas because this is the first time!");
				}
				StaticData.overwolfWrappwer.SendDeltaUpdate(GetDeltas(withPrices: false).items.Sum((DeltaResponseItem p) => p.baseData.amountNow));
				currentDeltaSave.previousMiscState = list;
				currentDeltaSave.savedCleanly = true;
				DeltaSaveObject.Save(currentDeltaSave);
			}
		}

		public static Dictionary<string, int> GetRawDeltas()
		{
			return currentDeltaSave.currentDeltas;
		}

		public static DeltaResponseObject GetDeltas(bool withPrices)
		{
			return new DeltaResponseObject(GetRawDeltas(), withPrices);
		}

		public static void RemoveDeltas()
		{
			currentDeltaSave.currentDeltas.Clear();
			DeltaSaveObject.Save(currentDeltaSave);
		}

		public static void ResetAll()
		{
			currentDeltaSave = new DeltaSaveObject();
			DeltaSaveObject.Save(currentDeltaSave);
		}
	}
}
