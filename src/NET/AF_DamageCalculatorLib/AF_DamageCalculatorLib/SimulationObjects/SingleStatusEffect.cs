using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class SingleStatusEffect
	{
		private EnemyInstance enemyInstance;

		public ProcType procType;

		public double typeDamage;

		public double baseTotalDamage;

		private int remainingMS;

		private int originalDurationMS;

		private int timeUntilNextActionMS;

		private int damageInstancesDone;

		public SingleStatusEffect(EnemyInstance enemyInstance, ProcType procType, int remainingMS, double typeDamage, double totalBaseDamage)
		{
			this.enemyInstance = enemyInstance;
			this.procType = procType;
			this.typeDamage = typeDamage;
			baseTotalDamage = totalBaseDamage;
			this.remainingMS = remainingMS;
			originalDurationMS = remainingMS;
			if (procType == ProcType.Poison || (uint)(procType - 11) <= 1u)
			{
				timeUntilNextActionMS = 1000;
			}
			else
			{
				timeUntilNextActionMS = -1;
			}
		}

		public void Tick(int deltaMS)
		{
			if (timeUntilNextActionMS != -1)
			{
				if (timeUntilNextActionMS == 0)
				{
					if (ApplyStatusAction())
					{
						timeUntilNextActionMS = 1000;
					}
					else
					{
						timeUntilNextActionMS = -1;
					}
				}
				if (timeUntilNextActionMS != -1)
				{
					timeUntilNextActionMS -= deltaMS;
					if (timeUntilNextActionMS < 0)
					{
						timeUntilNextActionMS = 0;
					}
				}
			}
			if (remainingMS == -1)
			{
				return;
			}
			remainingMS -= deltaMS;
			if (remainingMS < 0)
			{
				remainingMS = 0;
				if (timeUntilNextActionMS == 0)
				{
					ApplyStatusAction();
				}
			}
		}

		private bool ApplyStatusAction()
		{
			damageInstancesDone++;
			switch (procType)
			{
			case ProcType.Slash:
				enemyInstance.TakeDamage(DamageType.Cinematic, 0.35 * baseTotalDamage, DamageSource.StatusEffect);
				return damageInstancesDone < 6;
			case ProcType.Heat:
				enemyInstance.TakeDamage(DamageType.Heat, 0.5 * typeDamage, DamageSource.StatusEffect);
				return damageInstancesDone < 6;
			case ProcType.Poison:
				enemyInstance.TakeDamage(DamageType.Toxin, 0.5 * typeDamage, DamageSource.StatusEffect);
				return damageInstancesDone < 6;
			default:
				return false;
			}
		}

		public bool ShouldGetRemoved()
		{
			return remainingMS == 0;
		}

		public void RefreshDuration()
		{
			remainingMS = originalDurationMS;
		}
	}
}
