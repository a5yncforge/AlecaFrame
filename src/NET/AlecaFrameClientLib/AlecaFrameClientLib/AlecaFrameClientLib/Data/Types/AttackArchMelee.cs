using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class AttackArchMelee
	{
		public Dictionary<string, float> damage = new Dictionary<string, float>();

		public string name { get; set; }

		public float speed { get; set; }

		public int crit_chance { get; set; }

		public float crit_mult { get; set; }

		public int status_chance { get; set; }
	}
}
