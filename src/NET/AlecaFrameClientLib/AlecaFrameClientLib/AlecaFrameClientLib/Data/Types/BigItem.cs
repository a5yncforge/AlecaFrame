using System.Collections.Generic;
using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public abstract class BigItem
	{
		public List<BigItem> isPartOf = new List<BigItem>();

		public float? marketCost = 0f;

		public string name { get; set; }

		public ItemComponent[] components { get; set; }

		public string imageName { get; set; }

		public int masteryReq { get; set; }

		public string uniqueName { get; set; }

		public string category { get; set; }

		public string wikiaUrl { get; set; }

		public string releaseDate { get; set; }

		public double omegaAttenuation { get; set; }

		public double disposition { get; set; }

		public string description { get; set; }

		public string estimatedVaultDate { get; set; }

		public bool vaulted { get; set; }

		public string vaultDate { get; set; }

		public string productCategory { get; set; }

		public string type { get; set; }

		public Drop[] drops { get; set; }

		public abstract bool IsFullyMastered();

		public abstract int GetMasteryLevel(long XP);

		public abstract int GetMaxMasteryLevel();

		public abstract int GetAccountMasteryGivenPerLevel();

		protected bool isFullyMasteredInner(int expNeeded)
		{
			try
			{
				if (StaticData.dataHandler.warframeRootObject != null)
				{
					Xpinfo xpinfo = StaticData.dataHandler.warframeRootObject.XPInfo.FirstOrDefault((Xpinfo p) => p.ItemType == uniqueName);
					if (xpinfo != null)
					{
						return xpinfo.XP >= expNeeded;
					}
					List<Moapet> source = StaticData.dataHandler.warframeRootObject?.MoaPets?.Where((Moapet p) => p.ModularParts.Contains(uniqueName)).ToList();
					if (source.Any())
					{
						return source.Max((Moapet p) => p.XP) >= expNeeded;
					}
					IEnumerable<Kubrowpet> source2 = StaticData.dataHandler.warframeRootObject?.KubrowPets?.Where((Kubrowpet p) => p.ItemType == uniqueName);
					if (source2.Any())
					{
						return source2.Max((Kubrowpet p) => p.XP) >= expNeeded;
					}
					return false;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public long GetXP()
		{
			return (StaticData.dataHandler?.warframeRootObject?.XPInfo?.FirstOrDefault((Xpinfo p) => p.ItemType == uniqueName)?.XP).GetValueOrDefault();
		}

		public bool IsPrime()
		{
			if (name.ToLower().Contains("prime"))
			{
				return true;
			}
			return false;
		}

		public abstract bool IsOwned();
	}
}
