using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class BigItemComparer : EqualityComparer<BigItem>
	{
		public override bool Equals(BigItem x, BigItem y)
		{
			return x.uniqueName == y.uniqueName;
		}

		public override int GetHashCode(BigItem obj)
		{
			return obj.uniqueName.GetHashCode();
		}
	}
}
