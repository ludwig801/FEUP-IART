public class Counter
{
	private long value;

	public long Value
	{
		get
		{
			return value;
		}
	}

	public Counter()
	{
		Reset();
	}

	public void Inc()
	{
		this.value += 1L;
	}

	public void Add(long val)
	{
		this.value += val;
	}

	public void Reset()
	{
		this.value = 0L;
	}
}
