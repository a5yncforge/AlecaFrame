using System;

namespace AlecaFramePublicLib
{
	[Flags]
	public enum PublicLinkParts
	{
		None = 0,
		Trades = 1,
		Platinum = 2,
		Ducats = 4,
		Endo = 8,
		Credits = 0x10,
		AccountData = 0x20,
		Aya = 0x40,
		Relics = 0x80
	}
}
