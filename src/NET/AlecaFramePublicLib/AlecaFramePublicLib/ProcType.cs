using System;

namespace AlecaFramePublicLib
{
	[Flags]
	public enum ProcType
	{
		None = 0,
		Poison = 1,
		Corrosive = 2,
		Radiation = 3,
		Viral = 4,
		Magnetic = 5,
		Gas = 6,
		Blast = 7,
		Void = 8,
		Electricity = 9,
		Cold = 0xA,
		Heat = 0xB,
		Slash = 0xC,
		Puncture = 0xD,
		Impact = 0xE,
		Staggered = 0xF,
		BigStaggered = 0x10,
		Stunned = 0x11,
		Knockdown = 0x12,
		Ragdoll = 0x13,
		Lifted = 0x14
	}
}
