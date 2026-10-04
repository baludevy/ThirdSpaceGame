using Types;

namespace Server.Tools
{
    public class Hoe : Tool
    {
        public Hoe(ItemType itemType, int count, bool stackable = true, int maxStackSize = 64) : base(itemType, count, stackable, maxStackSize)
        {

        }

        public override void Use(Player player, int tileId)
        {
            Tile targetTile = WorldManager.Instance.tileManager.GetTile(tileId);

            if (targetTile.tileType == TileType.Grass)
                WorldManager.Instance.tileManager.ModifyTile(tileId, TileType.Soil);
            else if (targetTile.tileType == TileType.Soil)
                WorldManager.Instance.tileManager.ModifyTile(tileId, TileType.Grass);
        }
    }
}
