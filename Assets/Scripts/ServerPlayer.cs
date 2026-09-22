using UnityEngine;

public class ServerPlayer : MonoBehaviour
{
    public int id;
    public string username;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
    }

    public void DropItem(ItemType itemType)
    {
        var droppedItem = new DroppedItemEntity
        {
            id = 0,
            position = transform.position - Vector3.up,
            itemType = itemType
        };

        ServerGameManager.Instance.droppedItems.Add(droppedItem);
        ServerSend.ItemDropped(id, droppedItem);
    }
}
