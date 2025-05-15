public class RefValue<T>
{
	public T value;

	public RefValue(T v = default(T))
	{
		value = v;
	}

	public static implicit operator T(RefValue<T> value)
	{
		return value.value;
	}
}
