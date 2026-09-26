using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientChestManager : MonoBehaviour
{

    public static ClientChestManager Instance;
    [NonSerialized]
    public Dictionary<int, ChestScript> chests = new Dictionary<int, ChestScript>();
    
    [SerializeField]
    public Dictionary<ItemType, GameObject> itemTypePrefabs = new Dictionary<ItemType, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnDroppedItem(ItemType itemType, Vector3 position)
    {
        GameObject droppedItem = Instantiate(Instance.itemTypePrefabs[itemType], position, Quaternion.identity);
        droppedItem.GetComponent<BounceEffect>().Startbounce();
    }

    public void RegisterChest(int chestId, ChestScript chest)
    {
        chests[chestId] = chest;
    }
}
