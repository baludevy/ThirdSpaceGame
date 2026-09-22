using Server;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(this);
    }

    public World GetWorld()
    {
        return new World
        {
            entities = EntityManager.Instance.GetEntities()
        };
    }
}