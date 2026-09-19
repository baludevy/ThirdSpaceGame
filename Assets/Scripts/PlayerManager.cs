using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    
    private float snapshotInterval = 0.02f; // 50 Hz

    private readonly Queue<Vector3> snapshots = new Queue<Vector3>();

    private Vector3 fromPosition;
    private Vector3 toPosition;
    private float interpolationTimer;
    private bool interpolating;

    public void Initialize(int _id)
    {
        id = _id;
    }

    public void AddSnapshot(Vector3 position)
    {
        position.z = transform.position.z;
        snapshots.Enqueue(position);

        while (snapshots.Count > 2)
            snapshots.Dequeue();

        if (!interpolating)
        {
            interpolationTimer = 0f;
            StartNextInterpolation();
        }
    }

    private void StartNextInterpolation()
    {
        fromPosition = transform.position;
        toPosition = snapshots.Dequeue();
        interpolating = true;
    }

    private void Update()
    {
        if (!interpolating)
            return;

        float duration = Mathf.Max(snapshotInterval, 0.001f);
        interpolationTimer += Time.deltaTime;

        while (interpolationTimer >= duration)
        {
            transform.position = toPosition;
            interpolationTimer -= duration;

            if (snapshots.Count == 0)
            {
                interpolating = false;
                interpolationTimer = 0f;
                return;
            }

            StartNextInterpolation();
        }

        transform.position = Vector3.Lerp(
            fromPosition,
            toPosition,
            interpolationTimer / duration
        );
    }
}