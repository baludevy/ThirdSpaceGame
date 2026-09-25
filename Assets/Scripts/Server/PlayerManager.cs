using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public List<Player> players = new List<Player>();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
        {
            Entity entity = EntityManager.Instance.SpawnEntity(EntityType.player, position, rotation, broadcast: false);

            Player player = entity.GetComponent<Player>();
            player.Initialize(id, username);

            players.Add(player);

            ServerSend.SpawnEntity(entity);
        }

        public Player GetPlayer(int id) => players.Find(x => x.id == id);
    }
}
