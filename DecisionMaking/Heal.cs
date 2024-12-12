using UnityEngine;

public class Heal : MonoBehaviour
{
	public float healAmount = 1.0f;
	public float radius = 1.0f;
	public float minReappearTime = 2.0f;
	public float maxReappearTime = 5.0f;
	private WorldState WS;
	private bool used = false;

	void Start()
	{
		WS = WorldState.instance;
	}

	void Update()
	{
		if (Vector3.Distance(WS.player.transform.position, transform.position) < radius && !used)
		{
			WS.playerHealth += healAmount;
			used = true;
			gameObject.SetActive(false);
			Invoke("Reappear", Random.Range(minReappearTime, maxReappearTime));
		}

		if (Vector3.Distance(WS.ally.transform.position, transform.position) < radius && !used)
		{
			WS.allyHealth += healAmount;
			used = true;
			gameObject.SetActive(false);
			Invoke("Reappear", Random.Range(minReappearTime, maxReappearTime));
		}
	}

	void Reappear()
	{
		used = false;
		gameObject.SetActive(true);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}