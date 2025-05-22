using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestionManagerMultiplayer : NetworkBehaviour
{
    [System.Serializable]
    public class Answer
    {
        public string text;
        public bool correct;
    }

    [System.Serializable]
    public class Question
    {
        public string question;
        public List<Answer> answers;
    }

    [System.Serializable]
    public class QuestionDataM
    {
        public List<Question> questions;
    }

    [Header("UI References")]
    public TextAsset jsonFile;
    public TMP_Text questionText;
    public TMP_Text[] answerTexts;
    public GameObject panelUI;

    private List<Question> questions;

    // Jugadores conectados y en juego
    private List<ulong> activePlayers = new List<ulong>();
    private List<ulong> alivePlayers = new List<ulong>();

    private int currentQuestionIndex = 0;
    private int currentPlayerIndex = 0;

    // Flags locales
    private bool isMyTurn = false;
    private int myPlayerIndex = -1;

    private void Awake()
    {
        if (jsonFile == null) Debug.LogError("JsonFile no asignado.");
        if (questionText == null) Debug.LogError("QuestionText no asignado.");
        if (answerTexts == null || answerTexts.Length == 0) Debug.LogError("AnswerTexts no asignados.");
        if (panelUI == null) Debug.LogError("PanelUI no asignado.");
    }

    public override void OnNetworkSpawn()
    {
        // Carga preguntas en servidor y clientes
        LoadQuestions();
        ShuffleQuestions();
        panelUI.SetActive(false);
    }

    public void StartQuestions()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Solo el servidor puede iniciar las preguntas.");
            return;
        }

        if (questions == null || questions.Count == 0)
        {
            Debug.LogError("No hay preguntas cargadas en el servidor.");
            return;
        }

        // Recopilar jugadores conectados
        activePlayers.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            activePlayers.Add(client.Key);
        }

        if (activePlayers.Count < 2)
        {
            Debug.LogWarning("🚫 Se requieren al menos 2 jugadores.");
            return;
        }

        // Inicializar lista de jugadores vivos
        alivePlayers = new List<ulong>(activePlayers);

        currentQuestionIndex = 0;
        currentPlayerIndex = 0;

        ShowQuestionToCurrentPlayer();
    }

    void LoadQuestions()
    {
        if (jsonFile == null) return;
        var data = JsonUtility.FromJson<QuestionDataM>(jsonFile.text);
        questions = data?.questions;
        Debug.Log("📋 Preguntas cargadas: " + (questions != null ? questions.Count : 0));
    }

    void ShuffleQuestions()
    {
        if (questions == null) return;
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (questions[i], questions[j]) = (questions[j], questions[i]);
        }
    }

    void ShowQuestionToCurrentPlayer()
    {
        if (alivePlayers.Count <= 1 || currentQuestionIndex >= questions.Count)
            return;

        ulong targetClient = alivePlayers[currentPlayerIndex];
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { targetClient } }
        };

        UpdateClientQuestionClientRpc(currentQuestionIndex, currentPlayerIndex, rpcParams);
    }

    [ClientRpc]
    void UpdateClientQuestionClientRpc(int questionIndex, int playerIndex, ClientRpcParams rpcParams = default)
    {
        isMyTurn = true;
        myPlayerIndex = playerIndex;

        panelUI.SetActive(true);
        var q = questions[questionIndex];
        questionText.text = q.question;

        // Ocultar todos los botones
        foreach (var txt in answerTexts)
            txt.transform.parent.gameObject.SetActive(false);

        // Mostrar solo los tres botones del jugador actual
        int baseIndex = playerIndex * 3;
        for (int i = 0; i < 3 && i < q.answers.Count; i++)
        {
            int idx = baseIndex + i;
            if (idx < answerTexts.Length)
            {
                answerTexts[idx].text = q.answers[i].text;
                answerTexts[idx].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (!isMyTurn) return;

        int baseIndex = myPlayerIndex * 3;
        int localIndex = buttonIndex - baseIndex;
        if (localIndex < 0 || localIndex >= 3)
        {
            Debug.LogWarning("🚫 Este botón no pertenece al jugador actual.");
            return;
        }

        // Desactivar UI localmente
        isMyTurn = false;
        panelUI.SetActive(false);

        // Enviar elección al servidor
        SubmitAnswerServerRpc(localIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitAnswerServerRpc(int answerIndex, ServerRpcParams rpcParams = default)
    {
        if (currentQuestionIndex >= questions.Count || alivePlayers.Count <= 1)
            return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        if (clientId != alivePlayers[currentPlayerIndex])
        {
            Debug.LogWarning("Respuesta de jugador no válido.");
            return;
        }

        var answer = questions[currentQuestionIndex].answers[answerIndex];
        bool correct = answer.correct;

        if (!correct)
        {
            Debug.Log($"❌ Jugador {clientId} ha sido eliminado.");
            alivePlayers.Remove(clientId);
        }
        else
        {
            Debug.Log($"✅ Jugador {clientId} respondió correctamente.");
        }

        // Verificar fin de la partida
        if (alivePlayers.Count == 1)
        {
            ulong winner = alivePlayers[0];
            GameOverClientRpc(winner);
            Debug.Log($"🏆 Jugador {winner} gana la partida.");
            return;
        }

        // Avanzar pregunta
        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("📘 Fin de preguntas. Empate o varios supervivientes.");
            return;
        }

        // Avanzar al siguiente jugador vivo
        NextAlivePlayer();
        ShowQuestionToCurrentPlayer();
    }

    void NextAlivePlayer()
    {
        if (alivePlayers.Count == 0) return;
        currentPlayerIndex = (currentPlayerIndex + 1) % alivePlayers.Count;
    }

    [ClientRpc]
    void GameOverClientRpc(ulong winnerClientId)
    {
        // Aquí podrías mostrar UI de victoria
        panelUI.SetActive(false);
        Debug.Log($"🎉 ¡Jugador {winnerClientId} es el ganador!");
    }

    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }
}
