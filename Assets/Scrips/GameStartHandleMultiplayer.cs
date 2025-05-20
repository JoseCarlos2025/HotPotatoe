using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameStartHandleMultiplayer : NetworkBehaviour
{
    public GameManagerMultiplayer gameManager;
    XRGrabInteractable grabInteractable;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManagerMultiplayer>();
    }
    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManagerMultiplayer>();
    }

    protected new void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            NotifyGrabServerRpc();
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            NotifyReleaseServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void NotifyGrabServerRpc(ServerRpcParams rpcParams = default)
    {
        gameManager?.PlayerGrabbed(rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void NotifyReleaseServerRpc(ServerRpcParams rpcParams = default)
    {
        gameManager?.PlayerReleased(rpcParams.Receive.SenderClientId);
    }
}
