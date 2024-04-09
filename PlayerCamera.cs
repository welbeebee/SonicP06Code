using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	public enum State
	{
		Normal,
		Talk,
		FirstPerson,
		OverTheShoulder,
		OverTheShoulderFadeIn,
		Cinematic,
		ObjectEvent,
		EventFadeIn,
		EventBFadeIn,
		EventCFadeIn,
		Event,
		EventFadeOut,
		EventDFadeOut,
		Death
	}

	
	public StateMachine StateMachine;

	public Camera Camera;

	public Camera SkyboxCamera;

	public Camera OutlineCamera;

	public CameraEffects CameraEffects;

	public Animator Animator;

	public LayerMask layerMask;

	private const float RetailFov = 45f;

	private const float E3Fov = 55f;

	internal Vector3 WorldVelocity = Vector3.zero;

	internal Vector3 LastPosition = Vector3.zero;

	internal StageManager StageManager;

	internal CameraParameters parameters;

	internal PlayerBase PlayerBase;

	internal State CameraState;

	internal Transform Target;

	internal bool TrailerCamera;

	internal bool IsMachSpeed;

	internal bool MultDistance;

	internal bool IsOnEvent;

	internal bool UncancelableEvent;

	internal bool CanCancelCinematic;

	internal float Distance;

	internal float DistanceToTarget;

	internal float CinematicTime;

	internal float ObjectEventTime;

	private Vector3 TrailerRot;

	private Vector3 TrailerRotOffset;

	private bool HideTrailerCamControls;

	private bool HasObjPosition;

	private bool HasObjTarget;

	private float TrailerDist;

	private float TrailerPosYOffset;

	private float SmoothDistFactor;

	private float EventFadeTime;

	private float LeftStickX;

	private float LeftStickY;

	private float RightStickX;

	private float RightStickY;

	private float TalkCamHeight;

	private float TalkCamDist;

	private float RotYAxis;

	private float RotXAxis;

	private float CineTime;

	private float ObjEvtTime;

	private bool IsCamPush;

	private void Start()
	{
		Camera.fieldOfView = FovY();
		SkyboxCamera.fieldOfView = Camera.fieldOfView;
		OutlineCamera.fieldOfView = Camera.fieldOfView;
		TrailerDist = 3f;
		StateMachine.Initialize(StateNormal);
		Invoke("CameraReset", 0.01f);
	}

	private float SetDistance()
	{
		float result = 0f;
		if (Singleton<GameManager>.Instance.GameStory == GameManager.Story.Sonic)
		{
			if (StageManager._Stage == StageManager.Stage.wvo && StageManager.StageSection == StageManager.Section.B)
			{
				result = 3.5f;
				IsMachSpeed = true;
			}
			else if (StageManager._Stage == StageManager.Stage.csc && StageManager.StageSection == StageManager.Section.E)
			{
				result = 1.25f;
				IsMachSpeed = true;
			}
			else if (StageManager._Stage == StageManager.Stage.rct && StageManager.StageSection == StageManager.Section.B)
			{
				result = 6.5f;
				IsMachSpeed = true;
			}
			else if (StageManager._Stage == StageManager.Stage.kdv && StageManager.StageSection == StageManager.Section.C)
			{
				result = 2.5f;
				IsMachSpeed = true;
			}
		}
		if (!IsMachSpeed)
		{
			result = ((Singleton<Settings>.Instance.settings.CameraType == 0) ? 6.5f : 4.5f);
		}
		return result;
	}

	private float FovY()
	{
		if (Singleton<Settings>.Instance.settings.FieldOfView != 0)
		{
			return 55f;
		}
		return 45f;
	}

	private float SpringK()
	{
		if (Singleton<Settings>.Instance.settings.CameraType != 0)
		{
			if (!PlayerBase.GetPrefab("sonic_fast"))
			{
				return 0.17f;
			}
			return 0.34f;
		}
		return 0.98f;
	}

	private float Altitude()
	{
		float result = 0f;
		if (Singleton<GameManager>.Instance.GameStory == GameManager.Story.Sonic)
		{
			if (StageManager._Stage == StageManager.Stage.wvo && StageManager.StageSection == StageManager.Section.B)
			{
				result = 2.5f;
			}
			else if (StageManager._Stage == StageManager.Stage.csc && StageManager.StageSection == StageManager.Section.E)
			{
				result = 0.875f;
			}
			else if (StageManager._Stage == StageManager.Stage.rct && StageManager.StageSection == StageManager.Section.B)
			{
				result = 4.5f;
			}
			else if (StageManager._Stage == StageManager.Stage.kdv && StageManager.StageSection == StageManager.Section.C)
			{
				result = 1.75f;
			}
		}
		if (!IsMachSpeed)
		{
			result = ((Singleton<Settings>.Instance.settings.CameraType == 0) ? 4f : 3f);
		}
		return result;
	}

	public float CameraDistace()
	{
		return SetDistance() * (MultDistance ? 2f : 1f);
	}

	private float RawStickX()
	{
		return Singleton<RInput>.Instance.P.GetAxis("Right Stick X") * (float)((Singleton<Settings>.Instance.settings.InvertCamX == 1 && CameraState != State.OverTheShoulderFadeIn && CameraState != State.OverTheShoulder) ? 1 : (-1));
	}

	private float RawStickY()
	{
		return Singleton<RInput>.Instance.P.GetAxis("Right Stick Y") * (float)((Singleton<Settings>.Instance.settings.InvertCamY == 1 || CameraState == State.OverTheShoulderFadeIn || CameraState == State.OverTheShoulder) ? 1 : (-1));
	}

	private void StateNormalStart()
	{
	}

	public void StateNormal()
	{
		CameraState = State.Normal;
		CameraNormalUpdate();
	}

	private void StateNormalEnd()
	{
	}

	private void StateTalkStart()
	{
		TalkCamHeight = (PlayerBase.GetPrefab("tails") ? 0.375f : (PlayerBase.GetPrefab("omega") ? 0.95f : 0.45f));
		TalkCamDist = (PlayerBase.GetPrefab("omega") ? 1.5f : 1f);
	}

	public void StateTalk()
	{
		CameraState = State.Talk;
		base.transform.forward = -PlayerBase.transform.forward;
		base.transform.position = PlayerBase.transform.position + PlayerBase.transform.up * TalkCamHeight - -PlayerBase.transform.forward * TalkCamDist;
	}

	private void StateTalkEnd()
	{
	}

	private void StateFirstPersonStart()
	{
		base.transform.position = Target.position - base.transform.forward.MakePlanar() * 2f;
	}

	public void StateFirstPerson()
	{
		CameraState = State.FirstPerson;
		Vector3 zero = Vector3.zero;
		Vector3 normalized = (Target.position - base.transform.position).normalized;
		base.transform.RotateAround(Target.position, Vector3.up, Time.fixedDeltaTime * LeftStickX * 80f);
		base.transform.RotateAround(Target.position, base.transform.right, Time.fixedDeltaTime * LeftStickY * 80f);
		DistanceToTarget = Vector3.Distance(base.transform.position, Target.position);
		if (DistanceToTarget > 1.5f)
		{
			zero += normalized * (DistanceToTarget - 1.5f);
		}
		else if (DistanceToTarget < 1.5f)
		{
			zero -= normalized * (1.5f - DistanceToTarget);
		}
		float num = Target.position.y - base.transform.position.y;
		float num2 = num + 1f;
		float num3 = num - 1f;
		if (num2 < 0f)
		{
			zero += Target.up * num2;
		}
		else if (num3 > 0f)
		{
			zero += Target.up * num3;
		}
		base.transform.position += zero;
		base.transform.forward = (Target.position - base.transform.position).normalized;
	}

	private void StateFirstPersonEnd()
	{
	}

	private void StateOverTheShoulderStart()
	{
		base.transform.position = Target.position - base.transform.forward.MakePlanar() * 1f;
		RotYAxis = base.transform.eulerAngles.y;
		RotXAxis = base.transform.eulerAngles.x;
	}

	private void StateOverTheShoulder()
	{
		CameraState = State.OverTheShoulder;
		RotYAxis += Time.fixedDeltaTime * (0f - RightStickX) * 70f;
		RotXAxis += Time.fixedDeltaTime * (0f - RightStickY) * 70f;
		RotXAxis = ClampAngle(RotXAxis, -40f, 40f);
		Quaternion rotation = Quaternion.Euler(RotXAxis, RotYAxis, 0f);
		base.transform.rotation = rotation;
		base.transform.position = Target.position - base.transform.forward * 1f;
	}

	private void StateOverTheShoulderEnd()
	{
	}

	private float ClampAngle(float Angle, float Min, float Max)
	{
		if (Angle < -360f)
		{
			Angle += 360f;
		}
		if (Angle > 360f)
		{
			Angle -= 360f;
		}
		return Mathf.Clamp(Angle, Min, Max);
	}

	private void StateOverTheShoulderFadeInStart()
	{
		EventFadeTime = Time.time;
	}

	public void StateOverTheShoulderFadeIn()
	{
		CameraState = State.OverTheShoulderFadeIn;
		float num = (Time.time - EventFadeTime) * 0.75f;
		num *= num;
		Vector3 normalized = (Target.position - base.transform.position).normalized;
		Vector3 forward = Vector3.Slerp(base.transform.forward, normalized, num);
		forward.y = Mathf.Lerp(base.transform.forward.y, normalized.y, num);
		base.transform.forward = forward;
		base.transform.position = Vector3.Lerp(base.transform.position, Target.position - base.transform.forward.MakePlanar() * 1f, num);
		if (num > 0.5f)
		{
			StateMachine.ChangeState(StateOverTheShoulder);
		}
	}

	private void StateOverTheShoulderFadeInEnd()
	{
	}

	private void StateCinematicStart()
	{
		CineTime = Time.time;
		CinematicLogic();
	}

	private void StateCinematic()
	{
		CameraState = State.Cinematic;
		CinematicLogic();
		if (Time.time - CineTime > CinematicTime)
		{
			StateMachine.ChangeState(StateEventFadeOut);
		}
	}

	private void StateCinematicEnd()
	{
		Animator.SetTrigger("State Ended");
	}

	private void CinematicLogic()
	{
		base.transform.forward = PlayerBase.transform.forward;
		base.transform.position = PlayerBase.transform.position + PlayerBase.transform.up * 0.85f - PlayerBase.transform.forward * 6.5f;
	}

	private void StateObjectEventStart()
	{
		ObjEvtTime = Time.time;
		ObjectEventLogic();
	}

	private void StateObjectEvent()
	{
		CameraState = State.ObjectEvent;
		ObjectEventLogic();
		if (Time.time - ObjEvtTime > ObjectEventTime)
		{
			StateMachine.ChangeState(StateNormal);
			if (!TrailerCamera)
			{
				Invoke("CameraReset", 0.01f);
				return;
			}
			TrailerDist = 3f;
			TrailerRot = Vector3.zero;
			TrailerRotOffset = Vector3.zero;
			TrailerPosYOffset = 0f;
		}
	}

	private void StateObjectEventEnd()
	{
		Animator.SetTrigger("State Ended");
	}

	private void ObjectEventLogic()
	{
		base.transform.forward = (parameters.Target - parameters.Target).normalized;
		base.transform.position = parameters.Position;
	}

	private void StateEventFadeInStart()
	{
		EventFadeTime = Time.time;
	}

	private void StateEventFadeIn()
	{
		CameraState = State.EventFadeIn;
		float num = (Time.time - EventFadeTime) * 1.5f;
		num = Mathf.Clamp01(num * num);
		Vector3 normalized = (((!HasObjTarget) ? parameters.Position : parameters.ObjTarget.position) - base.transform.position).normalized;
		Vector3 forward = Vector3.Slerp(base.transform.forward, normalized, num);
		forward.y = Mathf.Lerp(base.transform.forward.y, normalized.y, num);
		base.transform.forward = forward;
		if (parameters.Mode == 11)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, Target.position - base.transform.forward * Distance + Vector3.up * Distance / 7.5f, num);
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, Target.position - base.transform.forward * Distance, num);
		}
		if (num >= 1f)
		{
			StateMachine.ChangeState(StateEvent);
		}
	}

	private void StateEventFadeInEnd()
	{
	}

	private void StateEventBFadeInStart()
	{
		if ((!HasObjPosition && !HasObjTarget) || parameters.Mode == 32)
		{
			CameraState = State.EventBFadeIn;
		}
		EventFadeTime = Time.time;
	}

	private void StateEventBFadeIn()
	{
		float num = (Time.time - EventFadeTime) * 1.5f;
		num = Mathf.Clamp01(num * num);
		Vector3 normalized = (((!HasObjTarget) ? parameters.Target : parameters.ObjTarget.position) - base.transform.position).normalized;
		Vector3 forward = Vector3.Slerp(base.transform.forward, normalized, num);
		forward.y = Mathf.Lerp(base.transform.forward.y, normalized.y, num);
		base.transform.forward = forward;
		base.transform.position = Vector3.Lerp(base.transform.position, (!HasObjPosition) ? parameters.Position : parameters.ObjPosition.position, num);
		if (num >= 1f)
		{
			StateMachine.ChangeState(StateEvent);
		}
	}

	private void StateEventBFadeInEnd()
	{
	}

	private void StateEventCFadeInStart()
	{
		EventFadeTime = Time.time;
	}

	private void StateEventCFadeIn()
	{
		CameraState = State.EventCFadeIn;
		float num = (Time.time - EventFadeTime) * 1.5f;
		num = Mathf.Clamp01(num * num);
		Vector3 normalized = (parameters.Position - base.transform.position).normalized;
		Vector3 forward = Vector3.Slerp(base.transform.forward, normalized, num);
		forward.y = Mathf.Lerp(base.transform.forward.y, normalized.y, num);
		base.transform.forward = forward;
		base.transform.position = Vector3.Lerp(base.transform.position, Target.position - base.transform.forward * Distance * (PlayerBase.GetPrefab("sonic_fast") ? 4f : 1f), num);
		if (num >= 1f)
		{
			StateMachine.ChangeState(StateEvent);
		}
	}

	private void StateEventCFadeInEnd()
	{
	}

	private void StateEventStart()
	{
		EventLogic();
	}

	private void StateEvent()
	{
		CameraState = State.Event;
		EventLogic();
	}

	private void StateEventEnd()
	{
	}

	private void EventLogic()
	{
		if (parameters.Mode == 1 || parameters.Mode == 10 || parameters.Mode == 11)
		{
			if (parameters.Mode == 11)
			{
				DistanceToTarget = Vector3.Distance(base.transform.position, Target.position + Vector3.up * Distance / 7.5f);
				base.transform.position = Target.position - base.transform.forward * Distance + Vector3.up * Distance / 7.5f;
			}
			else
			{
				DistanceToTarget = Vector3.Distance(base.transform.position, Target.position);
				base.transform.position = Target.position - base.transform.forward * Distance;
			}
			if (HasObjTarget)
			{
				base.transform.forward = (parameters.ObjTarget.position - base.transform.position).normalized;
			}
			else
			{
				base.transform.forward = (parameters.Position - base.transform.position).normalized;
			}
		}
		else if ((parameters.Mode == 3 || parameters.Mode == 30 || parameters.Mode == 31) && (HasObjPosition || HasObjTarget))
		{
			base.transform.position = (HasObjPosition ? parameters.ObjPosition.position : parameters.Position);
			base.transform.forward = ((HasObjTarget ? parameters.ObjTarget.position : parameters.Target) - base.transform.position).normalized;
		}
		else if ((parameters.Mode == 4 || parameters.Mode == 41 || parameters.Mode == 42) && HasObjPosition)
		{
			base.transform.position = parameters.ObjPosition.position;
			base.transform.forward = (Target.position - base.transform.position).normalized;
		}
		else if (parameters.Mode == 5 || parameters.Mode == 50)
		{
			DistanceToTarget = Vector3.Distance(base.transform.position, Target.position);
			base.transform.forward = (parameters.Position - base.transform.position).normalized;
		}
	}

	public Vector3 ProjectPositionOnRail(Vector3 Pos)
	{
		return ProjectOnSegment(parameters.Position, parameters.Target, Pos);
	}

	public Vector3 ProjectOnSegment(Vector3 V1, Vector3 V2, Vector3 Pos)
	{
		Vector3 rhs = Pos - V1;
		Vector3 normalized = (V2 - V1).normalized;
		float num = Vector3.Dot(normalized, rhs);
		if (num < 0f)
		{
			return V1;
		}
		if (num * num > (V2 - V1).sqrMagnitude)
		{
			return V2;
		}
		Vector3 vector = normalized * num;
		return V1 + vector;
	}

	private void StateEventFadeOutStart()
	{
		if (TrailerCamera)
		{
			StateMachine.ChangeState(StateNormal);
		}
		EventFadeTime = Time.time;
	}

	public void StateEventFadeOut()
	{
		CameraState = State.EventFadeOut;
		float num = (Time.time - EventFadeTime) * 1.5f;
		num = Mathf.Clamp01(num * num);
		CameraNormalUpdate(num);
		if (num >= 1f)
		{
			StateMachine.ChangeState(StateNormal);
		}
	}

	private void StateEventFadeOutEnd()
	{
	}

	private void StateEventDFadeOutStart()
	{
		if (TrailerCamera)
		{
			StateMachine.ChangeState(StateNormal);
		}
		EventFadeTime = Time.time;
	}

	public void StateEventDFadeOut()
	{
		CameraState = State.EventDFadeOut;
		float num = (Time.time - EventFadeTime) * 1.5f;
		num = Mathf.Clamp01(num * num);
		CameraNormalUpdate(num);
		if (num >= 1f)
		{
			StateMachine.ChangeState(StateNormal);
		}
	}

	private void StateEventDFadeOutEnd()
	{
	}

	private void StateDeathStart()
	{
	}

	private void StateDeath()
	{
		CameraState = State.Death;
	}

	private void StateDeathEnd()
	{
	}

	private void CameraCollision(Vector3 Dir, float Dist)
	{
		int num = 0;
		RaycastHit hitInfo;
		while (Physics.SphereCast(Target.position, 0.25f, Dir, out hitInfo, Dist, layerMask) && num < 50 && !IsCamPush)
		{
			Vector3 normalized = Vector3.Cross(Vector3.Cross(Dir, -hitInfo.normal).normalized, hitInfo.normal).normalized;
			Vector3 normalized2 = Vector3.Cross(Vector3.Cross(Dir, -normalized).normalized, base.transform.forward).normalized;
			base.transform.position += normalized2 * Time.fixedDeltaTime;
			Dir = (base.transform.position - Target.position).normalized;
			num++;
		}
		CameraPush(Dir, Dist);
	}

	private bool CameraPush(Vector3 Dir, float Dist)
	{
		if (Physics.SphereCast(Target.position, 0.25f, Dir, out var hitInfo, Dist, layerMask))
		{
			if (hitInfo.distance < Dist)
			{
				base.transform.position += -Dir * (Dist - hitInfo.distance);
				return true;
			}
		}
		else if (IsCamPush)
		{
			IsCamPush = false;
		}
		return false;
	}

	public void CameraReset()
	{
		if (!TrailerCamera)
		{
			if (!MultDistance)
			{
				base.transform.position = Target.position - PlayerBase.transform.forward * Distance;
			}
			else
			{
				base.transform.position = Target.position - PlayerBase.transform.forward * Distance + base.transform.up * 4f;
			}
			if (Target.position.y - base.transform.position.y > 0f)
			{
				Vector3 position = base.transform.position;
				position.y = (Target.position - PlayerBase.transform.forward.MakePlanar() * Distance).y;
				base.transform.position = position;
			}
			base.transform.forward = (Target.position - base.transform.position).normalized;
			if (CameraPush((base.transform.position - Target.position).normalized, Vector3.Distance(Target.position, base.transform.position)))
			{
				IsCamPush = true;
			}
		}
		else
		{
			TrailerDist = 3f;
			base.transform.position = PlayerBase.transform.position + Vector3.up * 0.3f - PlayerBase.transform.forward * TrailerDist;
			base.transform.forward = PlayerBase.transform.forward;
			TrailerRot = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			TrailerRotOffset = Vector3.zero;
			TrailerPosYOffset = 0f;
		}
	}

	private void CameraNormalUpdate(float Rate = 1f)
	{
		if (TrailerCamera)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 normalized = (Target.position - base.transform.position).normalized;
		if (!IsCamPush)
		{
			float num = 1.75f * Distance * Rate;
			zero += -Camera.transform.right * Time.fixedDeltaTime * RightStickX * num;
			bool flag = !MultDistance || (MultDistance && (IsMachSpeed || parameters.Mode == 102));
			float num2 = ((flag && RawStickY() > 0f) ? 2f : 1f);
			zero.y += Time.fixedDeltaTime * (flag ? RightStickY : 0.5f) * num2 * num;
			if (Singleton<RInput>.Instance.P.GetButton("Left Bumper"))
			{
				RightStickX = Mathf.Lerp(RightStickX, -1f * (float)((Singleton<Settings>.Instance.settings.InvertCamX == 1) ? 1 : (-1)), Time.fixedDeltaTime * num);
			}
			else if (Singleton<RInput>.Instance.P.GetButton("Right Bumper"))
			{
				RightStickX = Mathf.Lerp(RightStickX, 1f * (float)((Singleton<Settings>.Instance.settings.InvertCamX == 1) ? 1 : (-1)), Time.fixedDeltaTime * num);
			}
		}
		DistanceToTarget = Vector3.Distance(base.transform.position, Target.position);
		if (DistanceToTarget > Distance)
		{
			zero += normalized * (DistanceToTarget - Distance) * SpringK();
		}
		else if (DistanceToTarget < Distance)
		{
			zero -= normalized * (Distance - DistanceToTarget) * SpringK();
		}
		float num3 = Target.position.y - base.transform.position.y;
		float num4 = Altitude() + Mathf.SmoothStep(10f + Altitude(), 0f, Rate);
		float max = num3 + num4;
		float min = num3 - num4;
		SmoothDistFactor = Mathf.SmoothStep(SmoothDistFactor, (DistanceToTarget - Distance) * 2f, Time.fixedDeltaTime * 25f);
		bool flag2 = PlayerBase.GetState() == "AfterHoming" || PlayerBase.GetState() == "BoundAttack" || PlayerBase.GetState() == "Homing" || PlayerBase.GetState() == "Upheave" || PlayerBase.GetState() == "Fly" || PlayerBase.GetState() == "AerialTailSwipe" || PlayerBase.GetState() == "AirHammerSpin";
		if (num3 > 0f && (PlayerBase.CurSpeed > 0f || flag2))
		{
			float num5 = ((!flag2) ? SmoothDistFactor : ((Singleton<Settings>.Instance.settings.CameraType == 0) ? 0.5f : 1f)) * Rate;
			float num6 = Mathf.Lerp((Singleton<Settings>.Instance.settings.CameraType == 0) ? 0.5f : (PlayerBase.GetPrefab("sonic_fast") ? 0.2f : 0.1f), (Singleton<Settings>.Instance.settings.CameraType == 0) ? 20f : (PlayerBase.GetPrefab("sonic_fast") ? 5f : 2.5f), num5);
			zero.y = Mathf.Lerp(zero.y, num3, Time.fixedDeltaTime * num6 * num5 * Rate);
		}
		zero.y = Mathf.Clamp(zero.y, min, max);
		base.transform.position += zero * Rate;
	}

	public void SetParams(CameraParameters _Parameters)
	{
		if (CameraState == State.Death || CameraState == State.OverTheShoulder || (CameraState == State.Cinematic && !CanCancelCinematic) || UncancelableEvent)
		{
			return;
		}
		parameters = _Parameters;
		HasObjPosition = parameters.ObjPosition;
		HasObjTarget = parameters.ObjTarget;
		if (CanCancelCinematic)
		{
			Animator.SetTrigger("Reset Cinematic");
		}
		if (_Parameters.Mode == 1 || _Parameters.Mode == 11)
		{
			StateMachine.ChangeState(StateEventFadeIn);
			if (_Parameters.Mode == 11)
			{
				MultDistance = true;
			}
		}
		else if (_Parameters.Mode == 30 || _Parameters.Mode == 32)
		{
			StateMachine.ChangeState(StateEventBFadeIn);
		}
		else if (_Parameters.Mode == 10 || _Parameters.Mode == 2 || _Parameters.Mode == 3 || _Parameters.Mode == 31 || _Parameters.Mode == 4 || _Parameters.Mode == 41 || _Parameters.Mode == 42 || _Parameters.Mode == 5 || _Parameters.Mode == 100 || _Parameters.Mode == 104)
		{
			StateMachine.ChangeState(StateEvent);
		}
		else if (_Parameters.Mode == 50)
		{
			StateMachine.ChangeState(StateEventCFadeIn);
		}
		else if (_Parameters.Mode == 101 || _Parameters.Mode == 102 || _Parameters.Mode == 103)
		{
			if (CameraState == State.Talk || CameraState == State.FirstPerson || CameraState == State.OverTheShoulder || CameraState == State.OverTheShoulderFadeIn || CameraState == State.Cinematic || CameraState == State.ObjectEvent || CameraState == State.EventFadeIn || CameraState == State.EventBFadeIn || CameraState == State.EventCFadeIn || CameraState == State.Event)
			{
				StateMachine.ChangeState(StateEventFadeOut);
			}
			MultDistance = true;
		}
	}

	public void DestroyParams(CameraParameters _Parameters)
	{
		if (parameters == _Parameters && (CameraState == State.Event || CameraState == State.EventFadeIn) && !UncancelableEvent && CameraState != State.OverTheShoulder && CameraState != State.Cinematic)
		{
			if (_Parameters.Mode == 1 || _Parameters.Mode == 10 || _Parameters.Mode == 11 || _Parameters.Mode == 2 || _Parameters.Mode == 3 || _Parameters.Mode == 30 || _Parameters.Mode == 32 || _Parameters.Mode == 4 || _Parameters.Mode == 5 || _Parameters.Mode == 50 || _Parameters.Mode == 100 || _Parameters.Mode == 104)
			{
				StateMachine.ChangeState(StateEventFadeOut);
			}
			else if (_Parameters.Mode == 41 || _Parameters.Mode == 31)
			{
				StateMachine.ChangeState(StateNormal);
				Invoke("CameraReset", 0.01f);
			}
			else if (_Parameters.Mode == 42)
			{
				StateMachine.ChangeState(StateEventDFadeOut);
			}
		}
		if (_Parameters.Mode == 11 || _Parameters.Mode == 101 || _Parameters.Mode == 102 || _Parameters.Mode == 103)
		{
			MultDistance = false;
		}
	}

	public void OnPlayerDeath()
	{
		StateMachine.ChangeState(StateDeath);
	}

	public void OnPlayerTalkEnd()
	{
		StateMachine.ChangeState(StateNormal);
		if (!TrailerCamera)
		{
			base.transform.position = Target.position - PlayerBase.transform.forward * Distance;
			base.transform.forward = (Target.position - base.transform.position).normalized;
			return;
		}
		TrailerDist = 3f;
		TrailerRot = Vector3.zero;
		TrailerRotOffset = Vector3.zero;
		TrailerPosYOffset = 0f;
	}

	public void PlayCinematic(float Timer, string Animation)
	{
		CinematicTime = Timer;
		Animator.SetTrigger(Animation);
		StateMachine.ChangeState(StateCinematic);
	}

	public void PlayObjectEvent(float Timer, string Animation, CameraParameters _Parameters)
	{
		ObjectEventTime = Timer;
		Animator.SetTrigger(Animation);
		parameters = _Parameters;
		StateMachine.ChangeState(StateObjectEvent);
	}

	public void PlayShakeMotion(float Timer = 0f, float Intensity = 1f, bool FastShake = false)
	{
		StopCoroutine(TriggerShake(Timer, Intensity, FastShake));
		StartCoroutine(TriggerShake(Timer, Intensity, FastShake));
	}

	public void PlayImpactShakeMotion()
	{
		Animator.SetTrigger("Impact Shake");
	}

	private IEnumerator TriggerShake(float Timer, float Intensity, bool FastShake)
	{
		yield return new WaitForSeconds(Timer);
		Animator.SetFloat("Shake Intensity", Intensity);
		Animator.SetTrigger((!FastShake) ? "Shake" : "Fast Shake");
	}

	private void Update()
	{
		UpdateCameraForward();
		if (PlayerBase.GetPrefab("sonic_fast"))
		{
			Distance = Mathf.Lerp(Distance, CameraDistace() * ((!PlayerBase.PlayerManager.sonic_fast.UseSpeedBarrier) ? 1f : ((!MultDistance) ? 0.1f : 0.25f)) + 1.5f, Time.deltaTime * (PlayerBase.PlayerManager.sonic_fast.UseSpeedBarrier ? 1f : 2.5f));
		}
		else if (PlayerBase.GetPrefab("snow_board"))
		{
			Distance = Mathf.Lerp(Distance, CameraDistace() * ((!PlayerBase.IsGrounded() || CameraState != 0) ? 1f : 0.625f) + 1.5f, Time.deltaTime);
		}
		else if (PlayerBase.GetPrefab("shadow") || PlayerBase.GetPrefab("silver"))
		{
			Distance = Mathf.Lerp(Distance, CameraDistace() * ((PlayerBase.GetState() != "ChaosBoost" && PlayerBase.GetState() != "ESPAwaken") ? 1f : (PlayerBase.GetPrefab("shadow") ? 0.625f : 1.375f)), Time.deltaTime * 5f);
		}
		else
		{
			Distance = Mathf.Lerp(Distance, CameraDistace(), Time.deltaTime * 2.5f);
		}
		if (PlayerBase.GetPrefab("sonic_fast"))
		{
			Camera.fieldOfView = ((Singleton<GameManager>.Instance.GameState == GameManager.State.Result) ? 55f : ((!TrailerCamera) ? Mathf.Lerp(Camera.fieldOfView, FovY() + (PlayerBase.PlayerManager.sonic_fast.UseSpeedBarrier ? 20f : 0f), Time.deltaTime * (PlayerBase.PlayerManager.sonic_fast.UseSpeedBarrier ? 1f : 2.5f)) : FovY()));
			SkyboxCamera.fieldOfView = Camera.fieldOfView;
			OutlineCamera.fieldOfView = Camera.fieldOfView;
		}
		if (TrailerCamera && CameraState == State.Normal)
		{
			if (Singleton<RInput>.Instance.P.GetButton("Left Bumper"))
			{
				TrailerDist += Time.unscaledDeltaTime * 4f;
			}
			else if (Singleton<RInput>.Instance.P.GetButton("Right Bumper"))
			{
				TrailerDist -= Time.unscaledDeltaTime * 4f;
			}
			TrailerDist = Mathf.Clamp(TrailerDist, 0.75f, 20f);
			if (Singleton<GameManager>.Instance.GameState != GameManager.State.Paused || (Singleton<GameManager>.Instance.GameState == GameManager.State.Paused && !Singleton<RInput>.Instance.P.GetButton("Right Trigger")))
			{
				TrailerRot.y += Time.unscaledDeltaTime * RawStickX() * 90f;
				TrailerRot.x += Time.unscaledDeltaTime * (0f - RawStickY()) * 90f;
				TrailerRot.x = ClampAngle(TrailerRot.x, -45f, 45f);
			}
			else
			{
				TrailerPosYOffset += Time.unscaledDeltaTime * (0f - RawStickY()) * 5f;
			}
			if (Singleton<RInput>.Instance.P.GetButtonDown("Back"))
			{
				HideTrailerCamControls = !HideTrailerCamControls;
			}
			Quaternion rotation = Quaternion.Euler(TrailerRot.x, TrailerRot.y, TrailerRot.z);
			base.transform.rotation = rotation;
			base.transform.position = PlayerBase.transform.position + Vector3.up * 0.45f - base.transform.forward * TrailerDist + Vector3.up * TrailerPosYOffset;
		}
		if (PlayerBase.GetState() != "Tarzan" && CameraState == State.Event && parameters.Mode == 100)
		{
			UncancelableEvent = false;
			DestroyParams(parameters);
		}
	}

	private void LateUpdate()
	{
		if (!TrailerCamera || CameraState != 0)
		{
			return;
		}
		if (Singleton<GameManager>.Instance.GameState == GameManager.State.Paused)
		{
			float axis = Singleton<RInput>.Instance.P.GetAxis("Left Stick X");
			float num = 0f - Singleton<RInput>.Instance.P.GetAxis("Left Stick Y");
			TrailerRotOffset += Vector3.up * axis * 60f * Time.unscaledDeltaTime + Vector3.right * num * 60f * Time.unscaledDeltaTime;
			if (Singleton<RInput>.Instance.P.GetButton("Right Trigger"))
			{
				TrailerRotOffset += Vector3.forward * Time.unscaledDeltaTime * RawStickX() * 60f;
			}
		}
		Camera.transform.localEulerAngles += TrailerRotOffset;
	}

	private void FixedUpdate()
	{
		StateMachine.UpdateStateMachine();
		LeftStickX = Mathf.Lerp(LeftStickX, Singleton<RInput>.Instance.P.GetAxis("Left Stick X"), Time.fixedDeltaTime * 15f);
		LeftStickY = Mathf.Lerp(LeftStickY, 0f - Singleton<RInput>.Instance.P.GetAxis("Left Stick Y"), Time.fixedDeltaTime * 15f);
		if (!Singleton<RInput>.Instance.P.GetButton("Left Bumper") && !Singleton<RInput>.Instance.P.GetButton("Right Bumper"))
		{
			RightStickX = Mathf.Lerp(RightStickX, RawStickX(), Time.fixedDeltaTime * 10f);
		}
		RightStickY = Mathf.Lerp(RightStickY, RawStickY(), Time.fixedDeltaTime * 10f);
		if (!TrailerCamera)
		{
			if (CameraState == State.Normal || CameraState == State.EventFadeOut)
			{
				CameraCollision((base.transform.position - Target.position).normalized, Vector3.Distance(Target.position, base.transform.position));
			}
			else if ((CameraState == State.Event && (parameters.Mode == 1 || parameters.Mode == 10 || parameters.Mode == 11)) || CameraState == State.EventFadeIn)
			{
				CameraPush((base.transform.position - Target.position).normalized, Vector3.Distance(Target.position, base.transform.position));
			}
		}
		IsOnEvent = CameraState == State.Talk || CameraState == State.FirstPerson || CameraState == State.OverTheShoulder || CameraState == State.OverTheShoulderFadeIn || CameraState == State.Cinematic;
		WorldVelocity = (base.transform.position - LastPosition) / Time.fixedDeltaTime;
		LastPosition = base.transform.position;
	}

	private void OnGUI()
	{
		if (!TrailerCamera || HideTrailerCamControls || CameraState != 0)
		{
			return;
		}
		Vector2 vector = new Vector2(1280f, 720f) * 0.625f;
		GUI.matrix = Matrix4x4.TRS(s: new Vector3((float)Screen.width / vector.x, (float)Screen.height / vector.y, 1f), pos: new Vector3(0f, 0f, 0f), q: Quaternion.identity);
		GUI.Label(new Rect(15f, 180f, 500f, 20f), "Trailer Camera controls (Press 'Back' to hide)");
		GUI.Label(new Rect(15f, 195f, 500f, 20f), "(Right Stick has secondary controls on pause menu)");
		GUI.Label(new Rect(25f, 225f, 500f, 20f), "Left Stick: Change direction");
		GUI.Label(new Rect(25f, 240f, 500f, 20f), "Left/Right Bumpers: Change distance");
		GUI.Label(new Rect(25f, 255f, 500f, 20f), "Left Trigger: Reset camera");
		if (Singleton<GameManager>.Instance.GameState == GameManager.State.Paused)
		{
			if (Singleton<RInput>.Instance.P.GetButton("Right Trigger"))
			{
				GUI.Label(new Rect(15f, 285f, 500f, 20f), "Right Stick (Holding 'Right Trigger'):");
				GUI.Label(new Rect(25f, 315f, 500f, 20f), "Left/Right: Add dutch angle");
				GUI.Label(new Rect(25f, 330f, 500f, 20f), "Up/Down: Move up or down");
			}
			else
			{
				GUI.Label(new Rect(15f, 285f, 500f, 20f), "Right Stick (Not holding 'Right Trigger'):");
				GUI.Label(new Rect(25f, 315f, 500f, 20f), "All Directions: Orbit around target");
			}
		}
		else
		{
			GUI.Label(new Rect(15f, 285f, 500f, 20f), "Right Stick: Orbit around target");
		}
	}

	private void UpdateCameraForward()
	{
		if (CameraState == State.Normal || CameraState == State.FirstPerson)
		{
			if (CameraState == State.Normal && ((!TrailerCamera && Singleton<GameManager>.Instance.GameState != GameManager.State.Paused) || TrailerCamera) && Singleton<RInput>.Instance.P.GetButtonDown("Left Trigger"))
			{
				CameraReset();
			}
			if (!TrailerCamera)
			{
				base.transform.forward = (Target.position - base.transform.position).normalized;
			}
		}
		else if (CameraState == State.Event)
		{
			if (parameters.Mode == 2)
			{
				base.transform.position = ProjectPositionOnRail(Target.position);
				base.transform.forward = (Target.position - base.transform.position).normalized;
			}
			else if ((parameters.Mode != 32 && (parameters.Mode == 3 || parameters.Mode == 30 || parameters.Mode == 31) && !HasObjPosition && !HasObjTarget) || parameters.Mode == 32)
			{
				base.transform.position = ((parameters.Mode == 32) ? parameters.ObjPosition.position : parameters.Position);
				base.transform.forward = (((parameters.Mode == 32) ? parameters.ObjTarget.position : parameters.Target) - base.transform.position).normalized;
			}
			else if ((parameters.Mode == 4 || parameters.Mode == 41 || parameters.Mode == 42) && !HasObjPosition)
			{
				base.transform.position = parameters.Position;
				base.transform.forward = (Target.position - base.transform.position).normalized;
			}
			else if (parameters.Mode == 5 || parameters.Mode == 50)
			{
				base.transform.position = Target.position - base.transform.forward * Distance * (PlayerBase.GetPrefab("sonic_fast") ? 4f : 1f);
			}
			else if (parameters.Mode == 100)
			{
				Vector3 position = PlayerBase.transform.position + PlayerBase.transform.right * parameters.Position.x + PlayerBase.transform.up * parameters.Position.y + PlayerBase.transform.forward * parameters.Position.z;
				Vector3 vector = PlayerBase.transform.position + PlayerBase.transform.right * parameters.Target.x + PlayerBase.transform.up * parameters.Target.y + PlayerBase.transform.forward * parameters.Target.z;
				base.transform.position = position;
				base.transform.forward = (vector - base.transform.position).normalized;
			}
			else if (parameters.Mode == 104)
			{
				Vector3 position2 = Target.position + Vector3.right * parameters.Position.x + Vector3.up * parameters.Position.y + Vector3.forward * parameters.Position.z;
				Vector3 vector2 = Target.position + Vector3.right * parameters.Target.x + Vector3.up * parameters.Target.y + Vector3.forward * parameters.Target.z;
				base.transform.position = position2;
				base.transform.forward = (vector2 - base.transform.position).normalized;
			}
		}
		else if (CameraState == State.EventFadeOut || CameraState == State.EventDFadeOut)
		{
			float num = (Time.time - EventFadeTime) * 1.5f;
			num = Mathf.Clamp01(num * num);
			base.transform.forward = Vector3.Slerp(base.transform.forward, (Target.position - base.transform.position).normalized, Mathf.SmoothStep(0f, 1f, num * Time.timeScale));
		}
		else if (CameraState == State.Death)
		{
			base.transform.forward = (Target.position - base.transform.position).normalized;
		}
	}
}
