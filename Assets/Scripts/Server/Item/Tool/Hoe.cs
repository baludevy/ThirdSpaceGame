using Types;

namespace Server.Tools
{
    public class Hoe : Tool
    {
        public Hoe(ItemType itemType, int count, bool stackable = true, int maxStackSize = 64) : base(itemType, count, stackable, maxStackSize)
        {

        }

        public override void Use(Player player, uint tileId)
        {
            WorldManager world = WorldManager.Instance;

            Tile targetTile = world.tileManager.GetTile(tileId);
            targetTile.type = TileType.Soil;
            
            /* if (world.tileManager.SpawnTileObject(ObjectType.Crop, targetTile.position) is Crop crop)
                crop.cropType = CropType.Potato; */
            
            ServerSend.ChangeTile(targetTile);
        }
    }
}
