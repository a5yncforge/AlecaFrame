using System.Collections.Generic;
using AF_DamageCalculatorLib.SimulationObjects;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib
{
	public class DamageCalculatorEnemyHitEventData : DamageCalculatorEventData
	{
		public int shieldDamage;

		public int healthDamage;

		public int critTier;

		public List<ProcType> statusEffectsAdded;

		public DamageSource damageSource;

		public EnemyInstance.AttackMitigationLoggingData mitigationLoggingData;

		public override string ToString()
		{
			string text = $"HIT from {damageSource}: ShieldDamage: {shieldDamage} HealthDamage: {healthDamage} CritTier: {critTier}\n";
			if (statusEffectsAdded != null && statusEffectsAdded.Count > 0)
			{
				text += "\tAdded StatusEffects: ";
				foreach (ProcType item in statusEffectsAdded)
				{
					text = text + item.ToString() + " ";
				}
				text += "\n";
			}
			return text + "\tMitigation: \n" + mitigationLoggingData.ToString();
		}
	}
}
