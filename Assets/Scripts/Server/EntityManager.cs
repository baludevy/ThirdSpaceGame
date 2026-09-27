using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance;

        public ushort nextEntityID;
        public List<Entity> entities = new List<Entity>();

        [SerializeField]public Dictionary<EntityType, GameObject> entityPrefabs = new Dictionary<EntityType, GameObject>();


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public Entity SpawnEntity(EntityType type, Vector3 position, Quaternion rotation, bool broadcast = true)
        {
            if (entityPrefabs[type].GetComponent<Entity>() == null)
            {
                Debug.Log(entityPrefabs[type].name + "has no Entity script attached.");
                return null;
            }

            Entity entity = Instantiate(entityPrefabs[type], position, rotation).GetComponent<Entity>();
            SceneManager.MoveGameObjectToScene(entity.gameObject, ServerSceneManager.Instance.serverScene);
              
                //assign a unique identifier to the entity so that both the client and the server can reference them later
                entity.Initialize(nextEntityID);

                if(broadcast)
                ServerSend.SpawnEntity(entity);

                entities.Add(entity);

                nextEntityID++;
                
                return entity;

        }
    }
}