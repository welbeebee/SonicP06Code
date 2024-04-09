using UnityEngine;

public class StageManager : MonoBehaviour
{
	public enum Stage
	{
		wvo,
		dtd,
		wap,
		csc,
		flc,
		rct,
		tpj,
		kdv,
		aqa,
		twn,
		other
	}

	public enum Section
	{
		A,
		B,
		C,
		D,
		E,
		F
	}

	public enum PlayerName
	{
		Sonic_New,
		Sonic_Fast,
		Princess,
		Snow_Board,
		Shadow,
		Silver,
		Tails,
		Amy,
		Knuckles,
		Blaze,
		Rouge,
		Omega,
		Metal_Sonic
	}

	public enum Story
	{
		Default,
		Sonic,
		Shadow,
		Silver,
		Last,
		Other
	}

	public enum State
	{
		Playing,
		Event
	}

	[Header("Stage")]
	public Stage _Stage;

	public Section StageSection;

	public bool FirstSection;

	public bool MissionIsHub;

	[Header("BGM")]
	public AudioClip StageBGM;

	[Header("Optional")]
	public AudioClip E3StageBGM;

	internal State StageState;

	internal PlayerName Player;

	internal PlayerStart[] PlayerStarts;

	internal AudioSource BGMPlayer;

	private void Awake()
	{
		Application.targetFrameRate = 60;
		BGMPlayer = base.gameObject.AddComponent<AudioSource>();
		BGMPlayer.clip = ((Singleton<Settings>.Instance.settings.E3XBLAMusic == 1 && (bool)E3StageBGM) ? E3StageBGM : StageBGM);
		BGMPlayer.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MainMixer.FindMatchingGroups("Music")[0];
		BGMPlayer.bypassReverbZones = true;
		BGMPlayer.playOnAwake = true;
		BGMPlayer.loop = true;
		BGMPlayer.dopplerLevel = 0f;
		BGMPlayer.Play();
		PlayerStarts = Object.FindObjectsOfType<PlayerStart>();
		for (int i = 0; i < PlayerStarts.Length; i++)
		{
			if (!PlayerStarts[i].Amigo)
			{
				CheckpointData checkpoint = Singleton<GameManager>.Instance._PlayerData.checkpoint;
				bool flag = checkpoint?.Saved ?? false;
				int iD = (flag ? checkpoint.PlayerNo : PlayerStarts[i].Player_No);
				string player_Name = PlayerStarts[i].Player_Name;
				string text = (flag ? checkpoint.PlayerPrefab : PlayerStarts[i].GetPlayerName());
				Vector3 position = (flag ? (checkpoint.Position + Vector3.up * 0.25f) : PlayerStarts[i].transform.position);
				Quaternion rotation = (flag ? checkpoint.Rotation : PlayerStarts[i].transform.rotation);
				if (flag || PlayerStarts[i] != null)
				{
					UI component = (Object.Instantiate(Resources.Load("DefaultPrefabs/UI/" + ((Singleton<Settings>.Instance.settings.DisplayType == 0) ? "UI_Retail" : "UI_E3")), Vector3.zero, Quaternion.identity) as GameObject).GetComponent<UI>();
					component.StageManager = this;
					if (_Stage != Stage.other)
					{
						component.OpenCollectibles("Medal_Silver", 10);
					}
					if (MissionIsHub)
					{
						component.OpenRadarMap((int)StageSection);
					}
					PlayerBase component2 = (Object.Instantiate(Resources.Load("DefaultPrefabs/Player/" + text), position, rotation) as GameObject).GetComponent<PlayerBase>();
					Vector3 position2 = component2.transform.position - component2.transform.forward * 7f;
					PlayerCamera component3 = (Object.Instantiate(Resources.Load("DefaultPrefabs/Game/Camera"), position2, rotation) as GameObject).GetComponent<PlayerCamera>();
					component3.StageManager = this;
					component3.PlayerBase = component2;
					component3.Distance = component3.CameraDistace() * (component2.GetPrefab("snow_board") ? 0.625f : 1f) + ((component2.GetPrefab("sonic_fast") || component2.GetPrefab("snow_board")) ? 1.5f : 0f);
					component2.SetPlayer(iD, text);
					component2.StartPlayer();
					component2.SetStoredAttributes();
					if (player_Name.Contains("jeep") || player_Name.Contains("bike") || player_Name.Contains("hover") || player_Name.Contains("glider"))
					{
						StartCoroutine(component2.GetComponent<Shadow>().MountVehicle(player_Name.Split("_"[0])[1]));
					}
				}
				Singleton<GameManager>.Instance.OnStageStart(FirstSection, flag, MissionIsHub);
			}
			else if (!PlayerStarts[i].NotVisiblyInteractable)
			{
				AmigoSwitch component4 = (Object.Instantiate(Resources.Load("DefaultPrefabs/Objects/Amigo"), PlayerStarts[i].transform.position + Vector3.up * 0.25f, PlayerStarts[i].transform.rotation) as GameObject).GetComponent<AmigoSwitch>();
				component4.PlayerNo = PlayerStarts[i].Player_No;
				component4.PlayerPrefab = PlayerStarts[i].GetPlayerName();
			}
		}
		Settings.SetLocalSettings();
	}

	public void SetPlayer(string StateName)
	{
		switch (StateName)
		{
		case "sonic_new":
			Player = PlayerName.Sonic_New;
			break;
		case "sonic_fast":
			Player = PlayerName.Sonic_Fast;
			break;
		case "princess":
			Player = PlayerName.Princess;
			break;
		case "snow_board":
			Player = PlayerName.Snow_Board;
			break;
		case "shadow":
			Player = PlayerName.Shadow;
			break;
		case "silver":
			Player = PlayerName.Silver;
			break;
		case "tails":
			Player = PlayerName.Tails;
			break;
		case "amy":
			Player = PlayerName.Amy;
			break;
		case "knuckles":
			Player = PlayerName.Knuckles;
			break;
		case "blaze":
			Player = PlayerName.Blaze;
			break;
		case "rouge":
			Player = PlayerName.Rouge;
			break;
		case "omega":
			Player = PlayerName.Omega;
			break;
		case "metal_sonic":
			Player = PlayerName.Metal_Sonic;
			break;
		}
	}
}
