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

    private List<ulong> activePlayers = new List<ulong>();
    private int currentPlayerIndex = 0;
    private ulong currentPlayerId = 0;

    private bool isMyTurn = false;

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

        if (IsServer)
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
        currentPlayerId = activePlayers[currentPlayerIndex];

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
        isMyTurn = NetworkManager.Singleton.LocalClientId == activePlayers[playerIndex];
        panelUI.SetActive(isMyTurn); // Solo activa la UI si es tu turno

        var q = questions[questionIndex];
        questionText.text = q.question;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            answerTexts[i].transform.parent.gameObject.SetActive(false);
        }

        // Activar solo los 3 botones del jugador activo
        int baseIndex = playerIndex * 3;
        for (int i = 0; i < 3; i++)
        {
            int idx = baseIndex + i;
            if (idx < answerTexts.Length && i < q.answers.Count)
            {
                answerTexts[idx].text = q.answers[i].text;
                answerTexts[idx].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (!isMyTurn)
        {
            Debug.LogWarning("❌ No es tu turno");
            return;
        }

        int localClientIndex = activePlayers.IndexOf(NetworkManager.Singleton.LocalClientId);
        int expectedMin = localClientIndex * 3;
        int expectedMax = expectedMin + 2;

        if (buttonIndex < expectedMin || buttonIndex > expectedMax)
        {
            Debug.LogWarning($"⚠️ El botón {buttonIndex} no pertenece al jugador local (esperado: {expectedMin}-{expectedMax})");
            return;
        }

        int localIndex = buttonIndex - expectedMin;
        var answer = questions[currentQuestionIndex].answers[localIndex];

        if (answer.correct)
            Debug.Log("✅ Correcto");
        else
            Debug.Log("❌ Incorrecto");

        isMyTurn = false;
        panelUI.SetActive(false);

        if (IsServer)
        {
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
    }

    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }
}
