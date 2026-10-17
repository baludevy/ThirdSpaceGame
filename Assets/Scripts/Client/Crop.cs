using Types;
using UnityEngine;

namespace Client
{
    public class Crop : Object, Interactable
    {
        public SpriteRenderer cropRenderer;
        
        public CropType cropType;
        public int cropPhase;

        public void SetCropType(CropType cropType)
        {
            this.cropType = cropType;

            SetCropPhase(cropPhase);
        }

        public void SetCropPhase(int phase)
        {
            cropPhase = phase;

            cropRenderer.sprite = Catalog.Instance.GetCrop(cropType).Sprite[phase];
        }
        
        public void Interact()
        {
            
        }
        
        public InteractionKind GetInteractionKind() => InteractionKind.Object;

        public ushort GetId() => id;
        
        public bool CanInteract() => cropPhase == 2;
    }
}
