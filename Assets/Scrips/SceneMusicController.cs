using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    public void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case "Lobby":
                AudioManager.instance.FadeToMusic("Menu", 1f);
                break;
            case "Game":
            case "Multiplayer":
                AudioManager.instance.FadeToMusic("ThemeGameplay", 1f);
                break;
        }
    }
}
