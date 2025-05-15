using System;
using NCalc;
using UnityEngine.Serialization;

[Serializable]
public struct Formula
{
	[FormerlySerializedAs("forumla")]
	public string formula;

	private Expression _exp;

	private string _cachedFormula;

	private float _lastInput;

	private float _lastAnswer;

	public static implicit operator Formula(string str)
	{
		return new Formula
		{
			formula = str
		};
	}

	public float Evaluate(float x)
	{
		if (_exp == null || formula != _cachedFormula)
		{
			_exp = new Expression(formula);
			_exp.EvaluateFunction += NCalcExtensions.ExtraNCalcFunctions;
			_cachedFormula = formula;
			_lastInput = float.NaN;
			_lastAnswer = float.NaN;
		}
		if (_lastInput == x)
		{
			return _lastAnswer;
		}
		_exp.Parameters["x"] = x;
		_lastInput = x;
		object res = _exp.Evaluate();
		if (res is float f)
		{
			_lastAnswer = f;
		}
		else if (res is double d)
		{
			_lastAnswer = (float)d;
		}
		else
		{
			if (!(res is int i))
			{
				throw new ArgumentOutOfRangeException(res.GetType().Name);
			}
			_lastAnswer = i;
		}
		return _lastAnswer;
	}

	private bool IsValid(string f, ref string errorMessage)
	{
		try
		{
			Expression expression = new Expression(f, EvaluateOptions.NoCache);
			expression.EvaluateFunction += NCalcExtensions.ExtraNCalcFunctions;
			expression.Parameters["x"] = 0f;
			expression.Evaluate();
			return true;
		}
		catch (Exception ex)
		{
			errorMessage = ex.Message;
			return false;
		}
	}
}
