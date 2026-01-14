namespace AlecaFrameClientLib.Data.Types
{
	public class SecondaryMode : Damagetypes
	{
		public string name { get; set; }

		public float speed { get; set; }

		public float crit_chance { get; set; }

		public float crit_mult { get; set; }

		public float status_chance { get; set; }

		public string shot_type { get; set; }

		public string damage { get; set; }

		public float charge_time { get; set; }

		public float? shot_speed { get; set; }

		public Pellet pellet { get; set; }

		public float radius { get; set; }
	}
}
