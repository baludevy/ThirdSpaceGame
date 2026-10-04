using UnityEngine;

namespace Client
{
    public class Tool : Item
    {
        public Tool(ItemType type) : base(type)
        {
            this.type = type;
        }

        public override void Use()
        {
            Debug.Log("used tool");
        }
        
        public override bool CanBeUsed() => true;
    }
}
