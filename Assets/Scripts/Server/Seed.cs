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
            WorldManager worldManager = WorldManager.Instance;

            Tile targetTile = worldManager.tileManager.GetTile(tileId);

            if (targetTile.tileType != TileType.Soil || targetTile.occupant != null)
                return;

            Crop crop = new Crop
            {
                ownerTile = targetTile,
                type = ObjectType.Crop,
                cropType = cropType,
                position = targetTile.position,
            };

            targetTile.occupant = crop;

            worldManager.objectManager.SpawnObject(crop);

            player.inventory.Remove(this);
            ServerSend.UpdateInventory(player);
        }
    }
}
