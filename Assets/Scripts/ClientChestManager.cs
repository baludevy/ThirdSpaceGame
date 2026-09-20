using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientChestManager : MonoBehaviour {
    [NonSerialized]
    public Dictionary<int, ChestScript> chests = new Dictionary<int, ChestScript>();
    
    [SerializeField] public Dictionary<ItemType, GameObject> itemTypePrefabs = new Dictionary<ItemType, GameObject>();
    
    public static ClientChestManager Instance;
    
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
    
    public void SpawnDroppedItem(GameObject itemPrefab, Vector3 position) {
        GameObject droppeditem = Instantiate(itemPrefab, position, Quaternion.identity);
        droppeditem.GetComponent<BounceEffect>().Startbounce();
    }

    public void RegisterChest(int chestId, ChestScript chest) {
        chests[chestId] = chest;
    }
}