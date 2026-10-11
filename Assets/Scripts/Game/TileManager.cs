using System;
using System.Collections.Generic;
using Client;
using Types;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory = Client.Inventory;

namespace Game
{
    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance;

        public Vector2Int gridSize = new Vector2Int(101, 101);

        [Header("Blocks")]
        [SerializeField] private TileType startingTileType = TileType.Grass;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private TileMaterial[] tileMaterials;

        [Header("Selection")]
        [SerializeField] private Transform previewSquare;
        [SerializeField] private float raycastDistance = 1000f;
        public Camera playerCamera;

        private readonly List<Tile> tiles = new List<Tile>();
        private readonly Dictionary<Collider, Tile> colliderTiles =
            new Dictionary<Collider, Tile>();

        private readonly Dictionary<TileType, Material> materials =
            new Dictionary<TileType, Material>();

        [Serializable]
        public class TileMaterial
        {
            public TileType tileType;
            public Material material;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (tileMaterials != null)
            {
                foreach (TileMaterial entry in tileMaterials)
                {
                    if (entry != null && entry.material != null)
                        materials[entry.tileType] = entry.material;
                }
            }

            // The preview should never intercept selection raycasts.
            if (previewSquare != null)
            {
                foreach (Collider collider in
                         previewSquare.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = false;
                }

                previewSquare.gameObject.SetActive(false);
            }

            CreateBlocks();
        }

        private void CreateBlocks()
        {
            int xOffset = gridSize.x / 2;
            int zOffset = gridSize.y / 2;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    Vector2Int position =
                        new Vector2Int(x - xOffset, z - zOffset);

                    GameObject block =
                        GameObject.CreatePrimitive(PrimitiveType.Cube);

                    block.name = $"Block {position.x}, {position.y}";
                    block.transform.SetParent(transform, false);
                    
                    block.transform.localPosition = new Vector3(
                        position.x + 0.5f,
                        -0.5f,
                        position.y + 0.5f
                    );

                    Tile tile = new Tile(
                        tiles.Count,
                        position,
                        startingTileType,
                        block.GetComponent<Renderer>()
                    );

                    tiles.Add(tile);
                    colliderTiles.Add(block.GetComponent<Collider>(), tile);

                    ApplyMaterial(tile);
                }
            }
        }

        private void Update()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            if (playerCamera == null || Mouse.current == null)
            {
                UpdatePreview(null);
                return;
            }

            Tile hoveredTile = GetHoveredTile();
            UpdatePreview(hoveredTile);
            
            if (hoveredTile == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (Inventory.Instance == null ||
                Inventory.Instance.currentItem == null ||
                !Inventory.Instance.currentItem.CanBeUsed())
                return;

            ClientSend.UseHeldItem(hoveredTile.tileId);
        }

        private Tile GetHoveredTile()
        {
            Ray ray = playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    raycastDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            colliderTiles.TryGetValue(hit.collider, out Tile tile);
            return tile;
        }

        private void UpdatePreview(Tile tile)
        {
            if (previewSquare == null)
                return;

            previewSquare.gameObject.SetActive(tile != null);

            if (tile == null)
                return;

            previewSquare.position = transform.TransformPoint(
                new Vector3(
                    tile.position.x + 0.5f,
                    0.02f,
                    tile.position.y + 0.5f
                )
            );

            previewSquare.rotation = transform.rotation;
        }

        public void ModifyTile(int tileId, TileType tileType)
        {
            if (tileId < 0 || tileId >= tiles.Count)
                return;

            Tile tile = tiles[tileId];
            tile.tileType = tileType;

            ApplyMaterial(tile);
        }

        private void ApplyMaterial(Tile tile)
        {
            Material material;

            if (!materials.TryGetValue(tile.tileType, out material))
                material = defaultMaterial;

            if (material != null)
                tile.blockRenderer.sharedMaterial = material;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    public class Tile
    {
        public int tileId;
        public Vector2Int position;
        public TileType tileType;
        public Renderer blockRenderer;

        public Tile(
            int tileId,
            Vector2Int position,
            TileType tileType,
            Renderer blockRenderer)
        {
            this.tileId = tileId;
            this.position = position;
            this.tileType = tileType;
            this.blockRenderer = blockRenderer;
        }
    }
}