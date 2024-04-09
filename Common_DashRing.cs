using UnityEngine;

public class Common_DashRing : ObjectBase
{
	
	public float Speed;

	public float Timer;

	[Header("Prefab")]
	public AudioSource Audio;

	public ParticleSystem DashRingFX;

	public Material GlowMaterial;

	public Gradient GlowGradient;

	private PlayerManager PM;

	private Vector3 LaunchVelocity;

	private Vector3 StartLaunchVelocity;

	private Quaternion MeshLaunchRot;

	private float StartTime;

	private bool Falling;

	public void SetParameters(float _Speed, float _Timer)
	{
		Speed = _Speed;
		Timer = _Timer;
	}

	private void Update()
	{
		GlowMaterial.SetColor("_Color", GlowGradient.Evaluate(Mathf.Repeat(Time.time, 1f) / 1f));
	}

	private void StateDashRingStart()
	{
		PM.Base.SetState("DashRing");
		StartTime = Time.time;
		LaunchVelocity = base.transform.forward * Speed;
		StartLaunchVelocity = LaunchVelocity.normalized;
		PM.transform.position = base.transform.GetChild(0).position - base.transform.GetChild(0).up * 0.25f;
		float num = Vector3.Distance(PM.transform.forward.normalized, LaunchVelocity.MakePlanar().normalized);
		float num2 = Vector3.Dot(PM.transform.right.MakePlanar(), LaunchVelocity.MakePlanar());
		MeshLaunchRot = Quaternion.LookRotation(StartLaunchVelocity) * Quaternion.Euler(30f, 0f, (num - ((num > 1.75f) ? num : 0f)) * ((num2 > 0f) ? 40f : (-40f)));
		PM.transform.forward = LaunchVelocity.MakePlanar();
		PM.Base.MaxRayLenght = (PM.Base.GetPrefab("sonic_fast") ? 1.75f : 0.75f);
		Falling = false;
	}

	private void StateDashRing()
	{
		PM.Base.SetState("DashRing");
		bool flag = Time.time - StartTime < Timer;
		if (flag)
		{
			PM.Base.PlayAnimation("Spring Jump", "On Spring");
			Falling = false;
		}
		else if (PM.RBody.velocity.y > -0.1f)
		{
			PM.Base.PlayAnimation("Spring Jump", "On Spring");
			Falling = false;
		}
		else if (!Falling)
		{
			Falling = true;
			PM.Base.PlayAnimation("Roll And Fall", "On Roll And Fall");
		}
		if (flag)
		{
			MeshLaunchRot = Quaternion.Slerp(MeshLaunchRot, Quaternion.LookRotation(StartLaunchVelocity) * Quaternion.Euler(90f, 0f, 0f), Time.fixedDeltaTime * 5f);
			PM.RBody.velocity = LaunchVelocity;
			PM.transform.forward = LaunchVelocity.MakePlanar();
			PM.Base.CurSpeed = PM.RBody.velocity.magnitude;
			PM.Base.LockControls = true;
		}
		else
		{
			PM.Base.CurSpeed = PM.RBody.velocity.magnitude;
			PM.transform.forward = LaunchVelocity.MakePlanar();
			LaunchVelocity.y -= 9.81f * Time.fixedDeltaTime;
			PM.Base.LockControls = true;
			MeshLaunchRot = Quaternion.Slerp(MeshLaunchRot, (PM.RBody.velocity.y < 0f) ? Quaternion.LookRotation(LaunchVelocity.MakePlanar()) : (Quaternion.LookRotation(StartLaunchVelocity) * Quaternion.Euler(90f, 0f, 0f)), Time.fixedDeltaTime * 5f);
			PM.RBody.velocity = LaunchVelocity;
		}
		if (PM.Base.LockControls)
		{
			PM.Base.GeneralMeshRotation = MeshLaunchRot;
			PM.Base.TargetDirection = Vector3.zero;
		}
		if (PM.Base.IsGrounded() && !flag)
		{
			PM.Base.PositionToPoint();
			PM.Base.SetMachineState("StateGround");
			PM.Base.DoLandAnim();
			PM.PlayerEvents.CreateLandFXAndSound();
		}
	}

	private void StateDashRingEnd()
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !(player.GetState() == "Vehicle") && !player.IsDead)
		{
			DashRingFX.Play();
			Audio.Play();
			PM = collider.GetComponent<PlayerManager>();
			player.StateMachine.ChangeState(base.gameObject, StateDashRing);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 vector = base.transform.GetChild(0).position + base.transform.GetChild(0).forward * Speed * Timer;
		Gizmos.color = Color.white;
		Gizmos.DrawLine(base.transform.GetChild(0).position, vector);
		int num = 4 * (int)Speed;
		Vector3 vector2 = base.transform.GetChild(0).forward * Speed;
		Gizmos.color = Color.green;
		Vector3 vector3 = vector;
		Vector3 from = vector;
		_ = Vector3.zero;
		for (int i = 0; i < num; i++)
		{
			vector2.y -= 9.81f * Time.fixedDeltaTime;
			vector3 += vector2 * Time.fixedDeltaTime;
			_ = vector2.normalized;
			Gizmos.DrawLine(from, vector3);
			from = vector3;
		}
	}
}
