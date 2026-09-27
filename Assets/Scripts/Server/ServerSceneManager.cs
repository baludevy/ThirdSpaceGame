using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerSceneManager : MonoBehaviour
{
    public Scene serverScene;

    public static ServerSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        Instance = this;
        else
        Destroy(this);
        StartCoroutine(LoadServerScene());
    }
    private IEnumerator LoadServerScene()
    {
        LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive,LocalPhysicsMode.Physics3D);
    AsyncOperation load = SceneManager.LoadSceneAsync("ServerScene", parameters);
    if (load == null)
        {
            yield break;
        }
        while (!load.isDone)
        yield return null;

        serverScene = SceneManager.GetSceneByName("ServerScene");
    }
}