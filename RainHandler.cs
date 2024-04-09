using UnityEngine;

public class RainHandler : MonoBehaviour
{
	public ParticleSystemRenderer Module;

	private Transform PlayerCamera;

	private bool IsEnabled;

	private void Start()
	{
		IsEnabled = Singleton<Settings>.Instance.settings.RainEffects != 0;
		if (IsEnabled)
		{
			PlayerCamera = Camera.main.transform;
			Module.enabled = Singleton<Settings>.Instance.settings.RainEffects != 1;
		}
		else
		{
			Module.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (IsEnabled && (bool)PlayerCamera)
		{
			base.transform.position = new Vector3(PlayerCamera.position.x, base.transform.position.y, PlayerCamera.position.z);
		}
	}
}
