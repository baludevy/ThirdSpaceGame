using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player
{
    public GameObject gameObject;
    public int id;
    public string username;
}

public struct DroppedItemEntity
{
    public int id;
    public Vector2 position;
    public ItemType itemType;
}

public enum ItemType
{
    potato,
    carrot
}

public class ServerGameManager : MonoBehaviour
{
    public static ServerGameManager Instance;

    [NonSerialized]
    public List<DroppedItemEntity> droppedItems = new List<DroppedItemEntity>();

    [NonSerialized] public Dictionary<int, Player> players = new Dictionary<int, Player>();

    private Scene serverPlayerScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.LoadScene(
                "ServerScene",
                LoadSceneMode.Additive
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnPlayer(Player player)
    {
        serverPlayerScene = SceneManager.GetSceneByName("ServerScene");

        var serverPlayer = Instantiate(
            PrefabManager.Instance.ServerPlayerPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        SceneManager.MoveGameObjectToScene(
            serverPlayer,
            serverPlayerScene
        );

        player.gameObject = serverPlayer;

        player.gameObject.GetComponent<ServerPlayer>().Initialize(player.id, player.username);

        ServerSend.SpawnPlayer(player);

        var openedChests = new List<int>();

        foreach (var chest in ServerChestManager.Instance.chests)
        {
            if (chest.opened)
                openedChests.Add(chest.id);
        }

        ServerSend.InitializeWorld(droppedItems, openedChests, player.id);
    }

    public void AddPlayer(Player player)
    {
        players.Add(player.id, player);
    }

    public void RemovePlayer(Player player)
    {
        if (player.gameObject != null)
            Destroy(player.gameObject);

        players.Remove(player.id);
    }

    public void RemovePlayer(int playerId)
    {
        if (players.TryGetValue(playerId, out var player))
        {
            if (player.gameObject != null)
                Destroy(player.gameObject);

            players.Remove(playerId);
        }
    }

    public void DropItem(DroppedItemEntity item)
    {
        droppedItems.Add(item);
    }
}
