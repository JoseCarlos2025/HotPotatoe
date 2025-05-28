using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HotPotato : MonoBehaviour
{
    public float growRate = 0.2f;
    public float maxScale = 3f;
    public float timeToExplode = 60f;
    public ParticleSystem explosionParticles;
    public GameObject smokePrefab;

    public XRNode holdingNode = XRNode.LeftHand;

    public float hapticIntensityMultiplier = 1.0f;
    public float hapticFrequency = 0.5f;
    private float hapticTimer = 0f;

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
        UpdateHaptics();

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

    private void UpdateHaptics()
    {
        hapticTimer += Time.deltaTime;
        if (hapticTimer >= hapticFrequency)
        {
            hapticTimer = 0f;

            float intensity = Mathf.Clamp01(timer / timeToExplode) * hapticIntensityMultiplier;
            float duration = 0.1f;

            var side = holdingNode == XRNode.RightHand
                ? HapticsUtility.Controller.Right
                : HapticsUtility.Controller.Left;

            HapticsUtility.SendHapticImpulse(intensity, duration, side);
        }
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
        AudioManager.instance?.PlaySFX("explosion");

        // Vibración fuerte al que la sostiene
        var side = holdingNode == XRNode.RightHand
            ? HapticsUtility.Controller.Right
            : HapticsUtility.Controller.Left;

        HapticsUtility.SendHapticImpulse(1.0f, 0.3f, side);

        // Vibración leve en ambas manos
        HapticsUtility.SendHapticImpulse(0.5f, 0.2f, HapticsUtility.Controller.Left);
        HapticsUtility.SendHapticImpulse(0.5f, 0.2f, HapticsUtility.Controller.Right);

        // Explosión de partículas
        if (explosionParticles != null)
        {
            explosionParticles.transform.parent = null;
            explosionParticles.transform.position = transform.position;
            explosionParticles.Play();
            Destroy(explosionParticles.gameObject, explosionParticles.main.duration);
        }

        // Instanciar el humo
        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
            Destroy(smoke, 5f); // destruye el humo después de 5 segundos (ajustable)
        }

        OnExploded?.Invoke();
        Destroy(gameObject);
    }

    public void PassPotato()
    {
        timer = 0f;
        transform.localScale = initialScale;
        AudioManager.instance?.PlaySFX("correct");
    }

    public void AddPenaltyTime(float penaltySeconds)
    {
        timer += penaltySeconds;
        timer = Mathf.Min(timer, timeToExplode);
        UpdateScale();
        Debug.Log($"Penalización aplicada. Tiempo actual: {timer:F1}/{timeToExplode} segundos.");
        AudioManager.instance?.PlaySFX("incorrect");
    }
}
