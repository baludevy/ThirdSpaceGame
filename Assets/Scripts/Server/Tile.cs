using Types;
using UnityEngine;

namespace Server
{
    public class Tile
    {
        public uint id;
        
        // x and z is rounded y is not
        public Vector3 position;
        public TileType type;
        
        public Object occupant;

        public void Tick(float deltaTime)
        {
            if (occupant is Crop crop)
            {
                crop.lastGrowTime += deltaTime;

                if (crop.lastGrowTime >= 5f && crop.cropPhase < 2)
                {
                    Debug.Log("crop grew");
                    crop.cropPhase++;
                    crop.lastGrowTime = 0f;
                    
                    ServerSend.UpdateCrop(crop.id, crop.cropPhase);
                }
            }
        }
    }
}
