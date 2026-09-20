using System;
using UnityEngine;

public class ChestScript : MonoBehaviour, Interactable {
    public bool isOpened { get; private set; }
    public int chestId;

    public GameObject itemPrefab;
    public Sprite openedSprite;

    void Start() {
        ChestManager.Instance.RegisterChest(chestId, this);
    }

    public bool CanInteract() {
        return !isOpened;
    }

    public void Interact() {
        if(isOpened) return;
        
        ClientSend.OpenChest(chestId, itemPrefab.GetComponent<DroppedItem>().itemType);
        OpenChest();
    }

    public void OpenChest() {
        SetOpened(true);

        if (itemPrefab) {
            GameObject droppeditem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppeditem.GetComponent<BounceEffect>().Startbounce();
        }
    }

    public void SetOpened(bool opened) {
        
        GetComponent<SpriteRenderer>().sprite = openedSprite;
        isOpened = true;
    }
}