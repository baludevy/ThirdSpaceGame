using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Server
{
    public static class ServerHandle
    {
        public static void Username(NetPeer peer, NetDataReader reader)
        {
            string username = reader.GetString();
            
            ServerSend.InitializeWorld(peer.Id, WorldManager.Instance.GetWorld());
            PlayerManager.Instance.SpawnPlayer(peer.Id, username, Vector3.zero, Quaternion.identity);
            
            Debug.Log($"Peer{peer.RemoteId}'s username is {username}");
        }
    }
}
