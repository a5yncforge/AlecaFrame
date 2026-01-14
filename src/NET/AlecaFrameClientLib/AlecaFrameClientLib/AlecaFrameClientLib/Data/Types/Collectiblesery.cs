namespace AlecaFrameClientLib.Data.Types
{
	public class Collectiblesery
	{
		public string CollectibleType { get; set; }

		public int Count { get; set; }

		public string Tracking { get; set; }

		public int ReqScans { get; set; }

		public Incentivestate[] IncentiveStates { get; set; }
	}
}
