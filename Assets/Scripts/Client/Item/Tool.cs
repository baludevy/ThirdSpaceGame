using Game;
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
            Vector3 tilePosition = GridSelect.Instance.cellCenter;
            Debug.Log(tilePosition);
            
            if(GridSelect.Instance.hasValidPosition)
                ClientSend.UseHeldItem(tilePosition);
        }
        
        public override bool CanBeUsed() => true;
    }
}
