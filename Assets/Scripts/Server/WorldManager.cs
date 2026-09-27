using System;
using System.Collections.Generic;
using Server;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public uint tick;
    public static WorldManager Instance;
    
    [NonSerialized]
    public ObjectManager objectManager;
    [NonSerialized]
    public EntityManager entityManager;
    [NonSerialized]
    public PlayerManager playerManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        
        objectManager = new ObjectManager();
        entityManager = new EntityManager();
        playerManager = new PlayerManager();
    }

    private void FixedUpdate()
    {
        ProcessIncomingInputs();
        SendWorldUpdates();

        tick++;
    }

    private void ProcessIncomingInputs()
    {
        foreach (Player player in playerManager.players)
        {
            player.inputManager.ProcessInputs();
        }
    }

    private void SendWorldUpdates()
    {
        foreach (Player player in playerManager.players)
        {
            ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.entityId));
        }
    }
    
    public WorldUpdate GetWorldUpdate(int excludePlayer = -1)
    {
        List<PlayerUpdate> playerUpdates = new List<PlayerUpdate>();
        
        foreach (Player player in playerManager.players)
        {
            if (player.id == excludePlayer)
                continue;

            PlayerUpdate update = new PlayerUpdate
            {
                entityId = player.entityId,
                position = player.position,
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
