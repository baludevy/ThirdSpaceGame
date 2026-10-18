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
        public TileManager tileManager;
        [NonSerialized]
        public ObjectManager objectManager;
        [NonSerialized]
        public EntityManager entityManager;
        [NonSerialized]
        public PlayerManager playerManager;

        private bool isInitialized;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            tileManager = new TileManager();
            tileManager.GenerateTiles();
            
            objectManager = new ObjectManager();
            entityManager = new EntityManager();
            playerManager = new PlayerManager();
            
            ItemHelper.CreateDroppedItem(ItemType.Hoe, 1, new Vector3(-3, 0f, 2));
            ItemHelper.CreateDroppedItem(ItemType.PotatoSeed, 64, new Vector3(-4, 0f, 2));

            /* objectManager.SpawnObject(new Chest
            {
                id = 0,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(-3f, -0.1f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 1,
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 64,
                position = new Vector3(-1f, -0.1f, 0f)
            }); */

            /* objectManager.SpawnObject(new Chest
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

            if (entityManager.SpawnEntity(EntityType.pet, new Vector3(0, 0.075f, 0), broadcast: false) is Pet pet)
            {
                pet.petType = PetType.BrownCat;
                
                ServerSend.SpawnEntity(pet);
            }
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
            foreach (Tile tile in tileManager.tiles)
            {
                tile.Tick(Time.fixedDeltaTime);
            }
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
                    .ToList()
            };
        }

        public void ClearSession()
        {
            isInitialized = false;
            tick = 0;

            if (playerManager != null)
                playerManager.players.Clear();

            if (entityManager != null)
            {
                entityManager.entities.Clear();
                entityManager.nextEntityID = 0;
            }

            if (objectManager != null)
            {
                objectManager.objects.Clear();
                objectManager.nextObjectId = 0;
            }

            tileManager = null;
            objectManager = null;
            entityManager = null;
            playerManager = null;
        }
    }
}
