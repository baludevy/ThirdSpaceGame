using UnityEngine;

public class chestscript : MonoBehaviour, Interactable
{
    public bool isOpened {get; private set;}
    public string Chestid {get; private set;}
    public GameObject itemPrefab;
    public Sprite openedsprite;
    void Start()
    {
        Chestid ??= GlobalHelper.generateuniqueid(gameObject);
    }
    void Update()
    {
        
    }
    
    public bool CanInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        OpenChest();
    }
    private void OpenChest()
    {
        SetOpened(true);
        if (itemPrefab)
        {
            GameObject droppeditem  = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppeditem.GetComponent<BounceEffect>().StartBounce();
        }
    }
    public void SetOpened(bool opened)
    {
       if (isOpened = opened)
       {
            GetComponent<SpriteRenderer>().sprite = openedsprite;
       }
    }
}
