namespace AlecaFrameClientLib.Utils
{
	public class SquadRequirement
	{
		public string name;

		public string imageURL;

		public string internalName;

		public SquadRequirement(string name, string imageURL, string internalName)
		{
			this.name = name;
			this.imageURL = imageURL;
			this.internalName = internalName;
		}

		public SquadRequirement()
		{
		}
	}
}
