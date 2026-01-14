using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AF_DamageCalculatorLib.Classes;
using AF_DamageCalculatorLib.SimulationObjects;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib
{
	public class DamageCalculatorInstance
	{
		public class TargetInfo
		{
			public EnemyInstance enemy;

			public bool isHeadshot;
		}

		private BuildSourceDataFile sourceData;

		private EnemySetup enemySetup;

		private Random random = new Random(6969);

		internal WarframeInstance warframeInstance;

		internal WeaponInstance weaponInstance;

		private List<EnemyInstance> enemies = new List<EnemyInstance>();

		private bool internalSimulationCancelRequested;

		public SimulationResults ongoingSimulationResults;

		internal long currentTick;

		public event Action<DamageCalculatorEvent, DamageCalculatorEventData> OnEvent;

		public DamageCalculatorInstance(BuildSourceDataFile sourceData)
		{
			this.sourceData = sourceData;
			OnEvent += InternalSimulatorEventHandler;
		}

		public void SetWeaponBuild(WeaponBuild weaponBuild)
		{
			internalSimulationCancelRequested = true;
			lock (this)
			{
				weaponInstance = new WeaponInstance(sourceData, weaponBuild, this);
			}
		}

		public void SetWarframeBuild(WarframeBuild warframeBuild)
		{
			internalSimulationCancelRequested = true;
			lock (this)
			{
				warframeInstance = new WarframeInstance(sourceData, warframeBuild, this);
			}
		}

		public void SetEnemySetup(EnemySetup enemySetup)
		{
			internalSimulationCancelRequested = true;
			lock (this)
			{
				this.enemySetup = enemySetup;
				enemies.Clear();
				foreach (EnemySetup.EnemyEntry enemyEntry in enemySetup.enemyEntries)
				{
					for (int i = 0; i < enemyEntry.amount; i++)
					{
						enemies.Add(new EnemyInstance(sourceData, this, enemyEntry.info));
					}
				}
			}
		}

		public void InitializeSimulation()
		{
			if (sourceData == null)
			{
				throw new Exception("Source data not set");
			}
			if (enemySetup == null)
			{
				throw new Exception("Enemy setup not set");
			}
			if (warframeInstance == null)
			{
				throw new Exception("Warframe build not set");
			}
			if (weaponInstance == null)
			{
				throw new Exception("Weapon build not set");
			}
			currentTick = 0L;
			weaponInstance.Reset();
			warframeInstance.Reset();
			foreach (EnemyInstance enemy in enemies)
			{
				enemy.Reset();
			}
			ongoingSimulationResults = new SimulationResults();
		}

		internal void Tick(int deltaTimeMS)
		{
			warframeInstance.Tick(deltaTimeMS);
			weaponInstance.Tick(deltaTimeMS);
			foreach (EnemyInstance enemy in enemies)
			{
				enemy.Tick(deltaTimeMS);
			}
			RaiseEvent(DamageCalculatorEvent.TickComplete, null);
			currentTick++;
		}

		public SimulationResults DoCompleteSimulation(CancellationToken cancellationToken, TimeSpan timeRealLimit, TimeSpan simulationTickDeltaTime)
		{
			lock (this)
			{
				internalSimulationCancelRequested = false;
				Stopwatch stopwatch = Stopwatch.StartNew();
				InitializeSimulation();
				if (cancellationToken.IsCancellationRequested || internalSimulationCancelRequested)
				{
					ongoingSimulationResults.state = SimulationResults.SimulationResultState.Cancelled;
					return ongoingSimulationResults;
				}
				while (true)
				{
					Tick((int)simulationTickDeltaTime.TotalMilliseconds);
					if (enemies.All((EnemyInstance x) => x.IsDead()))
					{
						ongoingSimulationResults.state = SimulationResults.SimulationResultState.Finished;
						break;
					}
					if (cancellationToken.IsCancellationRequested || internalSimulationCancelRequested)
					{
						ongoingSimulationResults.state = SimulationResults.SimulationResultState.Cancelled;
						break;
					}
					if (stopwatch.Elapsed > timeRealLimit)
					{
						ongoingSimulationResults.state = SimulationResults.SimulationResultState.Timeout;
						break;
					}
				}
				ongoingSimulationResults.elapsedRealTime = stopwatch.Elapsed.TotalSeconds;
				ongoingSimulationResults.elapsedSimulatedTime = (double)currentTick * simulationTickDeltaTime.TotalSeconds;
				if (ongoingSimulationResults.state == SimulationResults.SimulationResultState.Finished)
				{
					List<long> list = (from x in enemies
						where x.IsDead()
						select x.enemyDeathTick - x.enemyFirstDamageTick).ToList();
					if (list.Count > 0)
					{
						ongoingSimulationResults.averageTTK = Math.Round(list.Average() * simulationTickDeltaTime.TotalSeconds, 2);
						ongoingSimulationResults.minTTK = Math.Round((double)list.Min() * simulationTickDeltaTime.TotalSeconds, 2);
						ongoingSimulationResults.maxTTK = Math.Round((double)list.Max() * simulationTickDeltaTime.TotalSeconds, 2);
						ongoingSimulationResults.averageTTKTicks = list.Average();
						ongoingSimulationResults.setupTTK = Math.Round(ongoingSimulationResults.elapsedSimulatedTime, 2);
					}
					double num = ongoingSimulationResults.damageDistributionDirect + ongoingSimulationResults.damageDistributionAOE + ongoingSimulationResults.damageDistributionStatus;
					if (num > 0.0)
					{
						ongoingSimulationResults.damageDistributionPercentDirect = Math.Round((double)(100 * ongoingSimulationResults.damageDistributionDirect) / num, 0);
						ongoingSimulationResults.damageDistributionPercentAOE = Math.Round((double)(100 * ongoingSimulationResults.damageDistributionAOE) / num, 0);
						ongoingSimulationResults.damageDistributionPercentStatus = Math.Round((double)(100 * ongoingSimulationResults.damageDistributionStatus) / num, 0);
					}
				}
				internalSimulationCancelRequested = false;
				return ongoingSimulationResults;
			}
		}

		internal void RaiseEvent(DamageCalculatorEvent eventType, DamageCalculatorEventData eventData)
		{
			this.OnEvent?.Invoke(eventType, eventData);
		}

		internal TargetInfo AcquireNextTargetInfo()
		{
			for (int i = 0; i < enemies.Count; i++)
			{
				if (!enemies[i].IsDead())
				{
					return new TargetInfo
					{
						enemy = enemies[i],
						isHeadshot = false
					};
				}
			}
			return null;
		}

		internal double GetRandomDouble()
		{
			return random.NextDouble();
		}

		internal void ApplyAOEDamage(DamageType electricity, double damageAmount, double radius, DamageSource damageSource)
		{
			foreach (EnemyInstance enemy in enemies)
			{
				if (!enemy.IsDead())
				{
					enemy.TakeDamage(electricity, damageAmount, damageSource);
				}
			}
		}

		internal void SendInternalEventToAllUpgrades(BuildUpgradeData.BuildModBuff.ModBufConditions eventType)
		{
			warframeInstance?.SendInternalEventToAllUpgrades(eventType);
			weaponInstance?.SendInternalEventToAllUpgrades(eventType);
		}

		private void InternalSimulatorEventHandler(DamageCalculatorEvent @event, DamageCalculatorEventData data)
		{
			if (@event != DamageCalculatorEvent.EnemyHit)
			{
				return;
			}
			DamageCalculatorEnemyHitEventData damageCalculatorEnemyHitEventData = (DamageCalculatorEnemyHitEventData)data;
			switch (damageCalculatorEnemyHitEventData.damageSource)
			{
			case DamageSource.Weapon:
				ongoingSimulationResults.damageDistributionDirect += damageCalculatorEnemyHitEventData.healthDamage + damageCalculatorEnemyHitEventData.shieldDamage;
				break;
			case DamageSource.WeaponAOE:
				ongoingSimulationResults.damageDistributionAOE += damageCalculatorEnemyHitEventData.healthDamage + damageCalculatorEnemyHitEventData.shieldDamage;
				break;
			case DamageSource.StatusEffect:
				ongoingSimulationResults.damageDistributionStatus += damageCalculatorEnemyHitEventData.healthDamage + damageCalculatorEnemyHitEventData.shieldDamage;
				break;
			}
			foreach (KeyValuePair<DamageType, EnemyInstance.AttackMitigationLoggingData.DataPoint> item in damageCalculatorEnemyHitEventData.mitigationLoggingData.byDamageType)
			{
				if (!ongoingSimulationResults.damageByType.ContainsKey(item.Key))
				{
					ongoingSimulationResults.damageByType.Add(item.Key, new EnemyInstance.AttackMitigationLoggingData.DataPoint());
				}
				ongoingSimulationResults.damageByType[item.Key].postMitigationDamage += item.Value.postMitigationDamage;
				ongoingSimulationResults.damageByType[item.Key].preMitigationDamage += item.Value.preMitigationDamage;
			}
			foreach (ProcType item2 in damageCalculatorEnemyHitEventData.statusEffectsAdded)
			{
				if (!ongoingSimulationResults.statusEffectsAdded.ContainsKey(item2))
				{
					ongoingSimulationResults.statusEffectsAdded.Add(item2, 0L);
				}
				ongoingSimulationResults.statusEffectsAdded[item2]++;
			}
		}

		public List<DamageCalculatorStatOutput> GetWarframeStats()
		{
			lock (this)
			{
				return warframeInstance.GetStats();
			}
		}

		public List<DamageCalculatorStatOutput> GetWeaponStats(WeaponInstance.StatRequestType request)
		{
			lock (this)
			{
				return weaponInstance.GetStats(request);
			}
		}

		public bool WeaponHasAOE()
		{
			lock (this)
			{
				return weaponInstance.HasAOE();
			}
		}

		public void WeaponChangeSelectedMode(int weaponModeID)
		{
			lock (this)
			{
				weaponInstance.ChangeSelectedMode(weaponModeID);
			}
		}

		public void WeaponChangeSelectedZoomLevel(int weaponZoomLevelID)
		{
			lock (this)
			{
				weaponInstance.ChangeSelectedZoomLevel(weaponZoomLevelID);
			}
		}
	}
}
