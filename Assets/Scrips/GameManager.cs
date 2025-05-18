using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public HotPotato hotPotato;
    public QuestionManager questionManager;

    [Header("Game Settings")]
    [SerializeField] private float penaltySeconds = 10f; // Tiempo que se suma al fallar

    private bool hasStarted = false;

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
        if (!hasStarted)
        {
            Debug.Log("🎮 Juego iniciado por primera vez.");
            hasStarted = true;

            // Instanciar la papa si no existe
            if (hotPotato != null)
            {
                hotPotato.Resume();
                hotPotato.PassPotato();
            }
            else
            {
                Debug.LogWarning("⚠️ No hay referencia a una papa activa.");
            }
        }
        else
        {
            Debug.Log("▶️ Reanudando juego.");
            hotPotato?.Resume();
        }
    }

    public void PauseGame()
    {
        Debug.Log("⏸ Juego en pausa.");
        hotPotato.Pause();
    }

    void HandlePotatoExploded()
    {
        Debug.Log("💥 La papa explotó. Fin del juego.");
        EndGame();
    }

    void HandleQuestionAnswered(bool wasCorrect)
    {
        if (wasCorrect)
        {
            Debug.Log("✅ Respuesta correcta. Reiniciando papa...");
            hotPotato.PassPotato();
        }
        else
        {
            Debug.Log("❌ Respuesta incorrecta. La papa se calienta...");
            hotPotato.AddPenaltyTime(penaltySeconds);
        }
    }

    void EndGame()
    {
        hotPotato.Pause();

        if (questionManager != null)
        {
            questionManager.ShowGameOverMessage();
        }

        // Aquí podrías agregar lógica para mostrar menú, reiniciar escena, etc.
    }
}
