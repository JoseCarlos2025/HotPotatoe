using UnityEngine;

public class ControladorDeCreditos : MonoBehaviour
{
    public GameObject panelCreditos;

    public void ShowCredits()
    {
        panelCreditos.SetActive(true);
    }

    public void HideCredits()
    {
        panelCreditos.SetActive(false);
    }
}
