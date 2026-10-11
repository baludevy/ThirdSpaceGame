using System;
using System.Collections.Generic;
using UnityEngine;
namespace Types
{
    [Serializable]
    public class PetDefinition
    {
        public string Name;
        public PetType Type;
        public GameObject Prefab;
    }

    public class PetCatalog : MonoBehaviour
    {
        public static PetCatalog Instance;

        [SerializeField] private List<PetDefinition> pets = new List<PetDefinition>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        
        public PetDefinition GetPet(PetType petType) => pets.Find(i => i.Type == petType);
    }
}
