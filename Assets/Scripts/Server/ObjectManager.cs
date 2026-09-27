using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Server
{
    public class ObjectManager : MonoBehaviour
    {
        public List<Object> objects = new List<Object>();

        public ushort nextObjectId;
        
        public static ObjectManager Instance;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
        
        public void RegisterObject(ServerObject serverObj)
        {
            if (serverObj.Type == ObjectType.chest)
            {
                ChestObject obj = new ChestObject
                {
                    id = nextObjectId,
                    type = serverObj.Type,
                    go = serverObj.gameObject
                };
                
                serverObj.SetObjectInstance(obj);
                
                objects.Add(obj);
            }
        }
    }
}