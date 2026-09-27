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
            if(Instance == null)
                Instance = this;
            else
                Destroy (this);
        }

        public void AddObject(Object obj, UnityEngine.Vector2 position)
        {
           GameObject go = Instantiate(objectPrefabs[obj.type], position, Quaternion.identity);

           obj.go = go;

           if (obj.type == ObjectType.chest)
            {
                ChestObject chestObj = obj as ChestObject;
                Chest chest = go.GetComponent<Chest>();

                chest.SetOpened(chestObj.opened);
            }

            objects.Add(obj);
        }

        public Object GetObject(ushort id) => objects.Find(x => x.id == id);
    }
}