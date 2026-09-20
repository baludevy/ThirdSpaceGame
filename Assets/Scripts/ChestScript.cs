using System;
using UnityEngine;

public class ChestScript : MonoBehaviour, Interactable {
    public bool isOpened { get; private set; }
    public int chestId;

    public ItemType itemType;
    public Sprite openedSprite;

    void Start() {
        ClientChestManager.Instance.RegisterChest(chestId, this);
    }

    public bool CanInteract() {
        return !isOpened;
    }

    public void Interact() {
        if (isOpened) return;

        ClientSend.OpenChest(chestId, itemType);
        OpenChest();
    }

    public void OpenChest() {
        SetOpened(true);

        ClientChestManager.Instance.SpawnDroppedItem(itemType, transform.position - new Vector3(0, 1, 0));
    }

    public void SetOpened(bool opened) {
        GetComponent<SpriteRenderer>().sprite = openedSprite;
        isOpened = true;
    }
}