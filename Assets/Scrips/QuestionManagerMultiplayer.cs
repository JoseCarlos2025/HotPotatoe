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
    private ulong localClientId;

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

        if (activePlayers.Count < 2 || activePlayers.Count > 4)
        {
            Debug.LogWarning("🚫 El número de jugadores debe estar entre 2 y 4.");
            return;
        }

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
        if (currentPlayerIndex >= activePlayers.Count)
            currentPlayerIndex = 0;

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

        // Desactivar todos los botones
        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].transform.parent.gameObject.SetActive(false);
        }

        // Mostrar solo los botones del jugador actual
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

        // Determinar si este botón pertenece al jugador actual
        int baseIndex = currentPlayerIndex * 3;
        if (buttonIndex < baseIndex || buttonIndex >= baseIndex + 3)
        {
            Debug.LogWarning("🚫 Este botón no pertenece al jugador actual.");
            return;
        }

        int localIndex = buttonIndex - baseIndex;
        var answer = questions[currentQuestionIndex].answers[localIndex];

        if (answer.correct)
            Debug.Log("✅ Correcto");
        else
            Debug.Log("❌ Incorrecto");

        isMyTurn = false;
        panelUI.SetActive(false);

        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("🏁 Juego terminado");
        }
        else
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            if (IsServer)
                ShowQuestionToCurrentPlayer();
        }
    }

    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }
}

