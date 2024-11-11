using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
	public List<Kinematic> items;
	public List<Kinematic> hostile;
	public List<Kinematic> friendly;
	public Kinematic player;
	public List<Node> safeZones;

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