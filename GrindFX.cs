
using UnityEngine;

public class GrindFX : MonoBehaviour
{
	public enum Type
	{
		Metal,
		Wind,
		Nature
	}

	
	public Type RailType;

	public ParticleSystem[] ManipulatedFX;

	public ParticleSystem[] FX;

	[Header("Settings")]
	public Transform Pivot;

	public AudioSource[] Sources;

	internal PlayerBase Player;

	internal bool StopFX;

	private Vector3 ForwardMag;

	private bool Stopped;

	private float Speed;

	private void Update()
	{
		if (Stopped)
		{
			return;
		}
		switch (RailType)
		{
		case Type.Metal:
		{
			ForwardMag = Vector3.Lerp(ForwardMag, base.transform.forward, Time.deltaTime * 10f);
			float num = Mathf.Clamp(Vector3.Dot(ForwardMag, base.transform.right), -35f, 35f);
			Speed = ((Player.CurSpeed > 0f) ? Player.CurSpeed : (0f - Player.CurSpeed));
			if (ManipulatedFX != null)
			{
				for (int i = 0; i < ManipulatedFX.Length; i++)
				{
					ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ManipulatedFX[i].velocityOverLifetime;
					AnimationCurve animationCurve = new AnimationCurve();
					animationCurve.AddKey(0f, 0f);
					animationCurve.AddKey(1f, (0f - num) * 10f);
					velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(25f, animationCurve);
					ParticleSystem.MainModule main = ManipulatedFX[i].main;
					main.startSpeed = Mathf.Lerp(15f, 60f, Speed / Sonic_New_Lua.c_grind_speed_max);
					main.gravityModifier = Mathf.Lerp(-1.5f, -6f, Speed / Sonic_New_Lua.c_grind_speed_max);
				}
			}
			Pivot.forward = ((Player.CurSpeed > 0f) ? base.transform.forward : (-base.transform.forward));
			bool flag = Player.GrindSpeed > Player.GrindSpeedOrg || (Player.GetPrefab("snow_board") && Singleton<RInput>.Instance.P.GetButton("Button A"));
			Sources[0].pitch = Mathf.Min(1f, Player.GrindSpeed / Player.GrindSpeedOrg) * 0.5f + 0.5f;
			Sources[0].volume = ((Player.GetState() == "Grinding" && TrickCheck() && !Player.RailSwitch) ? 0.65f : 0f);
			Sources[1].volume = ((Player.GetState() == "Grinding" && TrickCheck() && !Player.RailSwitch) ? Mathf.Lerp(Sources[1].volume, flag ? 0.65f : 0f, Time.deltaTime * 2f) : 0f);
			break;
		}
		case Type.Wind:
			Sources[0].volume = Mathf.Lerp(Sources[0].volume, (Player.GetState() == "Grinding" && TrickCheck() && !Player.RailSwitch) ? 0.6f : 0f, Time.deltaTime * 3f);
			break;
		case Type.Nature:
			Sources[0].volume = ((Player.GetState() == "Grinding" && TrickCheck() && !Player.RailSwitch) ? 1f : 0f);
			break;
		}
		if (FX != null)
		{
			for (int j = 0; j < FX.Length; j++)
			{
				ParticleSystem.EmissionModule emission = FX[j].emission;
				emission.enabled = Player.GetState() == "Grinding" && TrickCheck() && !Player.RailSwitch;
			}
		}
	}

	private bool TrickCheck()
	{
		if (Player.GrindTrick)
		{
			if (Player.GrindTrick)
			{
				return !StopFX;
			}
			return false;
		}
		return true;
	}

	public void StopRailFX()
	{
		Stopped = true;
		if (FX != null)
		{
			for (int i = 0; i < FX.Length; i++)
			{
				FX[i].Stop();
			}
		}
		for (int j = 0; j < Sources.Length; j++)
		{
			Sources[j].Stop();
		}
	}
}
