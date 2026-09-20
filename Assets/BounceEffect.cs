using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceEffect : Monobehaviour
{
    public float bounceHeight = 0.3f;  
    public float bounceDuration = 0.4f;
    public int bounceCount = 2;
    
    public void Startbounce()
    {
     
    }
    private IEnumerator BounceHandler(Transform objectTransform)
    {
        Vector3 originalPosition = objectTransform.position;
        float localHeight = bounceHeight;
        float localDuration = bounceDuration;
    }
}
