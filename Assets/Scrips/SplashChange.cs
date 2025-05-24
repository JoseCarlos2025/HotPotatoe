using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashChange : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
