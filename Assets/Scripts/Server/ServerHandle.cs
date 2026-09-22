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
        
            Debug.Log($"Peer{peer.RemoteId}'s username is {username}");
        }
    }
}
