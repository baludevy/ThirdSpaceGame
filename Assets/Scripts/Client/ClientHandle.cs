using Client;
using Game;
using LiteNetLib.Utils;
using Types;
using UnityEngine;

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
        int objectCount = reader.GetInt();

        for (int i = 0; i < objectCount; i++)
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

        int entityCount = reader.GetInt();

        for (int i = 0; i < entityCount; i++)
        {
            ushort entityId = reader.GetUShort();
            EntityType type = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            if (type == EntityType.player)
            {
                int playerId = reader.GetInt();
                string username = reader.GetString();

                PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
                continue;
            }

            if (type == EntityType.pet)
            {
                PetType petType = (PetType)reader.GetByte();
                EntityManager.Instance.ReplicateEntity(entityId, type, position, PrefabManager.Instance.petPrefabs[petType]);
                continue;
            }
            
            EntityManager.Instance.ReplicateEntity(entityId, type, position);
        }
    }

    public static void SpawnEntity(NetDataReader reader)
    {
        ushort entityId = reader.GetUShort();
        EntityType type = (EntityType)reader.GetByte();
        Vector2 position = reader.GetVector2();

        if (type == EntityType.player)
        {
            int playerId = reader.GetInt();
            string username = reader.GetString();

            PlayerManager.Instance.SpawnPlayer(entityId, playerId, username, position);
            return;
        }
        
        if (type == EntityType.pet)
        {
            PetType petType = (PetType)reader.GetByte();
            EntityManager.Instance.ReplicateEntity(entityId, type, position, PrefabManager.Instance.petPrefabs[petType]);
            return;
        }

        if (type == EntityType.item)
        {
            ItemType itemType = (ItemType)reader.GetByte();
            GameObject itemPrefab = PrefabManager.Instance.itemPrefabs[itemType];

            DroppedItem droppedItem = EntityManager.Instance.ReplicateEntity(entityId, type, position, itemPrefab) as DroppedItem;

            if (droppedItem != null && droppedItem.gameObject.TryGetComponent(out BounceEffect bounce))
            {
                bounce.StartBounce();
            }

            return;
        }
        
        Entity entity = EntityManager.Instance.ReplicateEntity(entityId, type, position);
    }
    
    public static void UpdateInventory(NetDataReader reader)
    {
        int inventorySlotCount = 27;

        for (int i = 0; i < inventorySlotCount; i++)
        {
            int slotIndex = reader.GetByte();
            int itemCount = reader.GetByte();

            if (itemCount > 0)
            {
                ItemType itemType = (ItemType)reader.GetByte();
                
                Inventory.Instance.UpdateSlot(slotIndex, itemType, itemCount);
                Debug.Log($"Slot({slotIndex}) contains: {itemType} - {itemCount} pieces");
                
                continue;
            }
            
            Inventory.Instance.UpdateSlot(slotIndex, ItemType.empty, 0);
        }
    }
    
    public static void UpdateWorld(NetDataReader reader)
    {
        uint tick = reader.GetUInt();

        int entityUpdateCount = reader.GetInt();

        for (int i = 0; i < entityUpdateCount; i++)
        {
            ushort entityId = reader.GetUShort();
            EntityType entityType = (EntityType)reader.GetByte();
            Vector2 position = reader.GetVector2();

            if (entityType == EntityType.player)
            {
                AnimationState animState = (AnimationState)reader.GetByte();
                Entity player = EntityManager.Instance.GetEntity(entityId);

                EntityInterpolationManager.Instance.AddSnapshot(player, tick, new PlayerSnapshot
                {
                    time = Time.time,
                    position = position,
                    animationState = animState
                });

                continue;
            }

            if (entityType == EntityType.pet)
            {
                bool facingRight = reader.GetBool();
                PetAnimationState animState = (PetAnimationState)reader.GetByte();
                Entity pet = EntityManager.Instance.GetEntity(entityId);

                EntityInterpolationManager.Instance.AddSnapshot(pet, tick, new PetSnapshot
                {
                    time = Time.time,
                    position = position,
                    facingRight = facingRight,
                    animationState = animState
                });

                continue;
            }

            Entity entity = EntityManager.Instance.GetEntity(entityId);
            EntityInterpolationManager.Instance.AddSnapshot(entity, tick, new EntitySnapshot
            {
                time = Time.time,
                position = position,
            });
        }
    }

    public static void DestroyEntity(NetDataReader reader)
    {
        ushort entityId = reader.GetUShort();

        EntityManager.Instance.DestroyEntity(entityId);
    }

    public static void ChestOpened(NetDataReader reader)
    {
        ushort objId = reader.GetUShort();
        bool opened = reader.GetBool();

        ObjectManager.Instance.GetObject(objId).GetComponent<Chest>().SetOpened(opened);
    }
}
