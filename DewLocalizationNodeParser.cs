using System;
using UnityEngine;

namespace DewInternal;

public static class DewLocalizationNodeParser
{
	private static char[] ArithmeticExpressionChars = "+-*/()".ToCharArray();

	private static char[] FieldStartChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();

	public static void ParseBacktickedString(string inputText, Action<string> onNormalText, Action<string> onTag, Action<string> onBacktickedText)
	{
		int iteration = 0;
		int cursor = 0;
		int currentBacktickStart = -1;
		int currentTagStart = -1;
		while (true)
		{
			iteration++;
			if (iteration > 500)
			{
				throw new Exception("Max iteration limit passed");
			}
			if (currentTagStart != -1)
			{
				int tagEnd = inputText.IndexOf('>', cursor);
				if (tagEnd == -1)
				{
					throw new Exception("Unclosed Tag");
				}
				string tagType = inputText.Substring(currentTagStart + 1, tagEnd - currentTagStart - 1);
				try
				{
					onTag?.Invoke(tagType);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				cursor = tagEnd + 1;
				currentTagStart = -1;
				continue;
			}
			int nextBacktick = inputText.IndexOf('`', cursor);
			int nextTagStart = inputText.IndexOf('<', cursor);
			if (nextBacktick != -1 && (nextTagStart == -1 || nextTagStart > nextBacktick))
			{
				if (currentBacktickStart == -1)
				{
					currentBacktickStart = nextBacktick;
					if (cursor != currentBacktickStart)
					{
						onNormalText?.Invoke(inputText.Substring(cursor, currentBacktickStart - cursor));
					}
					cursor = nextBacktick + 1;
					continue;
				}
				string rawExpression = inputText.Substring(currentBacktickStart + 1, nextBacktick - currentBacktickStart - 1);
				try
				{
					onBacktickedText?.Invoke(rawExpression);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				currentBacktickStart = -1;
				cursor = nextBacktick + 1;
			}
			else
			{
				if (nextTagStart == -1)
				{
					break;
				}
				currentTagStart = nextTagStart;
				if (cursor != nextTagStart)
				{
					onNormalText?.Invoke(inputText.Substring(cursor, nextTagStart - cursor));
				}
				cursor = currentTagStart + 1;
			}
		}
		try
		{
			onNormalText?.Invoke(inputText.Substring(cursor, inputText.Length - cursor));
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
	}

	public static string GetFormat(string formatString)
	{
		if (string.IsNullOrEmpty(formatString))
		{
			return "#,##0";
		}
		return formatString switch
		{
			"time" => "#,##0.#", 
			"percent" => "#,##0\\%", 
			"percent*100" => "#,##0%", 
			_ => formatString, 
		};
	}

	public static void ParseExpression(string inputText, Action<string> onNormalExpression, Action<string> onField)
	{
		inputText = inputText.Replace(" ", "");
		int cursor = 0;
		while (cursor < inputText.Length)
		{
			int fieldStart = inputText.IndexOfAny(FieldStartChars, cursor);
			if (fieldStart != -1)
			{
				if (cursor != fieldStart)
				{
					string prevExp = inputText.Substring(cursor, fieldStart - cursor);
					onNormalExpression?.Invoke(prevExp);
				}
				int fieldEnd = inputText.IndexOfAny(ArithmeticExpressionChars, fieldStart) - 1;
				if (fieldEnd == -2)
				{
					fieldEnd = inputText.Length - 1;
				}
				cursor = fieldEnd + 1;
				string field = inputText.Substring(fieldStart, fieldEnd - fieldStart + 1);
				onField?.Invoke(field);
				continue;
			}
			onNormalExpression?.Invoke(inputText.Substring(cursor, inputText.Length - cursor));
			break;
		}
	}
}
