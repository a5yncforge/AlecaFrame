using AlecaFramePublicLib;

namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateDateMission
	{
		public int Completes { get; set; }

		public int Tier { get; set; }

		public string Tag { get; set; }

		public Rewardscooldowntime RewardsCooldownTime { get; set; }

		public int GetCompletionXP()
		{
			return StaticData.dataHandler.basicRemoteData?.nodeXP.GetOrDefault(Tag) ?? 0;
		}
	}
}
