using System;
using NCalc;
using UnityEngine;

public static class NCalcExtensions
{
	public static void ExtraNCalcFunctions(string name, FunctionArgs functionArgs)
	{
		if (name == "Clamp")
		{
			EnsureNumOfParams(3);
			object value = functionArgs.Parameters[0].Evaluate();
			object min = functionArgs.Parameters[1].Evaluate();
			object max = functionArgs.Parameters[2].Evaluate();
			functionArgs.Result = Mathf.Clamp(ToFloat(value), ToFloat(min), ToFloat(max));
		}
		else if (name == "Clamp01")
		{
			EnsureNumOfParams(1);
			object value2 = functionArgs.Parameters[0].Evaluate();
			functionArgs.Result = Mathf.Clamp01(ToFloat(value2));
		}
		void EnsureNumOfParams(int count)
		{
			if (functionArgs.Parameters.Length != count)
			{
				throw new Exception($"Expected {count} parameters, got {functionArgs.Parameters.Length}");
			}
		}
	}

	private static float ToFloat(object val)
	{
		if (val is float)
		{
			return (float)val;
		}
		if (val is double d)
		{
			return (float)d;
		}
		if (val is int i)
		{
			return i;
		}
		throw new ArgumentException("val");
	}
}
