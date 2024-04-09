using UnityEngine;

public class DeathMesh : ObjectBase
{
	public enum Type
	{
		Normal,
		Fall,
		Drown
	}

	[Header("Set layer to 'water' if its a Sonic & Elise stage and the collision is a water death plane")]
	public Type DeathType;

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && player.GetState() != "Orca")
		{
			player.OnDeathEnter((int)DeathType);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.gameObject.transform.root.GetComponentInChildren<EnemyBase>())
		{
			collision.gameObject.SendMessage("OnExplosion", new HitInfo(base.transform, Vector3.zero, 10), SendMessageOptions.DontRequireReceiver);
		}
	}
}
