using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class Patchlog
	{
		public string name { get; set; }

		public DateTime date { get; set; }

		public string url { get; set; }

		public string additions { get; set; }

		public string changes { get; set; }

		public string fixes { get; set; }
	}
}
