using Unity.Netcode;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XRSpawnPositioner : NetworkBehaviour
{
    public Transform[] spawnPoints; // Asigna en el inspector
    private int currentIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (currentIndex >= spawnPoints.Length)
        {
            Debug.LogWarning("No hay más posiciones de spawn.");
            return;
        }

        Vector3 pos = spawnPoints[currentIndex].position;
        Quaternion rot = spawnPoints[currentIndex].rotation;

        // Le dice al cliente que se mueva a su spawn
        SendSpawnPositionClientRpc(clientId, pos, rot);

        currentIndex++;
    }

    [ClientRpc]
    private void SendSpawnPositionClientRpc(ulong targetClientId, Vector3 position, Quaternion rotation)
    {
        // Solo el cliente propietario se mueve
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        // Busca el XR Origin en escena
        var xrOrigin = FindAnyObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            Debug.LogError("No se encontró un XROrigin en la escena.");
        }
    }

    protected new void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}



