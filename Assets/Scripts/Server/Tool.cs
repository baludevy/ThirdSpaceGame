namespace Server
{
    public class Tool : Item
    {
        protected Tool(ItemType itemType, int count, bool stackable = true, int maxStackSize = 64) : base(itemType, count, stackable, maxStackSize)
        {
            
        }
    }
}
