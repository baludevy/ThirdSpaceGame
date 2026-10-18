using System;
using Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Server
{
    public class Crop : Object, Interactable
    {
        public Tile ownerTile;
        public CropType cropType;
        public int cropPhase;

        public float lastGrowTime;

        public void Interact(Player player)
        {
            ItemDefinition definition = Catalog.Instance.GetItem(ItemType.Carrot);

            if (cropType == CropType.Potato)
                definition = Catalog.Instance.GetItem(ItemType.Potato);
            
            Item item = CreateItem(definition, Random.Range(1, 3));
            Vector3 pos = position + Vector3.back + Vector3.up * 0.5f;

            if (WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, pos, false) is not DroppedItem droppedItem)
                return;
            
            droppedItem.item = item;
            
            WorldManager.Instance.objectManager.DestroyObject(id);
            ServerSend.SpawnEntity(droppedItem);
        }
        
        public static Item CreateItem(ItemDefinition item, int count)
        {
            return item.Type switch
            {
                ItemType.Potato => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Potato },
                ItemType.Carrot => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Carrot },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool CanInteract() => cropPhase == 2;
    }
}
