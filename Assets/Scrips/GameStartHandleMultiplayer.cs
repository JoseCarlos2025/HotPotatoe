using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameStartHandleMultiplayer : NetworkBehaviour
{
    public GameManagerMultiplayer gameManager;

    XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    protected new void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {

        // Notificar al GameManager
        if (gameManager != null && NetworkManager.Singleton.IsConnectedClient)
        {
            gameManager.RequestStartOrResumeGameServerRpc();
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (gameManager != null && NetworkManager.Singleton.IsConnectedClient)
        {
            gameManager.RequestPauseGameServerRpc();
        }
    }
}

