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

    public static void UpdateWorld(NetDataReader reader)
    {
        int playerUpdateCount = reader.GetInt();

        for (int i = 0; i < playerUpdateCount; i++)
        {
            int id = reader.GetInt();
            Vector2 position = reader.GetVector2();
            AnimationState animState = (AnimationState)reader.GetByte();
            
            Debug.Log(animState);
            
            Player player = PlayerManager.Instance.GetPlayer(id);

            player.UpdateAnimationState(animState);
            
            if (player != null)
                player.transform.position = position;
        }
    }
}
