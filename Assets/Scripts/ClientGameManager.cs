using System.Collections.Generic;
using UnityEngine;


public class ClientGameManager : MonoBehaviour {
    public static ClientGameManager Instance;

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

    public void SpawnPlayer(int id) {
        if (players.ContainsKey(id) &&
            players[id].gameObject == null) {
            
            
            if (id == myId) {
                PlayerManager localPlayer =
                    Instantiate(PrefabManager.Instance.LocalPlayerPrefab, Vector3.zero, Quaternion.identity)
                        .GetComponent<PlayerManager>();

                localPlayer.id = id;
                players[id].gameObject = localPlayer.gameObject;

                return;
            }


            
            PlayerManager remotePlayer =
                Instantiate(PrefabManager.Instance.PlayerPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<PlayerManager>();

            remotePlayer.id = id;
            players[id].gameObject = remotePlayer.gameObject;
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