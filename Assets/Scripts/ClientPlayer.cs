using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct MoveSnapshot
{
    public Vector3 position;
    public AnimationState animationState;

    public MoveSnapshot(
        Vector3 position,
        AnimationState animationState = AnimationState.idle)
    {
        this.position = position;
        this.animationState = animationState;
    }
}

public class ClientPlayer : MonoBehaviour
{
    public int id;

    [SerializeField] private TMP_Text usernameText;

    private float snapshotInterval = 0.02f; // 50 Hz

    private readonly Queue<MoveSnapshot> snapshots = new Queue<MoveSnapshot>();

    private Vector3 fromPosition;
    private Vector3 toPosition;

    private float interpolationTimer;
    private bool interpolating;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sprite;

    private AnimationState currentAnimationState;

    public void Initialize(int _id, string _username)
    {
        id = _id;

        if (usernameText != null)
            usernameText.text = _username;
    }

    public void AddSnapshot(Vector3 position, AnimationState animationState)
    {
        position.z = transform.position.z;
        
        snapshots.Enqueue(
            new MoveSnapshot(position, animationState)
        );

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
        if (snapshots.Count == 0)
        {
            interpolating = false;
            return;
        }

        fromPosition = transform.position;

        MoveSnapshot snapshot = snapshots.Dequeue();

        toPosition = snapshot.position;
        currentAnimationState = snapshot.animationState;

        ApplyAnimationState(currentAnimationState);

        interpolationTimer = 0f;
        interpolating = true;
    }

    private void ApplyAnimationState(AnimationState state)
    {
        animator.SetBool("Side", false);
        animator.SetBool("Back", false);
        animator.SetBool("Forward", false);

        switch (state)
        {
            case AnimationState.idle:
                break;

            case AnimationState.forward:
                animator.SetBool("Forward", true);
                break;

            case AnimationState.back:
                animator.SetBool("Back", true);
                break;

            case AnimationState.left:
                animator.SetBool("Side", true);

                if (sprite != null)
                    sprite.flipX = false;

                break;

            case AnimationState.right:
                animator.SetBool("Side", true);

                if (sprite != null)
                    sprite.flipX = true;

                break;
        }
    }

    private void Update()
    {
        if (!interpolating)
            return;

        float duration = Mathf.Max(
            snapshotInterval,
            0.001f
        );

        interpolationTimer += Time.deltaTime;

        if (interpolationTimer >= duration)
        {
            transform.position = toPosition;

            if (snapshots.Count > 0)
            {
                StartNextInterpolation();
            }
            else
            {
                interpolating = false;
                interpolationTimer = 0f;
            }

            return;
        }

        float t = interpolationTimer / duration;

        transform.position = Vector3.Lerp(
            fromPosition,
            toPosition,
            t
        );
    }
}