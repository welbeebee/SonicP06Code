using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
	public struct PlayerData
	{
		public int score;

		public float time;

		public int rings;

		public int maxCollectedRings;

		public CheckpointData checkpoint;
	}

	public enum State
	{
		Menu,
		Loading,
		Playing,
		Hub,
		Paused,
		Result
	}

	public enum Story
	{
		Sonic,
		Shadow,
		Silver,
		Last
	}

	public Story GameStory;

	public Scene FirstSection;

	public PlayerData _PlayerData;

	public State GameState;

	public bool GoToActSelect;

	public bool PlayedEventLimit;

	public float SaveTime;

	public float SectionSaveTime;

	public string FirstSectionPath;

	public string LoadingTo;

	public int ActSelectLastIndex;

	public PlayerAttributesData[] StoredPlayerVars;

	public List<string> LifeItemIDs = new List<string>();

	private global::Discord.Discord discord;

	private Dictionary<string, List<string>> Scenes = new Dictionary<string, List<string>>
	{
		{
			"Wave Ocean",
			new List<string> { "wvo_a_sn", "wvo_b_sn", "wvo_a_tl", "wvo_a_sd", "wvo_b_sd", "wvo_a_bz" }
		},
		{
			"Dusty Desert",
			new List<string> { "dtd_a_sn", "dtd_a_sd", "dtd_b_sd", "dtd_b_sv" }
		},
		{
			"White Acropolis",
			new List<string> { "wap_a_sn", "wap_b_sn", "wap_a_sd", "wap_b_sd", "wap_a_sv", "wap_b_sv" }
		},
		{
			"Crisis City",
			new List<string>
			{
				"csc_a_sn", "csc_b_sn", "csc_c_sn", "csc_e_sn", "csc_a_sd", "csc_b_sd", "csc_c_sd", "csc_f_sd", "csc_b_sv", "csc_f1_sv",
				"csc_f2_sv"
			}
		},
		{
			"E3 Crisis City",
			new List<string> { "csc_e3f_sv" }
		},
		{
			"Flame Core",
			new List<string> { "flc_a_sn", "flc_b_sn", "flc_a_sd", "flc_b_sd", "flc_a_sv", "flc_b_sv" }
		},
		{
			"Radical Train",
			new List<string> { "rct_a_sn", "rct_b_sn", "rct_a_sd", "rct_b_sd", "rct_a_sv" }
		},
		{
			"Tropical Jungle",
			new List<string> { "tpj_a_sn", "tpj_b_sn", "tpj_c_rg", "tpj_c_sv" }
		},
		{
			"Kingdom Valley",
			new List<string> { "kdv_a_sn", "kdv_b_sn", "kdv_c_sn", "kdv_d_sn", "kdv_a_sd", "kdv_b_sd", "kdv_d_sd", "kdv_b_sv", "kdv_d_sv" }
		},
		{
			"TGS Kingdom Valley",
			new List<string> { "kdv_tgsa_sn", "kdv_tgsb_sd", "kdv_tgsd_sv" }
		},
		{
			"Aquatic Base",
			new List<string> { "aqa_a_sn", "aqa_b_sn", "aqa_a_sd", "aqa_b_sd", "aqa_a_sv", "aqa_b_sv" }
		},
		{
			"End Of The World",
			new List<string> { "end_a_sn", "end_b_sn", "end_c_sn", "end_d_sn", "end_e_sn", "end_f_sn", "end_g_sn" }
		},
		{
			"Test Stage",
			new List<string> { "test_a_sn", "test_b_sn", "test_c_sn", "test_d_sn" }
		},
		{
			"Loading Screen",
			new List<string> { "RetailLoadingScreen", "E3LoadingScreen", "LoadingScreen" }
		}
	};

	private string[] SpecialNames = new string[2] { "Crisis City (E3)", "Kingdom Valley (TGS)" };

	internal bool CountTime;

	protected GameManager()
	{
	}

	[DllImport("user32.dll")]
	public static extern bool SetWindowText(IntPtr hwnd, string lpString);

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);

	public void StartGameManager()
	{
		SetWindowText(FindWindow(null, Application.productName), Game.TitleBarName);
	}

	private void OnEnable()
	{
		if (Process.GetProcessesByName("discord").Length != 0)
		{
			discord = new global::Discord.Discord(962193216269082684L, 1uL);
			Scene activeScene = SceneManager.GetActiveScene();
			OnSceneChanged(activeScene, activeScene);
			SceneManager.activeSceneChanged += OnSceneChanged;
		}
		else
		{
			UnityEngine.Debug.LogWarning("Discord application not found. Rich Presence not initialized.");
		}
	}

	private void Update()
	{
		if (discord != null)
		{
			discord.RunCallbacks();
		}
	}

	private void OnDisable()
	{
		if (discord != null)
		{
			discord.Dispose();
			SceneManager.activeSceneChanged -= OnSceneChanged;
		}
	}

	private void OnSceneChanged(Scene previousScene, Scene newScene)
	{
		if (discord == null)
		{
			return;
		}
		ActivityManager activityManager = discord.GetActivityManager();
		long start = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
		string text = "Menu";
		string largeImage = "sonicnexticon_new";
		foreach (KeyValuePair<string, List<string>> scene in Scenes)
		{
			if (scene.Value.Contains(newScene.name))
			{
				text = scene.Key ?? "";
				largeImage = text.ToLower().Replace(" ", "");
				if (scene.Key.Contains("E3 Crisis City"))
				{
					text = SpecialNames[0];
				}
				else if (scene.Key.Contains("TGS Kingdom Valley"))
				{
					text = SpecialNames[1];
				}
				if (!scene.Key.Contains("Loading Screen") && !scene.Key.Contains("Test Stage"))
				{
					text = text + " | " + GameStory;
				}
			}
		}
		Activity activity = default(Activity);
		activity.Timestamps.Start = start;
		activity.State = text;
		activity.Assets.LargeImage = largeImage;
		activity.Assets.LargeText = "Sonic the Hedgehog (P-06)";
		Activity activity2 = activity;
		activityManager.UpdateActivity(activity2, delegate(Result result)
		{
			if (result == Result.Ok)
			{
				UnityEngine.Debug.Log("Succesfully updated Rich Presence activity.");
			}
			else
			{
				UnityEngine.Debug.LogError("Failed to update Rich Presence activity.");
			}
		});
	}

	public void SetFirstSectionPath()
	{
		FirstSection = SceneManager.GetActiveScene();
		FirstSectionPath = FirstSection.path.Replace("Assets/", "").Replace(".unity", "");
	}

	public void SetLoadingTo(string BuildScene, string Mode = "")
	{
		LoadingTo = BuildScene;
		GameState = State.Loading;
		if (Mode == Game.AutoSaveMode)
		{
			SceneManager.LoadScene("AutoSaveScene", LoadSceneMode.Single);
		}
		else if (Mode == Game.BlankLoadMode)
		{
			SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
		}
		else if (Singleton<Settings>.Instance.settings.LoadingScreenType != 0)
		{
			SceneManager.LoadScene((Mode == Game.MenuLoadMode) ? "LoadingScreen" : "E3LoadingScreen", LoadSceneMode.Single);
		}
		else
		{
			SceneManager.LoadScene("RetailLoadingScreen", LoadSceneMode.Single);
		}
	}

	public void OnStageStart(bool IsFirstSection, bool KeepTime, bool IsHub)
	{
		if (IsFirstSection)
		{
			_PlayerData.score = 0;
			_PlayerData.rings = 0;
			_PlayerData.maxCollectedRings = 0;
			StoredPlayerVars = null;
			SetFirstSectionPath();
			if (!KeepTime)
			{
				SaveTime = 0f;
				SectionSaveTime = 0f;
			}
		}
		else if (FirstSectionPath == null)
		{
			SetFirstSectionPath();
		}
		_PlayerData.time = SaveTime;
		GC.Collect();
		GameState = ((!IsHub) ? State.Playing : State.Hub);
		CountTime = true;
	}

	public void OnChangeSection()
	{
		SaveTime = _PlayerData.time;
		SectionSaveTime = _PlayerData.time;
		if (_PlayerData.checkpoint != null)
		{
			_PlayerData.checkpoint.Saved = false;
		}
		LifeItemIDs.Clear();
		ResetTimeScaleAndSoundPitch();
	}

	public void OnStageRestart(bool RestartStage)
	{
		if (!RestartStage)
		{
			if (_PlayerData.checkpoint != null)
			{
				_PlayerData.checkpoint.Saved = false;
			}
			SaveTime = SectionSaveTime;
		}
		_PlayerData = default(PlayerData);
		GameData.StoryData storyData = GetStoryData();
		storyData.Lives--;
		SetStoryData(storyData);
		StoredPlayerVars = null;
		LifeItemIDs.Clear();
		SceneManager.LoadScene(RestartStage ? FirstSectionPath : SceneManager.GetActiveScene().name);
	}

	public void OnPlayerDeath()
	{
		GameData.StoryData storyData = GetStoryData();
		storyData.Lives--;
		SetStoryData(storyData);
		StoredPlayerVars = null;
		if (storyData.Lives < 0)
		{
			Exit(GameOver: true);
			return;
		}
		_PlayerData.score = 0;
		_PlayerData.rings = 0;
		_PlayerData.maxCollectedRings = 0;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void Exit(bool GameOver = false)
	{
		_PlayerData = default(PlayerData);
		GameState = State.Menu;
		StoredPlayerVars = null;
		PlayedEventLimit = false;
		if (!GameOver)
		{
			GoToActSelect = !SceneManager.GetActiveScene().name.Contains("test") && SceneManager.GetActiveScene().name != Game.TGS_KDV_SN_Scene && SceneManager.GetActiveScene().name != Game.TGS_KDV_SD_Scene && SceneManager.GetActiveScene().name != Game.TGS_KDV_SV_Scene && SceneManager.GetActiveScene().name != Game.E3_CSC_Scene;
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}
		else
		{
			Singleton<GameData>.Instance.OnGameOver();
			SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
		}
		ResetTimeScaleAndSoundPitch();
	}

	public void ResetTimeScaleAndSoundPitch()
	{
		if (GameState != State.Paused && Time.timeScale != 1f)
		{
			Time.timeScale = 1f;
		}
		Singleton<AudioManager>.Instance.MainMixer.SetFloat("MusicPitch", 1f);
		Singleton<AudioManager>.Instance.MainMixer.SetFloat("SoundsPitch", 1f);
		Singleton<AudioManager>.Instance.MainMixer.SetFloat("VoicesPitch", 1f);
	}

	public void KeepPlayerAttributes(PlayerAttributesData[] Variables)
	{
		StoredPlayerVars = Variables;
	}

	private void FixedUpdate()
	{
		if (CountTime && GameState == State.Playing)
		{
			_PlayerData.time += Time.fixedDeltaTime;
		}
		Singleton<GameData>.Instance.Playtime += Time.fixedUnscaledDeltaTime;
		Singleton<GameData>.Instance.Playtime = Mathf.Clamp(Singleton<GameData>.Instance.Playtime, 0f, 3599999f);
	}

	public string GetGameStory()
	{
		return GameStory.ToString();
	}

	public void SetGameStory(string Story)
	{
		GameStory = (Story)Enum.Parse(typeof(Story), Story);
	}

	public int GetLifeCount()
	{
		return GetStoryData().Lives;
	}

	public GameData.GlobalData GetGameData()
	{
		return Singleton<GameData>.Instance.Game;
	}

	public void SetGameData(GameData.GlobalData GlobalData)
	{
		Singleton<GameData>.Instance.Game = GlobalData;
	}

	public GameData.StoryData GetStoryData()
	{
		return GameStory switch
		{
			Story.Sonic => Singleton<GameData>.Instance.Sonic, 
			Story.Shadow => Singleton<GameData>.Instance.Shadow, 
			Story.Silver => Singleton<GameData>.Instance.Silver, 
			Story.Last => Singleton<GameData>.Instance.Last, 
			_ => Singleton<GameData>.Instance.Sonic, 
		};
	}

	public GameData.StoryData GetStringStoryData(string _Story)
	{
		return _Story switch
		{
			"Sonic" => Singleton<GameData>.Instance.Sonic, 
			"Shadow" => Singleton<GameData>.Instance.Shadow, 
			"Silver" => Singleton<GameData>.Instance.Silver, 
			"Last" => Singleton<GameData>.Instance.Last, 
			_ => Singleton<GameData>.Instance.Sonic, 
		};
	}

	public void SetStoryData(GameData.StoryData StoryData)
	{
		switch (GameStory)
		{
		case Story.Sonic:
			Singleton<GameData>.Instance.Sonic = StoryData;
			break;
		case Story.Shadow:
			Singleton<GameData>.Instance.Shadow = StoryData;
			break;
		case Story.Silver:
			Singleton<GameData>.Instance.Silver = StoryData;
			break;
		case Story.Last:
			Singleton<GameData>.Instance.Last = StoryData;
			break;
		}
	}
}
