using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

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
public class QuestionData
{
    public List<Question> questions;
}

public class QuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public Text[] answerTexts;
    public Button[] answerButtons;

    [Header("JSON File")]
    public TextAsset jsonFile;

    [Header("Game Over UI")]
    public GameObject gameOverUIPrefab;
    public Transform spawnPoint;

    private List<Question> questions;
    private int currentQuestionIndex = 0;

    public delegate void QuestionAnswered(bool correct);
    public event QuestionAnswered OnAnswered;

    private bool hasGameStarted = false;
    private bool isPaused = false;
    private bool isWaitingForNext = false;

    private int correctAnswers = 0;
    private float gameStartTime;
    private float gameEndTime;

    void Start()
    {
        LoadQuestions();
        ShowStartMessage();
    }

    void LoadQuestions()
    {
        QuestionData data = JsonUtility.FromJson<QuestionData>(jsonFile.text);
        List<Question> allQuestions = data.questions;

        for (int i = 0; i < allQuestions.Count; i++)
        {
            int rand = Random.Range(i, allQuestions.Count);
            var temp = allQuestions[i];
            allQuestions[i] = allQuestions[rand];
            allQuestions[rand] = temp;
        }

        int count = Mathf.Min(30, allQuestions.Count);
        questions = allQuestions.GetRange(0, count);
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

    public void ShowCurrentQuestion()
    {
        if (!hasGameStarted || currentQuestionIndex >= questions.Count || isPaused) return;

        var question = questions[currentQuestionIndex];
        questionText.text = question.question;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (i < question.answers.Count)
            {
                answerTexts[i].text = question.answers[i].text;
            }
            else
            {
                answerTexts[i].text = "";
            }
        }

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = true;
            }
        }
    }

    public void OnAnswerSelected(int index)
    {
        if (!hasGameStarted || isPaused || isWaitingForNext) return;

        isWaitingForNext = true;

        var selectedAnswer = questions[currentQuestionIndex].answers[index];
        bool wasCorrect = selectedAnswer.correct;

        if (wasCorrect)
        {
            correctAnswers++;
        }

        Debug.Log(wasCorrect ? "¡Respuesta correcta!" : "Respuesta incorrecta");
        OnAnswered?.Invoke(wasCorrect);

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = false;
            }
        }

        Invoke(nameof(NextQuestion), 1.5f); // Espera 1.5 segundos antes de continuar
    }

    public void StartGame()
    {
        hasGameStarted = true;
        isPaused = false;
        currentQuestionIndex = 0;
        correctAnswers = 0;
        gameStartTime = Time.time;
        ShuffleQuestions();
        ShowCurrentQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowCurrentQuestion();
            isWaitingForNext = false;
        }
        else
        {
            gameEndTime = Time.time;
            float totalTime = gameEndTime - gameStartTime;

            questionText.text = $"¡Felicidades, has ganado!\n" +
                                $"Aciertos: {correctAnswers}/{questions.Count}\n" +
                                $"Tiempo: {totalTime:F1} segundos";

            foreach (var t in answerTexts) t.text = "";

            if (answerButtons != null)
            {
                foreach (var b in answerButtons)
                {
                    b.interactable = false;
                }
            }

            ShowGameOverUI();
        }
    }

    public void NextQuestionImmediate()
    {
        CancelInvoke();
        isWaitingForNext = false;
        NextQuestion();
    }

    public void ShowStartMessage()
    {
        questionText.text = "Coge el tenedor para comenzar";
        foreach (var t in answerTexts) t.text = "";

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = false;
            }
        }
    }

    public void ShowGameOverMessage()
    {
        CancelInvoke();
        questionText.text = "¡La papa explotó! Fin del juego.";
        foreach (var t in answerTexts) t.text = "";

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = false;
            }
        }

        ShowGameOverUI();
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
        ShowCurrentQuestion();
    }

    void ShowGameOverUI()
    {
        if (gameOverUIPrefab != null && spawnPoint != null)
        {
            Instantiate(gameOverUIPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
