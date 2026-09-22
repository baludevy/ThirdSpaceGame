using Client;
using Game;
using LiteNetLib.Utils;
using UnityEngine;

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
                return;
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
}
