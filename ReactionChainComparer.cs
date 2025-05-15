using System;
using System.Collections.Generic;

public class ReactionChainComparer : IEqualityComparer<ReactionChain>
{
	public bool Equals(ReactionChain x, ReactionChain y)
	{
		return x.Equals(y);
	}

	public int GetHashCode(ReactionChain obj)
	{
		int hash = 0;
		if (obj._actors != null)
		{
			foreach (Actor a in obj._actors)
			{
				hash = HashCode.Combine(hash, a);
			}
		}
		return hash;
	}
}
