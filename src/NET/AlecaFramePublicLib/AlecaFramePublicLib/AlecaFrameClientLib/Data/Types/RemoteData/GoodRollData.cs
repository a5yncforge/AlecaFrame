using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types.RemoteData
{
	public class GoodRollData
	{
		public class GoodRoll
		{
			public List<string> mandatory = new List<string>();

			public List<string> optional = new List<string>();
		}

		public List<GoodRoll> goodAttrs = new List<GoodRoll>();

		public List<string> acceptedBadAttrs = new List<string>();
	}
}
