using UnityEngine;

public class RailCollision : ObjectBase
{
	public enum Type
	{
		Metal,
		Wind,
		Nature
	}

	public RailSystem BezierScript;

	public Type RailType;

	public bool Unswitchable;

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !player.IsDead)
		{
			player.OnRailEnter(BezierScript, (int)RailType);
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !player.IsDead)
		{
			player.OnRailEnter(BezierScript, (int)RailType);
		}
	}
}
