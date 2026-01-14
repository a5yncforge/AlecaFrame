namespace AlecaFrameClientLib.Data.Types
{
	public class Crewship : ModeableItem
	{
		public Weapon Weapon { get; set; }

		public Customization Customization { get; set; }

		public int[] SlotLevels { get; set; }

		public string ItemName { get; set; }

		public Railjackimage RailjackImage { get; set; }
	}
}
