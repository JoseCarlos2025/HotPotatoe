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

    private bool isMyTurn = false;
    private bool questionsStarted = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (activePlayers.Count == 0)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                activePlayers.Add(client.ClientId);
            }

            // NO EMPIEZA AUTOMÁTICAMENTE
            // Se llamará a StartQuestions() manualmente desde GameManager
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
        isMyTurn = NetworkManager.Singleton.LocalClientId == targetClientId;
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
        if (!IsOwner || !isMyTurn || !panelUI.activeSelf) return;

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
        bool isWinner = NetworkManager.Singleton.LocalClientId == winnerId;

        questionText.text = isWinner ? "🏆 ¡Ganaste!" : "😵 Has sido eliminado.";
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

    // ✅ Llamado por el GameManager cuando todos tienen tenedor
    public void StartQuestions()
    {
        if (!IsServer || questionsStarted) return;

        questionsStarted = true;

        if (questions == null || questions.Count == 0)
        {
            LoadQuestions();
            ShuffleQuestions();
        }

        ShowQuestionToCurrentPlayer();
    }

    // ✅ Llamado por el GameManager si se quiere pausar el juego
    public void PauseQuestions()
    {
        panelUI.SetActive(false);
    }

    // ✅ (Opcional) Reinicia todo para volver a empezar
    public void ResetQuestions()
    {
        if (!IsServer) return;

        currentQuestionIndex = 0;
        currentPlayerIndex = 0;
        questionsStarted = false;

        activePlayers.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            activePlayers.Add(client.ClientId);
        }

        LoadQuestions();
        ShuffleQuestions();
        panelUI.SetActive(false);
    }

    // ✅ Hace que el panel mire al jugador (Canvas en World Space)
    void Update()
    {
        if (!isMyTurn || !panelUI.activeSelf) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 direction = cam.transform.position - panelUI.transform.position;
        direction.y = 0; // Evita inclinación vertical
        panelUI.transform.rotation = Quaternion.LookRotation(direction);

        // (Opcional) Si quieres que se mueva delante del jugador:
        /*
        Vector3 targetPosition = cam.transform.position + cam.transform.forward * 2f;
        targetPosition.y = cam.transform.position.y;
        panelUI.transform.position = Vector3.Lerp(panelUI.transform.position, targetPosition, Time.deltaTime * 5f);
        panelUI.transform.rotation = Quaternion.LookRotation(cam.transform.position - panelUI.transform.position);
        */
    }
}
