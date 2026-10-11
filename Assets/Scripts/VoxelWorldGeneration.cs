using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

[DisallowMultipleComponent]
public class VoxelWorldGeneration : MonoBehaviour
{
    public enum Block : byte
    {
        Grass,
        Dirt,
        Water
    }

    public enum Biome : byte
    {
        Plains
    }

    [Serializable]
    public sealed class GenerationSettings
    {
        public int seed = 12345;
        [Range(8, 32)] public int chunkSize = 16;
        [Range(32, 256)] public int height = 128;
        public int seaLevel = 42;
        public int baseHeight = 48;
        [Range(1f, 70f)] public float relief = 36f;
        [Range(0.0001f, 0.05f)] public float continentScale = 0.0025f;
        [Range(0.001f, 0.1f)] public float hillScale = 0.015f;
        [Range(0.0001f, 0.1f)] public float biomeScale = 0.003f;
        public bool caves = true;
        [Range(0.01f, 0.15f)] public float caveScale = 0.045f;
        [Range(0.03f, 0.25f)] public float caveWidth = 0.10f;
        public bool trees = true;
        [Range(0f, 1f)] public float treeDensity = 0.65f;
        [Range(0.005f, 0.12f)] public float riverWidth = 0.025f;
    }

    [Header("Generation")]
    public GenerationSettings generation = new GenerationSettings();
    public bool generateOnStart = true;
    [Range(1, 12)] public int viewDistance = 3;
    [Range(0, 4)] public int unloadMargin = 1;
    [Range(1, 64)] public int columnsPerFrame = 12;
    [Range(1, 4096)] public int meshRowsPerFrame = 512;
    [Header("Rendering")]
    public Material blockMaterial;
    public bool generateColliders = true;
    public bool castShadows = true;
    [Header("Navigation")]
    public Transform target;
    public Camera interactionCamera;
    public bool createDemoCamera = true;
    public bool createDemoLight = true;
    public float flyspeed = 14f;
    public float sprintMultiplier = 3f;
    public float lookSensitivity = 0.12f;
    [Header("Editing")]
    public bool allowEditing = true;
    public float reach = 8f;
    public Block selected = Block.Grass;
    [Range(1, 256)] public int undoLimit = 64;
    public bool showHud = true;
    [Header("Persistence")]
    public string saveName = "voxel-world";
    public bool loadOnStart = true;
    public bool saveOnQuit = true;

    public event Action<Vector3Int, Block, Block> BlockChanged;
    public event Action<Vector2Int> ChunkReady;
    public int ModifiedBlockCount { get { return edits.Count;}}

    public string LastError { get; private set;}

    private const int SaveMagic = 0x56584C31;
    private const int SaveVersion = 1;
    private const int CoordinateLimit = 1000000;
    private const int MaximumSaveEdits = 4000000;
    private const int AtlasColumn = 16;
    private const int AtlasRows = 4;
    private const int TilePixels = 16;
    private readonly Dictionary<Vector3Int, Block> edits = new Dictionary<Vector3Int, Block>();
    private readonly Dictionary<Vector2Int, Dictionary<Vector3Int, Block>> editBuckets = new Dictionary<Vector2Int, Dictionary<Vector3Int, Block>>(); 
    private readonly HashSet<Vector2Int> dirty = new HashSet<Vector2Int>();
    private readonly List<Vector2Int> pending = new List<Vector2Int>();

}
