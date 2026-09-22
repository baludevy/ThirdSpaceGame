using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance;
        
        private List<Entity> entities = new List<Entity>();
        
        [SerializeField] public Dictionary<EntityType, GameObject> entityPrefabs = new Dictionary<EntityType, GameObject>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public void ReplicateEntity(ushort id, EntityType type, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = entityPrefabs[type];
            
            Entity entity = Instantiate(prefab, position, rotation).GetComponent<Entity>();
            
            entity.Initialize(id);
            
            entities.Add(entity);
        }
    }
}
