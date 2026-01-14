using System;

namespace AlecaFramePublicLib
{
	[Flags]
	public enum DamageType
	{
		None = 0,
		Impact = 1,
		Puncture = 2,
		Slash = 4,
		Physical = 7,
		BaseElemental = 0x4038,
		CombinedElemental = 0xFC0,
		Cold = 8,
		Heat = 0x10,
		Toxin = 0x20,
		Blast = 0x40,
		Corrosive = 0x80,
		Gas = 0x100,
		Magnetic = 0x200,
		Radiation = 0x400,
		Viral = 0x800,
		True = 0x1000,
		Void = 0x2000,
		Electricity = 0x4000,
		Tau = 0x8000,
		Cinematic = 0x10000,
		AF_All = 0x20000,
		ShieldDrain = 0x40000,
		Finisher = 0x80000
	}
}
