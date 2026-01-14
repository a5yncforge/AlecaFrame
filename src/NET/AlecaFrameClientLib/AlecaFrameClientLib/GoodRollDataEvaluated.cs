using System.Collections.Generic;

public class GoodRollDataEvaluated
{
	public class GoodRollEvaluated
	{
		public List<AttrEval> mandatory = new List<AttrEval>();

		public List<AttrEval> optional = new List<AttrEval>();
	}

	public class AttrEval
	{
		public string text;

		public bool matches;
	}

	public List<GoodRollEvaluated> goodAttrs = new List<GoodRollEvaluated>();

	public List<AttrEval> acceptedBadAttrs = new List<AttrEval>();
}
