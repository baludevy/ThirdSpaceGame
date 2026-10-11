using System;
using System.Collections.Generic;
using Game;
using LiteNetLib.Utils;
using UnityEngine;

namespace Client
{
    public class ObjectManager : MonoBehaviour
    {
        public List<Object> objects = new List<Object>();

        [SerializeField] public Dictionary<ObjectType, GameObject> objectPrefabs = new Dictionary<ObjectType, GameObject>();

        public static ObjectManager Instance;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public Object SpawnObject(ushort id, ObjectType type, Vector2 position)
        {
            GameObject go = Instantiate(objectPrefabs[type], position, Quaternion.identity);
            Object obj = go.GetComponent<Object>();
            
            obj.id = id;
            obj.type = type;

            objects.Add(obj);

            return obj;
        }

        public void DestroyObject(ushort id)
        {
            Destroy(GetObject(id).gameObject);
            objects.Remove(GetObject(id));
        }

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}
