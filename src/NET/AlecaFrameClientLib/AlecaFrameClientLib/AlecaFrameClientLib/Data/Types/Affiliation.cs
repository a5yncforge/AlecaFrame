namespace AlecaFrameClientLib.Data.Types
{
	public class Affiliation
	{
		public bool Initiated { get; set; }

		public int Standing { get; set; }

		public string Tag { get; set; }

		public int Title { get; set; }

		public int[] FreeFavorsEarned { get; set; }

		public int[] FreeFavorsUsed { get; set; }
	}
}
