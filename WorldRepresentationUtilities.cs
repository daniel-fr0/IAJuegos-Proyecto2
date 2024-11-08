using System;
using UnityEngine;

[Serializable]
public class WorldLevel
{
    public WorldConnection[] connections;
}

[Serializable]
public class WorldConnection
{
    public RectTransform fromRectTransform;
    public RectTransform toRectTransform;

	public Vector3 from;
	public Vector3 to;
}