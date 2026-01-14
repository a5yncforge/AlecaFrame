using System;
using System.Collections.Generic;
using AF_DamageCalculatorLib.Classes;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public class StatWorkingData
	{
		private double baseValue;

		private double nonFactoredBaseValue = 1.0;

		private double postBaseQuantizationCoefficient = -1.0;

		private double extraBaseValue;

		private List<double> baseAdditions = new List<double>();

		private List<double> multiplier = new List<double>();

		private List<double> stackingMultiply = new List<double>();

		private List<double> additive = new List<double>();

		private List<double> additiveAsPercentageOfBase = new List<double>();

		public bool modIndexSet;

		public int firstModIndexThatAffectsThisStat = 88888;

		public double finalValue;

		public double finalPreAdditionsValue;

		public void ClearLists()
		{
			multiplier.Clear();
			stackingMultiply.Clear();
			additive.Clear();
			additiveAsPercentageOfBase.Clear();
			baseAdditions.Clear();
			extraBaseValue = 0.0;
			firstModIndexThatAffectsThisStat = 88888;
			modIndexSet = false;
		}

		public void SetBaseValue(double value, double nonFactoredBaseValue = -1.0, double postBaseQuantizationCoefficient = -1.0)
		{
			baseValue = value;
			if (nonFactoredBaseValue == -1.0)
			{
				nonFactoredBaseValue = value;
			}
			this.nonFactoredBaseValue = nonFactoredBaseValue;
			this.postBaseQuantizationCoefficient = postBaseQuantizationCoefficient;
		}

		public void SetExtraBaseValue(double value)
		{
			extraBaseValue = value;
		}

		public void UpdateFinalValue()
		{
			double num = nonFactoredBaseValue;
			finalValue = baseValue;
			for (int i = 0; i < baseAdditions.Count; i++)
			{
				num += baseAdditions[i];
				finalValue += baseAdditions[i];
			}
			if (postBaseQuantizationCoefficient != -1.0)
			{
				num = Math.Round(num / postBaseQuantizationCoefficient) * postBaseQuantizationCoefficient;
				finalValue = Math.Round(finalValue / postBaseQuantizationCoefficient) * postBaseQuantizationCoefficient;
			}
			double num2 = 1.0;
			for (int j = 0; j < stackingMultiply.Count; j++)
			{
				num2 += stackingMultiply[j];
			}
			num *= num2;
			finalValue *= num2;
			finalPreAdditionsValue = finalValue;
			for (int k = 0; k < additiveAsPercentageOfBase.Count; k++)
			{
				finalValue += additiveAsPercentageOfBase[k] * num;
			}
			for (int l = 0; l < additive.Count; l++)
			{
				finalValue += additive[l];
			}
			finalValue += extraBaseValue;
			for (int m = 0; m < multiplier.Count; m++)
			{
				finalValue *= multiplier[m];
			}
		}

		public void AddBuff(BuildUpgradeData.BuildModBuff.ModBuffOperation operation, double value, bool isBuffAPercentageOfTheBase, int modIndex = -1)
		{
			if (modIndex != -1 && (modIndex < firstModIndexThatAffectsThisStat || !modIndexSet))
			{
				firstModIndexThatAffectsThisStat = modIndex;
				modIndexSet = true;
			}
			switch (operation)
			{
			case BuildUpgradeData.BuildModBuff.ModBuffOperation.Add:
				if (isBuffAPercentageOfTheBase)
				{
					additiveAsPercentageOfBase.Add(value);
				}
				else
				{
					additive.Add(value);
				}
				break;
			case BuildUpgradeData.BuildModBuff.ModBuffOperation.Multiply:
				multiplier.Add(value);
				break;
			case BuildUpgradeData.BuildModBuff.ModBuffOperation.StackingMultiply:
				stackingMultiply.Add(value);
				break;
			case BuildUpgradeData.BuildModBuff.ModBuffOperation.AddBase:
				baseAdditions.Add(value);
				break;
			default:
				throw new Exception("Invalid operation");
			}
		}
	}
}
