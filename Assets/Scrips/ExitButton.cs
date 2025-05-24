using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Si estás en el editor, detener el modo Play
        EditorApplication.ExitPlaymode();
#else
        // Si es una compilación, salir del juego
        Application.Quit();
#endif
    }
}
