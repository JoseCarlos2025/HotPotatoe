using UnityEngine;

public class HotPotato : MonoBehaviour
{
    public float growRate = 0.2f;
    public float maxScale = 3f;
    public float timeToExplode = 10f;
    private float timer = 0f;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float scaleFactor = 1 + (timer / timeToExplode) * (maxScale - 1);
        transform.localScale = initialScale * scaleFactor;

        if (timer >= timeToExplode || transform.localScale.x >= maxScale)
        {
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("¡La papa explotó!");

        Destroy(gameObject);
    }

    public void PassPotato()
    {
        timer = 0f;
        transform.localScale = initialScale;
    }
}

