using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player {
    public int id;
    public string username;
    public GameObject gameObject;
}

public class ServerGameManager : MonoBehaviour {
    public static ServerGameManager Instance;

    [NonSerialized]
    public Dictionary<int, Player> players = new();

    private Scene serverPlayerScene;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.LoadScene(
                "ServerScene",
                LoadSceneMode.Additive
            );
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SpawnPlayer(Player player) {
        serverPlayerScene = SceneManager.GetSceneByName("ServerScene");

        GameObject serverPlayer = Instantiate(
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
    }

    public void AddPlayer(Player player) {
        players.Add(player.id, player);
    }

    public void RemovePlayer(Player player) {
        if (player.gameObject != null)
            Destroy(player.gameObject);

        players.Remove(player.id);
    }

    public void RemovePlayer(int playerId) {
        if (players.TryGetValue(playerId, out Player player)) {
            if (player.gameObject != null)
                Destroy(player.gameObject);

            players.Remove(playerId);
        }
    }
}