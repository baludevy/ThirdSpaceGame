using Types;
using UnityEngine;

namespace Client
{
    public class Tile
    {
        public uint id;
        
        // x and z is rounded y is not
        public Vector3 position;
        public TileType type;
        
        public Object occupant;
    }
}
