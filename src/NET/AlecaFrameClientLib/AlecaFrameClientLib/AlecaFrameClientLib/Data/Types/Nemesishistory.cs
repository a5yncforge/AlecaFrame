namespace AlecaFrameClientLib.Data.Types
{
	public class Nemesishistory
	{
		public long fp { get; set; }

		public string manifest { get; set; }

		public string KillingSuit { get; set; }

		public int killingDamageType { get; set; }

		public string ShoulderHelmet { get; set; }

		public int AgentIdx { get; set; }

		public string BirthNode { get; set; }

		public int Rank { get; set; }

		public bool k { get; set; }

		public D d { get; set; }

		public int[] GuessHistory { get; set; }

		public int currentGuess { get; set; }

		public bool Traded { get; set; }

		public int PrevOwners { get; set; }
	}
}
