using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public GameObject localPlayerPrefab;
        public GameObject playerPrefab;

        public List<Player> players = new List<Player>();

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public void SpawnPlayer(ushort entityId, int playerId, string username, Vector3 position, Quaternion rotation)
        {
            Debug.Log($"Spawning player{playerId}");

            GameObject prefab = playerId == NetworkManager.Instance.Client.myId ? localPlayerPrefab : playerPrefab;

            Entity entity = EntityManager.Instance.ReplicateEntity(entityId, EntityType.player, position, rotation, prefab);

            Player player = entity.gameObject.GetComponent<Player>();
            player.Initialize(playerId, username);

            players.Add(player);
        }

        public Player GetPlayer(int id) => players.Find(x => x.id == id);
    }
}
