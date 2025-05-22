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

    public TextAsset jsonFile;
    public TMP_Text questionText;
    public TMP_Text[] answerTexts;
    public GameObject panelUI;

    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private bool isMyTurn = false;

    private List<ulong> activePlayers = new List<ulong>();
    private int currentPlayerIndex = 0;

    public void StartQuestions()
    {
        if (questions == null || questions.Count == 0)
        {
            LoadQuestions();
            ShuffleQuestions();
        }

        activePlayers.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
            activePlayers.Add(client.Key);

        currentQuestionIndex = 0;
        currentPlayerIndex = 0;

        ShowQuestionToCurrentPlayer();
    }

    void LoadQuestions()
    {
        var data = JsonUtility.FromJson<QuestionDataM>(jsonFile.text);
        questions = data.questions;
        Debug.Log("📋 Preguntas cargadas: " + questions.Count);
    }

    void ShuffleQuestions()
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = questions[i];
            questions[i] = questions[j];
            questions[j] = temp;
        }
    }

    void ShowQuestionToCurrentPlayer()
    {
        ulong currentPlayerId = activePlayers[currentPlayerIndex];

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { currentPlayerId }
            }
        };

        UpdateClientQuestionClientRpc(currentQuestionIndex, currentPlayerIndex, rpcParams);
    }

    [ClientRpc]
    void UpdateClientQuestionClientRpc(int questionIndex, int playerIndex, ClientRpcParams rpcParams = default)
    {
        isMyTurn = true;
        panelUI.SetActive(true);

        var q = questions[questionIndex];
        questionText.text = q.question;

        ShowAnswersForPlayer(playerIndex, q);
    }

    void ShowAnswersForPlayer(int playerIndex, Question question)
    {
        // Oculta todos los botones
        foreach (var t in answerTexts)
            t.transform.parent.gameObject.SetActive(false);

        int baseIndex = playerIndex * 3;

        for (int i = 0; i < 3; i++)
        {
            int idx = baseIndex + i;
            if (idx < answerTexts.Length && i < question.answers.Count)
            {
                answerTexts[idx].text = question.answers[i].text;
                answerTexts[idx].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    public void SelectAnswer(int localIndex)
    {
        if (!isMyTurn) return;

        // Verifica que el botón pulsado corresponde al jugador actual
        int baseIndex = currentPlayerIndex * 3;
        if (localIndex < 0 || localIndex >= 3)
        {
            Debug.LogWarning("⚠️ Botón no válido para este jugador.");
            return;
        }

        SubmitAnswerServerRpc(localIndex);
        isMyTurn = false;
        panelUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitAnswerServerRpc(int localIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (senderId != activePlayers[currentPlayerIndex])
        {
            Debug.LogWarning($"⚠️ Jugador {senderId} intentó responder fuera de turno.");
            return;
        }

        var question = questions[currentQuestionIndex];
        if (localIndex >= question.answers.Count)
        {
            Debug.LogWarning("⚠️ Índice de respuesta fuera de rango.");
            return;
        }

        var answer = question.answers[localIndex];

        if (answer.correct)
            Debug.Log($"✅ Jugador {senderId} respondió correctamente.");
        else
            Debug.Log($"❌ Jugador {senderId} respondió incorrectamente.");

        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("🏁 Juego terminado");
        }
        else
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            ShowQuestionToCurrentPlayer();
        }
    }

    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }
}
