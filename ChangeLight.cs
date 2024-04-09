using UnityEngine;

public class ChangeLight : ObjectBase
{
	
	public string MainLight;

	public string SubLight;

	public string Ambient;

	private SceneParameters SceneParams;

	private bool Inactive;

	public void SetParameters(string _MainLight, string _SubLight, string _Ambient)
	{
		MainLight = _MainLight;
		SubLight = _SubLight;
		Ambient = _Ambient;
	}

	private void Start()
	{
		SceneParams = Object.FindObjectOfType<SceneParameters>();
	}

	private void OnTriggerStay(Collider collider)
	{
		if ((bool)GetPlayer(collider) && !Inactive)
		{
			SceneParams.SetLightPreset(MainLight, SubLight, Ambient);
			if (!SceneParams.LightChange)
			{
				SceneParams.LightChange = true;
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if ((bool)GetPlayer(collider) && !Inactive)
		{
			SceneParams.LightChange = false;
		}
	}

	private void OFF()
	{
		Inactive = true;
		SceneParams.LightChange = false;
	}
}
