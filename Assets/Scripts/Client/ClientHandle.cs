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

        Debug.Log($"Local id is {id}");

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
            Vector3 position = reader.GetVector2();
            
            if (type == ObjectType.chest)
            {
                ChestObject chestObj = new ChestObject
                {
                    id = id,
                    type = type,
                    opened = reader.GetBool(),
                };

                ObjectManager.Instance.AddObject(chestObj, position);
            }
        }

        int entityCount = reader.GetInt();

        Debug.Log($"Initializing world {entityCount}");

        for (int i = 0; i < entityCount; i++)
        {
            ushort entityId = reader.GetUShort();
            EntityType type = (EntityType)reader.GetByte();
            Vector3 position = reader.GetVector3();
            Quaternion rotation = reader.GetQuaternion();

            if (type == EntityType.player)
            {
                int playerId = reader.GetInt();
                string username = reader.GetString();

                PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position, rotation);
                continue;
            }

            EntityManager.Instance.ReplicateEntity(entityId, type, position, rotation);
        }
    }

    public static void SpawnEntity(NetDataReader reader)
    {
        ushort entityId = reader.GetUShort();
        EntityType type = (EntityType)reader.GetByte();
        Vector3 position = reader.GetVector3();
        Quaternion rotation = reader.GetQuaternion();

        if (type == EntityType.player)
        {
            int playerId = reader.GetInt();
            string username = reader.GetString();

            PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position, rotation);
            return;
        }

        EntityManager.Instance.ReplicateEntity(entityId, type, position, rotation);
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

            Debug.Log(entityId);
            
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
        
        ObjectManager.Instance.GetObject(objId).go.GetComponent<Chest>().SetOpened(opened);
    }
}