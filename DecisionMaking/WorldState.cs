using UnityEngine;

public class WorldState : MonoBehaviour
{
	public Kinematic[] items;
	public Kinematic[] enemies;
	public Kinematic player;

	#region Singleton
	public static WorldState instance;
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(this);
		}
	}
	#endregion
}