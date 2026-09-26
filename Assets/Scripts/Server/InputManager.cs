using System.Collections.Generic;
using Server;
using UnityEngine;

public struct MoveInput
{
    public Vector3 position;
    public Game.AnimationState animState;
}

public class InputManager
{
    private int targetBufferSize = 2;
    private int bufferThreshold = 1;

    private readonly Player player;
    private readonly Queue<MoveInput> incomingMoveInputs = new();

    public InputManager(Player player)
    {
        this.player = player;
    }

    public void AddMoveInput(MoveInput moveInput)
    {
        incomingMoveInputs.Enqueue(moveInput);
    }

    public void ProcessInputs()
    {
        int bufferSize = incomingMoveInputs.Count;

        int inputsToConsume;

        if (bufferSize < targetBufferSize - bufferThreshold)
        {
            inputsToConsume = 0;
        }
        else if (bufferSize > targetBufferSize + bufferThreshold)
        {
            inputsToConsume = 2;
        }
        else
        {
            inputsToConsume = 1;
        }

        inputsToConsume = Mathf.Min(inputsToConsume, bufferSize - 1);

        
        for (int i = 0; i < inputsToConsume; i++)
        {
            MoveInput input = incomingMoveInputs.Dequeue();

            player.transform.position = input.position;
            player.animState = input.animState;
        }
    }
}
