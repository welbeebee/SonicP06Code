using UnityEngine;

public class BreakTower : MonoBehaviour
{
	[Header("Prefab")]
	public float FXEnableTime;

	public Vector3 FXLocalPos;

	[Header("Prefab")]
	public GameObject MainObj;

	public GameObject BrokenObj;

	[Header("Broken Obj")]
	public Animation Animation;

	public Transform FXObject;

	public ParticleSystem StartFX;

	public ParticleSystem[] FX;

	public AudioSource[] Audios;

	private bool Destroyed;

	private bool PlayedFX;

	private bool AttackedVehicle;

	private float EnableTime;

	private void Update()
	{
		if (!Destroyed)
		{
			return;
		}
		if (Time.time - EnableTime > FXEnableTime && !PlayedFX)
		{
			for (int i = 0; i < FX.Length; i++)
			{
				if ((bool)FX[i])
				{
					FX[i].Play();
				}
			}
			PlayedFX = true;
		}
		if ((bool)Animation && !Animation.isPlaying)
		{
			Animation = null;
		}
	}

	public void OnHit(HitInfo HitInfo)
	{
		if (Destroyed)
		{
			return;
		}
		MainObj.SetActive(value: false);
		BrokenObj.SetActive(value: true);
		EnableTime = Time.time;
		FXObject.localPosition = FXLocalPos;
		StartFX.Play();
		for (int i = 0; i < Audios.Length; i++)
		{
			if ((bool)Audios[i])
			{
				Audios[i].Play();
			}
		}
		Destroyed = true;
	}

	private void OnExplosion(HitInfo HitInfo)
	{
		if (!Destroyed)
		{
			OnHit(HitInfo);
		}
	}

	private void OnEventSignal()
	{
		if (!Destroyed)
		{
			OnHit(new HitInfo(base.transform, Vector3.zero));
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		VehicleBase componentInParent = collider.GetComponentInParent<VehicleBase>();
		if (!componentInParent || !Destroyed || AttackedVehicle)
		{
			return;
		}
		AttackedVehicle = true;
		Collider[] componentsInChildren = collider.GetComponentsInChildren<Collider>();
		Collider[] componentsInChildren2 = BrokenObj.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			if ((bool)componentsInChildren2[i].GetComponent<ReturnCollisionMessage>())
			{
				Collider[] array = componentsInChildren;
				foreach (Collider collider2 in array)
				{
					Physics.IgnoreCollision(componentsInChildren2[i], collider2);
				}
			}
		}
		componentInParent.OnDamage(15f, KnockBack: true);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Vector3 vector = base.transform.position + base.transform.up * FXLocalPos.y + base.transform.right * FXLocalPos.x + base.transform.forward * FXLocalPos.z;
		Gizmos.DrawLine(base.transform.position, vector);
		Gizmos.DrawWireSphere(vector, 1f);
	}
}
