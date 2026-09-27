using Client;
using Game;
using LiteNetLib.Utils;
using UnityEngine;
using AnimationState = Game.AnimationState;

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
            ushort id = reader.GetUShort();
            ObjectType type = (ObjectType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            ObjectManager.Instance.AddObject(id, type, position);
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
        Vector3 position = reader.GetVector2();

        if (type == EntityType.player)
        {
            int playerId = reader.GetInt();
            string username = reader.GetString();

            PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
            return;
        }

        EntityManager.Instance.ReplicateEntity(entityId, type, position);
    }

    public static void UpdateWorld(NetDataReader reader)
    {
        uint tick = reader.GetUInt();

        int playerUpdateCount = reader.GetInt();

        for (int i = 0; i < playerUpdateCount; i++)
        {
            ushort entityId = reader.GetUShort();
            Vector2 position = reader.GetVector2();
            AnimationState animState = (AnimationState)reader.GetByte();

            Entity entity = EntityManager.Instance.GetEntity(entityId);
            EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new PlayerSnapshot
            {
                time = Time.time,
                position = position,
                animationState = animState
            });
        }
    }

    public static void ChestOpened(NetDataReader reader)
    {
        ushort objId = reader.GetUShort();
        bool opened = reader.GetBool();
    }
}
