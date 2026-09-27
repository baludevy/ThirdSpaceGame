using Client;
using Game;
using LiteNetLib.Utils;
using UnityEngine;

public static class ClientHandle
{
    public static void Welcome(NetDataReader reader)
    {
        int id = reader.GetInt();

        NetworkManager.Instance.Client.myId = id;
        ClientSend.Username(NetworkUIManager.Instance.username);
    }

    public static void InitializeWorld(NetDataReader reader)
    {
        int objectCount = reader.GetInt();

        for (int i = 0; i < objectCount; i++)
        {
            ushort objectId = reader.GetUShort();
            ObjectType type = (ObjectType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            ObjectManager.Instance.AddObject(objectId, type, position);

            if (type == ObjectType.chest)
            {
                bool opened = reader.GetBool();
                
                ObjectManager.Instance.GetObject(objectId).GetComponent<Chest>().SetOpened(opened);
            }
        }

        int entityCount = reader.GetInt();

        for (int i = 0; i < entityCount; i++)
        {
            ushort entityId = reader.GetUShort();
            EntityType type = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            if (type == EntityType.player)
            {
                int playerId = reader.GetInt();
                string username = reader.GetString();

                PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
                continue;
            }

            EntityManager.Instance.ReplicateEntity(entityId, type, position);
        }
    }

    public static void SpawnEntity(NetDataReader reader)
    {
        ushort entityId = reader.GetUShort();
        EntityType type = (EntityType)reader.GetByte();
        Vector2 position = reader.GetVector2();

        if (type == EntityType.player)
        {
            int playerId = reader.GetInt();
            string username = reader.GetString();

            PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
            return;
        }

        Entity entity = EntityManager.Instance.ReplicateEntity(entityId, type, position);
        
        if (entity.GetEntityType() == EntityType.item)
        {
            if (entity.gameObject.TryGetComponent(out BounceEffect bounce))
            {
                bounce.StartBounce();
            }
        }
    }

    public static void UpdateWorld(NetDataReader reader)
    {
        uint tick = reader.GetUInt();

        int entityUpdateCount = reader.GetInt();

        for (int i = 0; i < entityUpdateCount; i++)
        {
            ushort entityId = reader.GetUShort();
            EntityType entityType = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            if (entityType == EntityType.player)
            {
                AnimationState animState = (AnimationState)reader.GetByte();
                Entity player = EntityManager.Instance.GetEntity(entityId);

                EntityInterpolationManager.Instance.AddSnapshot(player, tick, new PlayerSnapshot
                {
                    time = Time.time,
                    position = position,
                    animationState = animState
                });

                continue;
            }

            if (entityType == EntityType.pet)
            {
                PetAnimationState animState = (PetAnimationState)reader.GetByte();
                Entity pet = EntityManager.Instance.GetEntity(entityId);

                EntityInterpolationManager.Instance.AddSnapshot(pet, tick, new PetSnapshot
                {
                    time = Time.time,
                    position = position,
                    animationState = animState
                });

                continue;
            }

            Entity entity = EntityManager.Instance.GetEntity(entityId);
            EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new EntitySnapshot
            {
                time = Time.time,
                position = position,
            });
        }
    }

    public static void DestroyEntity(NetDataReader reader)
    {
        ushort entityId = reader.GetUShort();
        
        EntityManager.Instance.DestroyEntity(entityId);
    }

    public static void ChestOpened(NetDataReader reader)
    {
        ushort objId = reader.GetUShort();
        bool opened = reader.GetBool();
        
        ObjectManager.Instance.GetObject(objId).GetComponent<Chest>().SetOpened(opened);
    }
}
