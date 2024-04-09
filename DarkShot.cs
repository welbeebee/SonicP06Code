using UnityEngine;

public class DarkShot : ObjectBase
{
	
	public Rigidbody RigidBody;

	public float Speed = 0.15f;

	public float TurnSpeed = 2.5f;

	public GameObject Explosion;

	public ParticleSystem PsiFX;

	public ParticleSystem PsiOffFX;

	internal Transform Player;

	internal Transform Owner;

	private Transform PlayerPos;

	private Transform PlayerTransform;

	private bool Deflected;

	private bool Exploded;

	private bool IsPsychokinesis;

	internal bool PsychoThrown;

	private float StartTime;

	private void Start()
	{
		StartTime = Time.time;
	}

	private void FixedUpdate()
	{
		if (!Player)
		{
			AutoDestroy();
		}
		if (!Exploded && !IsPsychokinesis)
		{
			if (!PsychoThrown)
			{
				if ((bool)Owner && Time.time - StartTime < 1f)
				{
					Vector3 vector = Player.position + Player.up * 0.25f;
					base.transform.forward = Vector3.Lerp(base.transform.forward, (vector - base.transform.position).normalized, Time.fixedDeltaTime * TurnSpeed);
				}
				RigidBody.MovePosition(base.transform.position + base.transform.forward * Speed);
			}
			if (Time.time - StartTime > 5f)
			{
				Explode();
			}
		}
		if (IsPsychokinesis)
		{
			if (!PlayerTransform)
			{
				OnReleasePsycho();
			}
			else
			{
				RigidBody.velocity = (PlayerPos.position + PlayerPos.forward * -2.125f + PlayerPos.up * -1f - base.transform.position) * 24f;
			}
		}
		if (!PsychoThrown)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.25f);
		if (array == null)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				array[i].SendMessage("OnHit", new HitInfo(PlayerTransform, base.transform.forward * 25f), SendMessageOptions.DontRequireReceiver);
				ExplodeObj(PlayerTransform);
				AutoDestroy();
			}
		}
	}

	private void Explode()
	{
		Exploded = true;
		Object.Instantiate(Explosion, base.transform.position, Quaternion.identity);
		Object.Destroy(base.gameObject);
	}

	private void AutoDestroy()
	{
		Exploded = true;
		GameObject obj = Object.Instantiate(Explosion, base.transform.position, Quaternion.identity);
		Object.Destroy(obj.GetComponentInChildren<HurtPlayer>());
		Object.Destroy(obj.GetComponentInChildren<Collider>());
		Object.Destroy(base.gameObject);
	}

	private void ExplodeObj(Transform _Transform)
	{
		HitInfo value = new HitInfo(Player, base.transform.forward * 25f, 0);
		if (_Transform.gameObject.tag == "Vehicle")
		{
			_Transform.gameObject.SendMessage("OnVehicleHit", 1.5f, SendMessageOptions.DontRequireReceiver);
		}
		if (_Transform.gameObject.layer == LayerMask.NameToLayer("BreakableObj"))
		{
			_Transform.SendMessage("OnHit", value, SendMessageOptions.DontRequireReceiver);
		}
		if (PsychoThrown && (_Transform.gameObject.layer == LayerMask.NameToLayer("Enemy") || _Transform.gameObject.layer == LayerMask.NameToLayer("EnemyTrigger")))
		{
			_Transform.SendMessage("OnHit", new HitInfo(PlayerTransform, base.transform.forward * 25f), SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!IsPsychokinesis)
		{
			if (!Exploded)
			{
				Explode();
			}
			ExplodeObj(collider.transform);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsPsychokinesis)
		{
			if (!Exploded)
			{
				Explode();
			}
			ExplodeObj(collision.transform);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!IsPsychokinesis)
		{
			if (!Exploded)
			{
				Explode();
			}
			ExplodeObj(collision.transform);
		}
	}

	private void OnPsychoThrow(HitInfo HitInfo)
	{
		IsPsychokinesis = false;
		PsychoThrown = true;
		base.transform.forward = HitInfo.force;
		RigidBody.velocity = HitInfo.force;
		StartTime = Time.time;
		PsiFX.Stop();
		PsiOffFX.Play();
	}

	private void OnPsychokinesis(Transform _PlayerPos)
	{
		IsPsychokinesis = true;
		PlayerPos = _PlayerPos;
		PlayerTransform = _PlayerPos.root.transform;
		PsiFX.Play();
		PsiOffFX.Stop();
	}

	private void OnReleasePsycho()
	{
		AutoDestroy();
	}

	private void OnDeflect(Transform _PlayerPos)
	{
		if (base.enabled && !Deflected && !IsPsychokinesis && !PsychoThrown)
		{
			if ((bool)Owner)
			{
				Owner = null;
			}
			PlayerBase component = _PlayerPos.GetComponent<PlayerBase>();
			if ((bool)component && (bool)component.PlayerManager.silver && component.PlayerManager.silver.IsAwakened)
			{
				component.PlayerManager.silver.SilverEffects.CreatePsiDeflectFX(base.transform.position);
			}
			base.transform.forward = (base.transform.position - _PlayerPos.position).normalized;
			Deflected = true;
		}
	}
}
