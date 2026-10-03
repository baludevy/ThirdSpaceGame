using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Game
{
    public class TileGrid : MonoBehaviour
    {
        private List<Tile> tiles = new List<Tile>();

        public Vector2Int gridSize = new Vector2Int(101, 101);

        [SerializeField] private Transform previewSquare;
        [SerializeField] private Camera playerCamera;

        private void Awake()
        {
            int nextId = 0;

            int xOffset = gridSize.x /2;
            int yOffset = gridSize.x /2;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int gridPosition = new(x - xOffset, y - yOffset);
                    tiles.Add(new Tile(nextId++, gridPosition));
                }
            }
        }

        private void Update()
        {
            MovePreviewToMouseTile();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2Int tilePosition = GetHoveredTilePosition();

                Debug.Log($"clicked {tilePosition}");
            }
        }

        private void MovePreviewToMouseTile()
        {
            Vector2Int tilePosition = GetHoveredTilePosition();

            if (!IsInsideGrid(tilePosition))
            {
                previewSquare.gameObject.SetActive(false);
                return;
            }

            previewSquare.gameObject.SetActive(true);

            previewSquare.position = transform.TransformPoint(new Vector3(tilePosition.x + 0.5f, tilePosition.y + 0.5f, 0));
        }

        private Vector2Int GetHoveredTilePosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            UnityEngine.Vector3 mouseScreenPosition = new(mousePosition.x, mousePosition.y, - playerCamera.transform.position.z);
            Vector3 mouseWorldPosition = playerCamera.ScreenToWorldPoint(mouseScreenPosition);
            Vector3 localMousePosition = transform.InverseTransformPoint(mouseWorldPosition);

            Vector2Int tilePosition = new(Mathf.FloorToInt(localMousePosition.x),
                Mathf.FloorToInt(localMousePosition.y));
            
            return tilePosition;
        }

        private bool IsInsideGrid(Vector2Int position)
        {
            int minX = -gridSize.x / 2;
            int maxX = gridSize.x - 1 - gridSize.x / 2;

            int minY = -gridSize.y / 2;
            int maxY = gridSize.y - 1 - gridSize.y /2;

            return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
        }
    }


    public class Tile
    {
        public int titleId;
        public Vector2Int position;

        public Tile(int tileId, Vector2Int position)
        {
            this.titleId = tileId;
            this.position = position;
        }
    }
}