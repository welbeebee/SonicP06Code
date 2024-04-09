using UnityEngine;

public class WapConifer : MonoBehaviour
{
	[Header("Prefab")]
	public Animator Animator;

	private void OnCollisionEnter(Collision collision)
	{
		if ((collision.gameObject.tag == "Player" || collision.gameObject.tag == "Amigo" || collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("BrokenObj")) && !Animator.GetCurrentAnimatorStateInfo(0).IsName("wap_obj_tree_bump") && collision.relativeVelocity.magnitude > 5f)
		{
			Animator.SetTrigger("On Bump");
		}
	}
}
