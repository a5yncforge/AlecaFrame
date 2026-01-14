using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlecaFramePublicLib
{
	public class OCRResult
	{
		[NonSerialized]
		public OCRResultCode resultCode;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string text { get; set; }

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public List<OCRResultBox> boxes { get; set; }
	}
}
