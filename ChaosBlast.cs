
using UnityEngine;

public class ChaosBlast : AttackBase
{
	
	public float Radius;

	public AnimationCurve SizeOverLifetime;

	internal bool FullPower;

	private float StartTimer;

	private void Start()
	{
		StartTimer = Time.time;
	}

	private void FixedUpdate()
	{
		Radius = SizeOverLifetime.Evaluate(Time.time - StartTimer);
		if (Time.time - StartTimer < 1.65f)
		{
			AttackSphere_Dir(Radius, Shadow_Lua.c_blast_power, Shadow_Lua.c_blast_damage, "ChaosBlast");
			SwitchAttackSphere(Radius);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (Time.time - StartTimer < 1.65f)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, Radius);
		}
	}
}
