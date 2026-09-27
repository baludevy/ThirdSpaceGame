using System;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance;


    [SerializeField]
    public Dictionary<ItemType, GameObject> itemPrefabs = new Dictionary<ItemType, GameObject>();
    [SerializeField]
    public Dictionary<PetType, GameObject> petPrefabs = new Dictionary<PetType, GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
