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
            objectManager = new ObjectManager();
            entityManager = new EntityManager();
            playerManager = new PlayerManager();

            tileManager.Initialize(new Vector2Int(101, 101));

            objectManager.SpawnObject(new Chest
            {
                id = 0,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector2(-1.5f, 2f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 1,
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector2(0.5f, 2f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 2,
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 65,
                position = new Vector2(2.5f, 2f)
            });

            objectManager.SpawnObject(new Chest
            {
                id = 3,
                type = ObjectType.Chest,
                itemType = ItemType.CarrotSeed,
                itemAmount = 65,
                position = new Vector2(4.5f, 2f)
            });
            
            if (entityManager.SpawnEntity(EntityType.pet, Vector2.right, broadcast: false) is Pet cat)
            {
                cat.petType = PetType.Cat;
                ServerSend.SpawnEntity(cat);
            }

            if (entityManager.SpawnEntity(EntityType.pet, Vector2.left, broadcast: false) is Pet dog)
            {
                dog.petType = PetType.Dog;
                ServerSend.SpawnEntity(dog);
            }
        }

        private void FixedUpdate()
        {
            TickTiles();
            TickEntities();
            SendTileUpdates();
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

        private void SendTileUpdates()
        {
            if (tileManager.updatedTiles.Count > 0)
                ServerSend.UpdateTiles(tileManager.updatedTiles);

            tileManager.updatedTiles.Clear();
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
