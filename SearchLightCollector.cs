using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchLightCollector : MonoBehaviour
{
	
	public List<SearchLight> Towers;

	public GameObject[] ObjectGroup;

	public float ExecutionTime;

	private bool DestroyedAll;

	private void Update()
	{
		if (DestroyedAll)
		{
			return;
		}
		if (Towers.Count != 0)
		{
			for (int i = 0; i < Towers.Count; i++)
			{
				if (!Towers[i])
				{
					Towers.RemoveAt(i);
				}
			}
		}
		if ((Towers.Count == 0 || Towers == null || IsAllHeadsDestroyed()) && !DestroyedAll)
		{
			StartCoroutine(Execute());
			DestroyedAll = true;
		}
	}

	private bool IsAllHeadsDestroyed()
	{
		for (int i = 0; i < Towers.Count; i++)
		{
			if (!Towers[i].DestroyedHead)
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator Execute()
	{
		yield return new WaitForSeconds(ExecutionTime);
		for (int i = 0; i < ObjectGroup.Length; i++)
		{
			ObjectGroup[i].SetActive(!ObjectGroup[i].activeSelf);
		}
		Object.FindObjectOfType<StageManager>().StageState = StageManager.State.Event;
		Object.FindObjectOfType<PlayerBase>().Camera.CanCancelCinematic = true;
	}
}
