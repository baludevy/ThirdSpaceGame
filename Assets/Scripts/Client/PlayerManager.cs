using System;
using UnityEngine;

namespace Client
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public GameObject localPlayerPrefab;
        public GameObject playerPrefab;

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

            Entity entity = Instantiate(prefab, position, rotation).GetComponent<Entity>();
            entity.Initialize(entityId);
            entity.SetEntityType(EntityType.player);

            Player player = entity.gameObject.GetComponent<Player>();
            player.Initialize(playerId, username);
        }
    }
}
