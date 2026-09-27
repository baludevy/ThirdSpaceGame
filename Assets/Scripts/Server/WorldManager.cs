using System;
using System.Collections.Generic;
using Server;
using Types;
using UnityEngine;
using UnityEngine.UIElements;

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

    void Start()
    {
        if (entityManager.SpawnEntity(EntityType.pet, Vector2.right, broadcast: false) is Pet cat)
        {
            cat.petType = PetType.Cat;
            ServerSend.SpawnEntity(cat);
        }
        
        if(entityManager.SpawnEntity(EntityType.pet, Vector2.left, broadcast: false) is Pet dog)
        {
            dog.petType = PetType.Dog;
            ServerSend.SpawnEntity(dog);
        }
    }

    private void FixedUpdate()
    {
        TickEntities();
        SendWorldUpdates();

        tick++;
    }

    private void TickEntities()
    {
        foreach (Entity entity in entityManager.entities)
        {
            entity.Tick(Time.fixedDeltaTime);
        }
    }

    private void SendWorldUpdates()
    {
        foreach (Player player in playerManager.players)
        {
            ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.id));
        }
    }

    public WorldUpdate GetWorldUpdate(int excludePlayer = -1)
    {
        List<EntityUpdate> entityUpdates = new List<EntityUpdate>();

        foreach (Entity entity in entityManager.entities)
        {
            if (entity is Player player)
            {
                if (player.id == excludePlayer)
                    continue;

                PlayerUpdate playerUpdate = new PlayerUpdate
                {
                    entityId = player.entityId,
                    entityType = player.entityType,
                    position = player.position,
                    animationState = player.animState,
                };

                entityUpdates.Add(playerUpdate);

                continue;
            }

            if (entity is Pet pet)
            {
                PetUpdate petUpdate = new PetUpdate
                {
                    entityId = pet.entityId,
                    entityType = pet.entityType,
                    position = pet.position,
                    facingRight = pet.facingRight,
                    animationState = pet.animationState,
                };

                entityUpdates.Add(petUpdate);

                continue;
            }

            EntityUpdate update = new EntityUpdate
            {
                entityId = entity.entityId,
                entityType = entity.entityType,
                position = entity.position,
            };

            entityUpdates.Add(update);
        }

        return new WorldUpdate
        {
            tick = tick,
            entityUpdates = entityUpdates,
        };
    }
}
