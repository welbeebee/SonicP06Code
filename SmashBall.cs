using UnityEngine;

public class SmashBall : AttackBase
{
	
	public Rigidbody _Rigidbody;

	public GameObject ExplosionFX;

	public GameObject StunExpFX;

	public float Speed;

	internal bool Awakened;

	private GameObject ClosestTarget;

	private float HomingTime;

	private void Start()
	{
		Invoke("Explode", 3f);
		HomingTime = Time.time;
	}

	private void FixedUpdate()
	{
		ClosestTarget = FindTarget();
		if ((bool)ClosestTarget && ClosestTarget.layer == LayerMask.NameToLayer("Enemy"))
		{
			float num = Mathf.Lerp(15f, 30f, Mathf.Clamp01(Time.time - HomingTime));
			base.transform.forward = Vector3.Lerp(base.transform.forward, (ClosestTarget.transform.position - base.transform.position).normalized, Time.fixedDeltaTime * num);
		}
		_Rigidbody.velocity = base.transform.forward * Speed;
		if (PsychicAttackSphere(Speed, OnlyFlash: false, 0.3f) || SwitchAttackSphere(0.3f))
		{
			Explode();
		}
	}

	private void Explode()
	{
		Object.Instantiate(ExplosionFX, base.transform.position, Quaternion.identity);
		if (Awakened)
		{
			PsychicAttackSphere(Speed, OnlyFlash: true, 4.25f);
			Object.Instantiate(StunExpFX, base.transform.position, Quaternion.identity);
		}
		Object.Destroy(base.gameObject);
	}
}
