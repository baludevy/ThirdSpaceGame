using System;
using System.Collections.Generic;
using Game;
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
                type = ObjectType.chest,
                position = Vector2.zero,
            }
        };

        public ushort nextObjectId;

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}
