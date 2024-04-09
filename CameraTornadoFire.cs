using UnityEngine;

public class CameraTornadoFire : MonoBehaviour
{
	
	public ParticleSystem FX;

	internal Transform Player;

	internal Transform Tornado;

	private void Start()
	{
		FX.gameObject.SetActive(value: true);
	}

	private void Update()
	{
		ParticleSystem.ShapeModule shape = FX.shape;
		shape.radius = Mathf.Lerp(0.375f, 0.85f, Mathf.Clamp01(((Player.position - Tornado.position).magnitude - 70f) / 30f));
	}
}
