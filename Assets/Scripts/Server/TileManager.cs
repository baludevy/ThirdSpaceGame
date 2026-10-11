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

            tiles.Clear();
            updatedTiles.Clear();
            changedTiles.Clear();

            int nextId = 0;
            int xOffset = gridSize.x / 2;
            int zOffset = gridSize.y / 2;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    Vector2Int gridPosition = new Vector2Int(
                        x - xOffset,
                        z - zOffset
                    );

                    tiles.Add(new Tile(
                        nextId++,
                        gridPosition,
                        TileType.Grass
                    ));
                }
            }
        }

        public void ModifyTile(int tileId, TileType tileType)
        {
            Tile targetTile = GetTile(tileId);

            if (targetTile == null)
                return;

            targetTile.tileType = tileType;

            if (!updatedTiles.Contains(targetTile))
                updatedTiles.Add(targetTile);

            if (!changedTiles.Contains(targetTile))
                changedTiles.Add(targetTile);
        }

        public Tile GetTile(int tileId) => tiles.Find(id => id.tileId == tileId);
    }

    public class Tile
    {
        public int tileId;
        public Vector2Int position;
        public TileType tileType;

        public Object occupant;

        public Tile(int tileId, Vector2Int position, TileType tileType)
        {
            this.tileId = tileId;
            this.position = position;
            this.tileType = tileType;
        }

        public virtual void Tick(float deltaTime)
        {
            if (occupant is Crop crop)
            {
                if (crop.lastGrowTime > 200 && crop.cropPhase < 2)
                {
                    crop.cropPhase++;
                    ServerSend.UpdateCrop(crop.id, crop.cropPhase);
                    crop.lastGrowTime = 0;
                }

                crop.lastGrowTime++;
            }
        }
    }
}
