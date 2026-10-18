using System;
using System.Collections.Generic;
using Server;
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

    [Serializable]
    public class CropDefinition
    {
        public string Name;
        public CropType Type;
        public Sprite[] Sprite;
    }

    public class Catalog : MonoBehaviour
    {
        public static Catalog Instance;

        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();
        [SerializeField] private List<PetDefinition> pets = new List<PetDefinition>();
        [SerializeField] private List<CropDefinition> crops = new List<CropDefinition>();

        public ItemDefinition GetItem(ItemType itemType) => items.Find(i => i.Type == itemType);
        public PetDefinition GetPet(PetType petType) => pets.Find(i => i.Type == petType);
        public CropDefinition GetCrop(CropType cropType) => crops.Find(i => i.Type == cropType);

        public GameObject soilPrefab;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
