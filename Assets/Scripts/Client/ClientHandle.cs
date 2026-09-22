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

        ClientSend.Username(NetworkUIManager.Instance.username);
    }

    public static void SpawnEntity(NetDataReader reader)
    {
        ushort id = reader.GetUShort();
        EntityType type = (EntityType)reader.GetByte();
        Vector3 position = reader.GetVector3();
        Quaternion rotation = reader.GetQuaternion();
        
        EntityManager.Instance.ReplicateEntity(id, type, position, rotation);
    }
}
