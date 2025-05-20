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

        UpdateClientQuestionClientRpc(currentQuestionIndex, rpcParams);
    }

    [ClientRpc]
    void UpdateClientQuestionClientRpc(int questionIndex, ClientRpcParams rpcParams = default)
    {
        isMyTurn = true;
        panelUI.SetActive(true);

        var q = questions[questionIndex];
        questionText.text = q.question;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (i < q.answers.Count)
                answerTexts[i].text = q.answers[i].text;
            else
                answerTexts[i].text = "";
        }
    }

    public void SelectAnswer(int index)
    {
        if (!isMyTurn) return;

        var answer = questions[currentQuestionIndex].answers[index];

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
