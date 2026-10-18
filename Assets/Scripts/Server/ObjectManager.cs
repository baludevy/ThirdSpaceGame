using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class ObjectManager
    {
        public List<Object> objects = new List<Object>();

        public ushort nextObjectId;

        public Object SpawnObject(Object obj)
        {
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
