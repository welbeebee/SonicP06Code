
using UnityEngine;

public class ChainJump : ObjectBase
{
	public enum Mode
	{
		SetVector,
		GameObject
	}

	public Mode TargetMode;

	public bool JumpSplinter;

	public bool ReleaseOnLand;

	public GameObject LandTarget;

	public AudioClip JumpSound;

	public AudioSource Jump123Source;

	public AnimationCurve CamYOffset;

	public Vector3 LandPosition;

	private PlayerManager PM;

	private Vector3 PlayerPosition;

	private bool PlayJumpSounds;

	private float StartTime;

	private float JumpStartTime;

	private float Distance;

	private float Pitch;

	private int JumpState;

	private float Yaw;

	public void SetParameters(Vector3 _LandPosition)
	{
		LandPosition = _LandPosition;
	}

	private void Update()
	{
		Jump123Source.volume = Mathf.Lerp(Jump123Source.volume, 0f, Time.deltaTime);
		if (!PM || !(PM.Base.GetState() == "ChainJump") || Singleton<GameManager>.Instance.GameState == GameManager.State.Paused)
		{
			return;
		}
		if (JumpState == 0 && HasTarget())
		{
			if (Time.time - StartTime > Common_Lua.c_input_cd && Time.time - StartTime < 1.15f && Singleton<RInput>.Instance.P.GetButtonDown("Button A") && JumpState == 0)
			{
				PM.Base.Audio.PlayOneShot(JumpSound, PM.Base.Audio.volume);
				PM.Base.PlayerVoice.PlayRandom(1, RandomPlayChance: true);
				PM.Base.PlayAnimation((!JumpSplinter) ? "Chain Jump Wall" : "Chain Jump", (!JumpSplinter) ? "On Chain Jump Wall" : "On Chain Jump");
				JumpState = 1;
				JumpStartTime = Time.time;
				Pitch = 60f;
				Vector3 direction = (((TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position) - ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.1f))) * JumpSpeed();
				float num = Vector3.Distance(base.transform.forward.normalized, direction.MakePlanar().normalized);
				float num2 = Vector3.Dot(base.transform.right.MakePlanar(), direction.MakePlanar());
				Yaw = (num - ((num > 1.75f) ? num : 0f)) * ((num2 > 0f) ? 40f : (-40f));
				Jump123Source.Play();
				Jump123Source.volume = 1.5f;
			}
			if (!JumpSplinter && Singleton<RInput>.Instance.P.GetButtonDown("Button X"))
			{
				PM.Base.SetMachineState("StateAir");
				PM.Base.CurSpeed = 0f;
				PM.transform.forward = (HasTarget() ? base.transform.forward : (-base.transform.up));
			}
		}
		if (JumpState == 1)
		{
			PM.Base.CameraTarget.position += new Vector3(0f, CamYOffset.Evaluate(Time.time - JumpStartTime) * PM.Base.CamTargetFactor, 0f);
		}
	}

	private float JumpSpeed()
	{
		if (JumpSplinter && TargetMode == Mode.GameObject)
		{
			return 12.5f;
		}
		if (PM.Base.GetPrefab("sonic_fast"))
		{
			return 85f;
		}
		return 60f;
	}

	private void StateChainJumpStart()
	{
		PM.Base.SetState("ChainJump");
		if (JumpSplinter)
		{
			PM.Base.PlayAnimation("Chain Jump Wait", "On Chain Jump Wait");
			PM.transform.SetParent(base.transform);
		}
		else
		{
			PM.Base.PlayAnimation(HasTarget() ? "Chain Jump Wall Wait" : "Chain Jump Wait", HasTarget() ? "On Chain Jump Wall Wait" : "On Chain Jump Wait");
		}
		PM.transform.position = ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.35f));
		PM.Base.GeneralMeshRotation = Quaternion.LookRotation(base.transform.forward, HasTarget() ? Vector3.up : base.transform.up) * Quaternion.Euler(90f, 0f, 0f);
		StartTime = Time.time;
		JumpStartTime = Time.time;
		Distance = ((TargetMode == Mode.SetVector) ? (LandPosition - ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.35f))) : (LandTarget.transform.position - ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.25f)))).magnitude;
		JumpState = 0;
	}

	private void StateChainJump()
	{
		PM.Base.SetState("ChainJump");
		PM.Base.LockControls = true;
		PM.RBody.velocity = Vector3.zero;
		float num = Time.time - StartTime;
		if (JumpState == 0)
		{
			JumpStartTime = Time.time;
			PM.transform.up = ((!JumpSplinter) ? Vector3.up : base.transform.forward);
			if (JumpSplinter)
			{
				PM.Base.PlayAnimation("Chain Jump Wait", "On Chain Jump Wait");
			}
			PM.transform.position = ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.35f));
			PM.Base.GeneralMeshRotation = Quaternion.LookRotation(base.transform.forward, HasTarget() ? Vector3.up : base.transform.up) * Quaternion.Euler(90f, 0f, 0f);
			PM.Base.CurSpeed = 0f;
			if (!HasTarget() && PM.Base.IsGrounded())
			{
				PM.Base.PositionToPoint();
			}
			if (num > (HasTarget() ? 1.15f : ((!PM.Base.GetPrefab("sonic_fast")) ? 0.25f : 0.5f)))
			{
				if (JumpSplinter)
				{
					PM.transform.SetParent(null);
				}
				if (!HasTarget())
				{
					PM.Base.CurSpeed = ((!PM.Base.GetPrefab("sonic_fast")) ? 0f : Sonic_Fast_Lua.c_walk_speed_max);
				}
				PM.transform.forward = (HasTarget() ? base.transform.forward : (-base.transform.up));
				PM.Base.SetMachineState(HasTarget() ? "StateAir" : "StateGround");
			}
		}
		else
		{
			if (JumpSplinter && PM.transform.parent != null)
			{
				PM.transform.SetParent(null);
			}
			PM.Base.CurSpeed = JumpSpeed();
			float t = (Time.time - JumpStartTime) / Distance * JumpSpeed();
			float num2 = Time.time - JumpStartTime;
			Pitch = Mathf.Lerp(Pitch, (num2 > ((!PM.Base.GetPrefab("sonic_fast")) ? 0.16f : 0.3f)) ? (-90f) : 90f, Time.fixedDeltaTime * 7.5f);
			Yaw = Mathf.Lerp(Yaw, 0f, Time.fixedDeltaTime * 5f);
			PM.transform.position = Vector3.Lerp((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.35f), (TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position, t);
			PM.transform.forward = PM.Base.Mesh.forward.MakePlanar();
			PM.Base.GeneralMeshRotation = Quaternion.LookRotation(((TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position) - ((!JumpSplinter) ? PlayerPosition : (base.transform.position + base.transform.forward * 0.1f))) * Quaternion.Euler(Pitch, 0f, Yaw);
		}
	}

	private void StateChainJumpEnd()
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !player.IsDead)
		{
			PM = collider.GetComponent<PlayerManager>();
			PlayerPosition = base.transform.position + Vector3.up * 0.25f;
			if (Physics.Raycast(PlayerPosition + base.transform.forward * 0.5f, -base.transform.forward, out var hitInfo, 4f))
			{
				PlayerPosition = hitInfo.point + hitInfo.normal * (HasTarget() ? 0.5f : 0f);
				PM.PlayerEvents.CreateLandFXAndSoundCustomRot(hitInfo.point, Quaternion.LookRotation(base.transform.forward, HasTarget() ? Vector3.up : base.transform.up) * Quaternion.Euler(90f, 0f, 0f), hitInfo.transform.tag);
			}
			if (ReleaseOnLand)
			{
				player.transform.position = PlayerPosition;
				player.transform.forward = -base.transform.up;
				player.SetMachineState("StateGround");
				PM.Base.CurSpeed = ((!PM.Base.GetPrefab("sonic_fast")) ? 0f : Sonic_Fast_Lua.c_walk_speed_max);
			}
			else if (player.GetState() != "Result" && player.GetState() != "Grinding" && ((!HasTarget() && player.GetState() == "ChainJump") || HasTarget()))
			{
				player.StateMachine.ChangeState(base.gameObject, StateChainJump);
			}
		}
	}

	private bool HasTarget()
	{
		if (TargetMode == Mode.SetVector)
		{
			return LandPosition != Vector3.zero;
		}
		return LandTarget != null;
	}

	private void OnDrawGizmosSelected()
	{
		if (HasTarget())
		{
			Gizmos.DrawLine(base.transform.position, (TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube((TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position, new Vector3(1.5f, 1.5f, 1.5f));
			Gizmos.DrawWireSphere((TargetMode == Mode.SetVector) ? LandPosition : LandTarget.transform.position, 0.1f);
		}
	}
}
