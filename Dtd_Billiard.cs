
using UnityEngine;

public class Dtd_Billiard : MonoBehaviour
{
	
	public int Count;

	[Header("Prefab")]
	public GameObject BrokenPrefab;

	public Renderer[] Renderers;

	public Rigidbody RigidBody;

	public AudioSource[] Audio;

	public AudioSource CollideAudio;

	public AudioClip[] CollideClips;

	public Collider Collider;

	public GameObject Mesh;

	public GameObject SuccessFX;

	internal Dtd_BilliardCounter Counter;

	internal bool Holed;

	internal bool Destroyed;

	internal bool IsGrabbed;

	private MaterialPropertyBlock PropBlock;

	private Vector4[] UVSet = new Vector4[10]
	{
		new Vector4(1f, 1f, 0f, 0f),
		new Vector4(1f, 1f, 0f, 0.25f),
		new Vector4(1f, 1f, 0.3f, 0.25f),
		new Vector4(1f, 1f, 0.6f, 0.25f),
		new Vector4(1f, 1f, 0.6f, 0.5f),
		new Vector4(1f, 1f, 0.3f, 0.5f),
		new Vector4(1f, 1f, 0f, 0.5f),
		new Vector4(1f, 1f, 0f, 0.75f),
		new Vector4(1f, 1f, 0.3f, 0.75f),
		new Vector4(1f, 1f, 0.6f, 0.75f)
	};

	private float[] EffSpdSet = new float[10] { 3.25f, 3f, 2.75f, 2.5f, 2.25f, 2f, 1.75f, 1.5f, 1.25f, 1f };

	private Vector3 StartPosition;

	private Quaternion StartRotation;

	private int HP;

	private float Timer = -1f;

	private float PushTimer = -1f;

	public void SetParameters(int _Count)
	{
		Count = _Count;
	}

