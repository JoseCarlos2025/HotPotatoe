using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public GameObject ownedObjectPrefab; // Asignar desde el inspector

    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Solo el dueño crea su objeto
        {
            SpawnOwnedObjectServerRpc();
        }
    }

    [ServerRpc]
    void SpawnOwnedObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f; // 1.5 unidades sobre el jugador
        GameObject obj = Instantiate(ownedObjectPrefab, spawnPosition, Quaternion.identity);

        var netObj = obj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId); // Asigna propiedad al jugador que posee este Player
    }
}
