using System.Collections.Generic;
using Game;
using Types;
using UnityEngine;

namespace Server
{
    public class TileManager
    {
        public List<Tile> tiles = new List<Tile>();
        public List<Tile> updatedTiles = new List<Tile>();
        public List<Tile> changedTiles = new List<Tile>();

        private Vector2Int gridSize;

        public void Initialize(Vector2Int gridSize)
        {
            this.gridSize = gridSize;

            int nextId = 0;

            int xOffset = gridSize.x / 2;
            int yOffset = gridSize.y / 2;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int gridPosition = new Vector2Int(x - xOffset, y - yOffset);

                    tiles.Add(new Tile(nextId++, gridPosition, GlobalTilemap.Instance.GetTileType(gridPosition)));
                }
            }
        }

        public void ModifyTile(int tileId, TileType tileType)
        {
            Tile targetTile = GetTile(tileId);

            targetTile.tileType = tileType;
            updatedTiles.Add(targetTile);
            
            if(changedTiles.Contains(targetTile)) 
                changedTiles.Find(tile => tile == targetTile).tileType = tileType;
            else
                changedTiles.Add(targetTile);
        }

        public Tile GetTile(int tileId) => tiles.Find(id => id.tileId == tileId);
    }

    public class Tile
    {
        public int tileId;
        public Vector2Int position;
        public TileType tileType;

        public Tile(int tileId, Vector2Int position, TileType tileType)
        {
            this.tileId = tileId;
            this.position = position;
            this.tileType = tileType;
        }

        public virtual void Tick(float deltaTime)
        {

        }
    }
}
