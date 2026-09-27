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

            ServerSend.InitalizeWorld(peer.Id, WorldManager.Instance.objectManager.objects, WorldManager.Instance.entityManager.entities);
            WorldManager.Instance.playerManager.SpawnPlayer(peer.Id, username, Vector3.zero);
        }
        
        public static void PlayerMove(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;
            Vector3 position = reader.GetVector2();
            Game.AnimationState animState = (Game.AnimationState)reader.GetByte();

            WorldManager.Instance.playerManager.GetPlayer(id).inputManager.AddMoveInput(new MoveInput
            {
                position = position,
                animState = animState
            });
        }
        
        public static void Interact(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.Interact();
        }
    }
}