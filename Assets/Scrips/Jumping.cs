using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : MonoBehaviour
{
    [Header("Objetos que saltan")]
    public List<GameObject> jumpObjects = new List<GameObject>();

    private void Start()
    {
        foreach (GameObject obj in jumpObjects)
        {
            StartCoroutine(JumpLoop(obj));
        }
    }

    private IEnumerator JumpLoop(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;

        while (true)
        {
            // Generar parámetros aleatorios
            float height = Random.Range(0.5f, 1f);           // altura del salto
            float duration = Random.Range(0.35f, 1f);        // duración total
            float delay = Random.Range(0.1f, 1f);            // pausa entre saltos

            Vector3 peak = startPos + Vector3.up * height;

            float time = 0f;

            // Subida
            while (time < duration / 2f)
            {
                obj.transform.position = Vector3.Lerp(startPos, peak, time / (duration / 2f));
                time += Time.deltaTime;
                yield return null;
            }
            obj.transform.position = peak;

            time = 0f;

            // Bajada
            while (time < duration / 2f)
            {
                obj.transform.position = Vector3.Lerp(peak, startPos, time / (duration / 2f));
                time += Time.deltaTime;
                yield return null;
            }
            obj.transform.position = startPos;

            yield return new WaitForSeconds(delay);
        }
    }
}
