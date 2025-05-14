using UnityEngine;
using Unity.Netcode;

public class GameManagerMultiplayer : NetworkBehaviour
{
    public void StartOrResumeGame()
    {
        Debug.Log("Juego iniciado o reanudado.");
        // Aquí va tu lógica de reanudar juego, por ejemplo: Time.timeScale = 1;
    }

    public void PauseGame()
    {
        Debug.Log("Juego en pausa.");
        // Aquí va tu lógica de pausa, por ejemplo: Time.timeScale = 0;
    }

    // RPCs para que los clientes puedan pedir iniciar/pausar al host (autoridad)
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartOrResumeGameServerRpc()
    {
        StartOrResumeGame();
        // Si quieres notificar a todos los clientes:
        StartOrResumeGameClientRpc();
    }

    [ClientRpc]
    void StartOrResumeGameClientRpc()
    {
        if (!IsServer) StartOrResumeGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPauseGameServerRpc()
    {
        PauseGame();
        PauseGameClientRpc();
    }

    [ClientRpc]
    void PauseGameClientRpc()
    {
        if (!IsServer) PauseGame();
    }
}

