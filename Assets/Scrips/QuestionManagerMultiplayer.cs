using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

[System.Serializable]
public class AnswerM
{
    public string text;
    public bool correct;
}

[System.Serializable]
public class QuestionM
{
    public string question;
    public List<AnswerM> answers;
}

[System.Serializable]
public class QuestionDataM
{
    public List<QuestionM> questions;
}

public class QuestionManagerMultiplayer : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text questionText;
    public Text[] answerTexts;
    public GameObject panelUI;

    [Header("JSON File")]
    public TextAsset jsonFile;

    private List<QuestionM> questions;
    private int currentQuestionIndex = 0;

    private static List<ulong> activePlayers = new();
    private static int currentPlayerIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (activePlayers.Count == 0)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                activePlayers.Add(client.ClientId);
            }

            LoadQuestions();
            ShuffleQuestions();
            ShowQuestionToCurrentPlayer();
        }
    }

    void LoadQuestions()
    {
        var data = JsonUtility.FromJson<QuestionDataM>(jsonFile.text);
        questions = data.questions;
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            int rand = Random.Range(i, questions.Count);
            var temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void ShowQuestionToCurrentPlayer()
    {
        if (activePlayers.Count <= 1)
        {
            DeclareWinnerClientRpc(activePlayers[0]);
            return;
        }

        if (currentQuestionIndex >= questions.Count)
        {
            EndGameClientRpc();
            return;
        }

        ulong currentPlayerId = activePlayers[currentPlayerIndex];
        UpdateClientQuestionClientRpc(currentQuestionIndex, currentPlayerId);
    }

    [ClientRpc]
    void UpdateClientQuestionClientRpc(int questionIndex, ulong targetClientId)
    {
        bool isMyTurn = NetworkManager.Singleton.LocalClientId == targetClientId;
        panelUI.SetActive(isMyTurn);

        if (!isMyTurn) return;

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

    public void OnAnswerSelected(int index)
    {
        if (!IsOwner || !panelUI.activeSelf) return;

        var question = questions[currentQuestionIndex];
        bool correct = question.answers[index].correct;

        SubmitAnswerServerRpc(correct);
        panelUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitAnswerServerRpc(bool correct)
    {
        if (!correct)
        {
            ulong eliminated = activePlayers[currentPlayerIndex];
            Debug.Log("❌ Eliminado: " + eliminated);
            activePlayers.RemoveAt(currentPlayerIndex);

            if (currentPlayerIndex >= activePlayers.Count)
                currentPlayerIndex = 0;
        }
        else
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
        }

        currentQuestionIndex++;
        ShowQuestionToCurrentPlayer();
    }

    [ClientRpc]
    void DeclareWinnerClientRpc(ulong winnerId)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            questionText.text = "🏆 ¡Ganaste!";
        }
        else
        {
            questionText.text = "😵 Has sido eliminado.";
        }

        foreach (var t in answerTexts) t.text = "";
        panelUI.SetActive(true);
    }

    [ClientRpc]
    void EndGameClientRpc()
    {
        questionText.text = "✅ Fin del juego: Sin preguntas.";
        foreach (var t in answerTexts) t.text = "";
        panelUI.SetActive(true);
    }
}
