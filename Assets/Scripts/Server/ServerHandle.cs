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
            
            ServerSend.InitializeWorld(peer.Id, WorldManager.Instance.GetInitialWorld());
            PlayerManager.Instance.SpawnPlayer(peer.Id, username, Vector3.zero, Quaternion.identity);
            
            Debug.Log($"Peer{peer.RemoteId}'s username is {username}");
        }

        public static void PlayerMove(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;
            Vector3 position = reader.GetVector2();
            Game.AnimationState animState = (Game.AnimationState)reader.GetByte();
            
            Debug.Log(animState);
            
            PlayerManager.Instance.GetPlayer(id).inputManager.AddMoveInput(new MoveInput
            {
                position = position,
                animState = animState
            });
        }
    }
}
