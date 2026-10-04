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
                itemType = ItemType.hoe,
                position = new Vector2(-1.5f, 2f)
            },
            new Chest
            {
                id = 1,
                type = ObjectType.chest,
                itemType = ItemType.hoe,
                position = new Vector2(0.5f, 2f)
            },
            new Chest
            {
                id = 2,
                type = ObjectType.chest,
                itemType = ItemType.potatoSeed,
                position = new Vector2(2.5f, 2f)
            },
            new Chest
            {
                id = 3,
                type = ObjectType.chest,
                itemType = ItemType.carrotSeed,
                position = new Vector2(4.5f, 2f)
            },
        };

        public ushort nextObjectId;

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}
