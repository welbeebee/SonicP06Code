using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
	private PlayerCamera Camera;

	private Canvas UICanvas;

	private UI HUD;

	protected DebugManager()
	{
	}

	public void StartDebugManager()
	{
	}

	private void Update()
	{
		if (Singleton<RInput>.Instance.P.GetButtonDown("Left Stick Button"))
		{
			if (!Camera)
			{
				Camera = Object.FindObjectOfType<PlayerCamera>();
			}
			if ((bool)Camera)
			{
				Camera.TrailerCamera = !Camera.TrailerCamera;
			}
		}
		if (Singleton<RInput>.Instance.P.GetButtonDown("Right Stick Button"))
		{
			if (!HUD)
			{
				HUD = Object.FindObjectOfType<UI>();
			}
			if ((bool)HUD && !UICanvas)
			{
				UICanvas = HUD.GetComponent<Canvas>();
			}
			if ((bool)HUD && (bool)UICanvas)
			{
				UICanvas.enabled = !UICanvas.enabled;
			}
		}
	}
}
