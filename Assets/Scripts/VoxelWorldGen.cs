using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using NUnit.Framework;
using System.Runtime.InteropServices;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class MinecraftVoxelWorld : MonoBehaviour
{
    public enum Block : byte
    {
        Air,
        Grass,
        Dirt,
        Stone,
        Sand,
        Water,
        Log,
        Leaves,
        Snow,
        Ice,
        Coal,
        Iron,
        Copper,
        Gold,
        Bedrock,
        Planks
    }

    public enum Biome : byte
    {
        Ocean,
        Plains,
        Forest,
        Desert,
        Mountains,
        Tundra
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
        [Range(0.0001f, 0.02f)] public float biomeScale = 0.003f;
        public bool caves = true;
        [Range(0.01f, 0.15f)] public float caveScale = 0.045f;
        [Range(0.03f, 0.25f)] public float caveWidth = 0.10f;
        public bool trees = true;
        [Range(0f, 1f)] public float treeDensity = 0.65f;
        public bool ores = true;
        public bool rivers = true;
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
    public float flySpeed = 14f;
    public float sprintMultiplier = 3f;
    public float lookSensitivity = 0.12f;
    [Header("Editing")]
    public bool allowEditing = true;
    public float reach = 8f;
    public Block selectedBlock = Block.Grass;
    [Range(1, 256)] public int undoLimit = 64;
    public bool showHud = true;
    [Header("Persistence")]
    public string saveName = "voxel-world";
    public bool loadOnStart = true;
    public bool saveOnQuit = true;

    public event Action<Vector3Int, Block, Block> BlockChanged;
    public event Action<Vector2Int> ChunkReady;
    public int LoadedChunkCount { get { return chunks.Count; } }
    public int ModifiedBlockCount { get { return edits.Count; } }
    public bool IsGenerating
    {
        get
        {
            if (pending.Count > 0 || dirty.Count > 0) return true;
            foreach (Chunk chunk in chunks.Values)
            {
                if (!chunk.dataReady || !chunk.meshReady) return true;
            }
            return false;
        }
    }
    public string LastError { get; private set; }

    private const int SaveMagic = 0x56584C31;
    private const int SaveVersion = 1;
    private const int CoordinateLimit = 1000000;
    private const int MaximumSaveEdits = 4000000;
    private const int AtlasColumns = 16;
    private const int AtlasRows = 4;
    private const int TilePixels = 16;
    private readonly Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private readonly Dictionary<Vector3Int, Block> edits = new Dictionary<Vector3Int, Block>();
    private readonly Dictionary<Vector2Int, Dictionary<Vector3Int, Block>> editBuckets = new Dictionary<Vector2Int, Dictionary<Vector3Int, Block>>();
    private readonly HashSet<Vector2Int> dirty = new HashSet<Vector2Int>();
    private readonly List<Vector2Int> pending = new List<Vector2Int>();
    private readonly List<Vector2Int> removal = new List<Vector2Int>();
    private readonly Stack<EditCommand> undo = new Stack<EditCommand>();
    private readonly Stack<EditCommand> redo = new Stack<EditCommand>();
    private readonly List<Tree> treeBuffer = new List<Tree>();
    private readonly List<BlockChange> batch = new List<BlockChange>();
    private GenerationSettings settings;
    private Coroutine worker;
    private Material ownedMaterial;
    private Texture2D atlas;
    private GameObject demoCameraObject;
    private GameObject demoLightObject;
    private Transform chunkRoot;
    private Vector2Int center;
    private bool initialized;
    private bool centerValid;
    private bool shuttingDown;
    private bool inBatch;
    private float yaw;
    private float pitch;
    private float nextStreamCheck;
    private VoxelHit pointed;
    private bool hasPointed;
    private GUIStyle hudStyle;

    private static readonly Vector3Int[] Directions =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    private static readonly Vector3[,] Corners =
    {
        { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1) },
        { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) },
        { new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) },
        { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) },
        { new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
        { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) }
    };

    private sealed class Chunk
    {
        public Vector2Int key;
        public Block[] blocks;
        public GameObject gameObject;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public MeshCollider collider;
        public Mesh renderMesh;
        public Mesh collisionMesh;
        public bool dataReady;
        public bool meshReady;
        public int revision;
    }

    private struct Column
    {
        public int height;
        public Biome biome;
        public float temperature;
        public float moisture;
        public float river;
    }

    private struct Tree
    {
        public int x;
        public int y;
        public int z;
        public int height;
        public bool pine;
    }

    [Serializable]
    public struct VoxelHit
    {
        public Vector3Int position;
        public Vector3Int normal;
        public Block block;
        public Vector3 point;
        public float distance;
    }

    private struct BlockChange
    {
        public Vector3Int position;
        public Block before;
        public Block after;
        public bool hadOverride;
    }

    private sealed class EditCommand
    {
        public readonly List<BlockChange> changes = new List<BlockChange>();
    }

    private sealed class MeshBuffer
    {
        public readonly List<Vector3> vertices = new List<Vector3>(8192);
        public readonly List<Vector3> normals = new List<Vector3>(8192);
        public readonly List<Vector2> uv = new List<Vector2>(8192);
        public readonly List<int> triangles = new List<int>(12288);
        public readonly List<int> collision = new List<int>(12288);
    }

    private void Awake()
    {
        LastError = string.Empty;
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }

    private void OnEnable()
    {
        if (initialized && worker == null)
        {
            RefreshStreaming(true);
            worker = StartCoroutine(WorkLoop());
        }
    }

    private void OnDisable()
    {
        if (worker != null)
        {
            StopCoroutine(worker);
            worker = null;
        }
        removal.Clear();
        foreach (KeyValuePair<Vector2Int, Chunk> pair in chunks)
        {
            if (!pair.Value.dataReady)
            {
                removal.Add(pair.Key);
            }
            else if (!pair.Value.meshReady)
            {
                dirty.Add(pair.Key);
            }
        }
        for (int i = 0; i < removal.Count; i++)
        {
            RemoveChunk(removal[i]);
        }
        UnlockCursor();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            UnlockCursor();
        }
    }

    private void OnApplicationQuit()
    {
        shuttingDown = true;
        if (saveOnQuit && initialized)
        {
            SaveWorld();
        }
    }

    private void OnDestroy()
    {
        ClearRuntime();
        DestroyOwned(ownedMaterial);
        DestroyOwned(atlas);
        DestroyOwned(demoCameraObject);
        DestroyOwned(demoLightObject);
        UnlockCursor();
    }

    private void OnValidate()
    {
        if (generation == null)
        {
            generation = new GenerationSettings();
        }
        ClampSettings(generation);
        viewDistance = Mathf.Clamp(viewDistance, 1, 12);
        unloadMargin = Mathf.Clamp(unloadMargin, 0, 4);
        columnsPerFrame = Mathf.Clamp(columnsPerFrame, 1, 64);
        meshRowsPerFrame = Mathf.Clamp(meshRowsPerFrame, 1, 4096);
        reach = Mathf.Clamp(reach, 1f, 64f);
        flySpeed = Mathf.Max(0.1f, flySpeed);
        sprintMultiplier = Mathf.Max(1f, sprintMultiplier);
        undoLimit = Mathf.Clamp(undoLimit, 1, 256);
    }

    private static void ClampSettings(GenerationSettings value)
    {
        value.chunkSize = Mathf.Clamp(value.chunkSize, 8, 32);
        value.height = Mathf.Clamp(value.height, 32, 256);
        value.seaLevel = Mathf.Clamp(value.seaLevel, 4, value.height - 16);
        value.baseHeight = Mathf.Clamp(value.baseHeight, 8, value.height - 16);
        value.relief = Mathf.Clamp(value.relief, 1f, 70f);
        value.continentScale = Mathf.Clamp(value.continentScale, 0.0001f, 0.05f);
        value.hillScale = Mathf.Clamp(value.hillScale, 0.001f, 0.1f);
        value.biomeScale = Mathf.Clamp(value.biomeScale, 0.0001f, 0.02f);
        value.caveScale = Mathf.Clamp(value.caveScale, 0.01f, 0.15f);
        value.caveWidth = Mathf.Clamp(value.caveWidth, 0.03f, 0.25f);
        value.treeDensity = Mathf.Clamp01(value.treeDensity);
        value.riverWidth = Mathf.Clamp(value.riverWidth, 0.005f, 0.12f);
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode before generating the voxel world.", this);
            return;
        }
        ClearRuntime();
        OnValidate();
        settings = JsonUtility.FromJson<GenerationSettings>(JsonUtility.ToJson(generation));
        edits.Clear();
        editBuckets.Clear();
        undo.Clear();
        redo.Clear();
        batch.Clear();
        inBatch = false;
        LastError = string.Empty;
        if (!PrepareMaterial())
        {
            return;
        }
        GameObject root = new GameObject("Voxel Chunks");
        chunkRoot = root.transform;
        chunkRoot.SetParent(transform, false);
        initialized = true;
        if (loadOnStart)
        {
            LoadWorld();
        }
        PrepareDemo();
        RefreshStreaming(true);
        if (isActiveAndEnabled)
        {
            worker = StartCoroutine(WorkLoop());
        }
    }

    private void ClearRuntime()
    {
        if (worker != null)
        {
            StopCoroutine(worker);
            worker = null;
        }
        foreach (Chunk chunk in chunks.Values)
        {
            DisposeChunk(chunk);
        }
        chunks.Clear();
        dirty.Clear();
        pending.Clear();
        removal.Clear();
        DestroyOwned(chunkRoot != null ? chunkRoot.gameObject : null);
        chunkRoot = null;
        initialized = false;
        centerValid = false;
    }

    private void Update()
    {
        if (!initialized || shuttingDown)
        {
            return;
        }
        UpdateDemoCamera();
        if (Time.unscaledTime >= nextStreamCheck)
        {
            nextStreamCheck = Time.unscaledTime + 0.15f;
            RefreshStreaming(false);
        }
        UpdateEditing();
    }

    private Vector2Int CurrentCenter()
    {
        Vector3 position = target != null ? transform.InverseTransformPoint(target.position) : Vector3.zero;
        int x = Mathf.Clamp(Mathf.FloorToInt(position.x), -CoordinateLimit, CoordinateLimit);
        int z = Mathf.Clamp(Mathf.FloorToInt(position.z), -CoordinateLimit, CoordinateLimit);
        return new Vector2Int(FloorDiv(x, settings.chunkSize), FloorDiv(z, settings.chunkSize));
    }

    private void RefreshStreaming(bool force)
    {
        Vector2Int next = CurrentCenter();
        if (!force && centerValid && next == center && pending.Count > 0)
        {
            return;
        }
        center = next;
        centerValid = true;
        int radius = Mathf.Clamp(viewDistance, 1, 12);
        int keep = radius + Mathf.Clamp(unloadMargin, 0, 4);
        removal.Clear();
        foreach (Vector2Int key in chunks.Keys)
        {
            if (Mathf.Abs(key.x - center.x) > keep || Mathf.Abs(key.y - center.y) > keep)
            {
                removal.Add(key);
            }
        }
        for (int i = 0; i < removal.Count; i++)
        {
            RemoveChunk(removal[i]);
        }
        pending.Clear();
        for (int z = -radius; z <= radius; z++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2Int key = center + new Vector2Int(x, z);
                if (!chunks.ContainsKey(key))
                {
                    pending.Add(key);
                }
            }
        }
        pending.Sort(CompareChunkDistance);
    }

    private int CompareChunkDistance(Vector2Int a, Vector2Int b)
    {
        int first = (a - center).sqrMagnitude;
        int second = (b - center).sqrMagnitude;
        int order = first.CompareTo(second);
        if (order != 0)
        {
            return order;
        }
        order = a.x.CompareTo(b.x);
        return order != 0 ? order : a.y.CompareTo(b.y);
    }

    private IEnumerator WorkLoop()
    {
        while (initialized)
        {
            Vector2Int key;
            if (TryTakeDirty(out key))
            {
                Chunk chunk;
                if (chunks.TryGetValue(key, out chunk) && chunk.dataReady)
                {
                    yield return BuildMesh(chunk);
                }
                continue;
            }
            if (pending.Count > 0)
            {
                key = pending[0];
                pending.RemoveAt(0);
                if (!chunks.ContainsKey(key))
                {
                    Chunk chunk = CreateChunk(key);
                    yield return GenerateChunk(chunk);
                    if (IsLive(chunk))
                    {
                        dirty.Add(key);
                        MarkNeighbors(key);
                    }
                }
                continue;
            }
            yield return null;
        }
        worker = null;
    }

    private bool TryTakeDirty(out Vector2Int key)
    {
        key = default(Vector2Int);
        bool found = false;
        int best = int.MaxValue;
        foreach (Vector2Int candidate in dirty)
        {
            int distance = (candidate - center).sqrMagnitude;
            if (!found || distance < best)
            {
                key = candidate;
                best = distance;
                found = true;
            }
        }
        if (found)
        {
            dirty.Remove(key);
        }
        return found;
    }

    private Chunk CreateChunk(Vector2Int key)
    {
        Chunk chunk = new Chunk();
        chunk.key = key;
        chunk.blocks = new Block[settings.chunkSize * settings.chunkSize * settings.height];
        chunk.gameObject = new GameObject("Chunk " + key.x + ", " + key.y);
        chunk.gameObject.layer = gameObject.layer;
        chunk.gameObject.transform.SetParent(chunkRoot, false);
        chunk.gameObject.transform.localPosition = new Vector3(key.x * settings.chunkSize, 0f, key.y * settings.chunkSize);
        chunk.filter = chunk.gameObject.AddComponent<MeshFilter>();
        chunk.renderer = chunk.gameObject.AddComponent<MeshRenderer>();
        chunk.renderer.sharedMaterial = blockMaterial != null ? blockMaterial : ownedMaterial;
        chunk.renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        chunk.renderer.receiveShadows = true;
        if (generateColliders)
        {
            chunk.collider = chunk.gameObject.AddComponent<MeshCollider>();
        }
        chunks.Add(key, chunk);
        return chunk;
    }

    private bool IsLive(Chunk chunk)
    {
        Chunk current;
        return chunks.TryGetValue(chunk.key, out current) && ReferenceEquals(current, chunk);
    }

    private void RemoveChunk(Vector2Int key)
    {
        Chunk chunk;
        if (!chunks.TryGetValue(key, out chunk))
        {
            return;
        }
        chunks.Remove(key);
        dirty.Remove(key);
        DisposeChunk(chunk);
        MarkNeighbors(key);
    }

    private void DisposeChunk(Chunk chunk)
    {
        if (chunk.collider != null)
        {
            chunk.collider.sharedMesh = null;
        }
        if (chunk.filter != null)
        {
            chunk.filter.sharedMesh = null;
        }
        DestroyOwned(chunk.renderMesh);
        DestroyOwned(chunk.collisionMesh);
        if (chunk.gameObject != null)
        {
            chunk.gameObject.SetActive(false);
        }
        DestroyOwned(chunk.gameObject);
    }

    private void MarkNeighbors(Vector2Int key)
    {
        MarkDirty(key + Vector2Int.right);
        MarkDirty(key + Vector2Int.left);
        MarkDirty(key + Vector2Int.up);
        MarkDirty(key + Vector2Int.down);
    }

    private void MarkDirty(Vector2Int key)
    {
        Chunk chunk;
        if (chunks.TryGetValue(key, out chunk) && chunk.dataReady)
        {
            chunk.revision++;
            dirty.Add(key);
        }
    }

    private IEnumerator GenerateChunk(Chunk chunk)
    {
        int size = settings.chunkSize;
        int startX = chunk.key.x * size;
        int startZ = chunk.key.y * size;
        int budget = 0;
        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                if (!IsLive(chunk))
                {
                    yield break;
                }
                int worldX = startX + x;
                int worldZ = startZ + z;
                Column column = SampleColumn(worldX, worldZ);
                for (int y = 0; y < settings.height; y++)
                {
                    chunk.blocks[Index(x, y, z)] = SampleTerrain(worldX, y, worldZ, column);
                }
                budget++;
                if (budget >= Mathf.Max(1, columnsPerFrame))
                {
                    budget = 0;
                    yield return null;
                }
            }
        }
        if (!IsLive(chunk))
        {
            yield break;
        }
        if (settings.trees)
        {
            CollectTrees(startX, startZ, size);
            for (int i = 0; i < treeBuffer.Count; i++)
            {
                StampTrunk(chunk, treeBuffer[i]);
            }
            for (int i = 0; i < treeBuffer.Count; i++)
            {
                StampLeaves(chunk, treeBuffer[i]);
            }
        }
        Dictionary<Vector3Int, Block> bucket;
        if (editBuckets.TryGetValue(chunk.key, out bucket))
        {
            foreach (KeyValuePair<Vector3Int, Block> pair in bucket)
            {
                Vector3Int p = pair.Key;
                chunk.blocks[Index(p.x - startX, p.y, p.z - startZ)] = pair.Value;
            }
        }
        chunk.dataReady = true;
    }

    private Column SampleColumn(int x, int z)
    {
        Column column = new Column();
        column.temperature = Fractal2(x * settings.biomeScale, z * settings.biomeScale, 3, 101);
        column.moisture = Fractal2(x * settings.biomeScale, z * settings.biomeScale, 3, 203);
        float continents = Fractal2(x * settings.continentScale, z * settings.continentScale, 4, 307);
        float hills = Fractal2(x * settings.hillScale, z * settings.hillScale, 4, 401);
        float mountainNoise = Fractal2(x * 0.005f, z * 0.005f, 3, 503);
        float mountains = SmoothRange(0.48f, 0.80f, mountainNoise);
        float ridge = 1f - Mathf.Abs(hills * 2f - 1f);
        float elevation = settings.baseHeight + (continents - 0.48f) * settings.relief * 2.6f;
        elevation += (hills - 0.5f) * 12f;
        elevation += mountains * ridge * settings.relief;
        float riverNoise = Fractal2(x * 0.0035f, z * 0.0035f, 3, 607);
        column.river = settings.rivers ? 1f - SmoothRange(0f, settings.riverWidth, Mathf.Abs(riverNoise - 0.5f)) : 0f;
        float riverStrength = column.river * (1f - mountains);
        elevation = Mathf.Lerp(elevation, Mathf.Min(elevation, settings.seaLevel - 3), riverStrength);
        column.height = Mathf.Clamp(Mathf.FloorToInt(elevation), 5, settings.height - 14);
        if (column.height < settings.seaLevel - 2)
        {
            column.biome = Biome.Ocean;
        }
        else if (mountains > 0.48f)
        {
            column.biome = Biome.Mountains;
        }
        else if (column.temperature < 0.35f)
        {
            column.biome = Biome.Tundra;
        }
        else if (column.temperature > 0.58f && column.moisture < 0.46f)
        {
            column.biome = Biome.Desert;
        }
        else if (column.moisture > 0.51f)
        {
            column.biome = Biome.Forest;
        }
        else
        {
            column.biome = Biome.Plains;
        }
        return column;
    }

    private Block SampleTerrain(int x, int y, int z, Column column)
    {
        if (y <= 1 || y < 4 && Hash01(x, y, z, 17) > y * 0.25f)
        {
            return Block.Bedrock;
        }
        if (y > column.height)
        {
            if (y <= settings.seaLevel)
            {
                return y == settings.seaLevel && column.temperature < 0.31f ? Block.Ice : Block.Water;
            }
            return Block.Air;
        }
        int depth = column.height - y;
        if (settings.caves && y > 5 && depth > 4 && IsCave(x, y, z))
        {
            return Block.Air;
        }
        bool beach = column.height <= settings.seaLevel + 1;
        bool snowy = column.biome == Biome.Tundra || column.height > settings.seaLevel + 40;
        if (depth == 0)
        {
            if (beach || column.biome == Biome.Desert)
            {
                return Block.Sand;
            }
            if (snowy)
            {
                return Block.Snow;
            }
            if (column.biome == Biome.Mountains && column.height > settings.seaLevel + 25)
            {
                return Block.Stone;
            }
            return Block.Grass;
        }
        if (depth < 4)
        {
            return beach || column.biome == Biome.Desert ? Block.Sand : Block.Dirt;
        }
        if (settings.ores)
        {
            return SampleOre(x, y, z);
        }
        return Block.Stone;
    }

    private bool IsCave(int x, int y, int z)
    {
        float scale = settings.caveScale;
        float a = Noise3(x * scale, y * scale * 1.3f, z * scale, 701) - 0.5f;
        float b = Noise3(x * scale + 31f, y * scale, z * scale - 19f, 809) - 0.5f;
        float width = settings.caveWidth;
        bool tunnel = a * a + b * b < width * width;
        float chamber = Noise3(x * scale * 0.6f, y * scale * 0.8f, z * scale * 0.6f, 907);
        return tunnel || chamber > 0.78f;
    }

    private Block SampleOre(int x, int y, int z)
    {
        float vein = Noise3(x * 0.19f, y * 0.19f, z * 0.19f, 1009);
        if (vein < 0.70f)
        {
            return Block.Stone;
        }
        float type = Hash01(FloorDiv(x, 7), FloorDiv(y, 7), FloorDiv(z, 7), 1103);
        if (y < 24 && type > 0.78f)
        {
            return Block.Gold;
        }
        if (y < 56 && type > 0.47f)
        {
            return Block.Iron;
        }
        if (y < 76 && type > 0.23f)
        {
            return Block.Copper;
        }
        return Block.Coal;
    }

    private void CollectTrees(int startX, int startZ, int size)
    {
        treeBuffer.Clear();
        int minimumX = FloorDiv(startX - 3, 6);
        int maximumX = FloorDiv(startX + size + 2, 6);
        int minimumZ = FloorDiv(startZ - 3, 6);
        int maximumZ = FloorDiv(startZ + size + 2, 6);
        for (int gz = minimumZ; gz <= maximumZ; gz++)
        {
            for (int gx = minimumX; gx <= maximumX; gx++)
            {
                Tree tree;
                if (TryTree(gx, gz, out tree))
                {
                    treeBuffer.Add(tree);
                }
            }
        }
    }

    private bool TryTree(int gx, int gz, out Tree tree)
    {
        tree = new Tree();
        tree.x = gx * 6 + (int)(Hash01(gx, 0, gz, 1201) * 4f) + 1;
        tree.z = gz * 6 + (int)(Hash01(gx, 0, gz, 1301) * 4f) + 1;
        Column column = SampleColumn(tree.x, tree.z);
        if (column.height <= settings.seaLevel + 1)
        {
            return false;
        }
        if (column.biome == Biome.Desert || column.biome == Biome.Ocean || column.biome == Biome.Mountains)
        {
            return false;
        }
        float density = settings.treeDensity;
        if (column.biome == Biome.Plains)
        {
            density *= 0.12f;
        }
        if (Hash01(gx, 0, gz, 1409) > density)
        {
            return false;
        }
        tree.y = column.height + 1;
        tree.height = 4 + (int)(Hash01(gx, 0, gz, 1511) * 3f);
        tree.pine = column.biome == Biome.Tundra;
        if (tree.pine)
        {
            tree.height += 2;
        }
        return tree.y + tree.height + 2 < settings.height;
    }

    private void StampTrunk(Chunk chunk, Tree tree)
    {
        for (int y = 0; y < tree.height; y++)
        {
            PutGenerated(chunk, tree.x, tree.y + y, tree.z, Block.Log, false);
        }
    }

    private void StampLeaves(Chunk chunk, Tree tree)
    {
        int top = tree.y + tree.height;
        int bottom = tree.pine ? tree.y + 2 : top - 2;
        for (int y = bottom; y <= top + 1; y++)
        {
            int radius = tree.pine ? Mathf.Clamp((top - y + 2) / 2, 0, 3) : y >= top ? 1 : 2;
            for (int z = -radius; z <= radius; z++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (Mathf.Abs(x) == radius && Mathf.Abs(z) == radius && radius > 1)
                    {
                        continue;
                    }
                    PutGenerated(chunk, tree.x + x, y, tree.z + z, Block.Leaves, true);
                }
            }
        }
    }

    private void PutGenerated(Chunk chunk, int x, int y, int z, Block block, bool onlyAir)
    {
        x -= chunk.key.x * settings.chunkSize;
        z -= chunk.key.y * settings.chunkSize;
        if (x < 0 || z < 0 || x >= settings.chunkSize || z >= settings.chunkSize || y < 0 || y >= settings.height)
        {
            return;
        }
        int index = Index(x, y, z);
        if (!onlyAir || chunk.blocks[index] == Block.Air)
        {
            chunk.blocks [index] = block;
        }
    }

    private int Index(int x, int y, int z)
    {
        return x + settings.chunkSize * (z + settings.chunkSize * y);
    }

    public static int FloorDiv(int value, int divisor)
    {
        int result = value / divisor;
        return value % divisor < 0 ? result - 1 : result;
    }

    public Vector2Int BlockToChunk(Vector3Int position)
    {
        int size = settings != null ? settings.chunkSize : generation.chunkSize;
        return new Vector2Int(FloorDiv(position.x, size), FloorDiv(position.z, size));
    }

    public Vector3Int WorldToBlock(Vector3 worldPosition)
    {
        return Vector3Int.FloorToInt(transform.InverseTransformPoint(worldPosition));
    }

    public Vector3 BlockToWorld(Vector3Int position)
    {
        return transform.TransformPoint((Vector3)position + Vector3.one * 0.5f);
    }

    public bool TryGetBlock(Vector3Int position, out Block block)
    {
        block = Block.Air;
        if (!initialized || position.y < 0 || position.y >= settings.height)
        {
            return false;
        }
        Chunk chunk;
        Vector2Int key = BlockToChunk(position);
        if (!chunks.TryGetValue(key, out chunk) || !chunk.dataReady)
        {
            return false;
        }
        int x = position.x - key.x * settings.chunkSize;
        int z = position.z - key.y * settings.chunkSize;
        block = chunk.blocks[Index(x, position.y, z)];
        return true;
    }

    public Block GetBlock(Vector3Int position)
    {
        Block block;
        TryGetBlock(position, out block);
        return block;
    }

    public int GetSurfaceHeight(int x, int z)
    {
        if (!initialized)
        {
            return 0;
        }
        return SampleColumn(x, z). height;
    }

    public Biome GetBiome(int x, int z)
    {
        return initialized ? SampleColumn(x, z).biome : Biome.Plains;
    }

    public static bool IsSolid(Block block)
    {
        return block != Block.Air && block != Block.Water;
    }

    private Block MeshNeighbor(Chunk chunk, int x, int y, int z)
    {
        if (y < 0)
        {
            return Block.Bedrock;
        }
        if (y >= settings.height)
        {
            return Block.Air;
        }
        int size = settings.chunkSize;
        if (x >= 0 && x < size && z >= 0 && z < size)
        {
            return chunk.blocks[Index(x, y, z)];
        }
        Vector3Int global = new Vector3Int(chunk.key.x * size + x, y, chunk.key.y * size + z);
        return GetBlock(global);
    }

    private static bool ShouldDrawFace(Block current, Block neighbor)
    {
        if (current == Block.Water)
        {
            return neighbor == Block.Air;
        }
        return neighbor == Block.Air || neighbor == Block.Water;
    }

    private IEnumerator BuildMesh(Chunk chunk)
    {
        chunk.meshReady = false;
        int revision = chunk.revision;
        MeshBuffer buffer = new MeshBuffer();
        int size = settings.chunkSize;
        int rows = 0;
        for (int y = 0; y < settings.height; y++)
        {
            for (int z = 0; z < size; z++)
            {
                if (!IsLive(chunk))
                {
                    yield break;
                }
                for (int x = 0; x < size; x++)
                {
                    Block block = chunk.blocks[Index(x, y, z)];
                    if (block == Block.Air)
                    {
                        continue;
                    }
                    for (int face = 0; face < Directions.Length; face++)
                    {
                        Vector3Int d = Directions[face];
                        Block neighbor = MeshNeighbor(chunk, x + d.x, y + d.y, z + d.z);
                        if (ShouldDrawFace(block, neighbor))
                        {
                            AddFace(buffer, x, y, z, block, face);
                        }
                    }
                }
                rows++;
                if (rows >= Mathf.Max(1, meshRowsPerFrame))
                {
                    rows = 0;
                    yield return null;
                }
            }
        }
        if (!IsLive(chunk))
        {
            yield break;
        }
        if (chunk.revision != revision)
        {
            dirty.Add(chunk.key);
            yield break;
        }
        ApplyMesh(chunk, buffer);
        chunk.meshReady = true;
        if (ChunkReady != null)
        {
            ChunkReady(chunk.key);
        }
    }

    private void AddFace(MeshBuffer buffer, int x, int y, int z, Block block, int face)
    {
        int first = buffer.vertices.Count;
        Vector3 origin = new Vector3(x, y, z);
        int tile = TileFor(block, face);
        int row = face == 2 ? 0 : face == 3 ? 3 : face < 2 ? 1 : 2;
        float inset = 0.5f / TilePixels;
        float u0 = (tile + inset) / AtlasColumns;
        float u1 = (tile + 1f - inset) /AtlasColumns;
        float v0 = (row + inset) / AtlasRows;
        float v1 = (row + 1f - inset) /AtlasRows;
        for (int corner = 0; corner < 4; corner++)
        {
            buffer.vertices.Add(origin + Corners[face, corner]);
            buffer.normals.Add(Directions[face]);
        }
        buffer.uv.Add(new Vector2(u0, v0));
        buffer.uv.Add(new Vector2(u0, v1));
        buffer.uv.Add(new Vector2(u1, v1));
        buffer.uv.Add(new Vector2(u1, v0));
        AddQuad(buffer.triangles, first);
        if (IsSolid(block))
        {
            AddQuad(buffer.collision, first);
        }
    }

    private static void AddQuad(List<int> indices, int first)
    {
        indices.Add(first);
        indices.Add(first + 1);
        indices.Add(first + 2);
        indices.Add(first);
        indices.Add(first + 2);
        indices.Add(first + 3);
    }

    private static int TileFor(Block block, int face)
    {
        if (block == Block.Grass)
        {
            return face == 2 ? 1 : face == 3 ? 2 : 0;
        }
        if (block == Block.Log)
        {
            return face == 2 || face == 3 ? 6 : 5;
        }
        switch (block)
        {
            case Block.Dirt: return 2;
            case Block.Stone: return 3;
            case Block.Sand: return 4;
            case Block.Leaves: return 7;
            case Block.Water: return 8;
            case Block.Snow: return 9;
            case Block.Ice: return 10;
            case Block.Coal: return 11;
            case Block.Iron: return 12;
            case Block.Copper: return 13;
            case Block.Gold: return 14;
            case Block.Bedrock: return 15;
            case Block.Planks: return 6;
            default: return 3;
        }
    }

    private void ApplyMesh(Chunk chunk, MeshBuffer buffer)
    {
        Mesh render = new Mesh();
        render.name = "Voxel Render" + chunk.key;
        render.indexFormat = IndexFormat.UInt32;
        render.SetVertices(buffer.vertices);
        render.SetNormals(buffer.normals);
        render.SetUVs(0, buffer.uv);
        render.SetTriangles(buffer.triangles, 0);
        render.RecalculateBounds();
        Mesh oldRender = chunk.renderMesh;
        chunk.renderMesh = render;
        chunk.filter.sharedMesh = render;
        DestroyOwned(oldRender);
        if (chunk.collider != null)
        {
            Mesh oldCollision = chunk.collisionMesh;
            chunk.collider.sharedMesh = null;
            chunk.collisionMesh = null;
            if (buffer.collision.Count > 0)
            {
                Mesh collision = new Mesh();
                collision.name = "Voxel Collision " +chunk.key;
                collision.indexFormat = IndexFormat.UInt32;
                collision.SetVertices(buffer.vertices);
                collision.SetTriangles(buffer.collision, 0);
                collision.RecalculateBounds();            
                chunk.collisionMesh = collision;
                chunk.collider.sharedMesh = collision;
            }
            DestroyOwned(oldCollision);
        }
    }

    public bool SetBlock(Vector3Int position, Block block)
    {
        if (!initialized || (byte)block > (byte)Block.Planks || !ValidPosition(position))
        {
            return false;
        }
        Block before;
        if (!TryGetBlock(position, out before) || before == block || before == Block.Bedrock)
        {
            return false;
        }
        BlockChange change = new BlockChange();
        change.position = position;
        change.before = before;
        change.after = block;
        change.hadOverride = edits.ContainsKey(position);
        ApplyBlock(position, block, true);
        if (inBatch)
        {
            batch.Add(change);
        }
        else
        {
            EditCommand command = new EditCommand();
            command.changes.Add(change);
            PushUndo(command);
        }
        return true;
    }

    private bool ValidPosition(Vector3Int position)
    {
        return position.y >= 2 && position.y < settings.height  
        && position.x >= -CoordinateLimit && position.x <= CoordinateLimit
        && position.z >= -CoordinateLimit && position.z <= CoordinateLimit;
    }

    private void ApplyBlock(Vector3Int position, Block block, bool storeOverride)
    {
        Vector2Int key = BlockToChunk(position);
        Block before = GetBlock(position);
        if (storeOverride)
        {
            edits[position] = block;
            Dictionary<Vector3Int, Block> bucket;
            if (!editBuckets.TryGetValue(key, out bucket))
            {
                bucket = new Dictionary<Vector3Int, Block>();
                editBuckets.Add(key, bucket);
            }
            bucket[position] = block;
        }
        else
        {
            edits.Remove(position);
            Dictionary<Vector3Int, Block> bucket;
            if (editBuckets.TryGetValue(key, out bucket))
            {
                bucket.Remove(position);
                if (bucket.Count == 0)
                {
                    editBuckets.Remove(key);
                }
            }
        }
        Chunk chunk;
        if (chunks.TryGetValue(key, out chunk) && chunk.dataReady)
        {
            int x = position.x - key.x * settings.chunkSize;
            int z = position.z - key.y * settings.chunkSize;
            chunk.blocks[Index(x, position.y, z)] = block;
            MarkDirty(key);
            if(x == 0) MarkDirty(key + Vector2Int.left);
            if(x == settings.chunkSize - 1) MarkDirty(key + Vector2Int.right);
            if(z == 0) MarkDirty(key + Vector2Int.down);
            if(z == settings.chunkSize - 1) MarkDirty(key + Vector2Int.up);
        }
        if (BlockChanged != null)
        {
            BlockChanged(position, before, block);
        }
    }

    public void EndEditBatch()
    {
        if (!inBatch)
        {
            return;
        }
        inBatch = false;
        if (batch.Count > 0)
        {
            EditCommand command = new EditCommand();
            command.changes.AddRange(batch);
            PushUndo(command);
        }
        batch.Clear();
    }

    private void PushUndo(EditCommand command)
    {
        undo.Push(command);
        redo.Clear();
        if (undo.Count > Mathf.Max(1, undoLimit))
        {
            EditCommand[] commands = undo.ToArray();
            undo.Clear();
            int count = Mathf.Min(commands.Length, Mathf.Max(1, undoLimit));
            for (int i = count - 1; i >= 0; i--)
            {
                undo.Push(commands[i]);
            }
        }
    }

    public bool UndoEdit()
    {
        if (inBatch || undo.Count == 0)
        {
            return false;
        }
        EditCommand command = undo.Pop();
        for (int i = command.changes.Count - 1; i >= 0; i--)
        {
            BlockChange change = command.changes[i];
            ApplyBlock(change.position, change.before, change.hadOverride);
        }
        redo.Push(command);
        return true;
    }

    public bool RedoEdit()
    {
        if (inBatch || redo.Count == 0)
        {
            return false;
        }
        EditCommand command = redo.Pop();
        for (int i = 0; i < command.changes.Count; i++)
        {
            BlockChange change = command.changes[i];
            ApplyBlock(change.position, change.after, true);
        }
        undo.Push(command);
        return true;
    }

    public int FillBox(Vector3Int first, Vector3Int second, Block block)
    {
        if (!initialized)
        {
            return 0;
        }
        Vector3Int min = Vector3Int.Min(first, second);
        Vector3Int max = Vector3Int.Max(first, second);
        if (!ValidPosition(min) || !ValidPosition(max))
        {
            return 0;
        }
        if ((long)max.x - min.x >= 65536 || (long)max.z - min.z >= 65536)
        {
            return 0;
        }
        long volume = ((long)max.x - min.x + 1) * ((long)max.y - min.y +1);
        if (volume <= 0 || volume > 65536 || (long)max.z - min.z + 1 > 65536 / volume)
        {
            return 0;
        }
        bool ownsBatch = BeginEditBatch();
        int changed = 0;
        for (int y = min.y; y <= max.y; y++)
        {
            for (int z = min.z; z <= max.z; z++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    if (SetBlock(new Vector3Int(x, y, z), block)) changed++;
                }
            }
        }
          if (ownsBatch) EndEditBatch();
          return changed;
    }

    public int FillSphere(Vector3Int origin, int radius, Block block)
    {
        if (!initialized || !ValidPosition(origin)) return 0;
        radius = Mathf.Clamp(radius, 0, 16);
        bool ownsBatch = BeginEditBatch();
        int changed = 0;
        for (int y= -radius; y <= radius; y++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y + z * z > radius) continue;
                    if (SetBlock(origin + new Vector3Int(x, y, z), block))changed++;
                }
            }
        }
        if (!initialized || !ValidPosition(origin)) return 0;
    }
}



