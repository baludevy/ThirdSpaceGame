using System;
using System.Collections.Generic;
using UnityEngine;


public class ClientGameManager : MonoBehaviour {
    public static ClientGameManager Instance;

    [NonSerialized]
    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    public int myId;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SpawnPlayer(Player player) {
        if (player.gameObject == null) {
            if (player.id == myId) {
                ClientPlayer localClientPlayer =
                    Instantiate(PrefabManager.Instance.LocalPlayerPrefab, Vector3.zero, Quaternion.identity)
                        .GetComponent<ClientPlayer>();

                localClientPlayer.gameObject.GetComponent<ClientPlayer>().Initialize(player.id, player.username);
                player.gameObject = localClientPlayer.gameObject;

                return;
            }
            
            ClientPlayer remoteClientPlayer =
                Instantiate(PrefabManager.Instance.PlayerPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<ClientPlayer>();

            remoteClientPlayer.gameObject.GetComponent<ClientPlayer>().Initialize(player.id, player.username);
            player.gameObject = remoteClientPlayer.gameObject;
        }
    }
    
    public void AddPlayer(Player player) {
        players.Add(player.id, player);
    }

    public void RemovePlayer(Player player) {
        players.Remove(player.id);
    }

    public void RemovePlayer(int playerId) {
        players.Remove(playerId);
    }
}