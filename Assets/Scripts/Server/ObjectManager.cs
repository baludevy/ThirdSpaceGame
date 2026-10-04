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
                position = new Vector2(-1.5f, 2f)
            },
            new Chest
            {
                id = 1,
                type = ObjectType.Chest,
                itemType = ItemType.Hoe,
                itemAmount = 1,
                position = new Vector2(0.5f, 2f)
            },
            new Chest
            {
                id = 2,
                type = ObjectType.Chest,
                itemType = ItemType.PotatoSeed,
                itemAmount = 65,
                position = new Vector2(2.5f, 2f)
            },
            new Chest
            {
                id = 3,
                type = ObjectType.Chest,
                itemType = ItemType.CarrotSeed,
                itemAmount = 65,
                position = new Vector2(4.5f, 2f)
            },
        };

        public ushort nextObjectId;

        public void SpawnObject(Object obj)
        {
            obj.id = nextObjectId++;
            
            ServerSend.SpawnObject(obj);
            
            objects.Add(obj);
        }

        public void DestroyObject(ushort id)
        {
            objects.Remove(GetObject(id));
            ServerSend.DestroyObject(id);
        }

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}
