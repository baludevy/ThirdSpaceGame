using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Server
{
    public class TileManager
    {
        public List<Tile> tiles = new List<Tile>();

        public void GenerateTiles()
        {
            tiles.Clear();
            uint id = 0;

            foreach (Ground ground in UnityEngine.Object.FindObjectsByType<Ground>())
            {
                Collider collider = ground.GetComponent<Collider>();
                if (collider == null) continue;

                Bounds bounds = collider.bounds;

                for (int x = Mathf.CeilToInt(bounds.min.x); x < bounds.max.x; x++)
                {
                    for (int z = Mathf.CeilToInt(bounds.min.z); z < bounds.max.z; z++)
                    {
                        tiles.Add(new Tile
                        {
                            id = id++,
                            position = new Vector3(x, bounds.max.y, z)
                        });
                    }
                }
            }
        }

        private static Vector2Int GetTile(Vector3 position) => new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));

        private Tile FindTile(Vector3 position)
        {
            Vector2Int coordinates = GetTile(position);
            return tiles.Find(t => GetTile(t.position) == coordinates);
        }

        public Object SpawnTileObject(ObjectType type, Vector3 position = default)
        {
            Tile tile = FindTile(position);
            if (tile == null || tile.occupant != null) return null;

            position = new Vector3(tile.position.x, position.y, tile.position.z);

            Crop crop = new Crop
            {
                id = WorldManager.Instance.objectManager.nextObjectId++,
                type = type,
                position = position
            };

            tile.occupant = WorldManager.Instance.objectManager.SpawnObject(crop);

            return tile.occupant;
        }

        public Object GetObjectAtTilePosition(Vector3 position) => FindTile(position)?.occupant;

        public bool IsTileOccupied(Vector3 position) => GetObjectAtTilePosition(position) != null;
        public bool IsTileOccupied(uint tileId) => GetTile(tileId).occupant != null;

        public Tile GetTile(uint tileId) => tiles.Find(t => t.id == tileId);
    }
}
