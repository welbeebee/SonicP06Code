
using UnityEngine;

public class FireBall : AttackBase
{
	
	public Rigidbody _Rigidbody;

	public GameObject ExplosionFX;

	internal GameObject ClosestTarget;

	private float StartTime;

	private void Start()
	{
		StartTime = Time.time;
	}

	private void FixedUpdate()
	{
		if ((bool)ClosestTarget && ClosestTarget.layer == LayerMask.NameToLayer("Enemy"))
		{
			base.transform.forward = ClosestTarget.transform.position - base.transform.position;
		}
		_Rigidbody.velocity = base.transform.forward * Omega_Lua.c_omega_launcher_speed;
		if (Time.time - StartTime > Omega_Lua.c_omega_launcher_atime || AttackSphere(Omega_Lua.c_omega_launcher_power, Omega_Lua.c_omega_launcher_damage, "OnHit"))
		{
			Explode();
		}
	}

	private void Explode()
	{
		Object.Instantiate(ExplosionFX, base.transform.position, Quaternion.identity);
		Object.Destroy(base.gameObject);
	}
}
