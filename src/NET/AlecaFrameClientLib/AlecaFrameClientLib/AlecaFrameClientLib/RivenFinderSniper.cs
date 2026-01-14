using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types.RemoteData;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Newtonsoft.Json;

namespace AlecaFrameClientLib
{
	public class RivenFinderSniper
	{
		public class WeaponListResult
		{
			public string name;

			public string uniqueID;

			public string image;
		}

		public static List<WeaponListResult> GetRivenWeapons(string search)
		{
			return (from p in (from p in StaticData.dataHandler.rivenData.weaponStats
					where !string.IsNullOrWhiteSpace(p.Value.rivenUID) && p.Value.name.ToLower().Contains(search.ToLower())
					orderby p.Value.name
					select p).Take(25)
				select new WeaponListResult
				{
					name = p.Value.name,
					uniqueID = p.Key,
					image = Misc.GetFullImagePath(Misc.GetBigItemRefernceOrNull(p.Key)?.imageName)
				} into p
				where !string.IsNullOrEmpty(p.image)
				select p).ToList();
		}

		public static RivenSniperStatus GetRemoteAccountDataOrNull()
		{
			string playerUserHash = StatsHandler.GetPlayerUserHash();
			if (playerUserHash == null)
			{
				return null;
			}
			return JsonConvert.DeserializeObject<RivenSniperStatus>(HTTPHandler.MakeGETRequest(StaticData.RivenAPIHostname + "/sniper/status?token=" + playerUserHash + "&username=" + FoundryHelper.lastFetchedUsername));
		}
	}
}
