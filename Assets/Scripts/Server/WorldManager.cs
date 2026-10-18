using System;
using System.Linq;
using Types;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Server
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;

        public uint tick;

        [NonSerialized]
        public TileManager tileManager;

        [NonSerialized]
        public ObjectManager objectManager;

        [NonSerialized]
        public EntityManager entityManager;

        [NonSerialized]
        public PlayerManager playerManager;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void ResetSession()
        {
            ClearSession();

            tileManager = new TileManager();
            objectManager = new ObjectManager();
            entityManager = new EntityManager();
            playerManager = new PlayerManager();

            objectManager.objects.Clear();
            objectManager.nextObjectId = 0;

            tileManager.Initialize(new Vector2Int(101, 101));

            objectManager.SpawnObject(new Chest
            {
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(-1.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(0.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 65,
                position = new Vector3(2.5f, 2f, 0f)
            });

            objectManager.SpawnObject(new Chest
            {
                type = ObjectType.Chest,
                itemType = ItemType.CarrotSeed,
                itemAmount = 65,
                position = new Vector3(4.5f, 2f, 0f)
            });

            if (entityManager.SpawnEntity(EntityType.pet, Vector3.right, broadcast: false)is Pet cat)
                cat.petType = PetType.BrownCat;

            IsInitialized = true;
        }

        public void ClearSession()
        {
            IsInitialized = false;
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

            if (tileManager != null)
                tileManager.updatedTiles.Clear();

            tileManager = null;
            objectManager = null;
            entityManager = null;
            playerManager = null;
        }

        private void FixedUpdate()
        {
            if (!IsInitialized || NetworkManager.Instance == null ||
                !NetworkManager.Instance.Server.running)
                return;

            TickTiles();
            TickEntities();
            SendTileUpdates();
            SendWorldUpdate();

            tick++;
        } 

        private void TickTiles()
        {
            foreach (Tile tile in tileManager.tiles)
                tile.Tick(Time.fixedDeltaTime);
        }

        private void TickEntities()
        {
            foreach (Entity entity in entityManager.entities.ToArray())
                entity.Tick(Time.fixedDeltaTime);
        }

        private void SendTileUpdates()
        {
            if (tileManager.updatedTiles.Count > 0)
                ServerSend.UpdateTiles(tileManager.updatedTiles);

            tileManager.updatedTiles.Clear();
        }

        private void SendWorldUpdate()
        {
            foreach (Player player in playerManager.players.ToArray())
                ServerSend.UpdateWorld(player.id, GetWorldUpdate(player.id));
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

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            ClearSession();
            Instance = null;
        }
    }
}