using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Types;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game
{
    public class GlobalTilemap : MonoBehaviour
    {
        public static GlobalTilemap Instance;

        public Tilemap tilemap;

        [SerializeField] public List<TileBase> grassTiles = new List<TileBase>();
        [SerializeField] private List<TileBase> soilSprites = new List<TileBase>();

        [SerializeField] public Vector2Int grassPatternOrigin;

        private static int PositiveMod(int value, int size)
        {
            return (value % size + size) % size;
        }
        
        [SerializeField] public Dictionary<TileType, TileBase> tileSprites = new Dictionary<TileType, TileBase>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public TileType GetTileType(Vector2Int position)
        {
            TileBase tileAsset = GlobalTilemap.Instance.tilemap.GetTile(new Vector3Int(position.x, position.y, 0));

            if (tileAsset == null) return TileType.Empty;

            if (grassTiles.Contains(tileAsset)) return TileType.Grass;

            return TileType.Unknown;
        }
    }
}
