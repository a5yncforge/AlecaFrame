using System.Collections.Generic;
using System.Text;
using AF_DamageCalculatorLib.SimulationObjects;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib
{
	public class SimulationResults
	{
		public enum SimulationResultState
		{
			Ongoing,
			Finished,
			Timeout,
			Cancelled
		}

		public SimulationResultState state;

		public double elapsedRealTime;

		public double elapsedSimulatedTime;

		public double averageTTK;

		public double minTTK;

		public double maxTTK;

		public double setupTTK;

		public int damageDistributionDirect;

		public double damageDistributionPercentDirect;

		public int damageDistributionAOE;

		public double damageDistributionPercentAOE;

		public int damageDistributionStatus;

		public double damageDistributionPercentStatus;

		public Dictionary<DamageType, EnemyInstance.AttackMitigationLoggingData.DataPoint> damageByType = new Dictionary<DamageType, EnemyInstance.AttackMitigationLoggingData.DataPoint>();

		public Dictionary<ProcType, long> statusEffectsAdded = new Dictionary<ProcType, long>();

		public double averageTTKTicks;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Simulation results:");
			stringBuilder.AppendLine("State: " + state);
			stringBuilder.AppendLine("Elapsed real time: " + elapsedRealTime);
			stringBuilder.AppendLine("Elapsed simulated time: " + elapsedSimulatedTime);
			if (state == SimulationResultState.Finished)
			{
				stringBuilder.AppendLine("Average TTK: " + averageTTK.ToString("0.00"));
				stringBuilder.AppendLine("Min TTK: " + minTTK.ToString("0.00"));
				stringBuilder.AppendLine("Max TTK: " + maxTTK.ToString("0.00"));
				stringBuilder.AppendLine("Setup TTK: " + setupTTK.ToString("0.00"));
				stringBuilder.AppendLine("Damage distribution:");
				stringBuilder.AppendLine("\tDirect: " + damageDistributionDirect + " (" + (damageDistributionPercentDirect * 100.0).ToString("0.0") + "%)");
				stringBuilder.AppendLine("\tAOE: " + damageDistributionAOE + " (" + (damageDistributionPercentAOE * 100.0).ToString("0.0") + "%)");
				stringBuilder.AppendLine("\tStatus: " + damageDistributionStatus + " (" + (damageDistributionPercentStatus * 100.0).ToString("0.0") + "%)");
				stringBuilder.AppendLine("Damage by type:");
				foreach (KeyValuePair<DamageType, EnemyInstance.AttackMitigationLoggingData.DataPoint> item in damageByType)
				{
					stringBuilder.AppendLine("\t" + item.Key.ToString() + ": " + item.Value.preMitigationDamage.ToString("0.00") + " -> " + item.Value.postMitigationDamage.ToString("0.00") + " (" + item.Value.mitigationPercentage.ToString("0.0") + "%)");
				}
				stringBuilder.AppendLine("Status effects added:");
				foreach (KeyValuePair<ProcType, long> item2 in statusEffectsAdded)
				{
					stringBuilder.AppendLine("\t" + item2.Key.ToString() + ": " + item2.Value);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
