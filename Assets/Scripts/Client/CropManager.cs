using System;
using System.Collections.Generic;
using Types;
using UnityEngine;

namespace Client
{
    [Serializable]
    public class CropSpriteEntry
    {
        public CropType type;
        public List<Sprite> stages = new List<Sprite>();
    }

    public class CropManager : MonoBehaviour
    {
        public static CropManager Instance;
        
        [NonSerialized] public List<Crop> crops = new List<Crop>();

        [SerializeField] public List<CropSpriteEntry> cropSprites;
        [SerializeField] public GameObject cropPrefab;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else 
                Destroy(gameObject);
        }

        public void SpawnCrop(CropType crop, Vector2 position)
        {
            Crop cropInstance = Instantiate(cropPrefab, new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity).GetComponent<Crop>();

            crops.Add(cropInstance);
        }
    }
}
