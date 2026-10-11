namespace Client
{
    public class Item
    {
        public ItemType type;

        public Item(ItemType type)
        {
            this.type = type;
        }

        public virtual void Use()
        {
            
        }

        public virtual bool CanBeUsed() => false;
    }
}
