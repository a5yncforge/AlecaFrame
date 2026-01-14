namespace AlecaFrameClientLib.Data.Types
{
	public class Challengeinstancestate
	{
		public Id id { get; set; }

		public float Progress { get; set; }

		public WarframeWorldStateDateParam[] _params { get; set; }

		public bool IsRewardCollected { get; set; }
	}
}
