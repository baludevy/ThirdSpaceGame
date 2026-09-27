public enum ItemType
{
    carrot,
}

namespace Server
{
    public class DroppedItem : Entity
    {
        public ItemType itemType;
    }
}