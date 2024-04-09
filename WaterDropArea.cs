using UnityEngine;

public class WaterDropArea : ObjectBase
{
	private void OnTriggerStay(Collider col)
	{
		PlayerBase player = GetPlayer(col);
		if ((bool)player)
		{
			player.CameraFX.CameraRain.IsBlocked = true;
		}
	}
}
