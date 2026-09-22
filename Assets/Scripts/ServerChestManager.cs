using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerChest
{
    public int id;
    public ItemType itemType;
    public bool opened;
    public Vector2 position;
}

public class ServerChestManager : MonoBehaviour
{

    public static ServerChestManager Instance;
    
    [NonSerialized]
    public List<ServerChest> chests = new List<ServerChest>
    {
        new ServerChest
        {
            id = 0,
            position = new Vector2(-5.25f, -2.25f),
            itemType = ItemType.potato
        },
        new ServerChest
        {
            id = 1,
            position = new Vector2(-7.75f, -2.25f),
            itemType = ItemType.potato
        }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void OpenChest(int byPlayer, int chestId)
    {
        var chest = chests.Find(x => x.id == chestId);

        ServerGameManager.Instance.DropItem(new DroppedItemEntity
        {
            id = 0,
            position = chest.position - new Vector2(0, 1),
            itemType = chest.itemType
        });

        chest.opened = true;
        ServerSend.ChestOpened(byPlayer, chest.id, chest.itemType);
    }
}
