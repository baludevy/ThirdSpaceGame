using UnityEngine;

public class ServerPlayer : MonoBehaviour {
    public int id;
    public string username;

    public void Initialize(int _id, string _username) {
        id = _id;
        username = _username;
    }
}