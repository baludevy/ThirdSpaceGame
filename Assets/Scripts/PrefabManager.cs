using UnityEngine;

public class PrefabManager : MonoBehaviour
{

    public static PrefabManager Instance;
    public GameObject ServerGameManagerPrefab;
    public GameObject ClientGameManagerPrefab;

    public GameObject LocalPlayerPrefab;
    public GameObject PlayerPrefab;
    public GameObject ServerPlayerPrefab;

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
}
