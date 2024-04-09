using UnityEngine;

public class BoardRampIndicator : ObjectBase
{
	public enum Type
	{
		Blue,
		Red
	}

	
	public Type Color;

	[Header("Prefab")]
	public GameObject[] Arrows;

	private bool Triggered;

	private void OnTriggerEnter(Collider collider)
	{
		PlayerBase player = GetPlayer(collider);
		if ((bool)player && !player.IsDead && !Triggered)
		{
			Arrows[(int)Color].SetActive(value: true);
			Object.Destroy(base.gameObject, 1f);
			Triggered = true;
		}
	}
}
