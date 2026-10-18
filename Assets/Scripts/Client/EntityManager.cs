using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Client
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance;

        public List<Entity> entities = new List<Entity>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public Entity ReplicateEntity(ushort id, EntityType type, Vector3 position, GameObject prefab)
        {
            
            Entity entity = Instantiate(prefab, position, prefab.transform.rotation).GetComponent<Entity>();
            
            entity.Initialize(id);

            entities.Add(entity);

            EntityInterpolationManager.Instance.AddSnapshot(entity, 0, new EntitySnapshot
            {
                time = Time.time,
                position = position,
            });

            return entity;
        }

        public void DestroyEntity(ushort entityId)
        {
            Destroy(GetEntity(entityId).gameObject);
        }

        public Entity GetEntity(ushort id) => entities.Find(x => x.entityId == id);
    }
}
