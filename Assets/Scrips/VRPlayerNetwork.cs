using UnityEngine;
using Unity.Netcode;

public class VRPlayerNetwork : NetworkBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private LocalXRInput localXRInput;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Buscar el XR Origin ya presente en la escena
            localXRInput = FindAnyObjectByType<LocalXRInput>();
            if (localXRInput != null)
            {
                localXRInput.AssignTargets(head, leftHand, rightHand);
            }
            else
            {
                Debug.LogWarning("No se encontró el LocalXRInput en la escena.");
            }
        }
    }

    protected new void OnDestroy()
    {
        if (IsOwner && localXRInput != null)
        {
            localXRInput.AssignTargets(null, null, null); // Limpiar referencias
        }
    }
}
