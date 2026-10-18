namespace Client
{
    public class Seed : Item
    {
        public Seed(ItemType type) : base(type)
        {
            this.type = type;
        }
        
        public override void Use()
        {
            Game.GridSelect selection = Game.GridSelect.Instance;
            Game.TileManager tileManager = Game.TileManager.Instance;

            if (selection == null || !selection.hasValidPosition || tileManager == null)
                return;

            Tile tile = tileManager.GetTileAtPosition(selection.cellCenter);
            if (tile == null)
                return;

            ClientSend.UseHeldItem(tile.id);
        }
        
        public override bool CanBeUsed() => true;
    }
}
