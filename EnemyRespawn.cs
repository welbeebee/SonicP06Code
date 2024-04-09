using UnityEngine;

public class EnemyRespawn : MonoBehaviour
{
	public EnemyBase Enemy;

	public GameObject ObjectManager;

	internal Vector3 StartPos;

	internal Quaternion StartRot;

	private EnemyBase PooledEnemy;

	private bool Respawn;

	private float RespawnTime;

	private void Start()
	{
		PoolEnemy();
	}

	private void Update()
	{
		if (!Enemy && !Respawn)
		{
			RespawnTime = Time.time;
			Respawn = true;
		}
		if (Respawn && Time.time - RespawnTime > 10f)
		{
			Enemy = PooledEnemy;
			Enemy.gameObject.SendMessage("Transfer", SendMessageOptions.DontRequireReceiver);
			Enemy.gameObject.SetActive(value: true);
			Enemy.transform.SetParent(ObjectManager.transform);
			ObjectManager.SendMessage("AddObject", SendMessageOptions.DontRequireReceiver);
			PooledEnemy = null;
			PoolEnemy();
			Respawn = false;
		}
	}

	private void PoolEnemy()
	{
		if (!PooledEnemy)
		{
			PooledEnemy = Object.Instantiate(Enemy.gameObject, StartPos, StartRot).GetComponent<EnemyBase>();
			PooledEnemy.Restart = Enemy.Restart;
			PooledEnemy.IsRespawn = Enemy.IsRespawn;
			PooledEnemy.gameObject.SetActive(value: false);
			PooledEnemy.transform.SetParent(base.transform);
		}
	}
}
