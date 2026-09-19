using TMPro;
using UnityEngine;

public class NetworkUIManager : MonoBehaviour {
    [SerializeField] private GameObject connectionPanel;
    
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField usernameField;
    
    public string username => usernameField.text;
    
    public static NetworkUIManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void HostGame() {
        NetworkManager.Instance.StartHost();
    }

    public void JoinGame() {
        string ip = ipField.text;
        
        if(ip == "")
            ip = "127.0.0.1";
        
        NetworkManager.Instance.Join(ip);
    }
    
    public void SetConnectionPanel(bool active) {
        connectionPanel.SetActive(active);
    }
}