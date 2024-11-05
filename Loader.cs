using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject pathFinderPrefab;

    void Awake()
    {
        if (PathFinder.instance == null)
        {
            Instantiate(pathFinderPrefab);
        }
    }
}
