using UnityEngine;

public class ChestScript : MonoBehaviour, Interactable {
    public bool isOpened { get; private set; }
    public string ChestId { get; private set; }

    public GameObject itemPrefab;
    public Sprite openedSprite;

    void Start() {
        ChestId ??= GlobalHelper.GenerateUniqueId(gameObject);
    }

    public bool CanInteract() {
        return !isOpened;
    }

    public void Interact() {
        if(isOpened) return;
        
        OpenChest();
    }

    private void OpenChest() {
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