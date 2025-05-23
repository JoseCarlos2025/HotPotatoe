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
    private List<ulong> activePlayers = new List<ulong>();
    private List<ulong> alivePlayers = new List<ulong>();

    private int currentQuestionIndex = 0;
    private int currentPlayerIndex = 0;

    private bool isMyTurn = false;

    private void Awake()
    {
        if (jsonFile == null) Debug.LogError("JsonFile no asignado.");
        if (questionText == null) Debug.LogError("QuestionText no asignado.");
        if (answerTexts == null || answerTexts.Length == 0) Debug.LogError("AnswerTexts no asignados.");
        if (panelUI == null) Debug.LogError("PanelUI no asignado.");
    }

    public override void OnNetworkSpawn()
    {
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

        activePlayers.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
            activePlayers.Add(client.Key);

        if (activePlayers.Count < 2)
        {
            Debug.LogWarning("🚫 Se requieren al menos 2 jugadores.");
            return;
        }

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

        UpdateClientQuestionClientRpc(currentQuestionIndex, rpcParams);
    }

    [ClientRpc]
    void UpdateClientQuestionClientRpc(int questionIndex, ClientRpcParams rpcParams = default)
    {
        isMyTurn = true;
        panelUI.SetActive(true);

        // Limpiar respuestas
        foreach (var txt in answerTexts)
        {
            txt.text = "";
            txt.transform.parent.gameObject.SetActive(false);
        }

        var q = questions[questionIndex];
        questionText.text = q.question;

        for (int i = 0; i < q.answers.Count && i < answerTexts.Length; i++)
        {
            answerTexts[i].text = q.answers[i].text;
            answerTexts[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (!isMyTurn) return;

        isMyTurn = false;
        panelUI.SetActive(false);

        SubmitAnswerServerRpc(buttonIndex);
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
            alivePlayers.Remove(clientId);
            Debug.Log($"❌ Jugador {clientId} eliminado.");
        }
        else
        {
            Debug.Log($"✅ Jugador {clientId} respondió correctamente.");
        }

        if (alivePlayers.Count == 1)
        {
            ulong winner = alivePlayers[0];

            // Ganador
            var winnerParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { winner } }
            };
            ShowFinalMessageClientRpc("🎉 ¡Ganaste!", winnerParams);

            // Perdedores
            List<ulong> losers = new List<ulong>(activePlayers);
            losers.Remove(winner);
            var loserParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = losers.ToArray() }
            };
            ShowFinalMessageClientRpc("☹️ ¡Perdiste!", loserParams);

            return;
        }

        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("📘 Fin de preguntas.");
            return;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % alivePlayers.Count;
        ShowQuestionToCurrentPlayer();
    }

    [ClientRpc]
    void ShowFinalMessageClientRpc(string message, ClientRpcParams rpcParams = default)
    {
        panelUI.SetActive(true);
        questionText.text = message;

        // Ocultar respuestas
        foreach (var txt in answerTexts)
            txt.transform.parent.gameObject.SetActive(false);
    }

    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }
}
