using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance;

        public List<Entity> entities = new List<Entity>();

        [SerializeField] public Dictionary<EntityType, GameObject> entityPrefabs = new Dictionary<EntityType, GameObject>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public Entity ReplicateEntity(ushort id, EntityType type, Vector3 position, Quaternion rotation, GameObject specialPrefab = null)
        {
            GameObject prefab;

            if (specialPrefab != null)
            {
                prefab = specialPrefab;
            }
            else
            {
                prefab = entityPrefabs[type];
            }
            

            Entity entity = Instantiate(prefab, position, rotation).GetComponent<Entity>();

            entity.Initialize(id);

            entities.Add(entity);

            EntityInterpolationManager.Instance.AddSnapshot(entity, 0, new EntitySnapshot
            {
                time = Time.time,
                position = position,
            });

            return entity;
        }

        public Entity GetEntity(ushort id) => entities.Find(x => x.entityId == id);
    }
}
