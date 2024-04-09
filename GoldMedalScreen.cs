using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoldMedalScreen : UIBase
{
	
	public Animator Animator;

	public Text[] RewardTexts;

	private GameData.GlobalData Data;

	private bool CloseWindow;

	private string NextScene;

	private float StartTime;

	private bool GotSonicReward;

	private bool GotShadowReward;

	private bool GotSilverReward;

	private void Start()
	{
		Data = Singleton<GameManager>.Instance.GetGameData();
		GotSonicReward = Data.HasFlag("wvo_sn") && Data.HasFlag("dtd_sn") && Data.HasFlag("wap_sn") && Data.HasFlag("csc_sn") && Data.HasFlag("flc_sn") && Data.HasFlag("rct_sn") && Data.HasFlag("tpj_sn") && Data.HasFlag("kdv_sn") && Data.HasFlag("aqa_sn") && Data.HasFlag("wvo_tl") && !Data.HasFlag(Game.GotSnReward);
		GotShadowReward = Data.HasFlag("wap_sd") && Data.HasFlag("kdv_sd") && Data.HasFlag("csc_sd") && Data.HasFlag("flc_sd") && Data.HasFlag("rct_sd") && Data.HasFlag("aqa_sd") && Data.HasFlag("wvo_sd") && Data.HasFlag("dtd_sd") && Data.HasFlag("tpj_rg") && !Data.HasFlag(Game.GotSdReward);
		GotSilverReward = Data.HasFlag("csc_sv") && Data.HasFlag("tpj_sv") && Data.HasFlag("dtd_sv") && Data.HasFlag("wap_sv") && Data.HasFlag("rct_sv") && Data.HasFlag("aqa_sv") && Data.HasFlag("kdv_sv") && Data.HasFlag("flc_sv") && Data.HasFlag("wvo_bz") && !Data.HasFlag(Game.GotSvReward);
		RewardTexts[1].enabled = Singleton<GameManager>.Instance.GameStory == GameManager.Story.Sonic && GotSonicReward;
		RewardTexts[2].enabled = Singleton<GameManager>.Instance.GameStory == GameManager.Story.Shadow && GotShadowReward;
		RewardTexts[3].enabled = Singleton<GameManager>.Instance.GameStory == GameManager.Story.Silver && GotSilverReward;
		RewardTexts[0].enabled = !RewardTexts[1].enabled && !RewardTexts[2].enabled && !RewardTexts[3].enabled;
		NextScene = ((Data.HasFlag(Game.StgSnAllClear) && Data.HasFlag(Game.StgSdAllClear) && Data.HasFlag(Game.StgSvAllClear) && !Data.HasFlag(Game.CreditsPlayed)) ? Game.CreditsScene : "MainMenu");
		Settings.SetLocalSettings();
		StartTime = Time.time;
	}

	private void Update()
	{
		if (!(Time.time - StartTime > 3.3f) || CloseWindow || (!Singleton<RInput>.Instance.P.GetButtonDown("Button A") && !Singleton<RInput>.Instance.P.GetButtonDown("Button X") && !Singleton<RInput>.Instance.P.GetButtonDown("Start")))
		{
			return;
		}
		Animator.SetTrigger("On Close");
		switch (Singleton<GameManager>.Instance.GameStory)
		{
		case GameManager.Story.Sonic:
			if (GotSonicReward)
			{
				Data.ActivateFlag(Game.GotSnReward);
			}
			break;
		case GameManager.Story.Shadow:
			if (GotShadowReward)
			{
				Data.ActivateFlag(Game.GotSdReward);
			}
			break;
		case GameManager.Story.Silver:
			if (GotSilverReward)
			{
				Data.ActivateFlag(Game.GotSvReward);
			}
			break;
		}
		Singleton<GameManager>.Instance.SetGameData(Data);
		CloseWindow = true;
	}

	public void GoToLoadingScreen()
	{
		if (NextScene != Game.CreditsScene)
		{
			Singleton<GameManager>.Instance.SetLoadingTo(NextScene, Game.AutoSaveMode);
		}
		else
		{
			SceneManager.LoadScene(NextScene, LoadSceneMode.Single);
		}
	}
}
