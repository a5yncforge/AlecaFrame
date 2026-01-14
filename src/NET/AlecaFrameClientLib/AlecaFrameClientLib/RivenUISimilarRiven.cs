using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class RivenUISimilarRiven
{
	public class Attr
	{
		public string text;

		public bool positive;

		public bool matches;
	}

	public string source;

	public double similarity;

	public double price;

	public string link;

	public string id;

	[JsonConverter(typeof(StringEnumConverter))]
	public AlecaFrameRivenGrade grade;

	public List<Attr> attrs = new List<Attr>();
}
