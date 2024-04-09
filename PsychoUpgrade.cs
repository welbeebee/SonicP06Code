using UnityEngine;

public class PsychoUpgrade : ButtonIconBase
{
	public enum UpgradeType
	{
		Lotus,
		Flame,
		Sigil
	}

	
	public UpgradeType Type;

	[Header("Prefab")]
	public GameObject Mesh;

	public GameObject InterestPoint;

	public ParticleSystem GetFX;

	public AudioSource Audio;

	private bool Collected;

	private void Start()
	{
		if (HasUpgrade(Type.ToString()))
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !player.IsDead && player.GetPrefab("silver") && !Collected)
		{
			GameData.GlobalData gameData = Singleton<GameManager>.Instance.GetGameData();
			switch (Type)
			{
			case UpgradeType.Lotus:
				gameData.ActivateFlag(Game.LotusOfResilience);
				player.PlayerManager.silver.HasLotusOfResilience = true;
				player.Animator.SetBool("Has Lotus", player.PlayerManager.silver.HasLotusOfResilience);
				break;
			case UpgradeType.Flame:
				gameData.ActivateFlag(Game.FlameOfControl);
				player.PlayerManager.silver.HasFlameOfControl = true;
				break;
			case UpgradeType.Sigil:
				gameData.ActivateFlag(Game.SigilOfAwakening);
				player.PlayerManager.silver.HasSigilOfAwakening = true;
				break;
			}
			Singleton<GameManager>.Instance.SetGameData(gameData);
			player.HUD.StartMessageBox(GetText(GetUpgradeDesc()), null, 8f);
			player.HUD.UpdateActionGaugePanel();
			Mesh.SetActive(value: false);
			player.PlayerManager.silver.SilverEffects.PlayUpgradeGetFX();
			GetFX.Play();
			Audio.Play();
			Object.Destroy(InterestPoint);
			Object.Destroy(base.gameObject, 2f);
			Collected = true;
		}
	}

	private bool HasUpgrade(string Bool)
	{
		GameData.GlobalData gameData = Singleton<GameManager>.Instance.GetGameData();
		if (Type == UpgradeType.Sigil && (!gameData.HasFlag("csc_sv") || !gameData.HasFlag("tpj_sv") || !gameData.HasFlag("dtd_sv") || !gameData.HasFlag("wap_sv") || !gameData.HasFlag("rct_sv") || !gameData.HasFlag("aqa_sv") || !gameData.HasFlag("kdv_sv") || !gameData.HasFlag("flc_sv") || !gameData.HasFlag("wvo_bz")))
		{
			return true;
		}
		return Type switch
		{
			UpgradeType.Lotus => gameData.HasFlag(Game.LotusOfResilience), 
			UpgradeType.Flame => gameData.HasFlag(Game.FlameOfControl), 
			UpgradeType.Sigil => gameData.HasFlag(Game.SigilOfAwakening), 
			_ => gameData.HasFlag(Game.LotusOfResilience), 
		};
	}

	private string GetUpgradeDesc()
	{
		return Type switch
		{
			UpgradeType.Lotus => "LotusOfResilience", 
			UpgradeType.Flame => "FlameOfControl", 
			UpgradeType.Sigil => "SigilOfAwakening", 
			_ => "LotusOfResilience", 
		};
	}
}
