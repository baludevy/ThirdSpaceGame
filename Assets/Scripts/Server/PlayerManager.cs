using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class PlayerManager
    {
        public List<Player> players = new List<Player>();

        public void SpawnPlayer(int id, string username, Vector3 position)
        {
            if (WorldManager.Instance.entityManager.SpawnEntity(EntityType.player, position, broadcast: false) is Player player)
            {
                player.Initialize(id, username);

                players.Add(player);

                ServerSend.SpawnEntity(player);
            }
        }

        public void DestroyPlayer(int id)
        {
            Player targetPlayer = GetPlayer(id);
            Entity targetEntity = WorldManager.Instance.entityManager.GetEntity(targetPlayer.entityId);
            
            WorldManager.Instance.entityManager.DestroyEntity(targetEntity);
            players.Remove(targetPlayer);
        }

        public Player GetPlayer(int id) => players.Find(x => x.id == id);
    }
}
