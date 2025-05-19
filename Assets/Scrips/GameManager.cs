using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public HotPotato hotPotato;
    public QuestionManager questionManager;

    private bool isFirstStart = true;

    [Header("Game Settings")]
    [SerializeField] private float penaltySeconds = 10f;

    void OnEnable()
    {
        hotPotato.OnExploded += HandlePotatoExploded;
        questionManager.OnAnswered += HandleQuestionAnswered;
    }

    void OnDisable()
    {
        hotPotato.OnExploded -= HandlePotatoExploded;
        questionManager.OnAnswered -= HandleQuestionAnswered;
    }

    public void StartOrResumeGame()
    {
        if (isFirstStart)
        {
            Debug.Log("Juego iniciado por primera vez.");
            questionManager.StartGame();
            hotPotato.StartPotato();
            isFirstStart = false;
        }
        else
        {
            Debug.Log("Juego reanudado.");
            hotPotato.Resume();
            hotPotato.PassPotato();
        }
    }

    public void PauseGame()
    {
        Debug.Log("Juego en pausa.");
        hotPotato.Pause();
    }

    void HandlePotatoExploded()
    {
        Debug.Log("🔴 La papa explotó. Fin del juego.");
        questionManager.ShowGameOverMessage();
    }

    void HandleQuestionAnswered(bool wasCorrect)
    {
        Debug.Log($"Respuesta {(wasCorrect ? "correcta ✅" : "incorrecta ❌")}. {(wasCorrect ? "Reiniciando" : "Penalizando")} papa...");

        if (wasCorrect)
        {
            hotPotato.PassPotato();
        }
        else
        {
            hotPotato.AddPenaltyTime(penaltySeconds);
        }
    }
}
