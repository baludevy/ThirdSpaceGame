using Game;
using LiteNetLib.Utils;
using Types;
using UnityEngine;

namespace Client
{
    public static class ClientHandle
    {
        public static void Welcome(NetDataReader reader)
        {
            int id = reader.GetInt();

            NetworkManager.Instance.Client.myId = id;
            ClientSend.Username(NetworkUIManager.Instance.username);
        }

        public static void InitializeWorld(NetDataReader reader)
        {
            int tileCount = reader.GetInt();

            for (int i = 0; i < tileCount; i++)
                ReadTile(reader);

            int objectCount = reader.GetInt();

            for (int i = 0; i < objectCount; i++)
                ReadObject(reader);

            int entityCount = reader.GetInt();

            for (int i = 0; i < entityCount; i++)
                ReadEntity(reader);
        }

        public static void SpawnEntity(NetDataReader reader)
        {
            ReadEntity(reader, true);
        }

        public static void UpdateInventory(NetDataReader reader)
        {
            const int inventorySlotCount = 27;

            for (int i = 0; i < inventorySlotCount; i++)
            {
                int slotIndex = reader.GetByte();
                int itemCount = reader.GetByte();

                if (itemCount > 0)
                {
                    ItemType itemType = (ItemType)reader.GetByte();
                    Inventory.Instance.UpdateSlot(slotIndex, itemType, itemCount);
                }
                else
                {
                    Inventory.Instance.UpdateSlot(slotIndex, ItemType.empty, 0);
                }
            }
        }

        public static void UpdateTiles(NetDataReader reader)
        {
            int updatedTileCount = reader.GetInt();

            for (int i = 0; i < updatedTileCount; i++)
            {
                ReadTile(reader);
            }
        }

        public static void UpdateWorld(NetDataReader reader)
        {
            uint tick = reader.GetUInt();
            int entityUpdateCount = reader.GetInt();

            for (int i = 0; i < entityUpdateCount; i++)
                ReadEntityUdpate(reader, tick);
        }

        public static void DestroyEntity(NetDataReader reader)
        {
            EntityManager.Instance.DestroyEntity(reader.GetUShort());
        }

        public static void ChestOpened(NetDataReader reader)
        {
            ushort objectId = reader.GetUShort();
            bool opened = reader.GetBool();

            ObjectManager.Instance.GetObject(objectId).GetComponent<Chest>().SetOpened(opened);
        }

        private static void ReadTile(NetDataReader reader)
        {
            int tileId = reader.GetInt();
            TileType tileType = (TileType)reader.GetByte();

            TileManager.Instance.ModifyTile(tileId, tileType);
        }

        private static void ReadObject(NetDataReader reader)
        {
            ushort objectId = reader.GetUShort();
            ObjectType type = (ObjectType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            ObjectManager.Instance.AddObject(objectId, type, position);

            if (type == ObjectType.chest)
            {
                bool opened = reader.GetBool();
                ObjectManager.Instance.GetObject(objectId).GetComponent<Chest>().SetOpened(opened);
            }
        }

        private static void ReadEntity(NetDataReader reader, bool playSpawnEffect = false)
        {
            ushort entityId = reader.GetUShort();
            EntityType type = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            switch (type)
            {
                case EntityType.player:
                    int playerId = reader.GetInt();
                    string username = reader.GetString();

                    PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
                    break;


                case EntityType.pet:
                    PetType petType = (PetType)reader.GetByte();

                    EntityManager.Instance.ReplicateEntity(entityId, type, position, PrefabManager.Instance.petPrefabs[petType]);
                    break;


                case EntityType.item:
                    ItemType itemType = (ItemType)reader.GetByte();

                    DroppedItem droppedItem =
                        EntityManager.Instance.ReplicateEntity(entityId, type, position, PrefabManager.Instance.itemPrefabs[itemType]) as DroppedItem;

                    if (playSpawnEffect && droppedItem != null && droppedItem.TryGetComponent(out BounceEffect bounce))
                        bounce.StartBounce();

                    break;


                default:
                    EntityManager.Instance.ReplicateEntity(entityId, type, position);
                    break;
            }
        }

        private static void ReadEntityUdpate(NetDataReader reader, uint tick)
        {
            ushort entityId = reader.GetUShort();
            EntityType type = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            Entity entity = EntityManager.Instance.GetEntity(entityId);

            if (entity == null)
                return;

            switch (type)
            {
                case EntityType.player:
                    AnimationState animationState = (AnimationState)reader.GetByte();

                    EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new PlayerSnapshot
                    {
                        time = Time.time,
                        position = position,
                        animationState = animationState,
                    });

                    break;


                case EntityType.pet:
                    bool facingRight = reader.GetBool();
                    PetAnimationState petAnimationState = (PetAnimationState)reader.GetByte();

                    EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new PetSnapshot
                    {
                        time = Time.time,
                        position = position,
                        facingRight = facingRight,
                        animationState = petAnimationState,
                    });

                    break;


                default:
                    EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new EntitySnapshot
                    {
                        time = Time.time,
                        position = position,
                    });

                    break;
            }
        }
    }
}
