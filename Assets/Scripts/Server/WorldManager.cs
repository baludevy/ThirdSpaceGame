using System.Collections.Generic;
using Server;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public uint tick;
    
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

        tick++;
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
            ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.entityId));
        }
    }

    public World GetInitialWorld()
    {
        return new World
        {
            entities = EntityManager.Instance.entities
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
                entityId = player.entityId,
                position = player.transform.position,
                animState = player.animState,
            };

            playerUpdates.Add(update);
        }

        return new WorldUpdate
        {
            tick = tick,
            playerUpdates = playerUpdates,
        };
    }
}
