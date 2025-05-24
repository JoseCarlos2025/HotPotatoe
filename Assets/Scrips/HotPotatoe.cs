using UnityEngine;

public class HotPotato : MonoBehaviour
{
    public float growRate = 0.2f;
    public float maxScale = 3f;
    public float timeToExplode = 60f;
    public ParticleSystem explosionParticles;

    private float timer = 0f;
    private Vector3 initialScale;
    private bool isPaused = true;

    public delegate void PotatoExploded();
    public event PotatoExploded OnExploded;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime;
        UpdateScale();

        if (timer >= timeToExplode || transform.localScale.x >= maxScale)
        {
            Explode();
        }
    }

    private void UpdateScale()
    {
        float scaleFactor = 1 + (timer / timeToExplode) * (maxScale - 1);
        transform.localScale = initialScale * scaleFactor;
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void StartPotato()
    {
        isPaused = false;
        timer = 0f;
        transform.localScale = initialScale;
    }

    void Explode()
    {
        Debug.Log("¡La papa explotó!");
        AudioManager.instance.PlaySFX("explosion");

        // ✅ Reproducir partículas
        if (explosionParticles != null)
        {
            explosionParticles.transform.parent = null;
            explosionParticles.transform.position = transform.position;
            explosionParticles.Play();
            Destroy(explosionParticles.gameObject, explosionParticles.main.duration);
        }

        OnExploded?.Invoke();
        Destroy(gameObject);
    }


    public void PassPotato()
    {
        timer = 0f;
        transform.localScale = initialScale;
    }

    public void AddPenaltyTime(float penaltySeconds)
    {
        timer += penaltySeconds;
        timer = Mathf.Min(timer, timeToExplode);
        UpdateScale();
        Debug.Log($"Penalización aplicada. Tiempo actual: {timer:F1}/{timeToExplode} segundos.");
    }
}
