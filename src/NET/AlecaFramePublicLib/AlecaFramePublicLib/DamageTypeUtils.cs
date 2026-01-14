namespace AlecaFramePublicLib
{
	public class DamageTypeUtils
	{
		public static ProcType GetProcTypeFromDamageType(DamageType damageType)
		{
			switch (damageType)
			{
			case DamageType.Impact:
				return ProcType.Impact;
			case DamageType.Puncture:
				return ProcType.Puncture;
			case DamageType.Slash:
				return ProcType.Slash;
			case DamageType.Heat:
				return ProcType.Heat;
			case DamageType.Cold:
				return ProcType.Cold;
			case DamageType.Electricity:
				return ProcType.Electricity;
			case DamageType.Toxin:
				return ProcType.Poison;
			case DamageType.Void:
				return ProcType.Void;
			case DamageType.Blast:
				return ProcType.Blast;
			case DamageType.Corrosive:
				return ProcType.Corrosive;
			case DamageType.Gas:
				return ProcType.Gas;
			case DamageType.Magnetic:
				return ProcType.Magnetic;
			case DamageType.Radiation:
				return ProcType.Radiation;
			case DamageType.Viral:
				return ProcType.Viral;
			case DamageType.Tau:
				return ProcType.Void;
			default:
				return ProcType.None;
			}
		}
	}
}
