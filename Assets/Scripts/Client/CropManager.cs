using System;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.UIElements;

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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SpawnCrop(ushort cropId, CropType crop, int cropPhase, Vector2 position)
        {
            Crop cropInstance = ObjectManager.Instance.SpawnObject(cropId, ObjectType.Crop, position + Vector2.right * 0.5f).GetComponent<Crop>();
            cropInstance.cropType = crop;
            
            CropSpriteEntry cropSprite = cropSprites.Find(x => x.type == crop);

            cropInstance.GetComponent<SpriteRenderer>().sprite = cropSprite.stages[cropPhase];

            crops.Add(cropInstance);
        }

        public void UpdateCrop(ushort cropId, int cropPhase)
        {
            Crop crop = GetCrop(cropId);
            
            CropSpriteEntry cropSprite = cropSprites.Find(x => x.type == crop.cropType);
            crop.GetComponent<SpriteRenderer>().sprite = cropSprite.stages[cropPhase];
            
            crop.cropPhase = cropPhase;
        }
        
        public Crop GetCrop(ushort cropId) => crops.Find(x => x.id == cropId);
    }
}
