using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneChanger : MonoBehaviour
{
    // Cambia de escena por nombre
    public void ChangeSceneByName(string sceneName)
    {
        if (IsMultiplayerHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // Cambia de escena por índice
    public void ChangeSceneByIndex(int sceneIndex)
    {
        string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        ChangeSceneByName(sceneName);
    }

    // Recarga la escena actual
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        ChangeSceneByName(currentSceneName);
    }

    // Carga la siguiente escena del Build Settings
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            ChangeSceneByIndex(nextIndex);
        }
        else
        {
            Debug.LogWarning("No hay más escenas en el Build Settings.");
        }
    }

    // Devuelve true si se está en multijugador como host
    private bool IsMultiplayerHost()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening;
    }
}


