namespace AlecaFrameClientLib
{
	public class OCRLocationSettings
	{
		public float rectTop;

		public float rectBottom;

		public float rectWdith;

		public float rectSeparation;

		public float overallTopScreenPerOne;

		public float overallBottomScreenPerOne;

		public float relicCounterTop;

		public float relicCounterBottom;

		public OCRLocationSettings Clone()
		{
			return (OCRLocationSettings)MemberwiseClone();
		}
	}
}
