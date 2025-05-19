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

    private List<Question> questions;
    private int currentQuestionIndex = 0;

    public delegate void QuestionAnswered(bool correct);
    public event QuestionAnswered OnAnswered;

    private bool hasGameStarted = false;

    void Start()
    {
        LoadQuestions();
        ShowStartMessage();
    }

    void LoadQuestions()
    {
        QuestionData data = JsonUtility.FromJson<QuestionData>(jsonFile.text);
        questions = data.questions;
        ShuffleQuestions();
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
        if (!hasGameStarted || currentQuestionIndex >= questions.Count) return;

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
        if (!hasGameStarted) return;

        var selectedAnswer = questions[currentQuestionIndex].answers[index];
        bool wasCorrect = selectedAnswer.correct;

        Debug.Log(wasCorrect ? "✅ ¡Respuesta correcta!" : "❌ Respuesta incorrecta");
        OnAnswered?.Invoke(wasCorrect);

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = false;
            }
        }

        Invoke(nameof(NextQuestion), 1f);
    }

    public void StartGame()
    {
        hasGameStarted = true;
        currentQuestionIndex = 0;
        ShuffleQuestions();
        ShowCurrentQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowCurrentQuestion();
        }
        else
        {
            questionText.text = "🎉 ¡Felicidades, has ganado!";
            foreach (var t in answerTexts) t.text = "";

            if (answerButtons != null)
            {
                foreach (var b in answerButtons)
                {
                    b.interactable = false;
                }
            }
        }
    }

    public void NextQuestionImmediate()
    {
        CancelInvoke();
        NextQuestion();
    }

    public void ShowStartMessage()
    {
        questionText.text = "🍴 Coge el tenedor para comenzar";
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
        questionText.text = "💥 ¡La papa explotó! Fin del juego.";
        foreach (var t in answerTexts) t.text = "";

        if (answerButtons != null)
        {
            foreach (var b in answerButtons)
            {
                b.interactable = false;
            }
        }
    }
}
