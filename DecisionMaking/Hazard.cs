using UnityEngine;

public class Hazard : MonoBehaviour
{
	public float damage = 1.0f;
	public float radius = 1.0f;
	private WorldState WS;

	void Start()
	{
		WS = WorldState.instance;
	}
	void Update()
	{
		if (Vector3.Distance(WS.player.transform.position, transform.position) < radius && !WS.playerInvulnerable)
		{
			WS.playerHealth -= damage;
			WS.SetInvulnerability(WS.player);
		}

		if (Vector3.Distance(WS.ally.transform.position, transform.position) < radius && !WS.allyInvulnerable)
		{
			WS.allyHealth -= damage;
			WS.SetInvulnerability(WS.ally);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}