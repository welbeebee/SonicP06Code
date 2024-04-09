using UnityEngine;

public class HurtPlayer : ObjectBase
{
	public enum Type
	{
		Enemy,
		Projectile,
		Object
	}

	public Type hurtType;

	public bool OnCollision;

	public bool OnlyMachSpeed;

	internal bool BlockDmg;

	private void OnTriggerStay(Collider collider)
	{
		if (!OnCollision && !BlockDmg)
		{
			PlayerBase player = GetPlayer(collider);
			if ((bool)player && !(player.GetState() == "Vehicle") && (!OnlyMachSpeed || (OnlyMachSpeed && player.GetPrefab("sonic_fast"))))
			{
				player.OnHurtEnter((int)hurtType);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (OnCollision && !BlockDmg)
		{
			PlayerBase player = GetPlayer(collision.transform);
			if ((bool)player && !(player.GetState() == "Vehicle") && (!OnlyMachSpeed || (OnlyMachSpeed && player.GetPrefab("sonic_fast"))))
			{
				player.OnHurtEnter((int)hurtType);
			}
		}
	}
}
