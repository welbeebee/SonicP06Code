using UnityEngine;

public class ContactObject : MonoBehaviour
{
	[Header("Sound")]
	public float Volume = 0.5f;

	public bool RandomPitch;

	public AudioClip[] ContactClip;

	public GameObject AudioSourcePrefab;

	[Header("Particle")]
	public GameObject ContactParticle;

	private Rigidbody _Rigidbody;

	private Vector3 Position = Vector3.zero;

	private float SoundTimer = -1f;

	private float ParticleTimer = 1f;

	private void OnCollisionEnter(Collision collision)
	{
		if (SoundTimer == -1f)
		{
			SoundTimer = Time.time;
		}
		if (Time.time - SoundTimer >= 0.5f && collision.collider.transform.parent != base.transform.parent)
		{
			SoundTimer = Time.time;
			if (ContactClip != null)
			{
				AudioSource component = Object.Instantiate(AudioSourcePrefab, base.transform.position, Quaternion.identity).GetComponent<AudioSource>();
				component.clip = ContactClip[Random.Range(0, ContactClip.Length)];
				component.spatialBlend = 1f;
				if (RandomPitch)
				{
					component.pitch = Random.Range(0.75f, 1.25f);
				}
				if (!_Rigidbody)
				{
					_Rigidbody = base.transform.GetComponent<Rigidbody>();
				}
				if (!_Rigidbody)
				{
					_Rigidbody = base.transform.parent.GetComponent<Rigidbody>();
				}
				component.volume = Mathf.Min(_Rigidbody.velocity.magnitude / 10f, 1f) * Volume;
				component.Play();
			}
		}
		if (!ContactParticle)
		{
			return;
		}
		if (!_Rigidbody)
		{
			_Rigidbody = base.transform.GetComponent<Rigidbody>();
		}
		if (!_Rigidbody)
		{
			_Rigidbody = base.transform.parent.GetComponent<Rigidbody>();
		}
		if (!(Time.time - ParticleTimer >= 0.1f) || !(_Rigidbody.velocity.magnitude > 10f) || !(collision.collider.transform.parent != base.transform.parent))
		{
			return;
		}
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			Vector3 point = contactPoint.point;
			if (!_Rigidbody)
			{
				_Rigidbody = base.transform.GetComponent<Rigidbody>();
			}
			if (!_Rigidbody)
			{
				_Rigidbody = base.transform.parent.GetComponent<Rigidbody>();
			}
			if (Position != Vector3.zero)
			{
				if (!(Vector3.Distance(Position, point) >= 1f))
				{
					continue;
				}
				Position = point;
			}
			Object.Instantiate(ContactParticle, point, Quaternion.identity).transform.forward = contactPoint.normal;
			ParticleTimer = Time.time;
		}
	}
}
