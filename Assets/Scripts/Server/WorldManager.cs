using System.Collections.Generic;
using Server;
using UnityEngine;
using AnimationState = Game.AnimationState;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void FixedUpdate()
    {
        ProcessIncomingInputs();
        SendWorldUpdates();
    }

    private void ProcessIncomingInputs()
    {
        foreach (Player player in PlayerManager.Instance.players)
        {
            player.inputManager.ProcessInputs();
        }
    }

    private void SendWorldUpdates()
    {
        foreach (Player player in PlayerManager.Instance.players)
        {
            ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.id));
        }
    }

    public World GetInitialWorld()
    {
        return new World
        {
            entities = EntityManager.Instance.GetEntities()
        };
    }

    public WorldUpdate GetWorldUpdate(int excludePlayer = -1)
    {
        List<PlayerUpdate> playerUpdates = new List<PlayerUpdate>();

        foreach (Player player in PlayerManager.Instance.players)
        {
            if (player.id == excludePlayer)
                continue;

            PlayerUpdate update = new PlayerUpdate
            {
                id = player.id,
                position = player.transform.position,
                animState = player.animState,
            };

            playerUpdates.Add(update);
        }

        return new WorldUpdate
        {
            playerUpdates = playerUpdates,
        };
    }
}
