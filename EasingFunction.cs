using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class EasingFunction
{
	private const float NATURAL_LOG_OF_2 = 0.6931472f;

	public static float Get(this DewEase ease, float x)
	{
		return ease switch
		{
			DewEase.Linear => Linear(0f, 1f, x), 
			DewEase.EaseInQuad => EaseInQuad(0f, 1f, x), 
			DewEase.EaseOutQuad => EaseOutQuad(0f, 1f, x), 
			DewEase.EaseInOutQuad => EaseInOutQuad(0f, 1f, x), 
			DewEase.EaseInCubic => EaseInCubic(0f, 1f, x), 
			DewEase.EaseOutCubic => EaseOutCubic(0f, 1f, x), 
			DewEase.EaseInOutCubic => EaseInOutCubic(0f, 1f, x), 
			DewEase.EaseInQuart => EaseInQuart(0f, 1f, x), 
			DewEase.EaseOutQuart => EaseOutQuart(0f, 1f, x), 
			DewEase.EaseInOutQuart => EaseInOutQuart(0f, 1f, x), 
			DewEase.EaseInQuint => EaseInQuint(0f, 1f, x), 
			DewEase.EaseOutQuint => EaseOutQuint(0f, 1f, x), 
			DewEase.EaseInOutQuint => EaseInOutQuint(0f, 1f, x), 
			DewEase.EaseInSine => EaseInSine(0f, 1f, x), 
			DewEase.EaseOutSine => EaseOutSine(0f, 1f, x), 
			DewEase.EaseInOutSine => EaseInOutSine(0f, 1f, x), 
			DewEase.EaseInExpo => EaseInExpo(0f, 1f, x), 
			DewEase.EaseOutExpo => EaseOutExpo(0f, 1f, x), 
			DewEase.EaseInOutExpo => EaseInOutExpo(0f, 1f, x), 
			DewEase.EaseInCirc => EaseInCirc(0f, 1f, x), 
			DewEase.EaseOutCirc => EaseOutCirc(0f, 1f, x), 
			DewEase.EaseInOutCirc => EaseInOutCirc(0f, 1f, x), 
			DewEase.Spring => Spring(0f, 1f, x), 
			DewEase.EaseInBounce => EaseInBounce(0f, 1f, x), 
			DewEase.EaseOutBounce => EaseOutBounce(0f, 1f, x), 
			DewEase.EaseInOutBounce => EaseInOutBounce(0f, 1f, x), 
			DewEase.EaseInBack => EaseInBack(0f, 1f, x), 
			DewEase.EaseOutBack => EaseOutBack(0f, 1f, x), 
			DewEase.EaseInOutBack => EaseInOutBack(0f, 1f, x), 
			DewEase.EaseInElastic => EaseInElastic(0f, 1f, x), 
			DewEase.EaseOutElastic => EaseOutElastic(0f, 1f, x), 
			DewEase.EaseInOutElastic => EaseInOutElastic(0f, 1f, x), 
			_ => x, 
		};
	}

	public static float GetDelta(this DewEase ease, float x)
	{
		return ease switch
		{
			DewEase.Linear => LinearD(0f, 1f, x), 
			DewEase.EaseInQuad => EaseInQuadD(0f, 1f, x), 
			DewEase.EaseOutQuad => EaseOutQuadD(0f, 1f, x), 
			DewEase.EaseInOutQuad => EaseInOutQuadD(0f, 1f, x), 
			DewEase.EaseInCubic => EaseInCubicD(0f, 1f, x), 
			DewEase.EaseOutCubic => EaseOutCubicD(0f, 1f, x), 
			DewEase.EaseInOutCubic => EaseInOutCubicD(0f, 1f, x), 
			DewEase.EaseInQuart => EaseInQuartD(0f, 1f, x), 
			DewEase.EaseOutQuart => EaseOutQuartD(0f, 1f, x), 
			DewEase.EaseInOutQuart => EaseInOutQuartD(0f, 1f, x), 
			DewEase.EaseInQuint => EaseInQuintD(0f, 1f, x), 
			DewEase.EaseOutQuint => EaseOutQuintD(0f, 1f, x), 
			DewEase.EaseInOutQuint => EaseInOutQuintD(0f, 1f, x), 
			DewEase.EaseInSine => EaseInSineD(0f, 1f, x), 
			DewEase.EaseOutSine => EaseOutSineD(0f, 1f, x), 
			DewEase.EaseInOutSine => EaseInOutSineD(0f, 1f, x), 
			DewEase.EaseInExpo => EaseInExpoD(0f, 1f, x), 
			DewEase.EaseOutExpo => EaseOutExpoD(0f, 1f, x), 
			DewEase.EaseInOutExpo => EaseInOutExpoD(0f, 1f, x), 
			DewEase.EaseInCirc => EaseInCircD(0f, 1f, x), 
			DewEase.EaseOutCirc => EaseOutCircD(0f, 1f, x), 
			DewEase.EaseInOutCirc => EaseInOutCircD(0f, 1f, x), 
			DewEase.Spring => SpringD(0f, 1f, x), 
			DewEase.EaseInBounce => EaseInBounceD(0f, 1f, x), 
			DewEase.EaseOutBounce => EaseOutBounceD(0f, 1f, x), 
			DewEase.EaseInOutBounce => EaseInOutBounceD(0f, 1f, x), 
			DewEase.EaseInBack => EaseInBackD(0f, 1f, x), 
			DewEase.EaseOutBack => EaseOutBackD(0f, 1f, x), 
			DewEase.EaseInOutBack => EaseInOutBackD(0f, 1f, x), 
			DewEase.EaseInElastic => EaseInElasticD(0f, 1f, x), 
			DewEase.EaseOutElastic => EaseOutElasticD(0f, 1f, x), 
			DewEase.EaseInOutElastic => EaseInOutElasticD(0f, 1f, x), 
			_ => x, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Linear(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * MathF.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuad(float start, float end, float value)
	{
		end -= start;
		return end * value * value + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuad(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * value * (value - 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value + start;
		}
		value -= 1f;
		return (0f - end) * 0.5f * (value * (value - 2f) - 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInCubic(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutCubic(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value + 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value + 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuart(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuart(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return (0f - end) * (value * value * value * value - 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuart(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value * value + start;
		}
		value -= 2f;
		return (0f - end) * 0.5f * (value * value * value * value - 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuint(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value * value + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuint(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value * value * value + 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value * value * value + 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(value * (MathF.PI / 2f)) + end + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutSine(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Sin(value * (MathF.PI / 2f)) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * 0.5f * (Mathf.Cos(MathF.PI * value) - 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInExpo(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Pow(2f, 10f * (value - 1f)) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutExpo(float start, float end, float value)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * value) + 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * Mathf.Pow(2f, 10f * (value - 1f)) + start;
		}
		value -= 1f;
		return end * 0.5f * (0f - Mathf.Pow(2f, -10f * value) + 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInCirc(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * (Mathf.Sqrt(1f - value * value) - 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutCirc(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * Mathf.Sqrt(1f - value * value) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutCirc(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return (0f - end) * 0.5f * (Mathf.Sqrt(1f - value * value) - 1f) + start;
		}
		value -= 2f;
		return end * 0.5f * (Mathf.Sqrt(1f - value * value) + 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInBounce(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		return end - EaseOutBounce(0f, end, d - value) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutBounce(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < 0.36363637f)
		{
			return end * (7.5625f * value * value) + start;
		}
		if (value < 0.72727275f)
		{
			value -= 0.54545456f;
			return end * (7.5625f * value * value + 0.75f) + start;
		}
		if ((double)value < 0.9090909090909091)
		{
			value -= 0.8181818f;
			return end * (7.5625f * value * value + 0.9375f) + start;
		}
		value -= 21f / 22f;
		return end * (7.5625f * value * value + 63f / 64f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutBounce(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		if (value < d * 0.5f)
		{
			return EaseInBounce(0f, end, value * 2f) * 0.5f + start;
		}
		return EaseOutBounce(0f, end, value * 2f - d) * 0.5f + end * 0.5f + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInBack(float start, float end, float value)
	{
		end -= start;
		value /= 1f;
		float s = 1.70158f;
		return end * value * value * ((s + 1f) * value - s) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutBack(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value -= 1f;
		return end * (value * value * ((s + 1f) * value + s) + 1f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutBack(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value /= 0.5f;
		if (value < 1f)
		{
			s *= 1.525f;
			return end * 0.5f * (value * value * ((s + 1f) * value - s)) + start;
		}
		value -= 2f;
		s *= 1.525f;
		return end * 0.5f * (value * value * ((s + 1f) * value + s) + 2f) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInElastic(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		float p = d * 0.3f;
		float a = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= d) == 1f)
		{
			return start + end;
		}
		float s;
		if (a == 0f || a < Mathf.Abs(end))
		{
			a = end;
			s = p / 4f;
		}
		else
		{
			s = p / (MathF.PI * 2f) * Mathf.Asin(end / a);
		}
		return 0f - a * Mathf.Pow(2f, 10f * (value -= 1f)) * Mathf.Sin((value * d - s) * (MathF.PI * 2f) / p) + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutElastic(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		float p = d * 0.3f;
		float a = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= d) == 1f)
		{
			return start + end;
		}
		float s;
		if (a == 0f || a < Mathf.Abs(end))
		{
			a = end;
			s = p * 0.25f;
		}
		else
		{
			s = p / (MathF.PI * 2f) * Mathf.Asin(end / a);
		}
		return a * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * d - s) * (MathF.PI * 2f) / p) + end + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutElastic(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		float p = d * 0.3f;
		float a = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= d * 0.5f) == 2f)
		{
			return start + end;
		}
		float s;
		if (a == 0f || a < Mathf.Abs(end))
		{
			a = end;
			s = p / 4f;
		}
		else
		{
			s = p / (MathF.PI * 2f) * Mathf.Asin(end / a);
		}
		if (value < 1f)
		{
			return -0.5f * (a * Mathf.Pow(2f, 10f * (value -= 1f)) * Mathf.Sin((value * d - s) * (MathF.PI * 2f) / p)) + start;
		}
		return a * Mathf.Pow(2f, -10f * (value -= 1f)) * Mathf.Sin((value * d - s) * (MathF.PI * 2f) / p) * 0.5f + end + start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float LinearD(float start, float end, float value)
	{
		return end - start;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuadD(float start, float end, float value)
	{
		return 2f * (end - start) * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuadD(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * value - end * (value - 2f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuadD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * value;
		}
		value -= 1f;
		return end * (1f - value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInCubicD(float start, float end, float value)
	{
		return 3f * (end - start) * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutCubicD(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return 3f * end * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutCubicD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return 1.5f * end * value * value;
		}
		value -= 2f;
		return 1.5f * end * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuartD(float start, float end, float value)
	{
		return 4f * (end - start) * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuartD(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return -4f * end * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuartD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return 2f * end * value * value * value;
		}
		value -= 2f;
		return -2f * end * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInQuintD(float start, float end, float value)
	{
		return 5f * (end - start) * value * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutQuintD(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return 5f * end * value * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutQuintD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return 2.5f * end * value * value * value * value;
		}
		value -= 2f;
		return 2.5f * end * value * value * value * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInSineD(float start, float end, float value)
	{
		return (end - start) * 0.5f * MathF.PI * Mathf.Sin(MathF.PI / 2f * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutSineD(float start, float end, float value)
	{
		end -= start;
		return MathF.PI / 2f * end * Mathf.Cos(value * (MathF.PI / 2f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutSineD(float start, float end, float value)
	{
		end -= start;
		return end * 0.5f * MathF.PI * Mathf.Sin(MathF.PI * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInExpoD(float start, float end, float value)
	{
		return 6.931472f * (end - start) * Mathf.Pow(2f, 10f * (value - 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutExpoD(float start, float end, float value)
	{
		end -= start;
		return 3.465736f * end * Mathf.Pow(2f, 1f - 10f * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutExpoD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return 3.465736f * end * Mathf.Pow(2f, 10f * (value - 1f));
		}
		value -= 1f;
		return 3.465736f * end / Mathf.Pow(2f, 10f * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInCircD(float start, float end, float value)
	{
		return (end - start) * value / Mathf.Sqrt(1f - value * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutCircD(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return (0f - end) * value / Mathf.Sqrt(1f - value * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutCircD(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * value / (2f * Mathf.Sqrt(1f - value * value));
		}
		value -= 2f;
		return (0f - end) * value / (2f * Mathf.Sqrt(1f - value * value));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInBounceD(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		return EaseOutBounceD(0f, end, d - value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutBounceD(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < 0.36363637f)
		{
			return 2f * end * 7.5625f * value;
		}
		if (value < 0.72727275f)
		{
			value -= 0.54545456f;
			return 2f * end * 7.5625f * value;
		}
		if ((double)value < 0.9090909090909091)
		{
			value -= 0.8181818f;
			return 2f * end * 7.5625f * value;
		}
		value -= 21f / 22f;
		return 2f * end * 7.5625f * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutBounceD(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		if (value < d * 0.5f)
		{
			return EaseInBounceD(0f, end, value * 2f) * 0.5f;
		}
		return EaseOutBounceD(0f, end, value * 2f - d) * 0.5f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInBackD(float start, float end, float value)
	{
		float s = 1.70158f;
		return 3f * (s + 1f) * (end - start) * value * value - 2f * s * (end - start) * value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutBackD(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value -= 1f;
		return end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutBackD(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value /= 0.5f;
		if (value < 1f)
		{
			s *= 1.525f;
			return 0.5f * end * (s + 1f) * value * value + end * value * ((s + 1f) * value - s);
		}
		value -= 2f;
		s *= 1.525f;
		return 0.5f * end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInElasticD(float start, float end, float value)
	{
		return EaseOutElasticD(start, end, 1f - value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseOutElasticD(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		float p = d * 0.3f;
		float a = 0f;
		float s;
		if (a == 0f || a < Mathf.Abs(end))
		{
			a = end;
			s = p * 0.25f;
		}
		else
		{
			s = p / (MathF.PI * 2f) * Mathf.Asin(end / a);
		}
		return a * MathF.PI * d * Mathf.Pow(2f, 1f - 10f * value) * Mathf.Cos(MathF.PI * 2f * (d * value - s) / p) / p - 3.465736f * a * Mathf.Pow(2f, 1f - 10f * value) * Mathf.Sin(MathF.PI * 2f * (d * value - s) / p);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EaseInOutElasticD(float start, float end, float value)
	{
		end -= start;
		float d = 1f;
		float p = d * 0.3f;
		float a = 0f;
		float s;
		if (a == 0f || a < Mathf.Abs(end))
		{
			a = end;
			s = p / 4f;
		}
		else
		{
			s = p / (MathF.PI * 2f) * Mathf.Asin(end / a);
		}
		if (value < 1f)
		{
			value -= 1f;
			return -3.465736f * a * Mathf.Pow(2f, 10f * value) * Mathf.Sin(MathF.PI * 2f * (d * value - 2f) / p) - a * MathF.PI * d * Mathf.Pow(2f, 10f * value) * Mathf.Cos(MathF.PI * 2f * (d * value - s) / p) / p;
		}
		value -= 1f;
		return a * MathF.PI * d * Mathf.Cos(MathF.PI * 2f * (d * value - s) / p) / (p * Mathf.Pow(2f, 10f * value)) - 3.465736f * a * Mathf.Sin(MathF.PI * 2f * (d * value - s) / p) / Mathf.Pow(2f, 10f * value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SpringD(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		end -= start;
		return end * (6f * (1f - value) / 5f + 1f) * (-2.2f * Mathf.Pow(1f - value, 1.2f) * Mathf.Sin(MathF.PI * value * (2.5f * value * value * value + 0.2f)) + Mathf.Pow(1f - value, 2.2f) * (MathF.PI * (2.5f * value * value * value + 0.2f) + 23.561945f * value * value * value) * Mathf.Cos(MathF.PI * value * (2.5f * value * value * value + 0.2f)) + 1f) - 6f * end * (Mathf.Pow(1f - value, 2.2f) * Mathf.Sin(MathF.PI * value * (2.5f * value * value * value + 0.2f)) + value / 5f);
	}

	public static EaseFunction GetEasingFunction(DewEase easingFunction)
	{
		return easingFunction switch
		{
			DewEase.EaseInQuad => EaseInQuad, 
			DewEase.EaseOutQuad => EaseOutQuad, 
			DewEase.EaseInOutQuad => EaseInOutQuad, 
			DewEase.EaseInCubic => EaseInCubic, 
			DewEase.EaseOutCubic => EaseOutCubic, 
			DewEase.EaseInOutCubic => EaseInOutCubic, 
			DewEase.EaseInQuart => EaseInQuart, 
			DewEase.EaseOutQuart => EaseOutQuart, 
			DewEase.EaseInOutQuart => EaseInOutQuart, 
			DewEase.EaseInQuint => EaseInQuint, 
			DewEase.EaseOutQuint => EaseOutQuint, 
			DewEase.EaseInOutQuint => EaseInOutQuint, 
			DewEase.EaseInSine => EaseInSine, 
			DewEase.EaseOutSine => EaseOutSine, 
			DewEase.EaseInOutSine => EaseInOutSine, 
			DewEase.EaseInExpo => EaseInExpo, 
			DewEase.EaseOutExpo => EaseOutExpo, 
			DewEase.EaseInOutExpo => EaseInOutExpo, 
			DewEase.EaseInCirc => EaseInCirc, 
			DewEase.EaseOutCirc => EaseOutCirc, 
			DewEase.EaseInOutCirc => EaseInOutCirc, 
			DewEase.Linear => Linear, 
			DewEase.Spring => Spring, 
			DewEase.EaseInBounce => EaseInBounce, 
			DewEase.EaseOutBounce => EaseOutBounce, 
			DewEase.EaseInOutBounce => EaseInOutBounce, 
			DewEase.EaseInBack => EaseInBack, 
			DewEase.EaseOutBack => EaseOutBack, 
			DewEase.EaseInOutBack => EaseInOutBack, 
			DewEase.EaseInElastic => EaseInElastic, 
			DewEase.EaseOutElastic => EaseOutElastic, 
			DewEase.EaseInOutElastic => EaseInOutElastic, 
			_ => null, 
		};
	}

	public static EaseFunction GetEasingFunctionDerivative(DewEase easingFunction)
	{
		return easingFunction switch
		{
			DewEase.EaseInQuad => EaseInQuadD, 
			DewEase.EaseOutQuad => EaseOutQuadD, 
			DewEase.EaseInOutQuad => EaseInOutQuadD, 
			DewEase.EaseInCubic => EaseInCubicD, 
			DewEase.EaseOutCubic => EaseOutCubicD, 
			DewEase.EaseInOutCubic => EaseInOutCubicD, 
			DewEase.EaseInQuart => EaseInQuartD, 
			DewEase.EaseOutQuart => EaseOutQuartD, 
			DewEase.EaseInOutQuart => EaseInOutQuartD, 
			DewEase.EaseInQuint => EaseInQuintD, 
			DewEase.EaseOutQuint => EaseOutQuintD, 
			DewEase.EaseInOutQuint => EaseInOutQuintD, 
			DewEase.EaseInSine => EaseInSineD, 
			DewEase.EaseOutSine => EaseOutSineD, 
			DewEase.EaseInOutSine => EaseInOutSineD, 
			DewEase.EaseInExpo => EaseInExpoD, 
			DewEase.EaseOutExpo => EaseOutExpoD, 
			DewEase.EaseInOutExpo => EaseInOutExpoD, 
			DewEase.EaseInCirc => EaseInCircD, 
			DewEase.EaseOutCirc => EaseOutCircD, 
			DewEase.EaseInOutCirc => EaseInOutCircD, 
			DewEase.Linear => LinearD, 
			DewEase.Spring => SpringD, 
			DewEase.EaseInBounce => EaseInBounceD, 
			DewEase.EaseOutBounce => EaseOutBounceD, 
			DewEase.EaseInOutBounce => EaseInOutBounceD, 
			DewEase.EaseInBack => EaseInBackD, 
			DewEase.EaseOutBack => EaseOutBackD, 
			DewEase.EaseInOutBack => EaseInOutBackD, 
			DewEase.EaseInElastic => EaseInElasticD, 
			DewEase.EaseOutElastic => EaseOutElasticD, 
			DewEase.EaseInOutElastic => EaseInOutElasticD, 
			_ => null, 
		};
	}
}
