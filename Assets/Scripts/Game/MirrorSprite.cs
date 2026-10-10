using System;
using UnityEngine;
using UnityEngine.Rendering;

 [DisallowMultipleComponent]
 [RequireComponent(typeof(SpriteRenderer))]
 [DefaultExecutionOrder(9000)]
public class MirrorSprite : MonoBehaviour
{
    public SpriteRenderer source;
    public Camera reflectionCamera;

    public Vector3 rotationOffset;

    private SpriteRenderer target;
    private MaterialPropertyBlock properties;

    void Awake()
    {
        target = GetComponent<SpriteRenderer>();
        properties = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if(!target) target = GetComponent<SpriteRenderer>();
        if(properties == null) properties = new MaterialPropertyBlock();
        RenderPipelineManager.beginCameraRendering += BeforeCameraRender;
    }

    void LateUpdate() => CopySprite();

    void BeforeCameraRender(ScriptableRenderContext context, Camera camera)
    {
        if(camera == reflectionCamera) CopySprite();
    }

    void CopySprite()
    {
        if(!source || source == target || !reflectionCamera)
        {
            target.enabled = false;
            return;
        }

        target.enabled = source.enabled && source.gameObject.activeInHierarchy;
        target.sprite = source.sprite;
        target.color = source.color;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.sharedMaterial = source.sharedMaterial;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.tileMode = source.tileMode;
        target.adaptiveModeThreshold = source.adaptiveModeThreshold;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.spriteSortPoint = source.spriteSortPoint;
        target.maskInteraction = source.maskInteraction;
        source.GetPropertyBlock(properties);
        target.SetPropertyBlock(properties);

        transform.SetPositionAndRotation(source.transform.position, reflectionCamera.transform.rotation * Quaternion.Euler(rotationOffset));

        transform.localScale = source.transform.lossyScale;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeforeCameraRender;
        if (target) target.enabled = false;
    }
}