using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Server
{
    
    public class TileManager
    {
        public List<Tile> tiles = new List<Tile>();

        private Vector2Int gridSize;

        public void Initialize(Vector2Int gridSize)
        {
            this.gridSize = gridSize;

            int nextId = 0;
            
            int xOffset = gridSize.x / 2;
            int yOffset = gridSize.y /2;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int gridPosition = new(x - xOffset, y - yOffset);
                    tiles.Add(new Tile(nextId++, gridPosition));
                }
            }
        }

        public Tile GetTile(int tileId) => tiles.Find(id => id.tileId == tileId);
    }
    
    public class Tile
    {
        public int tileId;
        public Vector2Int position;

        public Tile(int tileId, Vector2Int position)
        {
            this.tileId = tileId;
            this.position = position;
        }

        public virtual void Tick(float deltaTime)
        {
            
        }
    }
}