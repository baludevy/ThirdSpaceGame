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

        public override void Use(Player player, uint tileId)
        {
            Debug.Log("seed");

            WorldManager worldManager = WorldManager.Instance;

            Tile targetTile = worldManager.tileManager.GetTile(tileId);

            if (targetTile.type != TileType.Soil || targetTile.occupant != null)
                return;

            if (worldManager.tileManager.SpawnTileObject(ObjectType.Crop, targetTile.position) is Crop crop)
                crop.cropType = cropType;

            player.inventory.Remove(this);
            ServerSend.UpdateInventory(player);
        }
    }
}
