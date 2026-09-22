using System;
using UnityEngine;

namespace Server
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        
        public GameObject playerPrefab;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
        {
            Entity entity = Instantiate(playerPrefab, position, rotation).GetComponent<Entity>();
            entity.Initialize(EntityManager.Instance.nextEntityID);
            
            Player player = entity.GetComponent<Player>();
            player.Initialize(id, username);
            
            ServerSend.SpawnEntity(entity);
        }
    }
}
