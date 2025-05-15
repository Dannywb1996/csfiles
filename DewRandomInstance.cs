using System;
using UnityEngine;

public class DewRandomInstance
{
	private System.Random random;

	public float value => (float)random.NextDouble();

	public Vector2 insideUnitCircle
	{
		get
		{
			float angle = (float)(random.NextDouble() * 2.0 * Math.PI);
			float distance = (float)Math.Sqrt(random.NextDouble());
			return new Vector2(distance * (float)Math.Cos(angle), distance * (float)Math.Sin(angle));
		}
	}

	public Vector3 insideUnitSphere
	{
		get
		{
			float theta = (float)(random.NextDouble() * 2.0 * Math.PI);
			float phi = (float)Math.Acos(2.0 * random.NextDouble() - 1.0);
			float num = (float)Math.Pow(random.NextDouble(), 1.0 / 3.0);
			float x = num * (float)(Math.Sin(phi) * Math.Cos(theta));
			float y = num * (float)(Math.Sin(phi) * Math.Sin(theta));
			float z = num * (float)Math.Cos(phi);
			return new Vector3(x, y, z);
		}
	}

	public Vector3 onUnitSphere
	{
		get
		{
			float theta = (float)(random.NextDouble() * 2.0 * Math.PI);
			float num = (float)Math.Acos(2.0 * random.NextDouble() - 1.0);
			float x = (float)(Math.Sin(num) * Math.Cos(theta));
			float y = (float)(Math.Sin(num) * Math.Sin(theta));
			float z = (float)Math.Cos(num);
			return new Vector3(x, y, z);
		}
	}

	public Quaternion rotation
	{
		get
		{
			float u1 = (float)random.NextDouble();
			float u2 = (float)random.NextDouble();
			float u3 = (float)random.NextDouble();
			float num = (float)Math.Sqrt(1f - u1);
			float sqrtU1 = (float)Math.Sqrt(u1);
			float twoPI_U2 = (float)(Math.PI * 2.0 * (double)u2);
			float twoPI_U3 = (float)(Math.PI * 2.0 * (double)u3);
			float x = num * (float)Math.Sin(twoPI_U2);
			float y = num * (float)Math.Cos(twoPI_U2);
			float z = sqrtU1 * (float)Math.Sin(twoPI_U3);
			float w = sqrtU1 * (float)Math.Cos(twoPI_U3);
			return new Quaternion(x, y, z, w);
		}
	}

	public Quaternion rotationUniform
	{
		get
		{
			Vector3 dir = onUnitSphere;
			return Quaternion.FromToRotation(Vector3.forward, dir) * Quaternion.Euler(0f, 0f, (float)(random.NextDouble() * 360.0));
		}
	}

	public DewRandomInstance(int seed)
	{
		random = new System.Random(seed);
	}

	public float Range(float min, float max)
	{
		return min + (float)random.NextDouble() * (max - min);
	}

	public int Range(int min, int max)
	{
		return random.Next(min, max);
	}

	public Color ColorHSV(float hueMin = 0f, float hueMax = 1f, float saturationMin = 0f, float saturationMax = 1f, float valueMin = 0f, float valueMax = 1f, float alphaMin = 1f, float alphaMax = 1f)
	{
		float h = Range(hueMin, hueMax);
		float s = Range(saturationMin, saturationMax);
		float v = Range(valueMin, valueMax);
		float a = Range(alphaMin, alphaMax);
		Color c = Color.HSVToRGB(h, s, v);
		c.a = a;
		return c;
	}
}
