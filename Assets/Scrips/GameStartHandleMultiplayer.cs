using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && args.interactorObject is IXRSelectInteractor interactor)
            {
                var clientId = interactor.transform.root.GetComponent<NetworkObject>()?.OwnerClientId ?? 0;
                networkObject.ChangeOwnership(clientId);
            }
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.RemoveOwnership(); // Vuelve al servidor
            }
        }
    }

}

