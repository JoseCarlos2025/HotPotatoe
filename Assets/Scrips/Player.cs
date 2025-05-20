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
            RequestSpawnOwnedObjectServerRpc();
        }
    }

    [ServerRpc]
    void RequestSpawnOwnedObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        StartCoroutine(SpawnAfterDelay(OwnerClientId));
    }

    private IEnumerator SpawnAfterDelay(ulong clientId)
    {
        yield return new WaitForSeconds(10f); 

        Vector3 spawnPosition = transform.position + Vector3.right * 2f;
        GameObject obj = Instantiate(ownedObjectPrefab, spawnPosition, Quaternion.identity);

        var netObj = obj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);
    }
}
