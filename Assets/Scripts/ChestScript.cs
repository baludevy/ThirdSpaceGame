using System;
using UnityEngine;

public class ChestScript : MonoBehaviour, Interactable {
    public bool isOpened { get; private set; }
    public int chestId;

    public GameObject itemPrefab;
    public Sprite openedSprite;

    void Start() {
        ClientChestManager.Instance.RegisterChest(chestId, this);
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
            ClientChestManager.Instance.SpawnDroppedItem(itemPrefab, transform.position - new Vector3(0, 1, 0));
        }
    }

    public void SetOpened(bool opened) {
        
        GetComponent<SpriteRenderer>().sprite = openedSprite;
        isOpened = true;
    }
}