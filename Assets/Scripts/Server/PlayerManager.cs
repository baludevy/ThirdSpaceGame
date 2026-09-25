using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        
        public List<Player> players = new List<Player>();
        
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

            EntityManager.Instance.nextEntityID++;
            
            EntityManager.Instance.GetEntities().Add(entity);
            
            Player player = entity.GetComponent<Player>();
            player.Initialize(id, username);
            
            players.Add(player);
            
            ServerSend.SpawnEntity(entity);
        }
        
        public Player GetPlayer(int id) => players.Find(x => x.id == id);

        public void UpdatePlayer(int id, Vector3 position, Game.AnimationState animState)
        {
            Player player = players.Find(x => x.id == id);
            if (player == null)
                return;
            
            player.gameObject.transform.position = position;
        }
    }
}
