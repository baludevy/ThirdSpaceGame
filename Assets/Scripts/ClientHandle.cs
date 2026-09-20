using LiteNetLib.Utils;
using UnityEngine;

public static class ClientHandle {
    public static void Welcome(NetDataReader reader) {
        Player localPlayer = new Player {
            id = reader.GetInt(),
        };

        ClientGameManager.Instance.AddPlayer(localPlayer);
        ClientGameManager.Instance.myId = localPlayer.id;

        Debug.Log($"Local id is {localPlayer.id}");

        ClientSend.Username(NetworkUIManager.Instance.username);
    }

    public static void PlayerJoined(NetDataReader reader) {
        Player joinedPlayer = new Player {
            id = reader.GetInt(),
            username = reader.GetString(),
        };

        ClientGameManager.Instance.AddPlayer(joinedPlayer);

        Debug.Log($"{joinedPlayer.username} joined the game");
    }

    public static void PlayerLeft(NetDataReader reader) {
        int id = reader.GetInt();

        if (ClientGameManager.Instance.players[id].gameObject != null) {
            Object.Destroy(ClientGameManager.Instance.players[id].gameObject);
        }

        ClientGameManager.Instance.RemovePlayer(id);
    }

    public static void SpawnPlayer(NetDataReader reader) {
        int id = reader.GetInt();
        Debug.Log($"Spawning player {id}");
        
        Player player = ClientGameManager.Instance.players[id];

        ClientGameManager.Instance.SpawnPlayer(player);
    }

    public static void PlayerMove(NetDataReader reader)
    {
        int id = reader.GetInt();
        Vector2 position = reader.GetVector2(); 
        AnimationState animationState = (AnimationState)reader.GetByte();

        ClientGameManager manager = ClientGameManager.Instance;
        if (manager == null)
            return;

        if (id == manager.myId)
            return;

        if (!manager.players.TryGetValue(id, out Player player))
            return;

        if (player == null || player.gameObject == null)
            return;

        player.gameObject.GetComponent<ClientPlayer>().AddSnapshot(position, animationState);
    }

    public static void ChestOpened(NetDataReader reader) {
        int chestId = reader.GetInt();
        
        ClientChestManager.Instance.chests[chestId].OpenChest();
    }

    public static void ItemDropped(NetDataReader reader) {
        Vector2 position = reader.GetVector2();
        ItemType itemType = (ItemType)reader.GetByte();
        
        ClientChestManager.Instance.SpawnDroppedItem(ClientChestManager.Instance.itemTypePrefabs[itemType], position);
    }

    public static void InitializeWorld(NetDataReader reader) {
        int droppedItemEntityCount = reader.GetInt();

        for (int i = 0; i < droppedItemEntityCount; i++) {
            int id = reader.GetInt();
            Vector2 position = reader.GetVector2();
            ItemType itemType = (ItemType)reader.GetByte();
            
            ClientChestManager.Instance.SpawnDroppedItem(ClientChestManager.Instance.itemTypePrefabs[itemType], position);
            Debug.Log($"Dropped item: {id} ({itemType})");
        }
        
        int openedChestCount = reader.GetInt();

        for (int i = 0; i < openedChestCount; i++) {
            int chestId = reader.GetInt();
            
            ClientChestManager.Instance.chests[chestId].SetOpened(true);
        }
    }
}