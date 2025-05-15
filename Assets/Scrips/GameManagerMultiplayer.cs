using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManagerMultiplayer : NetworkBehaviour
{
    private HashSet<ulong> grabbingPlayers = new HashSet<ulong>();

    public void StartOrResumeGame()
    {
        Debug.Log("Juego iniciado o reanudado.");
    }

    public void PauseGame()
    {
        Debug.Log("Juego en pausa.");
    }

    public void PlayerGrabbed(ulong clientId)
    {
        grabbingPlayers.Add(clientId);
        CheckGrabStatus();
    }

    public void PlayerReleased(ulong clientId)
    {
        grabbingPlayers.Remove(clientId);
        CheckGrabStatus();
    }

    private void CheckGrabStatus()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;

        if (grabbingPlayers.Count == totalPlayers)
        {
            StartOrResumeGame();
            StartOrResumeGameClientRpc();
        }
        else
        {
            PauseGame();
            PauseGameClientRpc();
        }
    }

    [ClientRpc]
    void StartOrResumeGameClientRpc()
    {
        if (!IsServer)
            StartOrResumeGame();
    }

    [ClientRpc]
    void PauseGameClientRpc()
    {
        if (!IsServer)
            PauseGame();
    }
}


