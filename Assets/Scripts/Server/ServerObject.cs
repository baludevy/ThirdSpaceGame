using System.Runtime.Serialization;
using UnityEngine;

namespace Server
{
    public class ServerObject : MonoBehaviour
    {
        public Object objectInstance;
        [SerializeField] private ObjectType type;
        public virtual ObjectType Type => type;

        private void Start()
        {
            Register();
        }
        public void Register()
        {
            ObjectManager.Instance.RegisterObject(this);
        }
        public void SetObjectInstance(Object obj)
        {
            objectInstance = obj;
        }
    }
}