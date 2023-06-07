using Managers;
using Pathfinding;
using UnityEngine;

public class ServiceProvider : MonoBehaviour
{
    private static ServiceProvider _instance;
    public static ServiceProvider Instance { get { return _instance; } private set { _instance = value; } }
    private void Awake()
    {

        if (FindObjectsOfType<ServiceProvider>().Length > 1)
        {
            Debug.Log("Found another instance of ServiceProvider. Deleting");
            Destroy(this.gameObject);
        }
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

    }

    public GridManager GridManager;
    public BlockManager BlockManager;
}
