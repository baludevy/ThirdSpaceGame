using Types;
using UnityEngine;

namespace Server.Tools
{
    public class Hoe : Tool
    {
        public Hoe(ItemType itemType, int count, bool stackable = true, int maxStackSize = 64) : base(itemType, count, stackable, maxStackSize)
        {

        }

        public override void Use(Player player, Vector3 tilePosition)
        {
            WorldManager world = WorldManager.Instance;

            if(world.tileManager.IsTileOccupied(tilePosition))
                return;
            
            if (world.tileManager.SpawnTileObject(ObjectType.Crop, tilePosition) is Crop crop )
                crop.cropType = CropType.Potato;
        }
    }
}
