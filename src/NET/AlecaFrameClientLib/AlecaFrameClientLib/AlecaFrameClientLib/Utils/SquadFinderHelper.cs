using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib.Utils
{
	public static class SquadFinderHelper
	{
		private enum ItemType
		{
			Prime,
			NonPrime,
			Both
		}

		public static List<SquadRequirement> GetAvailableSquadRequirementOptions(string squadType)
		{
			List<SquadRequirement> list = new List<SquadRequirement>();
			switch (squadType)
			{
			case "RelicOpeningAdvanced":
			case "RelicOpeningBasic":
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/AnyLith",
					name = "Any Lith",
					imageURL = Misc.GetFullImagePath("lith-intact.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/AnyMeso",
					name = "Any Meso",
					imageURL = Misc.GetFullImagePath("meso-intact.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/AnyNeo",
					name = "Any Neo",
					imageURL = Misc.GetFullImagePath("neo-intact.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/AnyAxi",
					name = "Any Axi",
					imageURL = Misc.GetFullImagePath("axi-intact.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/AnyRequiem",
					name = "Any Requiem",
					imageURL = Misc.GetFullImagePath("requiem-intact.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/Any",
					name = "Any relic",
					imageURL = "assets/img/infinity.png"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/aRadiant",
					name = "Radiant",
					imageURL = "assets/img/trace.webp"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/2OrMore",
					name = "2+ rounds",
					imageURL = "assets/img/repeat.png"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/10OrMore",
					name = "5+ rounds",
					imageURL = "assets/img/repeat.png"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/10OrMore",
					name = "10+ rounds",
					imageURL = "assets/img/repeat.png"
				});
				AddWarframes(list, ItemType.Prime);
				AddWeapons(list, ItemType.Prime);
				foreach (KeyValuePair<string, ItemComponent> item in from p in StaticData.dataHandler.warframeParts.Union(StaticData.dataHandler.weaponParts)
					where p.Value.tradable
					select p)
				{
					list.Add(new SquadRequirement
					{
						name = ((item.Value.name == "Blueprint") ? item.Value.GetRealExternalName() : item.Value.GetRealExternalName().Replace(" Blueprint", "")),
						imageURL = Misc.GetFullImagePath(item.Value.imageName),
						internalName = item.Value.uniqueName
					});
				}
				foreach (KeyValuePair<string, DataRelic> item2 in StaticData.dataHandler.relics.Where((KeyValuePair<string, DataRelic> p) => p.Key.EndsWith("Bronze")))
				{
					list.Add(new SquadRequirement
					{
						name = item2.Value.name.Replace(" Intact", ""),
						imageURL = Misc.GetFullImagePath(item2.Value.imageName),
						internalName = item2.Value.uniqueName.Substring(0, item2.Value.uniqueName.LastIndexOf("Bronze"))
					});
				}
				break;
			case "Farming":
				StaticData.dataHandler.resources.Values.Cast<BigItem>().Union(StaticData.dataHandler.misc.Values.Cast<BigItem>(), new BigItemComparer());
				foreach (BigItem item3 in StaticData.dataHandler.resources.Values.Cast<BigItem>().Union(StaticData.dataHandler.misc.Values.Cast<BigItem>(), new BigItemComparer()))
				{
					if (!string.IsNullOrWhiteSpace(item3.imageName) && !string.IsNullOrWhiteSpace(item3.name) && !item3.uniqueName.Contains("/FusionBundles/"))
					{
						list.Add(new SquadRequirement
						{
							name = item3.name,
							imageURL = Misc.GetFullImagePath(item3.imageName),
							internalName = item3.uniqueName
						});
					}
				}
				list.Add(new SquadRequirement("Endo", "assets/img/endo.png", "/AF_Internal/Endo"));
				break;
			case "GrandBosses":
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/Eidolon",
					name = "Eidolon (Only first Eidolon)",
					imageURL = "assets/img/number-1.png"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/Tridolon",
					name = "Tridolon (All three Eidolons)",
					imageURL = "assets/img/number-3.png"
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/ProfitTaker",
					name = "Profit-Taker Orb",
					imageURL = Misc.GetFullImagePath("profit-taker-orb-articula.png")
				});
				list.Add(new SquadRequirement
				{
					internalName = "/AF_Internal/Exploiter",
					name = "Exploiter Orb",
					imageURL = Misc.GetFullImagePath("exploiter-orb-articula.png")
				});
				AddWarframes(list, ItemType.NonPrime);
				break;
			case "BountyFarming":
			{
				for (int i = 1; i < 6; i++)
				{
					list.Add(new SquadRequirement("Cetus T" + i, Misc.GetFullImagePath("cetus-wisp.png"), "/AF_Internal/CetusT" + i));
					list.Add(new SquadRequirement("Fortuna T" + i, Misc.GetFullImagePath("fortuna-background.png"), "/AF_Internal/FortunaT" + i));
					list.Add(new SquadRequirement("Deimos T" + i, Misc.GetFullImagePath("necraloid-glyph.png"), "/AF_Internal/DeimosT" + i));
					list.Add(new SquadRequirement("Zariman T" + i, Misc.GetFullImagePath("zariman-glyph.png"), "/AF_Internal/ZarimanT" + i));
				}
				for (int j = 1; j < 4; j++)
				{
					list.Add(new SquadRequirement("Isolation vault T" + j, Misc.GetFullImagePath("necraseal-emblem.png"), "/AF_Internal/VaultT" + j));
				}
				list.Add(new SquadRequirement("Narmer", Misc.GetFullImagePath("narmer-eye-sigil.png"), "/AF_Internal/Narmer"));
				break;
			}
			case "Sanctuary":
				list.Add(new SquadRequirement("Normal", Misc.GetFullImagePath("cephalon-simaris-sigil.png"), "/AF_Internal/SanctNormal"));
				list.Add(new SquadRequirement("Elite", Misc.GetFullImagePath("cephalon-simaris-sigil.png"), "/AF_Internal/SanctElite"));
				break;
			case "Lich":
				list.Add(new SquadRequirement("Lich", Misc.GetFullImagePath("lich-token.png"), "/AF_Internal/LichReal"));
				list.Add(new SquadRequirement("Sister", Misc.GetFullImagePath("ph-corpus-lich-sigil.png"), "/AF_Internal/LichSister"));
				break;
			case "Duviri":
				list.Add(new SquadRequirement("The circuit", "assets/img/duviriCircuit.png", "/AF_Internal/DuviriCircuit"));
				list.Add(new SquadRequirement("The duviri experience", "assets/img/duviriExperience.png", "/AF_Internal/DuviriExperience"));
				list.Add(new SquadRequirement("The lone story", "assets/img/duviriLoneStory.png", "/AF_Internal/DuviriLoneStory"));
				list.Add(new SquadRequirement("15 mins", "assets/img/recent.png", "/AF_Internal/Time15"));
				list.Add(new SquadRequirement("30 mins", "assets/img/recent.png", "/AF_Internal/Time30"));
				list.Add(new SquadRequirement("60+ mins", "assets/img/recent.png", "/AF_Internal/Time60"));
				AddWarframes(list, ItemType.NonPrime);
				break;
			case "Arbitration":
				list.Add(new SquadRequirement("15 mins", "assets/img/recent.png", "/AF_Internal/Time15"));
				list.Add(new SquadRequirement("30 mins", "assets/img/recent.png", "/AF_Internal/Time30"));
				list.Add(new SquadRequirement("60+ mins", "assets/img/recent.png", "/AF_Internal/Time60"));
				AddWarframes(list, ItemType.NonPrime);
				break;
			case "HardMode":
				list.Add(new SquadRequirement("15 mins", "assets/img/recent.png", "/AF_Internal/Time15"));
				list.Add(new SquadRequirement("30 mins", "assets/img/recent.png", "/AF_Internal/Time30"));
				list.Add(new SquadRequirement("60+ mins", "assets/img/recent.png", "/AF_Internal/Time60"));
				AddWarframes(list, ItemType.NonPrime);
				break;
			}
			return list;
		}

		private static void AddWeapons(List<SquadRequirement> squadRequirements, ItemType type)
		{
			foreach (BigItem item in StaticData.dataHandler.primaryWeapons.Values.Cast<BigItem>().Union(StaticData.dataHandler.secondaryWeapons.Values).Union(StaticData.dataHandler.meleeWeapons.Values))
			{
				if (type == ItemType.Both || (type == ItemType.Prime && item.IsPrime()) || (type == ItemType.NonPrime && !item.IsPrime()))
				{
					squadRequirements.Add(new SquadRequirement
					{
						name = item.name,
						imageURL = Misc.GetFullImagePath(item.imageName),
						internalName = item.uniqueName
					});
				}
			}
		}

		private static void AddWarframes(List<SquadRequirement> squadRequirements, ItemType type)
		{
			foreach (DataWarframe value in StaticData.dataHandler.warframes.Values)
			{
				if (type == ItemType.Both || (type == ItemType.Prime && value.IsPrime()) || (type == ItemType.NonPrime && !value.IsPrime()))
				{
					squadRequirements.Add(new SquadRequirement
					{
						name = value.name,
						imageURL = Misc.GetFullImagePath(value.imageName),
						internalName = value.uniqueName
					});
				}
			}
		}
	}
}
