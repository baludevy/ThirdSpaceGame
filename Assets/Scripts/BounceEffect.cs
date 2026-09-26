using System.Collections;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    public float bounceHeight = 0.3f;
    public float bounceDuration = 0.4f;
    public int bounceCount = 2;

    public void Startbounce()
    {
        StartCoroutine(BounceHandler());
    }

    private IEnumerator BounceHandler()
    {
        var originalPosition = transform.position;
        float localHeight = bounceHeight * Random.Range(0.9f, 1.1f);
        float localDuration = bounceDuration * Random.Range(0.9f, 1.1f);

        for (int i = 0; i < bounceCount; i++)
        {
            yield return Bounce(transform, originalPosition, localHeight * Random.Range(0.9f, 1.1f), localDuration / 2);
            localHeight *= 0.5f;
            localDuration *= 0.8f;
        }

        transform.position = originalPosition;
    }

    private IEnumerator Bounce(Transform objectTransform, Vector3 start, float height, float duration)
    {
        var peak = start + Vector3.up * height;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            objectTransform.position = Vector3.Lerp(start, peak, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            objectTransform.position = Vector3.Lerp(peak, start, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
