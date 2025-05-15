using System;
using System.Collections.Generic;

public struct ReactionChain : IEquatable<ReactionChain>
{
	internal List<Actor> _actors;

	private ReactionChain(ReactionChain original)
	{
		if (original._actors == null)
		{
			_actors = null;
		}
		else
		{
			_actors = new List<Actor>(original._actors);
		}
	}

	private void Add(Actor actor)
	{
		if (_actors == null)
		{
			_actors = new List<Actor>(4);
		}
		_actors.Add(actor);
	}

	public ReactionChain New(Actor actor)
	{
		ReactionChain newContext = new ReactionChain(this);
		newContext.Add(actor);
		return newContext;
	}

	public bool DidReact(Actor actor, bool checkOnlyType = false)
	{
		if (_actors == null)
		{
			return false;
		}
		if (checkOnlyType)
		{
			foreach (Actor actor2 in _actors)
			{
				if (actor2.GetType() == actor.GetType())
				{
					return true;
				}
			}
			return false;
		}
		return _actors.Contains(actor);
	}

	public bool Equals(ReactionChain other)
	{
		if (_actors != null && other._actors != null)
		{
			if (_actors.Count != other._actors.Count)
			{
				return false;
			}
			for (int i = 0; i < _actors.Count; i++)
			{
				if (_actors[i] != other._actors[i])
				{
					return false;
				}
			}
			return true;
		}
		return _actors == null == (other._actors == null);
	}

	public override bool Equals(object obj)
	{
		if (obj is ReactionChain other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_actors == null || _actors.Count == 0)
		{
			return 0;
		}
		int code = 0;
		for (int i = 0; i < _actors.Count; i++)
		{
			code = HashCode.Combine(code, _actors[i]);
		}
		return code;
	}
}
