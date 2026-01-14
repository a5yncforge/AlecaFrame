using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class ExtraModData
	{
		public ExtraModDataChallenge challenge { get; set; }

		public string compat { get; set; }

		public int lim { get; set; }

		public int lvlReq { get; set; }

		public int lvl { get; set; }

		public int rerolls { get; set; }

		public string pol { get; set; }

		public ExtraModDataBuff[] buffs { get; set; }

		public ExtraModDataCurse[] curses { get; set; }

		public bool IsRivenUnveiled()
		{
			return challenge == null;
		}

		public static ExtraModData DeserializeFromString(string json)
		{
			ExtraModData result = new ExtraModData();
			if (!string.IsNullOrEmpty(json))
			{
				result = JsonConvert.DeserializeObject<ExtraModData>(json);
			}
			return result;
		}
	}
}
