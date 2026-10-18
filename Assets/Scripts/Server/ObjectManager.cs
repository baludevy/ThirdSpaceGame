using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class ObjectManager
    {
        public List<Object> objects = new List<Object>
        {
            new Chest
            {
                id = 0,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(-1.5f, 2f, 0f)
            },
            new Chest
            {
                id = 1,
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector3(0.5f, 2f, 0f)
            },
            new Chest
            {
                id = 2,
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 65,
                position = new Vector3(2.5f, 2f, 0f)
            },
            new Chest
            {
                id = 3,
                type = ObjectType.Chest,
                itemType = ItemType.CarrotSeed,
                itemAmount = 65,
                position = new Vector3(4.5f, 2f, 0f)
            },
        };

        public ushort nextObjectId;

        public Object SpawnObject(ObjectType type, Vector3 position = default)
        {
            Object obj = CreateObject(nextObjectId++, type, position);
            ServerSend.SpawnObject(obj);

            objects.Add(obj);

            return obj;
        }

        public static Object CreateObject(ushort id, ObjectType type, Vector3 position)
        {
            return type switch
            {
                ObjectType.Crop => new Crop { type = type, position = position },

                _ => new Object { type = type, position = position }
            };
        }

        public void DestroyObject(ushort id)
        {
            objects.Remove(GetObject(id));
            ServerSend.DestroyObject(id);
        }

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}
