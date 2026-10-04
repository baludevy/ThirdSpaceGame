using Types;
using UnityEngine;

namespace Server
{
    public class Seed : Item
    {
        public CropType cropType;
        
        public Seed(ItemType itemType, int count, bool stackable = true, int maxStackSize = 64) : base(itemType, count, stackable, maxStackSize)
        {

        }

        public override void Use(Player player, int tileId)
        {
            Debug.Log("planting seed");
            
            WorldManager worldManager = WorldManager.Instance;
            
            Tile targetTile = worldManager.tileManager.GetTile(tileId);
            
            if(targetTile.tileType != TileType.Soil)
                return;
            
            worldManager.objectManager.SpawnObject(new Crop
            {
                type = ObjectType.Crop,
                cropType = cropType,
                position = targetTile.position,
            });

            player.inventory.Remove(type);
        }
    }
}
