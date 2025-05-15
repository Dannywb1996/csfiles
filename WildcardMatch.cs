using System.Collections.Generic;

public static class WildcardMatch
{
	public static bool EqualsWildcard(this string text, string wildcardString)
	{
		bool isLike = true;
		byte matchCase = 0;
		int currentPatternStartIndex = 0;
		int lastCheckedHeadIndex = 0;
		int lastCheckedTailIndex = 0;
		int reversedWordIndex = 0;
		List<char[]> reversedPatterns = new List<char[]>();
		if (text == null || wildcardString == null)
		{
			return false;
		}
		char[] word = text.ToCharArray();
		char[] filter = wildcardString.ToCharArray();
		for (int i = 0; i < filter.Length; i++)
		{
			if (filter[i] == '?')
			{
				matchCase++;
				break;
			}
		}
		for (int j = 0; j < filter.Length; j++)
		{
			if (filter[j] == '*')
			{
				matchCase += 2;
				break;
			}
		}
		if ((matchCase == 0 || matchCase == 1) && word.Length != filter.Length)
		{
			return false;
		}
		switch (matchCase)
		{
		case 0:
			isLike = text == wildcardString;
			break;
		case 1:
		{
			for (int num5 = 0; num5 < text.Length; num5++)
			{
				if (word[num5] != filter[num5] && filter[num5] != '?')
				{
					isLike = false;
				}
			}
			break;
		}
		case 2:
		{
			for (int num6 = 0; num6 < filter.Length; num6++)
			{
				if (filter[num6] != '*')
				{
					if (filter[num6] != word[num6])
					{
						return false;
					}
					continue;
				}
				lastCheckedHeadIndex = num6;
				break;
			}
			for (int num7 = 0; num7 < filter.Length; num7++)
			{
				if (filter[filter.Length - 1 - num7] != '*')
				{
					if (filter[filter.Length - 1 - num7] != word[word.Length - 1 - num7])
					{
						return false;
					}
					continue;
				}
				lastCheckedTailIndex = num7;
				break;
			}
			char[] reversedWord = new char[word.Length - lastCheckedHeadIndex - lastCheckedTailIndex];
			char[] reversedFilter = new char[filter.Length - lastCheckedHeadIndex - lastCheckedTailIndex];
			for (int num8 = 0; num8 < reversedWord.Length; num8++)
			{
				reversedWord[num8] = word[word.Length - (num8 + 1) - lastCheckedTailIndex];
			}
			for (int num9 = 0; num9 < reversedFilter.Length; num9++)
			{
				reversedFilter[num9] = filter[filter.Length - (num9 + 1) - lastCheckedTailIndex];
			}
			for (int num10 = 0; num10 < reversedFilter.Length; num10++)
			{
				if (reversedFilter[num10] != '*')
				{
					continue;
				}
				if (num10 - currentPatternStartIndex > 0)
				{
					char[] pattern2 = new char[num10 - currentPatternStartIndex];
					for (int num11 = 0; num11 < pattern2.Length; num11++)
					{
						pattern2[num11] = reversedFilter[currentPatternStartIndex + num11];
					}
					reversedPatterns.Add(pattern2);
				}
				currentPatternStartIndex = num10 + 1;
			}
			for (int num12 = 0; num12 < reversedPatterns.Count; num12++)
			{
				for (int num13 = 0; num13 < reversedPatterns[num12].Length; num13++)
				{
					if (reversedPatterns[num12].Length - 1 - num13 > reversedWord.Length - 1 - reversedWordIndex)
					{
						return false;
					}
					if (reversedPatterns[num12][num13] != reversedWord[reversedWordIndex + num13])
					{
						reversedWordIndex++;
						num13 = -1;
					}
					else if (num13 == reversedPatterns[num12].Length - 1)
					{
						reversedWordIndex += reversedPatterns[num12].Length;
					}
				}
			}
			break;
		}
		case 3:
		{
			for (int k = 0; k < filter.Length; k++)
			{
				if (filter[k] != '*')
				{
					if (filter[k] != word[k] && filter[k] != '?')
					{
						return false;
					}
					continue;
				}
				lastCheckedHeadIndex = k;
				break;
			}
			for (int l = 0; l < filter.Length; l++)
			{
				if (filter[filter.Length - 1 - l] != '*')
				{
					if (filter[filter.Length - 1 - l] != word[word.Length - 1 - l] && filter[filter.Length - 1 - l] != '?')
					{
						return false;
					}
					continue;
				}
				lastCheckedTailIndex = l;
				break;
			}
			char[] reversedWord = new char[word.Length - lastCheckedHeadIndex - lastCheckedTailIndex];
			char[] reversedFilter = new char[filter.Length - lastCheckedHeadIndex - lastCheckedTailIndex];
			for (int m = 0; m < reversedWord.Length; m++)
			{
				reversedWord[m] = word[word.Length - (m + 1) - lastCheckedTailIndex];
			}
			for (int n = 0; n < reversedFilter.Length; n++)
			{
				reversedFilter[n] = filter[filter.Length - (n + 1) - lastCheckedTailIndex];
			}
			for (int num = 0; num < reversedFilter.Length; num++)
			{
				if (reversedFilter[num] != '*')
				{
					continue;
				}
				if (num - currentPatternStartIndex > 0)
				{
					char[] pattern = new char[num - currentPatternStartIndex];
					for (int num2 = 0; num2 < pattern.Length; num2++)
					{
						pattern[num2] = reversedFilter[currentPatternStartIndex + num2];
					}
					reversedPatterns.Add(pattern);
				}
				currentPatternStartIndex = num + 1;
			}
			for (int num3 = 0; num3 < reversedPatterns.Count; num3++)
			{
				for (int num4 = 0; num4 < reversedPatterns[num3].Length; num4++)
				{
					if (reversedPatterns[num3].Length - 1 - num4 > reversedWord.Length - 1 - reversedWordIndex)
					{
						return false;
					}
					if (reversedPatterns[num3][num4] != '?' && reversedPatterns[num3][num4] != reversedWord[reversedWordIndex + num4])
					{
						reversedWordIndex++;
						num4 = -1;
					}
					else if (num4 == reversedPatterns[num3].Length - 1)
					{
						reversedWordIndex += reversedPatterns[num3].Length;
					}
				}
			}
			break;
		}
		}
		return isLike;
	}

	public static bool EqualsWildcard(this string text, string wildcardString, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return text.ToLower().EqualsWildcard(wildcardString.ToLower());
		}
		return text.EqualsWildcard(wildcardString);
	}
}
