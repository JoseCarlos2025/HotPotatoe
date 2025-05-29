using UnityEngine;
using System.Collections;

public class WindSpawner : MonoBehaviour
{
    public GameObject windParticlePrefab;
    public int totalParticles = 20;

    public Vector3 basePosition = new Vector3(5956.24f, 4f, -453.53f);
    public float spreadRadius = 10f;

    public float minSpawnDelay = 0.5f; // mínimo tiempo entre partículas
    public float maxSpawnDelay = 2.0f; // máximo tiempo entre partículas

    void Start()
    {
        StartCoroutine(SpawnParticlesOverTime());
    }

    IEnumerator SpawnParticlesOverTime()
    {
        for (int i = 0; i < totalParticles; i++)
        {
            Vector3 randomPos = GetRandomPositionAroundBase();
            Instantiate(windParticlePrefab, randomPos, Quaternion.identity);

            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    Vector3 GetRandomPositionAroundBase()
    {
        float offsetX = Random.Range(-spreadRadius, spreadRadius);
        float offsetZ = Random.Range(-spreadRadius, spreadRadius);
        return basePosition + new Vector3(offsetX, 0f, offsetZ);
    }
}
