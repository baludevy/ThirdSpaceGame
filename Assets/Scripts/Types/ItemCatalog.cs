using System;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    [Serializable]
    public class ItemDefinition
    {
        public String Name;
        public ItemType Type;
        public bool Stackable = true;
        public int MaxStackSize = 64;
        public Sprite Sprite;
    }

    public class ItemCatalog : MonoBehaviour
    {
        public static ItemCatalog Instance;

        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

        public ItemDefinition GetItem(ItemType itemType) => items.Find(i => i.Type == itemType);

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
