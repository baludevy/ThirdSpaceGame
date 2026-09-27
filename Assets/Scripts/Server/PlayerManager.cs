using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class PlayerManager
    {
        public List<Player> players = new List<Player>();

        public void SpawnPlayer(int id, string username, Vector3 position)
        {
            Player player = WorldManager.Instance.entityManager.SpawnEntity(EntityType.player, position, broadcast: false) as Player;

            if (player != null)
            {
                player.Initialize(id, username);

                players.Add(player);

                ServerSend.SpawnEntity(player);
            }
        }

        public Player GetPlayer(int id) => players.Find(x => x.id == id);
    }
}
