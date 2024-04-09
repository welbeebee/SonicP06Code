using UnityEngine;

public class BrokenStairs : ObjectBase
{
	
	public float Time;

	public bool Fallen;

	[Header("Prefab")]
	public int AnimationIndex;

	public GameObject Object;

	public Collider Collider;

	public Animator Animator;

	public AudioSource Audio;

	public AudioClip CrackAudio;

	public ParticleSystem[] FX;

	private bool Broken;

	private bool PlayAnimation;

	private float StartTime;

	public void SetParameters(float _Time)
	{
		Time = _Time;
	}

	private void Start()
	{
		if (Fallen && Object.activeSelf)
		{
			Object.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (Broken && UnityEngine.Time.time - StartTime > Time && !PlayAnimation)
		{
			Audio.pitch = 1f;
			Audio.Play();
			Animator.SetTrigger("Play");
			Animator.SetTrigger("Stop Shake");
			FX[1].Play();
			PlayAnimation = true;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!Broken && !Fallen)
		{
			PlayerBase player = GetPlayer(collision.transform);
			if ((bool)player && player.IsGrounded() && !(player.RaycastHit.collider != Collider))
			{
				StartTime = UnityEngine.Time.time;
				Audio.pitch = Random.Range(0.75f, 1.25f);
				Audio.PlayOneShot(CrackAudio, 0.75f);
				Animator.SetInteger("Index", AnimationIndex);
				Animator.SetTrigger("Start Shake");
				FX[0].Play();
				Broken = true;
			}
		}
	}
}
