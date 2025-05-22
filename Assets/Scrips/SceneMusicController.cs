using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    public void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case "Menu":
                AudioManager.instance.PlayMusic("Menu");
                break;
            case "Game":
                AudioManager.instance.PlayMusic("ThemeGameplay");
                break;
            case "Multiplayer":
                AudioManager.instance.PlayMusic("ThemeGameplay");
                break;
        }
    }
}