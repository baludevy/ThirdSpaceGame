using Client;
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
}
