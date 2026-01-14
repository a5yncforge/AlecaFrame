namespace AlecaFrameClientLib.Data.Types
{
	public class DataArcane : DataMod
	{
		public int buildPrice { get; set; }

		public int buildQuantity { get; set; }

		public int buildTime { get; set; }

		public bool consumeOnBuild { get; set; }

		public int skipBuildTimePrice { get; set; }
	}
}
