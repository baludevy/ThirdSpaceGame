using System;
using System.Linq;
using Types;
using UnityEngine;

namespace Server
{
    public class WorldManager : MonoBehaviour
    {
        public uint tick;
        public static WorldManager Instance;
        
        [NonSerialized]
        public ObjectManager objectManager;
        [NonSerialized]
        public EntityManager entityManager;
        [NonSerialized]
        public PlayerManager playerManager;


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            objectManager = new ObjectManager();
            entityManager = new EntityManager();
            playerManager = new PlayerManager();
            
            ItemHelper.CreateDroppedItem(ItemType.Potato, 30, new Vector3(2, -0.35f, 2));

            /* objectManager.SpawnObject(new Chest
            {
                id = 0,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(-1.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 1,
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(0.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 2,
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 65,
                position = new Vector3(2.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 3,
                type = ObjectType.Chest,
                itemType = ItemType.CarrotSeed,
                itemAmount = 65,
                position = new Vector3(4.5f, 2f, 0f)
            }); */
        }

        private void FixedUpdate()
        {
            TickTiles();
            TickEntities();
            SendWorldUpdates();

            tick++;
        }

        private void TickTiles()
        {
            
        }

        private void TickEntities()
        {
            foreach (Entity entity in entityManager.entities)
            {
                entity.Tick(Time.fixedDeltaTime);
            }
        }

        private void SendWorldUpdates()
        {
            foreach (Player player in playerManager.players)
            {
                ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.id));
            }
        }

        public WorldUpdate GetWorldUpdate(int excludePlayer = -1)
        {
            return new WorldUpdate
            {
                tick = tick,
                entityUpdates = entityManager.entities
                    .Where(entity => entity is not Player player || player.id != excludePlayer)
                    .Select(entity => entity.GetUpdate())
                    .ToList(),
            };
        }
    }
}
