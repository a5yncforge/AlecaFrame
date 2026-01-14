using System.Collections.Generic;

namespace AlecaFrameClientLib.Data
{
	public class WorldStateBaroData
	{
		public bool baroEnabled;

		public string baroArrivesOrEndsIn = "";

		public List<BaroReturnGroup> itemGroups = new List<BaroReturnGroup>();
	}
}
