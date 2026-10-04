using System.Data.Common;

namespace Server
{
    
    public class Barrel : Object
    {
        public Inventory inventory = new Inventory(0, 27);

        public override void Start()
        {
            inventory.containerId = id;
        }
    }
}