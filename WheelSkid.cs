using System;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelSkid : MonoBehaviour
{
	public VehicleBase Base;

	public WheelCollider Collider;

	public float SlipLimit;

	public float SkidMult;

	public float MarkWidth;

	public bool IsBrake;

	private float LastFixedUpdateTime;

	private float NormalSkid;

	private int LastSkid = -1;

	private const float WHEEL_SLIP_MULTIPLIER = 5f;

	private void Awake()
	{
		LastFixedUpdateTime = Time.time;
	}

	private void FixedUpdate()
	{
		LastFixedUpdateTime = Time.time;
	}

	private void LateUpdate()
	{
		Collider.GetGroundHit(out var hit);
		if (Collider.isGrounded && hit.collider.transform.tag == "Normal")
		{
			float z = base.transform.InverseTransformDirection(Base._Rigidbody.velocity).z;
			float num = Collider.radius * ((float)Math.PI * 2f * Collider.rpm / 60f);
			float num2 = Vector3.Dot(Base._Rigidbody.velocity, base.transform.forward);
			float num3 = Mathf.Abs(num2 - num) * 10f;
			num3 = Mathf.Max(0f, num3 * (10f - Mathf.Abs(num2)));
			bool flag = Collider.isGrounded && (hit.forwardSlip >= SlipLimit || hit.forwardSlip <= 0f - SlipLimit || hit.sidewaysSlip >= SlipLimit * 0.25f || hit.sidewaysSlip <= (0f - SlipLimit) * 0.25f) && z >= 0f;
			NormalSkid = Mathf.Lerp(NormalSkid, flag ? 1f : 0f, Time.deltaTime * 10f);
			float num4 = ((!IsBrake) ? NormalSkid : Base.BrakeSkid);
			if (num4 > 0f)
			{
				Vector3 pos = hit.point + Base._Rigidbody.velocity * (Time.time - LastFixedUpdateTime);
				LastSkid = Singleton<Skidmarks>.Instance.AddSkidMark(pos, hit.normal, num4 * SkidMult, MarkWidth, LastSkid);
			}
			else
			{
				LastSkid = -1;
			}
		}
		else
		{
			LastSkid = -1;
		}
	}
}
