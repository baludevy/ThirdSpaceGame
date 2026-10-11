using UnityEngine;

public class GroundGrassSpawner : MonoBehaviour
{
    [Header("Grass Settings")] public Material grassMaterial;
    public ComputeShader cullCompute;
    public int grassCount = 100000;
    public float grassHeight = 1f;
    public float grassWidth = 0.5f;

    [Header("Culling Settings")] public float maxDrawDistance = 100f;
    public float cullBufferMargin = 2f;

    [Header("Spawn Settings")] public float groundOffset = 0.05f;

    private Mesh grassMesh;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer visibleBuffer;
    private ComputeBuffer argsBuffer;
    private Bounds drawBounds;

    private Transform cameraTransform;
    private int kernelIndex;
    private int dispatchGroupCount;

    private static readonly int IdAllPositions = Shader.PropertyToID("allPositions");
    private static readonly int IdVisiblePositions = Shader.PropertyToID("visiblePositions");
    private static readonly int IdInstanceCount = Shader.PropertyToID("_InstanceCount");
    private static readonly int IdMaxDistance = Shader.PropertyToID("_MaxDistance");
    private static readonly int IdCameraPos = Shader.PropertyToID("_CameraPos");
    private static readonly int IdPositions = Shader.PropertyToID("_Positions");

    private readonly uint[] indirectArgs = new uint[5];
    private const int positionStrideBytes = sizeof(float) * 4;
    private const int threadGroupSize = 64;

    private void Start()
    {
        cameraTransform = Camera.main ? Camera.main.transform : null;
        CreateCrossedQuadMesh();
        InitGrass();
        InitBuffers();
    }

    private void CreateCrossedQuadMesh()
    {
        grassMesh = new Mesh { name = "GrassCross" };

        var vertices = new Vector3[8];
        vertices[0] = new Vector3(-grassWidth * 0.5f, 0, 0);
        vertices[1] = new Vector3(grassWidth * 0.5f, 0, 0);
        vertices[2] = new Vector3(-grassWidth * 0.5f, grassHeight, 0);
        vertices[3] = new Vector3(grassWidth * 0.5f, grassHeight, 0);
        vertices[4] = new Vector3(0, 0, -grassWidth * 0.5f);
        vertices[5] = new Vector3(0, 0, grassWidth * 0.5f);
        vertices[6] = new Vector3(0, grassHeight, -grassWidth * 0.5f);
        vertices[7] = new Vector3(0, grassHeight, grassWidth * 0.5f);

        var triangles = new[] { 0, 2, 1, 2, 3, 1, 4, 6, 5, 6, 7, 5 };

        var uvs = new Vector2[8];
        for (int i = 0; i < 8; i++)
            uvs[i] = new Vector2((i & 1) == 0 ? 0 : 1, (i < 2 || i == 4 || i == 5) ? 0 : 1);

        grassMesh.SetVertices(vertices);
        grassMesh.SetTriangles(triangles, 0);
        grassMesh.SetUVs(0, uvs);
        grassMesh.RecalculateNormals();
        grassMesh.RecalculateBounds();
    }

    private void InitGrass()
    {
        var positions = new Vector4[grassCount];
        var groundRenderer = GetComponent<Renderer>();
        var groundBounds = groundRenderer
            ? groundRenderer.bounds
            : new Bounds(transform.position, transform.localScale * 10f);

        for (int i = 0; i < grassCount; i++)
        {
            var position = new Vector3(
                Random.Range(groundBounds.min.x, groundBounds.max.x),
                groundBounds.max.y + groundOffset,
                Random.Range(groundBounds.min.z, groundBounds.max.z)
            );

            var rotationY = Random.Range(0f, 360f);
            positions[i] = new Vector4(position.x, position.y, position.z, rotationY);
        }

        positionBuffer = new ComputeBuffer(grassCount, positionStrideBytes, ComputeBufferType.Structured);
        positionBuffer.SetData(positions);

        drawBounds = groundBounds;
        drawBounds.Expand(maxDrawDistance * 2f + 20f);
    }

    private void InitBuffers()
    {
        visibleBuffer = new ComputeBuffer(grassCount, positionStrideBytes, ComputeBufferType.Append);
        visibleBuffer.SetCounterValue(0);

        kernelIndex = cullCompute.FindKernel("CSMain");
        cullCompute.SetBuffer(kernelIndex, IdAllPositions, positionBuffer);
        cullCompute.SetBuffer(kernelIndex, IdVisiblePositions, visibleBuffer);
        cullCompute.SetInt(IdInstanceCount, grassCount);
        cullCompute.SetFloat(IdMaxDistance, maxDrawDistance + cullBufferMargin);

        argsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        indirectArgs[0] = (uint)grassMesh.GetIndexCount(0);
        indirectArgs[1] = 0u;
        indirectArgs[2] = (uint)grassMesh.GetIndexStart(0);
        indirectArgs[3] = (uint)grassMesh.GetBaseVertex(0);
        indirectArgs[4] = 0u;
        argsBuffer.SetData(indirectArgs);

        grassMaterial.enableInstancing = true;
        grassMaterial.SetShaderPassEnabled("ShadowCaster", false);
        grassMaterial.SetBuffer(IdPositions, visibleBuffer);

        dispatchGroupCount = Mathf.CeilToInt(grassCount / (float)threadGroupSize);
    }

    private void Update()
    {
        if (!cameraTransform) return;

        visibleBuffer.SetCounterValue(0);
        cullCompute.SetVector(IdCameraPos, cameraTransform.position);
        cullCompute.Dispatch(kernelIndex, dispatchGroupCount, 1, 1);

        ComputeBuffer.CopyCount(visibleBuffer, argsBuffer, sizeof(uint));
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, drawBounds, argsBuffer);
    }

    private void OnDisable()
    {
        positionBuffer?.Release();
        visibleBuffer?.Release();
        argsBuffer?.Release();
        if (grassMesh) Destroy(grassMesh);
    }

    private void OnDestroy()
    {
        positionBuffer?.Release();
        visibleBuffer?.Release();
        argsBuffer?.Release();
        if (grassMesh) Destroy(grassMesh);
    }
} 