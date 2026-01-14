using System;
using System.Collections.Generic;
using System.Linq;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFrameClientLib.Utils
{
	public static class TranslationHelper
	{
		public static Dictionary<string, Misc.WarframeLanguage> languagesAvailable = new Dictionary<string, Misc.WarframeLanguage>
		{
			{
				"fr",
				Misc.WarframeLanguage.French
			},
			{
				"en",
				Misc.WarframeLanguage.English
			},
			{
				"es",
				Misc.WarframeLanguage.Spanish
			},
			{
				"de",
				Misc.WarframeLanguage.German
			},
			{
				"ru",
				Misc.WarframeLanguage.Russian
			}
		};

		private static readonly List<string> russiansBladeWithOvrok = new List<string>
		{
			"Dakra", "Destreza", "Fang", "Galatine", "Gram", "Guandao", "Kronen", "Nami Skyla", "Nikana", "Orthos",
			"Scindo", "Silva & Aegis"
		};

		public static string GetFrenchName(string enlgishName, ItemComponent part)
		{
			string name = part.name;
			switch (name)
			{
			case "Blueprint":
				return part.isPartOf.name + " (Schéma)";
			case "Neuroptics":
				return part.isPartOf.name + " Neuroptiques (Schéma)";
			case "Systems":
				return part.isPartOf.name + " Systèmes" + ((part.isPartOf is DataSentinel) ? "" : " (Schéma)");
			case "Chassis":
				return part.isPartOf.name + " Châssis (Schéma)";
			case "Barrel":
				return part.isPartOf.name + " Canon";
			case "Receiver":
				return part.isPartOf.name + " Culasse";
			case "Stock":
				return part.isPartOf.name + " Crosse";
			case "Link":
				return part.isPartOf.name + " Lien";
			case "Lower Limb":
				return part.isPartOf.name + " Partie Inférieure";
			case "Upper Limb":
				return part.isPartOf.name + " Partie Supérieure";
			case "Blade":
				return part.isPartOf.name + (part.isPartOf.name.Contains("Dual") ? "Lames" : " Lame");
			case "Pouch":
				return part.isPartOf.name + " Pochette";
			case "Gauntlet":
				return part.isPartOf.name + " Gantelet";
			case "Boot":
				return part.isPartOf.name + " Botte";
			case "Ornament":
				return part.isPartOf.name + " Ornement";
			case "Cerebrum":
				return part.isPartOf.name + " Cerveau";
			case "Carapace":
				return part.isPartOf.name + " Carapace";
			case "Chain":
				return part.isPartOf.name + " Chaîne";
			case "Grip":
				if (part.isPartOf.name.Contains("Zhuge"))
				{
					return part.isPartOf.name + " Crosse";
				}
				return part.isPartOf.name + " Prise";
			case "String":
				return part.isPartOf.name + " Corde";
			case "Head":
				return part.isPartOf.name + " Tête";
			case "Disc":
				return part.isPartOf.name + " Disque";
			case "Stars":
				return part.isPartOf.name + " Étoiles";
			case "Blades":
				return part.isPartOf.name + " Lames";
			case "Guard":
				return part.isPartOf.name + " Garde";
			case "Handle":
				return GetHandleNameInFrench(part.isPartOf);
			case "Hilt":
				if (part.isPartOf.name.Contains("Nikana"))
				{
					return part.isPartOf.name + " Garde";
				}
				return part.isPartOf.name + " Quillon";
			case "Kavasa Prime Band":
				return "Kavasa Prime Lanièreç";
			case "Kavasa Prime Buckle":
				return "Kavasa Prime boucle";
			case "Wings":
				return part.isPartOf.name + " Ailes";
			case "Harness":
				return part.isPartOf.name + " Harnais";
			case "Forma":
				return "Forma (Schéma)";
			default:
				Console.WriteLine("Unknown item type when translating into French: " + name + " (" + part.GetRealExternalName() + ")");
				return enlgishName;
			}
		}

		public static string GetGermanName(string enlgishName, ItemComponent part)
		{
			string name = part.name;
			switch (name)
			{
			case "Blueprint":
				return part.isPartOf.name + ": Blaupause";
			case "Neuroptics":
				return part.isPartOf.name + ": Neuroptik Blaupause";
			case "Systems":
				return part.isPartOf.name + ": Systeme " + ((part.isPartOf is DataSentinel) ? "" : " Blaupause");
			case "Chassis":
				return part.isPartOf.name + ": Chassis Blaupause";
			case "Barrel":
				return part.isPartOf.name + ": Lauf";
			case "Receiver":
				return part.isPartOf.name + ": Gehäuse";
			case "Stock":
				return part.isPartOf.name + ": Schaft";
			case "Link":
				return part.isPartOf.name + ": Verbindung";
			case "Lower Limb":
				return part.isPartOf.name + ": Unterteil";
			case "Upper Limb":
				return part.isPartOf.name + ": Oberteil";
			case "Blade":
				return part.isPartOf.name + ": Klinge";
			case "Pouch":
				return part.isPartOf.name + ": Tasche";
			case "Gauntlet":
				return part.isPartOf.name + ": Handschuh";
			case "Boot":
				return part.isPartOf.name + ": Stiefel";
			case "Ornament":
				return part.isPartOf.name + ": Ornament";
			case "Cerebrum":
				return part.isPartOf.name + ": Cerebrum";
			case "Carapace":
				return part.isPartOf.name + ": Panzer";
			case "Chain":
				return part.isPartOf.name + ": Kette";
			case "Grip":
				return part.isPartOf.name + ": Griff";
			case "String":
				return part.isPartOf.name + ": Sehne";
			case "Head":
				return part.isPartOf.name + ": Kopf";
			case "Disc":
				return part.isPartOf.name + ": Scheibe";
			case "Stars":
				return part.isPartOf.name + ": Sterne";
			case "Blades":
				return part.isPartOf.name + ": Klingen";
			case "Guard":
				return part.isPartOf.name + ": Schutz";
			case "Handle":
				return part.isPartOf.name + ": Griff";
			case "Hilt":
				return part.isPartOf.name + ": Griff";
			case "Kavasa Prime Band":
				return "Kavasa Prime Band";
			case "Kavasa Prime Buckle":
				return "Kavasa Prime Schnalle";
			case "Wings":
				return part.isPartOf.name + ": Flügel";
			case "Harness":
				return part.isPartOf.name + ": Harnisch";
			case "Forma":
				return "Forma Blaupause";
			default:
				Console.WriteLine("Unknown item type when translating into English: " + name + " (" + part.GetRealExternalName() + ")");
				return enlgishName;
			}
		}

		public static string GetRussianName(string enlgishName, ItemComponent part, Dictionary<string, Dictionary<string, DataTranslation>> translations)
		{
			string name = part.isPartOf.name;
			if (translations != null && translations.ContainsKey(part.isPartOf.uniqueName))
			{
				Dictionary<string, DataTranslation> dictionary = translations[part.isPartOf.uniqueName];
				if (dictionary.ContainsKey("ru"))
				{
					name = dictionary["ru"].name;
				}
				else
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to load russian translation (1) for " + part.uniqueName);
				}
			}
			else
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to load russian translation (2) for " + part.uniqueName);
			}
			string name2 = part.name;
			switch (name2)
			{
			case "Blueprint":
				return "Чертеж: " + name;
			case "Neuroptics":
				return "Чертеж: " + name + ": Нейрооптика";
			case "Systems":
				return ((part.isPartOf is DataSentinel) ? "" : "Чертеж: ") + name + ": Система";
			case "Chassis":
				return "Чертеж: " + name + ": Каркас";
			case "Barrel":
				return name + ": Ствол";
			case "Receiver":
				return name + ": Приёмник";
			case "Stock":
				return name + ": Приклад";
			case "Link":
				return name + ": Связь";
			case "Lower Limb":
				return name + ": Нижнее Плечо";
			case "Upper Limb":
				return name + ": Верхнее Плечо";
			case "Blade":
				if (russiansBladeWithOvrok.Contains(part.isPartOf.name.Replace(" Prime", "")))
				{
					return name + ": Клинок";
				}
				return name + (name.Contains("Dual") ? ": Лезвия" : ": Лезвие");
			case "Pouch":
				return name + ": Кисет";
			case "Gauntlet":
				return name + ": Перчатка";
			case "Boot":
				return name + ": Ступня";
			case "Ornament":
				return "Орнамент: " + name;
			case "Cerebrum":
				return name + ": Мозг";
			case "Carapace":
				return name + ": Панцирь";
			case "Chain":
				return name + ": Цепь";
			case "Grip":
				return name + ": Рукоять";
			case "String":
				return name + ": Тетива";
			case "Head":
				return name + ": Молот";
			case "Disc":
				return name + ": Диск";
			case "Stars":
				return name + ": Сюрикены";
			case "Blades":
				return name + ": Лезвия";
			case "Guard":
				return name + ": Щит";
			case "Handle":
				return name + ": Рукоять";
			case "Hilt":
				return name + ": Рукоять";
			case "Kavasa Prime Band":
				return "КавасаПрайм: Лента";
			case "Kavasa Prime Buckle":
				return "КавасаПрайм: Застежка";
			case "Wings":
				return "Чертеж: " + name + ": Крылья";
			case "Harness":
				return "Чертеж: " + name + ": Упряжь";
			case "Forma":
				return "Чертеж: форма";
			default:
				Console.WriteLine("Unknown item type when translating into Russian: " + name2 + " (" + part.GetRealExternalName() + ")");
				return enlgishName;
			}
		}

		public static string GetESPANAName(string enlgishName, ItemComponent part)
		{
			string name = part.name;
			switch (name)
			{
			case "Blueprint":
				return "Plano De " + part.isPartOf.name;
			case "Neuroptics":
				return "Plano De Neurópticas De " + part.isPartOf.name;
			case "Systems":
				return ((part.isPartOf is DataSentinel) ? "" : "Plano De ") + "Sistemas De " + part.isPartOf.name;
			case "Chassis":
				return "Plano De Chasis De " + part.isPartOf.name;
			case "Barrel":
				return "Cañón De " + part.isPartOf.name;
			case "Receiver":
				return "Receptor De " + part.isPartOf.name;
			case "Stock":
				return "Culata De " + part.isPartOf.name;
			case "Link":
				return "Enlace De " + part.isPartOf.name;
			case "Lower Limb":
				return "Pala Inferior De " + part.isPartOf.name;
			case "Upper Limb":
				return "Pala Superior De " + part.isPartOf.name;
			case "Blade":
				return "Hoja De " + part.isPartOf.name;
			case "Pouch":
				return "Cartuchera De " + part.isPartOf.name;
			case "Gauntlet":
				if (part.isPartOf.name.Contains("Kogake Prime"))
				{
					return "Guantelete De " + part.isPartOf.name;
				}
				return "Guante De " + part.isPartOf.name;
			case "Boot":
				return "Bota De " + part.isPartOf.name;
			case "Ornament":
				if (part.isPartOf.name.Contains("Tipedo Prime"))
				{
					return "Ornamento De " + part.isPartOf.name;
				}
				return "Adorno De " + part.isPartOf.name;
			case "Cerebrum":
				return "Cerebro De " + part.isPartOf.name;
			case "Carapace":
				return "Caparazón De " + part.isPartOf.name;
			case "Chain":
				return "Cadena De " + part.isPartOf.name;
			case "Grip":
				return "Empuñadura De " + part.isPartOf.name;
			case "String":
				return "Cuerda De " + part.isPartOf.name;
			case "Head":
				return "Cabeza De " + part.isPartOf.name;
			case "Disc":
				return "Disco De " + part.isPartOf.name;
			case "Stars":
				return "Estrellas De " + part.isPartOf.name;
			case "Blades":
				return "Hojas De " + part.isPartOf.name;
			case "Guard":
				if (part.isPartOf.name.Contains("Silva & Aegis"))
				{
					return "Escudo De " + part.isPartOf.name;
				}
				return "Protector De " + part.isPartOf.name;
			case "Handle":
				return "Empuñadura De " + part.isPartOf.name;
			case "Hilt":
				return "Empuñadura De " + part.isPartOf.name;
			case "Kavasa Prime Band":
				return "Cinta Del Collar Kavasa Prime";
			case "Kavasa Prime Buckle":
				return "Hebilla Del Collar Kavasa Prime";
			case "Wings":
				return "Alas De " + part.isPartOf.name;
			case "Harness":
				return "Arnés De " + part.isPartOf.name;
			case "Forma":
				return "Plano De Forma";
			default:
				Console.WriteLine("Unknown item type when translating into ESPANAAA: " + name + " (" + part.GetRealExternalName() + ")");
				return enlgishName;
			}
		}

		private static string GetHandleNameInFrench(BigItem isPartOf)
		{
			switch (isPartOf.name)
			{
			case "Guandao Prime":
			case "Scourge Prime":
			case "Fragor Prime":
			case "Kronen Prime":
			case "Orthos Prime":
			case "Reaper Prime":
			case "Scindo Prime":
			case "Tipedo Prime":
			case "Volnus Prime":
			case "Ninkondi Prime":
			case "Bo Prime":
				return isPartOf.name + " Manche";
			case "Redeemer Prime":
				return isPartOf.name + " Poignée";
			case "Dual Kamas Prime":
				return "Dual Kamas Prime Manches";
			case "Dual Keres Prime":
				return "Dual Keres Prime Manches";
			default:
				return isPartOf.name + " Garde";
			}
		}

		public static void CheckTranslations()
		{
			throw new Exception("Disabled for now, v2 wfm api not supported!");
		}

		public static string GetLanguageCodeFromLanguage(Misc.WarframeLanguage warframeLanguage)
		{
			IEnumerable<KeyValuePair<string, Misc.WarframeLanguage>> source = languagesAvailable.Where((KeyValuePair<string, Misc.WarframeLanguage> p) => p.Value == warframeLanguage);
			if (source.Count() > 0)
			{
				return source.First().Key;
			}
			throw new Exception("Unknown language (GetLanguageCodeFromLanguage)!");
		}

		public static Misc.WarframeLanguage GetLanguageFromLanguageCode(string languageCode)
		{
			if (languagesAvailable.ContainsKey(languageCode))
			{
				return languagesAvailable[languageCode];
			}
			return Misc.WarframeLanguage.UNKNOWN;
		}
	}
}