	private void Awake()
	{
		if (!Counter)
		{
			PropBlock = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		if (!Counter)
		{
			SetUpHP(Count);
		}
		StartPosition = base.transform.position;
		StartRotation = base.transform.rotation;
	}

	public void SetUpHP(int Amount, bool IsCounter = false)
	{
		HP = Amount;
		PropBlock = new MaterialPropertyBlock();
		UpdateMat();
	}

	private void UpdateMat()
	{
		if (!Holed)
		{
			Renderers[0].GetPropertyBlock(PropBlock);
			PropBlock.SetVector("_MainTex_ST", UVSet[HP]);
			PropBlock.SetFloat("_ScrollSpd", EffSpdSet[HP]);
			Renderers[0].SetPropertyBlock(PropBlock);
		}
	}

	private void Update()
	{
		if (!Holed)
		{
			Audio[0].volume = Mathf.Lerp(0f, 1f, RigidBody.velocity.magnitude / 10f) * 5f;
			Audio[0].pitch = Mathf.Lerp(0.5f, 1f, RigidBody.velocity.magnitude / 10f) * 5f;
			Audio[1].volume = Mathf.Lerp(Audio[1].volume, 0f, Time.deltaTime);
		}
		for (int i = 0; i < Renderers.Length; i++)
		{
			Renderers[i].GetPropertyBlock(PropBlock);
			PropBlock.SetColor("_ExtFresColor", new Vector4(0f, 255f, 216f, 1f));
			PropBlock.SetFloat("_ExtFresPower", 2f);
			PropBlock.SetFloat("_ExtFresThre", Mathf.Lerp(PropBlock.GetFloat("_ExtFresThre"), IsGrabbed ? 0.005f : 0f, Time.deltaTime * 10f));
			PropBlock.SetColor("_OutlineColor", new Vector4(0f, 255f, 168f, 1f));
			PropBlock.SetFloat("_OutlinePulseSpd", 0f);
			PropBlock.SetFloat("_OutlineInt", IsGrabbed ? 1f : 0f);
			Renderers[i].SetPropertyBlock(PropBlock);
		}
	}

	private void FixedUpdate()
	{
		if (!Holed)
		{
			RigidBody.AddForce(RigidBody.velocity.normalized * 10f, ForceMode.Force);
		}
		if (IsGrabbed)
		{
			Vector3 velocity = RigidBody.velocity;
			velocity.x = Mathf.Lerp(velocity.x, 0f, Time.fixedDeltaTime * 7.5f);
			velocity.z = Mathf.Lerp(velocity.z, 0f, Time.fixedDeltaTime * 7.5f);
			RigidBody.velocity = velocity;
			IsGrabbed = false;
		}
	}

	public void Explode()
	{
		Destroyed = true;
		if (!Holed)
		{
			Invoke("Restore", 3f);
			RigidBody.useGravity = false;
			RigidBody.isKinematic = true;
			Collider.enabled = false;
			Mesh.SetActive(value: false);
			GameObject gameObject = Object.Instantiate(BrokenPrefab, base.transform.position, base.transform.rotation);
			ExtensionMethods.SetBrokenColFix(base.transform, gameObject);
			HitInfo hitInfo = new HitInfo(base.transform, Vector3.zero);
			hitInfo.force = base.transform.position;
			gameObject.SendMessage("OnCreate", hitInfo, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Restore()
	{
		Holed = false;
		Destroyed = false;
		HP = Count;
		UpdateMat();
		base.transform.position = StartPosition;
		base.transform.rotation = StartRotation;
		RigidBody.useGravity = true;
		RigidBody.isKinematic = false;
		RigidBody.velocity = Vector3.zero;
		RigidBody.angularVelocity = Vector3.zero;
		Collider.enabled = true;
		Mesh.SetActive(value: true);
		SuccessFX.SetActive(value: false);
	}

	public void Reset()
	{
		HP = Count;
		UpdateMat();
		base.transform.position = StartPosition;
		base.transform.rotation = StartRotation;
		RigidBody.velocity = Vector3.zero;
		RigidBody.angularVelocity = Vector3.zero;
	}

	public void Break()
	{
		Invoke("Explode", 0.5f);
	}

	private void OnHit(HitInfo HitInfo)
	{
		if (!Holed && Counter.CanContinue())
		{
			OnPush();
			Audio[1].volume = 1f;
			Audio[2].Play();
			Vector3 normalized = Vector3.Slerp(HitInfo.force.normalized, (RigidBody.worldCenterOfMass - HitInfo.player.position).normalized, 0.75f).normalized;
			RigidBody.AddForce((HitInfo.force.normalized + normalized).normalized * HitInfo.force.magnitude * 0.25f, ForceMode.VelocityChange);
		}
	}

	public void OnDamage()
	{
		if ((bool)Counter)
		{
			if (HP > 0)
			{
				HP--;
				UpdateMat();
			}
			else
			{
				Break();
			}
		}
	}

	public void OnPush()
	{
		if (!Counter)
		{
			if (HP > 0)
			{
				HP--;
				UpdateMat();
			}
			else
			{
				Break();
			}
		}
		else
		{
			Counter.OnBilliard();
		}
	}

	public void OnSwitch()
	{
		if (!Holed)
		{
			Holed = true;
			RigidBody.useGravity = false;
			RigidBody.isKinematic = true;
			Collider.enabled = false;
			Mesh.SetActive(value: false);
			SuccessFX.transform.rotation = Quaternion.identity;
			SuccessFX.SetActive(value: true);
		}
	}

	private void OnPsychoThrow(HitInfo HitInfo)
	{
		if (!Holed && Counter.CanContinue())
		{
			RigidBody.AddForce(HitInfo.force * 0.5f, ForceMode.VelocityChange);
			Audio[1].volume = 1f;
			OnPush();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Timer == -1f)
		{
			Timer = Time.time;
		}
		if (!(Time.time - Timer < 0.25f) && !(collision.collider.transform.parent == base.transform.parent) && !(collision.gameObject.tag == "Player") && !(collision.gameObject.tag == "Amigo"))
		{
			Timer = Time.time;
			CollideAudio.clip = CollideClips[Random.Range(0, CollideClips.Length)];
			CollideAudio.spatialBlend = 1f;
			CollideAudio.pitch = Random.Range(0.75f, 1f);
			CollideAudio.volume = Mathf.Min(RigidBody.velocity.magnitude / 10f, 1f) * 3f;
			CollideAudio.Play();
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			PlayerBase component = collision.gameObject.GetComponent<PlayerBase>();
			if (component.GetState() == "Dash")
			{
				component.SetMachineState("StateAir");
				OnPush();
			}
			if (component.GetState() != "PsychoSmash" && component.GetState() != "PsychicShot" && component.CurSpeed > 0f && Vector3.Dot(collision.gameObject.transform.forward, base.transform.position - collision.gameObject.transform.position) > 0f)
			{
				component.CurSpeed = 0f;
				component.FrontalCollision = true;
				RigidBody.velocity *= 0.75f;
			}
		}
		if (collision.gameObject.tag == "Amigo")
		{
			AmigoAIBase component2 = collision.gameObject.GetComponent<AmigoAIBase>();
			if (component2.CurSpeed > 0f && Vector3.Dot(collision.gameObject.transform.forward, base.transform.position - collision.gameObject.transform.position) > 0f)
			{
				component2.CurSpeed = 0f;
				component2.FrontalCollision = true;
				RigidBody.velocity *= 0.75f;
			}
		}
		PhysicsObj component3 = collision.collider.GetComponent<PhysicsObj>();
		if (!component3 || !component3.IsPsychokinesis)
		{
			return;
		}
		bool flag = false;
		if ((bool)component3.brokenPrefab)
		{
			component3.OnHit(new HitInfo(base.transform, -component3.RigidBody.velocity.normalized * (component3.RigidBody.velocity.magnitude * 0.5f), 10));
			flag = true;
		}
		else
		{
			if (PushTimer == -1f)
			{
				PushTimer = Time.time;
			}
			if (Time.time - PushTimer < 0.125f || collision.collider.transform.parent == base.transform.parent)
			{
				return;
			}
			PushTimer = Time.time;
			flag = true;
		}
		if (flag)
		{
			OnPush();
			RigidBody.velocity *= 0.5f;
		}
	}
}
