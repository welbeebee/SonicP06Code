using System.Collections;
using UnityEngine;

public class Common_WarpHole : EventStation
{
	
	public string Event;

	public Vector3 Target;

	[Header("Prefab")]
	public AudioSource Audio;

	public GameObject TargetOutFX;

	private bool Warped;

	public void SetParameters(string _Event, Vector3 _Target)
	{
		Event = _Event;
		Target = _Target;
	}

	private void OnTriggerEnter(Collider col)
	{
		PlayerBase player = GetPlayer(col);
		if ((bool)player && !(player.GetState() == "Vehicle") && (!(Event == "") || !(Target == Vector3.zero)) && !Warped)
		{
			player.OnWarpHoleEnter((!(Event != "")) ? 1 : 0, base.transform.position, Target);
			Audio.Play();
			StartCoroutine(OnWarp(player));
			Warped = true;
		}
	}

	private IEnumerator OnWarp(PlayerBase PlayerBase)
	{
		yield return new WaitForSeconds(2.5f);
		if (Event == "" && Target != Vector3.zero)
		{
			Object.Instantiate(TargetOutFX, Target, Quaternion.identity);
		}
		if (Event != "")
		{
			if (PlayerBase.GetPrefab("sonic_new"))
			{
				Singleton<GameManager>.Instance.KeepPlayerAttributes(new PlayerAttributesData[4]
				{
					new PlayerAttributesData("GemGeneral", PlayerBase.PlayerManager.sonic.IsSuper, PlayerBase.HUD.ActionDisplay, PlayerBase.PlayerManager.sonic.GemSelector),
					new PlayerAttributesData("GemLevels", null, null, PlayerBase.HUD.ActiveGemLevel),
					new PlayerAttributesData("GemDisplay", null, PlayerBase.HUD.GemDisplay),
					new PlayerAttributesData("Shield/" + PlayerBase.PlayerPrefab, PlayerBase.HasShield)
				});
			}
			else if (PlayerBase.GetPrefab("shadow"))
			{
				Singleton<GameManager>.Instance.KeepPlayerAttributes(new PlayerAttributesData[4]
				{
					new PlayerAttributesData("BoostGeneral", PlayerBase.PlayerManager.shadow.IsChaosBoost, PlayerBase.HUD.ActionDisplay, PlayerBase.PlayerManager.shadow.ChaosBoostLevel),
					new PlayerAttributesData("BoostDisplay", _VarBool: false, PlayerBase.HUD.ChaosMaturityDisplay),
					new PlayerAttributesData("UninhibitMode", PlayerBase.PlayerManager.shadow.IsFullPower),
					new PlayerAttributesData("Shield/" + PlayerBase.PlayerPrefab, PlayerBase.HasShield)
				});
			}
			else if (PlayerBase.GetPrefab("silver"))
			{
				Singleton<GameManager>.Instance.KeepPlayerAttributes(new PlayerAttributesData[3]
				{
					new PlayerAttributesData("ESPGeneral", PlayerBase.PlayerManager.silver.IsAwakened, PlayerBase.HUD.ActionDisplay),
					new PlayerAttributesData("ESPDisplay", _VarBool: false, PlayerBase.HUD.ESPMaturityDisplay),
					new PlayerAttributesData("Shield/" + PlayerBase.PlayerPrefab, PlayerBase.HasShield)
				});
			}
			else
			{
				Singleton<GameManager>.Instance.KeepPlayerAttributes(new PlayerAttributesData[6]
				{
					new PlayerAttributesData("GemGeneral", _VarBool: false, PlayerBase.HUD.ActionDisplay),
					new PlayerAttributesData("GemLevels", null, null, PlayerBase.HUD.ActiveGemLevel),
					new PlayerAttributesData("GemDisplay", null, PlayerBase.HUD.GemDisplay),
					new PlayerAttributesData("BoostGeneral", _VarBool: false, PlayerBase.HUD.ActionDisplay),
					new PlayerAttributesData("BoostDisplay", _VarBool: false, PlayerBase.HUD.ChaosMaturityDisplay),
					new PlayerAttributesData("Shield/" + PlayerBase.PlayerPrefab, PlayerBase.HasShield)
				});
			}
			CallEvent(Event);
		}
		else
		{
			Warped = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (Target != Vector3.zero)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(base.transform.position, Target);
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(Target, new Vector3(0.5f, 0.5f, 0.5f));
		}
	}
}
