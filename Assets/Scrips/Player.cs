using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Player : NetworkBehaviour
{
    public GameObject ownedObjectPrefab; // Asignar desde el inspector

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Calcula localmente la posición a la izquierda del jugador
            Vector3 spawnPos = transform.position - transform.right * 2f;
            RequestSpawnOwnedObjectServerRpc(spawnPos);
        }
    }

    [ServerRpc]
    void RequestSpawnOwnedObjectServerRpc(Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        StartCoroutine(SpawnAfterDelay(OwnerClientId, spawnPosition));
    }

    private IEnumerator SpawnAfterDelay(ulong clientId, Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(3f); // Espera 3 segundos

        GameObject obj = Instantiate(ownedObjectPrefab, spawnPosition, Quaternion.identity);

        var netObj = obj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);
    }
}
