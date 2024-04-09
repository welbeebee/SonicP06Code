using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
	public enum Type
	{
		Stage,
		Menu,
		Retail
	}

	
	public Type LoadType;

	public Transform StageNames;

	public Text StageMission;

	public TextMeshProUGUI StageMissionTMP;

	public Animator Animator;

	[Header("Retail")]
	public Sprite CmnImage;

	public Sprite[] Images;

	public Sprite ExtraKDVImage;

	public Sprite ExtraCSCImage;

	public Image[] ImagesHolders;

	public Sprite[] SonicTips_360;

	public Sprite[] SonicTips_PS3;

	public Sprite[] ShadowTips_360;

	public Sprite[] ShadowTips_PS3;

	public Sprite[] SilverTips_360;

	public Sprite[] SilverTips_PS3;

	public Sprite[] PrincessTips_360;

	public Sprite[] PrincessTips_PS3;

	public Sprite[] TailsTips_360;

	public Sprite[] TailsTips_PS3;

	public Sprite[] RougeTips_360;

	public Sprite[] RougeTips_PS3;

	public Sprite[] BlazeTips_360;

	public Sprite[] BlazeTips_PS3;

	public Image TipsHolder;

	private AsyncOperation AsyncOp;

	private bool ForceLoad;

	private float ForceLoadTimer;

	private string TextToShow;

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		string text = Singleton<GameManager>.Instance.LoadingTo.Split('_')[0];
		if (LoadType == Type.Stage)
		{
			string text2 = text + "_name";
			if (Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SN_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SD_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SV_Scene)
			{
				text2 += "_tgs";
			}
			else if (Singleton<GameManager>.Instance.LoadingTo == Game.E3_CSC_Scene)
			{
				text2 += "_e3";
			}
			StageNames.Find(text2).gameObject.SetActive(value: true);
		}
		if (LoadType == Type.Retail)
		{
			string text3 = Singleton<GameManager>.Instance.LoadingTo.Split('_')[0];
			string text4 = Singleton<GameManager>.Instance.LoadingTo.Split('_')[2];
			if (text3 == "test")
			{
				for (int i = 0; i < ImagesHolders.Length; i++)
				{
					ImagesHolders[i].sprite = CmnImage;
				}
				TipsHolder.enabled = false;
			}
			else
			{
				if (Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SN_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SD_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SV_Scene)
				{
					for (int j = 0; j < ImagesHolders.Length; j++)
					{
						ImagesHolders[j].sprite = ExtraKDVImage;
					}
				}
				else if (Singleton<GameManager>.Instance.LoadingTo == Game.E3_CSC_Scene)
				{
					for (int k = 0; k < ImagesHolders.Length; k++)
					{
						ImagesHolders[k].sprite = ExtraCSCImage;
					}
				}
				else
				{
					for (int l = 0; l < Images.Length; l++)
					{
						if (Images[l].name.Contains(text3))
						{
							for (int m = 0; m < ImagesHolders.Length; m++)
							{
								ImagesHolders[m].sprite = Images[l];
							}
						}
					}
				}
				switch (text4)
				{
				case "sn":
					if (text3 == "dtd" || text3 == "tpj")
					{
						TipsHolder.sprite = SetTipSprite(PrincessTips_360, PrincessTips_PS3);
					}
					else
					{
						TipsHolder.sprite = SetTipSprite(SonicTips_360, SonicTips_PS3);
					}
					break;
				case "sd":
					TipsHolder.sprite = SetTipSprite(ShadowTips_360, ShadowTips_PS3);
					break;
				case "sv":
					TipsHolder.sprite = SetTipSprite(SilverTips_360, SilverTips_PS3);
					break;
				case "tl":
					TipsHolder.sprite = SetTipSprite(TailsTips_360, TailsTips_PS3);
					break;
				case "rg":
					TipsHolder.sprite = SetTipSprite(RougeTips_360, RougeTips_PS3);
					break;
				case "bz":
					TipsHolder.sprite = SetTipSprite(BlazeTips_360, BlazeTips_PS3);
					break;
				}
			}
		}
		if (LoadType != Type.Menu)
		{
			if (Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SN_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SD_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.TGS_KDV_SV_Scene || Singleton<GameManager>.Instance.LoadingTo == Game.E3_CSC_Scene)
			{
				TextToShow = "stg_extra";
			}
			else
			{
				string text5 = Singleton<GameManager>.Instance.LoadingTo.Split('_')[2];
				switch (Singleton<GameManager>.Instance.GameStory)
				{
				case GameManager.Story.Sonic:
					TextToShow = "sonic";
					break;
				case GameManager.Story.Shadow:
					TextToShow = "shadow";
					break;
				case GameManager.Story.Silver:
					TextToShow = "silver";
					break;
				}
				TextToShow = TextToShow + "_stg_" + text;
				switch (text5)
				{
				case "tl":
					TextToShow += "_tails";
					break;
				case "rg":
					TextToShow += "_rouge";
					break;
				case "bz":
					TextToShow += "_blaze";
					break;
				}
			}
			if (LoadType == Type.Retail)
			{
				StageMission.text = Singleton<MSTManager>.Instance.GetSystem(TextToShow, ToUpper: false);
			}
			else
			{
				StageMissionTMP.text = "<sprite=0> " + Singleton<MSTManager>.Instance.GetSystem(TextToShow, ToUpper: true);
			}
		}
		AsyncOp = SceneManager.LoadSceneAsync(Singleton<GameManager>.Instance.LoadingTo, LoadSceneMode.Single);
		AsyncOp.allowSceneActivation = false;
		StartCoroutine(LoadSceneWait(AsyncOp));
		ForceLoadTimer = Time.time;
	}

	private Sprite SetTipSprite(Sprite[] Tips_360, Sprite[] Tips_PS3)
	{
		if (Singleton<Settings>.Instance.settings.ButtonIcons == 1)
		{
			return Tips_PS3[Random.Range(0, Tips_PS3.Length)];
		}
		return Tips_360[Random.Range(0, Tips_360.Length)];
	}

	private void Update()
	{
		if (AsyncOp.isDone)
		{
			FinishLoad();
		}
		if (Time.time - ForceLoadTimer > 10f && !ForceLoad && !AsyncOp.isDone)
		{
			SceneManager.LoadScene(Singleton<GameManager>.Instance.LoadingTo, LoadSceneMode.Single);
			FinishLoad();
			ForceLoad = true;
		}
	}

	private void FinishLoad()
	{
		StartCoroutine(PlayEndAnimations());
	}

	private IEnumerator PlayEndAnimations()
	{
		yield return new WaitForSeconds(0f);
		Animator.SetTrigger("Is Loaded");
		Animator.SetTrigger("Text Loaded");
	}

	private IEnumerator LoadSceneWait(AsyncOperation Op)
	{
		while (Op.progress < 0.9f && !ForceLoad)
		{
			yield return null;
		}
		Op.allowSceneActivation = true;
	}
}
