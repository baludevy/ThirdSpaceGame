using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(9000)]
public class PlanarReflection : MonoBehaviour
{
    public Camera mainCamera;
    public Transform waterPlane;
    public RenderTexture reflectionTexture;
    public float clipOffset = 0.03f;

    private Camera reflectionCamera;
    readonly UniversalRenderPipeline.SingleCameraRequest request = new UniversalRenderPipeline.SingleCameraRequest();

    void OnEnable()
    {
        reflectionCamera = GetComponent<Camera>();
        reflectionCamera.enabled = false;
        UniversalAdditionalCameraData data = reflectionCamera.GetUniversalAdditionalCameraData();
        data.renderType = CameraRenderType.Base;
        data.renderPostProcessing = false;
        data.antialiasing = AntialiasingMode.None;
        reflectionCamera.useOcclusionCulling = false;
        reflectionCamera.allowMSAA = false;
        reflectionCamera.allowDynamicResolution = false;
    }

    void LateUpdate()
    {
        if(!mainCamera || mainCamera == reflectionCamera || !waterPlane || !reflectionTexture) return;

        Vector3 n = waterPlane.up.normalized;
        Vector3 p = waterPlane.position;
        if(Vector3.Dot(n, mainCamera.transform.position - p) < 0f) n = -n;

        float d = -Vector3.Dot(n, p);
        Matrix4x4 reflection = Matrix4x4.identity;
        for(int row = 0; row < 3; row++)
        {
            for(int col = 0; col < 3; col++)
                reflection[row, col] -= 2f * n[row] * n[col];
            reflection[row, 3] = -2f * d * n[row];
        }

        Transform source = mainCamera.transform;
        transform.SetPositionAndRotation(reflection.MultiplyPoint(source.position),
            Quaternion.LookRotation(reflection.MultiplyVector(source.forward),
                                    reflection.MultiplyVector(source.up)));

        reflectionCamera.orthographic = mainCamera.orthographic;
        reflectionCamera.fieldOfView = mainCamera.fieldOfView;
        reflectionCamera.orthographicSize = mainCamera.orthographicSize;
        reflectionCamera.nearClipPlane = mainCamera.nearClipPlane;
        reflectionCamera.farClipPlane = mainCamera.farClipPlane;
        reflectionCamera.aspect = mainCamera.aspect;
        reflectionCamera.rect = new Rect(0, 0, 1, 1);
        reflectionCamera.targetTexture = reflectionTexture;
        reflectionCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflection;
        reflectionCamera.projectionMatrix = mainCamera.projectionMatrix;

        Matrix4x4 view = reflectionCamera.worldToCameraMatrix;
        Vector3 cp = view.MultiplyPoint(p + n * clipOffset);
        Vector3 cn = view.MultiplyVector(n).normalized;
        reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(
            new Vector4(cn.x, cn.y, cn.z, -Vector3.Dot(cp, cn)));
        reflectionCamera.cullingMatrix = reflectionCamera.projectionMatrix * view;

        if(!reflectionTexture.IsCreated()) reflectionTexture.Create();
        request.destination = reflectionTexture;
        bool previous = GL.invertCulling;
        try
        {
            GL.invertCulling = !previous;
            RenderPipeline.SubmitRenderRequest(reflectionCamera, request);
        }
        finally {GL.invertCulling = previous; }
    }
}
