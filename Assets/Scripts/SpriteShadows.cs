using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadows : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.shadowCastingMode = ShadowCastingMode.TwoSided;
        sprite.receiveShadows = true;
    }
}
