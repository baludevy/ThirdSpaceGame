using System;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    [Serializable]
    public class ItemDefinition
    {
        public string Name;
        public ItemType Type;
        public bool Stackable = true;
        public int MaxStackSize = 64;
        public Sprite Sprite;
        public GameObject DroppedPrefab;
    }

    [Serializable]
    public class PetDefinition
    {
        public string Name;
        public PetType Type;
        public GameObject Prefab;
    }

    public class Catalog : MonoBehaviour
    {
        public static Catalog Instance;

        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();
        [SerializeField] private List<PetDefinition> pets = new List<PetDefinition>();

        public ItemDefinition GetItem(ItemType itemType) => items.Find(i => i.Type == itemType);
        public PetDefinition GetPet(PetType petType) => pets.Find(i => i.Type == petType);

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
