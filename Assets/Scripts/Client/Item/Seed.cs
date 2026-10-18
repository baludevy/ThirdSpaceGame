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
            
        }
        
        public override bool CanBeUsed() => true;
    }
}
