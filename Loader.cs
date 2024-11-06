using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject PathFinderManagerPrefab;

    void Awake()
    {
        if (PathFinderManager.instance == null)
        {
            Instantiate(PathFinderManagerPrefab);
        }
    }
}
