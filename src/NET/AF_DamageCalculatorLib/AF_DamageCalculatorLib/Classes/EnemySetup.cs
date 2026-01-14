using System.Collections.Generic;

namespace AF_DamageCalculatorLib.Classes
{
	public class EnemySetup
	{
		public class Metadata
		{
			public string name;

			public string description;

			public string author;

			public Metadata(string name, string author)
			{
				this.name = name;
				description = "";
				this.author = author;
			}

			public Metadata(string name)
			{
				this.name = name;
				description = "";
				author = "";
			}

			public Metadata(string name, string description, string author)
			{
				this.name = name;
				this.description = description;
				this.author = author;
			}
		}

		public class EnemyEntry
		{
			public EnemyInfo info;

			public int amount;

			public EnemyEntry(EnemyInfo enemyInfo, int amount)
			{
				info = enemyInfo;
				this.amount = amount;
			}
		}

		public class EnemyInfo
		{
			public string uniqueName;

			public int level;

			public bool steelPath;

			public EnemyInfo(string uniqueName, int level, bool steelPath = false)
			{
				this.uniqueName = uniqueName;
				this.level = level;
				this.steelPath = steelPath;
			}
		}

		public Metadata metadata;

		public List<EnemyEntry> enemyEntries = new List<EnemyEntry>();
	}
}
