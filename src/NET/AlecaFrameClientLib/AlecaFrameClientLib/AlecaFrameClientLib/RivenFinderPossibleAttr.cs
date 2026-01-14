namespace AlecaFrameClientLib
{
	internal class RivenFinderPossibleAttr
	{
		public string name { get; }

		public string internalName { get; }

		public double min { get; }

		public double max { get; }

		public bool isPercentage { get; }

		public bool isMultiplier { get; }

		public RivenFinderPossibleAttr(string name, string internalName, double min, double max, bool isPercentage, bool isMultiplier)
		{
			this.name = name;
			this.internalName = internalName;
			this.min = min;
			this.max = max;
			this.isPercentage = isPercentage;
			this.isMultiplier = isMultiplier;
		}
	}
}
