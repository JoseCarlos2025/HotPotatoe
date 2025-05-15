using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManagerMultiplayer : NetworkBehaviour
{
    private HashSet<ulong> grabbingPlayers = new HashSet<ulong>();

    [Header("Referencias")]
    public QuestionManagerMultiplayer questionManager; // <- Arrastrar en el Inspector

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

    void StartOrResumeGame()
    {
        Debug.Log("✅ Juego iniciado o reanudado.");
        questionManager.StartQuestions(); // Aquí se le avisa al otro script
    }

    void PauseGame()
    {
        Debug.Log("⏸️ Juego en pausa.");
        questionManager.PauseQuestions(); // (Opcional)
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
