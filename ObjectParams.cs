using System;
using UnityEngine;

[Serializable]
public class ObjectParams
{
	public enum Type
	{
		Enable,
		Disable,
		Switch
	}

	public GameObject Object;

	public string OptionalMessage;

	public Type Mode;

	public float Timer;

	public float SwitchTimer;

	public string OptionalSwitchMessage;

	public Transform NewParent;

	public string OptionalParentMessage;
}
