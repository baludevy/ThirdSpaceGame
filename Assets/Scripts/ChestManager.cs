using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour {
    public Dictionary<int, ChestScript> chests = new Dictionary<int, ChestScript>();
    
    public static ChestManager Instance;
    
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

    public void RegisterChest(int chestId, ChestScript chest) {
        chests[chestId] = chest;
    }
}