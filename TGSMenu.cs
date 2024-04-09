using UnityEngine;
using UnityEngine.SceneManagement;

public class TGSMenu : MonoBehaviour
{
	public StateMachine StateMachine;

	public Animator UIAnimator;

	public Animator[] Characters;

	public Animator[] SelectorAnimators;

	public RectTransform CharSelector;

	public RectTransform[] CharOptions;

	private bool Started;

	private bool UsingYAxis;

	private bool StartYScrolling;

	private bool FastYScroll;

	private bool LoadStage;

	private float YAxis;

	private float AxisYTime;

	private float StartTime;

	private int Index;

	private int LastIndex;

	private void Start()
	{
		StateMenuStart();
		StateMachine.Initialize(StateMenu);
	}

	private void Update()
	{
		StateMachine.UpdateStateMachine();
		YAxis = 0f - Singleton<RInput>.Instance.P.GetAxis("Left Stick Y") + (0f - Singleton<RInput>.Instance.P.GetAxis("D-Pad Y"));
		if (YAxis == 0f)
		{
			UsingYAxis = false;
			StartYScrolling = false;
			FastYScroll = false;
			AxisYTime = Time.time;
		}
		else if (Time.time - AxisYTime > ((!FastYScroll) ? 0.25f : 0.1f) && !StartYScrolling)
		{
			FastYScroll = true;
			StartYScrolling = true;
			AxisYTime = Time.time;
			UsingYAxis = false;
			StartYScrolling = false;
		}
	}

	private void StateMenuStart()
	{
		Index = 0;
		OnOptionChange(Index);
	}

	private void StateMenu()
	{
		if (!Started)
		{
			if (!UsingYAxis && YAxis != 0f)
			{
				UsingYAxis = true;
				bool flag = false;
				if (YAxis < 0f && Index > 0)
				{
					Index--;
					flag = true;
				}
				if (YAxis > 0f && Index < 2)
				{
					Index++;
					flag = true;
				}
				if (flag)
				{
					OnOptionChange(Index);
				}
			}
			if (Singleton<RInput>.Instance.P.GetButtonDown("Button A") || Singleton<RInput>.Instance.P.GetButtonDown("Button X") || Singleton<RInput>.Instance.P.GetButtonDown("Start"))
			{
				StartTime = Time.time;
				LoadStage = true;
				UIAnimator.SetTrigger("On Fade Out");
				Started = true;
			}
			if (Singleton<RInput>.Instance.P.GetButtonDown("Button B"))
			{
				StartTime = Time.time;
				UIAnimator.SetTrigger("On Fade Out");
				Started = true;
			}
		}
		else
		{
			if (!(Time.time - StartTime > 0.15f))
			{
				return;
			}
			if (LoadStage)
			{
				switch (Index)
				{
				case 1:
					Singleton<GameManager>.Instance.SetGameStory("Shadow");
					Singleton<GameManager>.Instance.SetLoadingTo("kdv_tgsb_sd");
					break;
				case 2:
					Singleton<GameManager>.Instance.SetGameStory("Silver");
					Singleton<GameManager>.Instance.SetLoadingTo("kdv_tgsd_sv");
					break;
				default:
					Singleton<GameManager>.Instance.SetGameStory("Sonic");
					Singleton<GameManager>.Instance.SetLoadingTo("kdv_tgsa_sn");
					break;
				}
			}
			else
			{
				SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
			}
		}
	}

	private void StateMenuEnd()
	{
	}

	private void OnOptionChange(int Index)
	{
		for (int i = 0; i < SelectorAnimators.Length; i++)
		{
			SelectorAnimators[i].SetTrigger("On Change");
		}
		for (int j = 0; j < Characters.Length; j++)
		{
			Characters[j].SetBool("Is Active", Index == j);
		}
		switch (Index)
		{
		case 1:
			CharSelector.sizeDelta = new Vector2(249.25f, CharSelector.sizeDelta.y);
			break;
		case 2:
			CharSelector.sizeDelta = new Vector2(215f, CharSelector.sizeDelta.y);
			break;
		default:
			CharSelector.sizeDelta = new Vector2(190f, CharSelector.sizeDelta.y);
			break;
		}
		CharSelector.anchoredPosition = new Vector3(CharSelector.anchoredPosition.x, CharOptions[Index].anchoredPosition.y);
	}
}
