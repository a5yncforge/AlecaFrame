using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class User
	{
		public float reputation { get; set; }

		public string locale { get; set; }

		public string platform { get; set; }

		public string avatar { get; set; }

		public DateTime lastSeen { get; set; }

		public string ingameName { get; set; }

		public string id { get; set; }

		public string status { get; set; }
	}
}
